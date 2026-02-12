using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineDuplicator : MonoBehaviour
{
    public SplineContainer container;
    public Spline startingSpline;
    public Spline innerSpline;
    public Spline outerSpline;
    public float width;
    public static int sample = 14;

    public SplineContainer newContainer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ContextMenu("Take Two")]
    public void SplineGenerationAttemptTwo()
    {
        innerSpline = GenerateOffsetSpline(startingSpline, width * 0.5f, false);
        container.AddSpline(innerSpline);
    }

    [ContextMenu("My Attempt")]
    public void MyTest()
    {
        startingSpline = container.Spline;
        innerSpline = MyAttempt(1);
        outerSpline = MyAttempt(-1);
        //container.AddSpline(innerSpline);
        //container.AddSpline(outerSpline);
        newContainer.AddSpline(outerSpline);
        newContainer.AddSpline(innerSpline);
    }

    public static Spline GenerateOffsetSpline(Spline baseSpline, float offsetDistance, bool sampleKnots = true)
    {
        int count = baseSpline.Count;
        if (count < 2) return null;

        // Build a list of evenly spaced samples (knots or parametric samples)
        List<Vector3> positions = new List<Vector3>();
        List<Vector3> tangents = new List<Vector3>();

        if (sampleKnots)
        {
            for (int i = 0; i < count; i++)
            {
                positions.Add(baseSpline[i].Position);
                // Estimate tangent from neighbours
                Vector3 tangent = (i == count - 1)
                    ? (positions[i] - positions[i - 1])
                    : ((Vector3)baseSpline[i + 1].Position - positions[i]);
                tangents.Add(tangent.normalized);
            }
        }
        else
        {
            int samples = sample;
            for (int i = 0; i <= samples; i++)
            {
                float t = i / (float)samples;
                positions.Add(baseSpline.EvaluatePosition(t));
                tangents.Add(((Vector3)baseSpline.EvaluateTangent(t)).normalized);
            }
        }

        // --- Parallel-transport frame setup ---
        Vector3 prevTangent = tangents[0].normalized;
        Vector3 up = Vector3.up;
        Vector3 right = Vector3.Cross(up, prevTangent).normalized;

        List<BezierKnot> newKnots = new List<BezierKnot>();

        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 tangent = tangents[i].normalized;

            // Compute rotation from previous tangent to current tangent
            Quaternion rot = Quaternion.FromToRotation(prevTangent, tangent);
            right = rot * right;
            up = Vector3.Cross(tangent, right).normalized;

            // Offset the point
            Vector3 offsetPos = positions[i] + right * offsetDistance;

            // Determine neighbor positions for auto smoothing
            Vector3 prevPos = (i > 0) ? positions[i - 1] : positions[i];
            Vector3 nextPos = (i < positions.Count - 1) ? positions[i + 1] : positions[i];

            BezierKnot newKnot = SplineUtility.GetAutoSmoothKnot(offsetPos, prevPos, nextPos);

            newKnots.Add(newKnot);

            prevTangent = tangent;
        }

        Spline result = new Spline();
        foreach (var k in newKnots)
            result.Add(k);

        return result;
    }


    [ContextMenu("Generate Track")]
    public void GenerateSplines()
    {
        startingSpline = container.Spline;
        List<Vector3> leftPoints = new List<Vector3>();
        List<Vector3> rightPoints = new List<Vector3>();

        for (int i = 0; i <= sample; i++)
        {
            float t = i / (float)sample;
            Debug.Log(t);
            Vector3 pos = startingSpline.EvaluatePosition(t);
            Vector3 tangent = startingSpline.EvaluateTangent(t);
            //tangent = tangent.normalized;

            Vector3 up = Vector3.up;
            Vector3 right = Vector3.Cross(up, tangent).normalized;

            float halfWidth = width * 0.5f;
            leftPoints.Add(pos - right * halfWidth);
            rightPoints.Add(pos + right * halfWidth);
            
        }

        innerSpline = BuildSpline(rightPoints);
        outerSpline = BuildSpline(leftPoints);

        container.AddSpline(innerSpline);
        container.AddSpline(outerSpline);
    }

    public Spline BuildSpline(List<Vector3> points)
    {
        Spline s = new Spline();
        foreach (var p in points)
        {

            BezierKnot knot = new BezierKnot(p);
            s.Add(knot);
        }
        var allKnots = new SplineRange(0, s.Count);
        s.Closed = true;
        s.SetTangentMode(allKnots, TangentMode.AutoSmooth);
        return s;
    }

    public Spline MyAttempt(int val)
    {
        Spline s = new Spline();

        for (int knotIndex = 0; knotIndex < startingSpline.Count; knotIndex++)
        {
            float tEnd = 0;
            Debug.Log(knotIndex);
            float tStart = startingSpline.ConvertIndexUnit(knotIndex, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            if (knotIndex < startingSpline.Count)
            {
                tEnd = startingSpline.ConvertIndexUnit(knotIndex + 1, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            }
            else
            {
                tEnd = startingSpline.ConvertIndexUnit(0, PathIndexUnit.Knot, PathIndexUnit.Normalized);
            }
            // Halfway param between those knots
            float tHalf = Mathf.Lerp(tStart, tEnd, 0.5f);

            // Evaluate position at halfway point
            Vector3 halfwayPos = startingSpline.EvaluatePosition(tHalf);

            Vector3 startingKnot = startingSpline.EvaluatePosition(tStart);

            Vector3 forward = (halfwayPos - startingKnot).normalized;

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;


            startingKnot = startingKnot + (val * right * width * 0.5f);

            halfwayPos = halfwayPos + (val*right * width * 0.5f);

            s.Add(startingKnot);
            s.Add(halfwayPos);
        }
        s.Closed = true;
        return s;
    }

    [ContextMenu("GPT Attempt")]
    public void GPTAttempt()
    {
        innerSpline = OffsetSpline(startingSpline, width * 0.5f);
        outerSpline = OffsetSpline(startingSpline, width * -0.5f);
        //SplineContainer newContainer = new SplineContainer();
        newContainer.AddSpline(innerSpline);
        newContainer.AddSpline(outerSpline);
    }

    public Spline OffsetSpline(Spline original, float offset)
    {
        Spline s = new Spline();

        for (int i = 0; i < original.Count; i++)
        {
            BezierKnot knot = original[i]; // direct access to the actual control knot
            Vector3 pos = knot.Position;
            Vector3 tangent = ((Vector3)(knot.TangentOut - knot.Position)).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

            // Offset the entire knot
            pos += right * offset;

            // Recreate handles, also offset in the same direction
            Vector3 tangentIn  = (Vector3)knot.TangentIn  + right * offset;
            Vector3 tangentOut = (Vector3)knot.TangentOut + right * offset;

            BezierKnot newKnot = new BezierKnot(pos, tangentIn, tangentOut, knot.Rotation);
            s.Add(newKnot);
        }

        s.Closed = original.Closed;
        return s;
    }
    
    
}
