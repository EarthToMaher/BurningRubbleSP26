using UnityEngine;
using UnityEngine.Splines;

public class PaintTrackArea : MonoBehaviour
{
    [Header("References")]
    public Terrain terrain;
    public Spline leftSpline;
    public SplineContainer container;
    public Spline rightSpline;

    [Header("Settings")]
    [Range(32, 2048)] public int splineSamples = 200;
    public int terrainTextureIndex = 0; // Which texture layer to paint
    public float blendFalloff = 1.0f;   // Smooth edge blend in meters

    [ContextMenu("Paint Track")]
    public void PaintTrack()
    {
        if (terrain == null || leftSpline == null || rightSpline == null)
        {
            Debug.LogError("Missing references!");
            return;
        }

        leftSpline = container[0];
        rightSpline = container[1];

        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrainData.size;

        int mapWidth = terrainData.alphamapWidth;
        int mapHeight = terrainData.alphamapHeight;
        int numLayers = terrainData.alphamapLayers;

        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, mapWidth, mapHeight);

        // Sample spline points
        Vector3[] leftPoints = new Vector3[splineSamples];
        Vector3[] rightPoints = new Vector3[splineSamples];
        for (int i = 0; i < splineSamples; i++)
        {
            float t = i / (float)(splineSamples - 1);
            leftPoints[i] = leftSpline.EvaluatePosition(t);
            rightPoints[i] = rightSpline.EvaluatePosition(t);
        }

        // Loop through terrain alpha map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Convert alphamap pixel to world position
                Vector3 worldPos = new Vector3(
                    (x / (float)mapWidth) * terrainSize.x + terrainPos.x,
                    0,
                    (y / (float)mapHeight) * terrainSize.z + terrainPos.z
                );

                // Check if inside or near the track area
                if (PointIsBetweenSplines(worldPos, leftPoints, rightPoints))
{
    for (int layer = 0; layer < numLayers; layer++)
        alphaMap[y, x, layer] = (layer == terrainTextureIndex) ? 1f : 0f;
}
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);
        Debug.Log("Track painted on terrain!");
    }

    /// <summary>
    /// Returns distance from a point to the nearest segment between the left/right splines.
    /// </summary>
    float DistanceToTrack(Vector3 point, Vector3[] left, Vector3[] right)
    {
        float minDist = float.MaxValue;

        for (int i = 0; i < left.Length - 1; i++)
        {
            Vector3 l0 = left[i];
            Vector3 l1 = left[i + 1];
            Vector3 r0 = right[i];
            Vector3 r1 = right[i + 1];

            // Build quad segment
            Vector3 nearest = ClosestPointOnQuadXZ(point, l0, l1, r1, r0);
            float d = Vector2.Distance(new Vector2(point.x, point.z), new Vector2(nearest.x, nearest.z));
            if (d < minDist) minDist = d;
        }

        return minDist;
    }

    /// <summary>
    /// Returns the closest point (XZ) on a quad defined by 4 corners.
    /// </summary>
    Vector3 ClosestPointOnQuadXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Vector3 closestABCD = ClosestPointOnTriangleXZ(p, a, b, c);
        Vector3 second = ClosestPointOnTriangleXZ(p, a, c, d);
        float da = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(closestABCD.x, closestABCD.z));
        float db = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(second.x, second.z));
        return da < db ? closestABCD : second;
    }

    Vector3 ClosestPointOnTriangleXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Flatten to XZ
        Vector2 p2 = new Vector2(p.x, p.z);
        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);
        Vector2 c2 = new Vector2(c.x, c.z);

        // Find closest point on each edge
        Vector2 ab = ClosestPointOnSegment(p2, a2, b2);
        Vector2 bc = ClosestPointOnSegment(p2, b2, c2);
        Vector2 ca = ClosestPointOnSegment(p2, c2, a2);

        // Choose nearest
        float da = Vector2.Distance(p2, ab);
        float db = Vector2.Distance(p2, bc);
        float dc = Vector2.Distance(p2, ca);

        Vector2 closest = (da < db && da < dc) ? ab : (db < dc ? bc : ca);
        return new Vector3(closest.x, 0, closest.y);
    }

    Vector2 ClosestPointOnSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }

    bool PointIsBetweenSplines(Vector3 point, Vector3[] left, Vector3[] right)
{
    for (int i = 0; i < left.Length - 1; i++)
    {
        if (PointInQuadXZ(point, left[i], left[i + 1], right[i + 1], right[i]))
            return true;
    }
    return false;
}

bool PointInQuadXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
{
    return PointInTriangleXZ(p, a, b, c) || PointInTriangleXZ(p, a, c, d);
}

bool PointInTriangleXZ(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
{
    Vector2 p2 = new Vector2(p.x, p.z);
    Vector2 a2 = new Vector2(a.x, a.z);
    Vector2 b2 = new Vector2(b.x, b.z);
    Vector2 c2 = new Vector2(c.x, c.z);

    float area = TriangleArea(a2, b2, c2);
    float area1 = TriangleArea(p2, b2, c2);
    float area2 = TriangleArea(a2, p2, c2);
    float area3 = TriangleArea(a2, b2, p2);

    return Mathf.Abs(area - (area1 + area2 + area3)) < 0.001f;
}

float TriangleArea(Vector2 a, Vector2 b, Vector2 c)
{
    return Mathf.Abs((a.x * (b.y - c.y) +
                      b.x * (c.y - a.y) +
                      c.x * (a.y - b.y)) * 0.5f);
}
}