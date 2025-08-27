using Cysharp.Threading.Tasks; // UniTask 사용을 위해 추가
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

public class ResourceManager : MonoBehaviour
{
    // 로드된 핸들과 참조 카운트를 저장하는 딕셔너리
    // 키: 어드레서블 키(string), 값: 로드 핸들
    private Dictionary<string, AsyncOperationHandle> _assetHandles = new Dictionary<string, AsyncOperationHandle>();
    private Dictionary<string, int> _referenceCounts = new Dictionary<string, int>();

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<ResourceManager>())
        {
            ServiceLocator.Register<ResourceManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // [수정] 반환 타입을 UniTask로 변경하여 await 키워드 사용 가능하게 함
    public async UniTask<T> LoadAsync<T>(string key) where T : Object
    {
        Debug.Log($"[ResourceManager] LoadAsync called for key: {key}");

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[ResourceManager] 에셋 키가 null이거나 비어있습니다.");
            return null;
        }

        // Try to get an existing valid handle
        if (_assetHandles.TryGetValue(key, out var handle) && handle.IsValid())
        {
            Debug.Log($"[ResourceManager] Found valid existing handle for key: {key}. Incrementing ref count.");
            _referenceCounts[key]++;
            return await handle.Convert<T>().Task;
        }
        // If handle is not found or not valid, load a new one
        else if (_assetHandles.ContainsKey(key) && !handle.IsValid())
        {
            Debug.LogWarning($"[ResourceManager] Found INVALID existing handle for key: {key}. Removing and loading new.");
            // If we found an invalid handle for this key, remove it before loading a new one
            _assetHandles.Remove(key);
            _referenceCounts.Remove(key); // Also remove its reference count
        }
        else
        {
            Debug.Log($"[ResourceManager] No valid existing handle found for key: {key}. Starting new load.");
        }


        var newHandle = Addressables.LoadAssetAsync<T>(key);
        _assetHandles.Add(key, newHandle);
        _referenceCounts.Add(key, 1);
        Debug.Log($"[ResourceManager] Started Addressables.LoadAssetAsync for key: {key}. Handle status: {newHandle.Status}");

        T result = await newHandle.Task;
        Debug.Log($"[ResourceManager] Addressables.LoadAssetAsync for key: {key} completed. Final status: {newHandle.Status}");

        if (newHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[ResourceManager] 에셋 로드 실패: {key} | {newHandle.OperationException}");
            // On failure, release the handle and remove it from tracking
            Addressables.Release(newHandle);
            _assetHandles.Remove(key);
            _referenceCounts.Remove(key);
            return null;
        }
        Debug.Log($"[ResourceManager] Asset '{key}' successfully loaded. Result: {result?.name ?? "NULL"}");
        return result;
    }

    public AsyncOperationHandle<IList<T>> LoadAllAsync<T>(string key) where T : Object
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[ResourceManager] 에셋 키(레이블)가 null이거나 비어있습니다.");
            return default;
        }

        if (_assetHandles.TryGetValue(key, out var handle))
        {
            _referenceCounts[key]++;
            return handle.Convert<IList<T>>();
        }

        // LoadAssetsAsync (복수형)을 사용합니다.
        var newHandle = Addressables.LoadAssetsAsync<T>(key, null);
        _assetHandles.Add(key, newHandle);
        _referenceCounts.Add(key, 1);

        newHandle.Completed += (h) =>
        {
            if (h.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[ResourceManager] 에셋 목록 로드 실패: {key} | {h.OperationException}");
                _assetHandles.Remove(key);
                _referenceCounts.Remove(key);
            }
        };

        return newHandle;
    }


    /// <summary>
    /// 지정된 키의 어드레서블 에셋을 릴리즈합니다.
    /// 참조 카운트가 0이 되면 실제로 메모리에서 해제됩니다.
    /// </summary>
    /// <param name="key">어드레서블 에셋 키</param>
    public void Release(string key)
    {
        if (string.IsNullOrEmpty(key) || !_assetHandles.ContainsKey(key))
        {
            // Debug.LogWarning($"[ResourceManager] 릴리즈하려는 에셋이 로드 목록에 없습니다: {key}");
            return;
        }

        _referenceCounts[key]--;
        if (_referenceCounts[key] <= 0)
        {
            Addressables.Release(_assetHandles[key]);
            _assetHandles.Remove(key);
            _referenceCounts.Remove(key);
        }
    }
}
