using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Cysharp.Threading.Tasks;

public static class AddressableKeyRef
{
    private static AddressableKeyTable _table;
    private static AddressableKeyTable Table
    {
        get
        {
            if(_table == null)
            {
                _table = Resources.Load<AddressableKeyTable>("AddressableKeyTable");
            }
            return _table;
        }
    }

    public static string ImageA => Table.ImageA;
    public static string PrefabA => Table.PrefabA;
    public static string AudioClipA => Table.AudioClipA;
    public static string MaterialA => Table.MaterialA;

}

public class AddressableMgr : Singleton<AddressableMgr>
{
    #region 데이터 초기화 관련 필드
    public Text txt_patchSize;
    public Slider slider_patch;
    public Text txt_patchPercent;

    // Group의 Label들
    public AssetLabelReference[] Labels;

    [SerializeField] private long _patchTotalSize;
    // Label별 용량의 크기.
    private Dictionary<string, long> _patchSizePerLabels = new();

    private bool _bInit = false;
    #endregion

    // Sprite, Shader, Material 처럼 인스터스가 아닌 리소스들
    private Dictionary<string, (AsyncOperationHandle handle, int refCnt)> _sharedAssets = new();
    // 인스터스화가 필요한 데이터들.
    private Dictionary<string, List<AsyncOperationHandle<GameObject>>> _instances = new();
    
    public override void Init() { }

    #region 데이터 확인 및 불러오기, 제거
    public void InitCheck()
    {
        StartCoroutine(InitAddressable());
        StartCoroutine(CheckUpdateFiles());
    }

    public void OnClickDeleteData()
    {
        List<string> labels = new();
        for (int i = 0; i < Labels.Length; i++)
        {
            labels.Add(Labels[i].labelString);
        }

        _patchTotalSize = default;

        foreach (var label in labels)
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

        List<string> labels = new();
        for (int i = 0; i < Labels.Length; i++)
        {
            labels.Add(Labels[i].labelString);
        }

        _patchTotalSize = default;

        foreach(var label in labels)
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
            txt_patchPercent.text = "100 %";
        }

        StartCoroutine(PatchFiles());
    }

    IEnumerator PatchFiles()
    {
        List<string> labels = new();
        for (int i = 0; i < Labels.Length; i++)
        {
            labels.Add(Labels[i].labelString);
        }

        foreach (var label in labels)
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

            slider_patch.value = total / _patchTotalSize;
            txt_patchPercent.text = (int)(slider_patch.value * 100) + " %";

            if(total == _patchTotalSize)
            {
                SceneMgr.Instance.LoadScene(eScene.Play);
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
    public async UniTask<T> LoadAsset<T>(string key)
    {
        if(_sharedAssets.TryGetValue(key, out var loadAsset))
        {
            _sharedAssets[key] = (loadAsset.handle, loadAsset.refCnt + 1);
            return (T)loadAsset.handle.Result;
        }

        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.Task;

        if(handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load Addressable asset : {key}");
            Addressables.Release(handle);
            return default;
        }

        _sharedAssets[key] = (handle, 1);
        return handle.Result;
    }

    public void ReleaseShared<T>(string key)
    {
        if(_sharedAssets.TryGetValue(key, out var loadedAsset))
        {
            int releasedCnt = loadedAsset.refCnt - 1;
            if(releasedCnt <= 0)
            {
                Addressables.Release(loadedAsset.handle);
                _sharedAssets.Remove(key);
            }
            else
            {
                _sharedAssets[key] = (loadedAsset.handle, releasedCnt);
            }
        }
    }

    public async UniTask<GameObject> Instantiate(string key)
    {
        var handle = Addressables.InstantiateAsync(key);
        await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load Addressable asset : {key}");
            Addressables.Release(handle);
            return default;
        }

        if (!_instances.TryGetValue(key, out var list))
        {
            list = new();
            _instances[key] = list;
        }

        list.Add(handle);
        return handle.Result;
    }

    public void ReleaseInstance(string key, AsyncOperationHandle<GameObject> instance)
    {
        if(_instances.TryGetValue(key, out var list))
        {
            if(list.Remove(instance))
            {
                Addressables.ReleaseInstance(instance);
            }

            if(list.Count == 0)
            {
                _instances.Remove(key);
            }
        }
    }
    #endregion
}
