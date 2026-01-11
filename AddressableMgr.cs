using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;

public class AddressableMgr : MonoBehaviour
{
    #region 데이터 초기화 관련 필드
    public Text txt_patchSize;
    public Slider slider_patch;
    public Text txt_patchPercent;

    // Group의 Label들
    [SerializeField] private string[] Labels;

    [SerializeField] private long _patchTotalSize;
    // Label별 용량의 크기.
    private Dictionary<string, long> _patchSizePerLabels = new();

    private bool _bInit = false;
    #endregion

    // Sprite, Shader, Material 처럼 인스터스가 아닌 리소스들
    private Dictionary<string, (AsyncOperationHandle handle, int refCnt)> _sharedAssets = new();
    // 인스터스화가 필요한 데이터들.
    private Dictionary<string, List<AsyncOperationHandle<GameObject>>> _sharedInstances = new();

    [Inject] private SceneMgr _sceneMgr;

    // 어드레서블에서 Remove하려고 대기중인 참조들.
    private Dictionary<string, AsyncOperationHandle<GameObject>> _markReleaseObjs = new();
    private HashSet<string> _markReleaseKeys = new();

    private Coroutine cor_autoReleaseProcess;
    private bool _bAutoReleaseEnabled = true;


    #region 데이터 확인 및 불러오기, 제거
    public void InitCheck()
    {
        StartCoroutine(InitAddressable());
        StartCoroutine(CheckUpdateFiles());
    }

    private void OnDestroy()
    {
        ReleaseAll();
    }

    public void OnClickDeleteData()
    {
        _patchTotalSize = default;

        foreach (var label in Labels)
        {
            // byte로 받아옴
            Addressables.ClearDependencyCacheAsync(label);
        }
    }

    IEnumerator InitAddressable()
    {
        if (!_bInit)
        {
            var init = Addressables.InitializeAsync();
            yield return init;
            _bInit = true;
        }
    }

    IEnumerator CheckUpdateFiles()
    {
        yield return new WaitUntil(() => _bInit);

        _patchTotalSize = default;

        foreach(var label in Labels)
        {
            // byte로 받아옴
            var handle = Addressables.GetDownloadSizeAsync(label);
            yield return handle;
            _patchTotalSize += handle.Result;
            
        }

        if(_patchTotalSize > decimal.Zero)
        {
            txt_patchSize.text = GetFileSize(_patchTotalSize);
        }
        else
        {
            slider_patch.value = 1;
            txt_patchSize.text = "0mb";
            txt_patchPercent.text = "100 %";
        }

        StartCoroutine(PatchFiles());
    }

    IEnumerator PatchFiles()
    {
        foreach (var label in Labels)
        {
            // byte로 받아옴
            var handle = Addressables.GetDownloadSizeAsync(label);
            yield return handle;

            if(handle.Result != decimal.Zero)
            {
                StartCoroutine(DownLoadLabel(label));
            }
        }

        yield return CheckDownLoad();
    }

    IEnumerator DownLoadLabel(string label)
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();
        _patchSizePerLabels.Add(label, 0);

        var handle = Addressables.DownloadDependenciesAsync(label, false);
        while(!handle.IsDone)
        {
            _patchSizePerLabels[label] = handle.GetDownloadStatus().DownloadedBytes;
            yield return wfeof;
        }

