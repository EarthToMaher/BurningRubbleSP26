using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public class VoxelLoader : MonoBehaviour
{
    public string address; // e.g. "Chunk_01"

    private AsyncOperationHandle<GameObject> loadHandle;
    private AsyncOperationHandle<GameObject> instantiateHandle;

    IEnumerator Start()
    {
        // Load & instantiate asynchronously in one step
        instantiateHandle = Addressables.InstantiateAsync(address, transform.position, Quaternion.identity);
        yield return instantiateHandle;

        if (instantiateHandle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject instance = instantiateHandle.Result;
            Debug.Log($"Successfully instantiated {address}!");
        }
        else
        {
            Debug.LogError($"Failed to instantiate addressable asset: {address}");
        }
    }

    private void OnDestroy()
    {
        // Release the instantiated object
        Addressables.ReleaseInstance(instantiateHandle);
    }
}