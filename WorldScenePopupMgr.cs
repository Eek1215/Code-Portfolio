using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

public interface IPopupMgr
{
    bool IsPossibleClickBackBtn { get; }
    void AddPopup(Popup popup);
    void RemovePopup(Popup target = null);
    bool IsContains(Popup obj);
    void PopupClean();
    bool IsStackClean { get; }
    void SetPossibleClickBackBtn(bool isPossible);

    UniTask<T> LoadPopupAsync<T>(string key) where T : Popup;
}

public class PopupMgr : MonoBehaviour, IPopupMgr
{
    private List<Popup> _popups = new();
    [Inject] private AddressableMgr _addressableMgr;
    [Inject] private IObjectResolver _resolver;

    protected Dictionary<string, Popup> _cachedPopups = new();

    public bool IsPossibleClickBackBtn { get; private set; } = true;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPossibleClickBackBtn)
            {
                if (_popups.Count > 0)
                {
                    RemovePopup();
                }
            }
        }
    }

    public void AddPopup(Popup popup)
    {
        if (IsContains(popup))
            return;

        _popups.Add(popup);
        _resolver.Inject(popup);
        popup.gameObject.SetActive(true);
    }

    public void RemovePopup(Popup target = null)
    {
        // 지우려는 특정 오브젝트가 있을 경우. 지우되 아니라면 후입부터 순차적으로 제거.
        if (target == null)
        {
            Popup obj;
            if (_popups.Count > 0)
            {
                obj = _popups[_popups.Count - 1];
                _popups.RemoveAt(_popups.Count - 1);
                obj.gameObject.SetActive(false);
            }
        }
        else
        {
            if (IsContains(target))
            {
                _popups.Remove(target);
                target.gameObject.SetActive(false);
            }
        }
    }

    /* 해당 오브젝트가 들어있는 지 확인하는 것. 이것으로 중복으로 넣지 않는다던가
     * 어떤 팝업이 떠있는 동안은 뜨면 안된다거나 등의 활용이 가능 */
    public bool IsContains(Popup obj)
    {
        return _popups.Contains(obj);
    }

    public bool IsContains(string key)
    {
        return _cachedPopups.ContainsKey(key);
    }

    /* 말 그대로 비워주는 함수지만 잘 사용하지는 않음 */
    public void PopupClean()
    {
        while (_popups.Count > 0)
        {
            RemovePopup();
        }
    }

    /* 스택이 비어있는지 확인하는데 true일 때 가장 메인화면에서
     * Esc를 눌렀을 때 종료하기 팝업을 띄워준다 */
    public bool IsStackClean
    {
        get { return _popups.Count == 0; }
    }

    public void SetPossibleClickBackBtn(bool isPossible)
    {
        IsPossibleClickBackBtn = isPossible;
    }

    public async UniTask<T> LoadPopupAsync<T>(string key) where T : Popup
    {
        if (_cachedPopups.TryGetValue(key, out var cached))
        {
            return cached as T;
        }

        AsyncOperationHandle<GameObject> handle = await _addressableMgr.InstantiateAsync(key, transform);
        _cachedPopups[key] = handle.Result.GetComponent<Popup>();

        return _cachedPopups[key] as T;
    }

    protected void InjectComponents(Type type, Popup popup)
    {
        var components = popup.GetComponentsInChildren(type);
        foreach (var component in components)
        {
            _resolver.Inject(component);
        }
    }

    protected async UniTask<T> CreatePopup<T>(string key, Action<T> createFunc, params Type[] injectTypes) where T : Popup
    {
        if (IsContains(key))
            return null;

        T popup = await LoadPopupAsync<T>(key);

        if (injectTypes != null)
        {
            foreach (var type in injectTypes)
            {
                InjectComponents(type, popup);
            }
        }

        AddPopup(popup);
        createFunc?.Invoke(popup);
        return popup;
    }
}


public class WorldScenePopupMgr : PopupMgr
{
    public async UniTask<PhotonLobbyPopup> ShowPhotonLobbyPopup()
    {
        var popup = await CreatePopup<PhotonLobbyPopup>(AddressableKeys.PhotonLobbyPopup, null, 
            typeof(StudentSlotParent), typeof(AllStudentSlotParent));
            popup.Initialize();
        return popup;
    }

    public async UniTask<PhotonRoomPopup> ShowPhotonRoomPopup(StudentUnitData[] students)
    {
        return await CreatePopup<PhotonRoomPopup>(
            AddressableKeys.PhotonRoomPopup,
            async popup =>{ await popup.OnEnterPlayer(students); },
            typeof(StudentSlotParent));
    }

    public async UniTask<PVEBattleEntryPopup> ShowPVEBattleEntryPopup(MonsterUnitData[] monsters)
    {
        return await CreatePopup<PVEBattleEntryPopup>(
            AddressableKeys.PVEBattleEntryPopup,
            popup => popup.InitializeData(monsters),
            typeof(StudentSlotParent), typeof(MonsterSlotParent));
    }
}