        _patchSizePerLabels[label] = handle.GetDownloadStatus().TotalBytes;
        Addressables.Release(handle);
    }

    IEnumerator CheckDownLoad()
    {
        WaitForEndOfFrame wfeof = new WaitForEndOfFrame();

        var total = 0f;
        txt_patchPercent.text = "0 %";

        while(true)
        {
            foreach(var file in _patchSizePerLabels)
            {
                total += file.Value;
            }

            if (total > 0)
            {
                slider_patch.value = total / _patchTotalSize;
                txt_patchPercent.text = (int)(slider_patch.value * 100) + " %";
            }

            if(total == _patchTotalSize)
            {
                yield return new WaitForSeconds(2f);

                _sceneMgr.LoadScene(eScene.World);
                break;
            }

            total = 0f;
            yield return wfeof;
        }
    }

    private string GetFileSize(long byteVal)
    {
        string size = "0 Bytes";

        if(byteVal >= 1073741824.0)
        {
            size = string.Format("{0:##.##}", byteVal / 1073741824.0) + " GB";
        }
        else if(byteVal >= 1048576.0)
        {
            size = string.Format("{0:##.##}", byteVal / 1048576.0) + " MB";
        }
        else if (byteVal >= 1024.0)
        {
            size = string.Format("{0:##.##}", byteVal / 1024.0) + " KB";
        }
        else if(byteVal > 0 && byteVal < 1024.0)
        {
            size = byteVal + " Bytes";
        }

        return size;
    }
    #endregion

    #region 데이터 Load 및 Release
    void StartAutoReleaseProcess()
    {
        if (cor_autoReleaseProcess == null)
        {
            cor_autoReleaseProcess = StartCoroutine(AutoReleaseProcess());
        }
    }

    public async UniTask<T> LoadAssetAsync<T>(string key)
    {
        StartAutoReleaseProcess();

        if(_markReleaseKeys.Contains(key))
        {
            CancelMarkKeyAsset(key);
        }

        if (_sharedAssets.TryGetValue(key, out var loadAsset))
        {
            _sharedAssets[key] = (loadAsset.handle, loadAsset.refCnt + 1);

            if(loadAsset.handle.Result is T)
            {
                return (T)loadAsset.handle.Result;
            }
            else
            {
                Debug.LogError($"Type mismatch for key: {key}. Expected: {typeof(T)}, Got: {loadAsset.handle.Result?.GetType()}");
                return default;
            }
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;

        if(handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load Addressable asset : {key}");
            Addressables.Release(handle);
            return default;
        }

        Debug.Log($"[LoadAsset] Successfully loaded: {key}, Result type: {handle.Result?.GetType().Name ?? "NULL"}");

        _sharedAssets[key] = (handle, 1);
        return handle.Result;
    }

    public async UniTask<AsyncOperationHandle<GameObject>> InstantiateAsync(string key, Transform parent)
    {
        StartAutoReleaseProcess();

        if(_markReleaseObjs.ContainsKey(key))
        {
            CancelMarkObjAsset(key);
        }


        var handle = Addressables.InstantiateAsync(key, parent);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load Addressable asset : {key}");
            Addressables.Release(handle);
            return default;
        }

        if (!_sharedInstances.TryGetValue(key, out var list))
        {
            list = new();
            list.Add(handle);
            _sharedInstances[key] = list;
        }

        return handle;
    }

    // Asset 타입을 반환. Release 호출 -> Mark에 담아둠 
    public void ReleaseAsset(string key)
    {
        if (_sharedAssets.TryGetValue(key, out var loadedAsset))
        {
            int releasedCnt = loadedAsset.refCnt - 1;
            // Ref 카운트가 0이 된다면 Mark 리스트에 넣어서 제거될 타이밍에 제거하기.
            if (releasedCnt <= 0)
            {
                _sharedAssets[key] = (loadedAsset.handle, 0);
                MarkAssetForRelease(key);
            }
            else
            {
                _sharedAssets[key] = (loadedAsset.handle, releasedCnt);
            }
        }
    }

    public void ReleaseAssets(params string[] keys)
    {
        foreach (var key in keys)
        {
            ReleaseAsset(key);
        }
    }

    public void ReleaseInstance(string key, AsyncOperationHandle<GameObject> instance)
    {
        if (_sharedInstances.TryGetValue(key, out var list))
        {
            MarkInstanceForRelease(key, instance);            
        }
    }

    void MarkInstanceForRelease(string key, AsyncOperationHandle<GameObject> instance)
    {
        if (_sharedInstances.TryGetValue(key, out var inst))
        {
            // Asset이랑 다르게 Instance가 여러개여도 하나씩 해제를 해야 함. 생성한거라서.
            if (!_markReleaseObjs.ContainsKey(key))
            {
                _markReleaseObjs.Add(key, instance);
            }
        }
    }

    void MarkAssetForRelease(string key)
    {
        if (_sharedAssets.TryGetValue(key, out var loadedAsset))
        {
            if (loadedAsset.refCnt > 0)
            {
                Debug.LogError($"레퍼런스가 1개 이상 사용중인데 해제 대기중 {key}");
                return;
            }

            if (!_markReleaseKeys.Contains(key))
            {
                _markReleaseKeys.Add(key);
            }
        }
    }

    // 제거하려는 스케줄러에 넣어놨지만 다시 사용해서 스케줄러에서 빼내는 것.
    void CancelMarkKeyAsset(string key)
    {
        if (_markReleaseKeys.Contains(key))
        {
            _markReleaseKeys.Remove(key);
        }
    }

    void CancelMarkObjAsset(string key)
    {
        if(_markReleaseObjs.ContainsKey(key))
        {
            _markReleaseObjs.Remove(key);
        }
    }

    public void ReleaseAll()
    {
        foreach (var kvp in _sharedAssets)
        {
            Addressables.Release(kvp.Value.handle);
        }
        _markReleaseKeys.Clear();

        foreach (var kvp in _sharedInstances)
        {
            foreach (var handle in kvp.Value)
            {
                if (handle.IsValid())
                {
                    Addressables.ReleaseInstance(handle);
                }
            }

            kvp.Value.Clear();
        }
        _markReleaseObjs.Clear();

        _sharedAssets.Clear();
    }
    #endregion

    private IEnumerator AutoReleaseProcess()
    {
        WaitForSeconds delay = new WaitForSeconds(3f);
        while(true)
        {
            // 프로세스가 계속 돌다가 쌓인 레퍼런스가 있다면 제거.
            if(_bAutoReleaseEnabled && (_markReleaseKeys.Count > 0 || _markReleaseObjs.Count > 0))
            {
                yield return ProcessBatch();
            }
            else
            {
                yield return delay;
            }
        }
    }

    // 실제 쌓여있는 리소스들을 해제하는 중.
    private int _processed;
    private IEnumerator ProcessBatch()
    {
        // 한 번에 처리할 개수.
        const int BATCH_SIZE = 3;
        int batchedCnt = 0;

        int i = 0;
        foreach(string key in _markReleaseKeys)
        {
            if (_sharedAssets.TryGetValue(key, out var loadedAsset))
            {
                Debug.Log("KEY : " + loadedAsset.refCnt);
                Addressables.Release(loadedAsset.handle);
                if (loadedAsset.refCnt == 0)
                {
                    _sharedAssets.Remove(key);
                    batchedCnt++;
                    _processed++;
                }
            }

            i++;
            if(i > BATCH_SIZE) break;
        }

        i = batchedCnt;
        foreach(var obj in _markReleaseObjs)
        {
            _sharedInstances.Remove(obj.Key);

            Addressables.ReleaseInstance(obj.Value.Result);

            if (_sharedInstances[obj.Key].Count == 0)
            {
                _sharedInstances.Remove(obj.Key);
                batchedCnt++;
                _processed++;
            }

            i++;
            if (i > BATCH_SIZE) break;
        }

        if (_processed > 0 && _processed % 10 == 0)
        {
            yield return Resources.UnloadUnusedAssets();
        }
    }
}
