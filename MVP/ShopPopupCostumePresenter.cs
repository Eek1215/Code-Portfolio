using System;
using System.Collections.Generic;

public struct CostumeGachaResult
{
    public CostumeGachaResult(CostumesData data, eCostumeType type, string itemKey, uint addCnt)
    {
        if (type == eCostumeType.None)
        {
            CostumeType = eCostumeType.None;
            ItemKey = "";
            CntWhenGacha = 0;
            AddCnt = 0;
        }
        else
        {
            CostumeType = type;
            ItemKey = itemKey;
            CntWhenGacha = data.GetCostume(CostumeType, ItemKey).CurrentPieceCnt;
            AddCnt = addCnt;
        }
    }

    public CostumeData GetData(CostumesData data)
    {
        return data.GetCostume(CostumeType, ItemKey);
    }

    public eCostumeType CostumeType;
    public string ItemKey;
    public uint CntWhenGacha;
    public uint AddCnt;
}

public class ShopPopupCostumePresenter
{
    private readonly uint GEM_COST = 300;

    private List<CostumeGachaResult> _result;
    private IShopPopupCostumeContent _view;

    // ������ �ݱ��ư ��Ƽ�� ����
    public event Action<bool> OnSetActiveBtnClose;
    // ������ �� ���� ��ư�� ���� Toggle
    public event Action<bool> SetClickAbleToggle;
    // �ڽ�Ƭ �̱� �� ��ư�� �˸� ����.
    public event Action CheckUniformTabNotice;

    private GameMgr _gameMgr;
    private DataMgr _dataMgr;
    private UIMgr _uiMgr;
    private NoticeMgr _noticeMgr;
    private ResolutionMgr _resolutionMgr;
    private PopupMgr _popupMgr;

    public ShopPopupCostumePresenter(IShopPopupCostumeContent view, GameMgr gameMgr, DataMgr dataMgr
        , UIMgr uiMgr, NoticeMgr noticeMgr, ResolutionMgr resolutionMgr, PopupMgr popupMgr)
    {
        _result = new List<CostumeGachaResult>();
        _gameMgr = gameMgr;
        _dataMgr = dataMgr;
        _uiMgr = uiMgr;
        _noticeMgr = noticeMgr;
        _resolutionMgr = resolutionMgr;
        _popupMgr = popupMgr;

        _view = view;
        Clean();
        
        _view.BtnInitialize();
        _view.CheckBtnShowAdsNotice(_dataMgr.Additional.DailyAdsCostumeRemainingGachaCnt);
        
        _view.OnClickAds += OnClickAds;
        _view.OnClickGachaWithGem += OnClickGachaWithGem;
        _view.OnClickGacha1Ticket += OnClickGacha1Ticket;
        _view.OnClickGacha3Ticket += OnClickGacha3Ticket;
        _view.OnClickConfirm += OnClickConfirm;
    }

    public void OnDispose()
    {
        _view.OnClickAds -= OnClickAds;
        _view.OnClickGachaWithGem -= OnClickGachaWithGem;
        _view.OnClickGacha1Ticket -= OnClickGacha1Ticket;
        _view.OnClickGacha3Ticket -= OnClickGacha3Ticket;
        _view.OnClickConfirm -= OnClickConfirm;

        OnSetActiveBtnClose = null;
        SetClickAbleToggle = null;
        CheckUniformTabNotice = null;

        _result = null;
        _gameMgr = null;
        _dataMgr = null;
        _uiMgr = null;
        _noticeMgr = null;
        _resolutionMgr = null;
    }

    void Clean()
    {
        _view.Clean();
        UpdateVariableUI();

        OnSetActiveBtnClose?.Invoke(true);
    }

    void OnClickAds()
    {
        if (!GetCheckGachaAble())
        {
            return;
        }

        if (_dataMgr.Additional.DailyAdsCostumeRemainingGachaCnt > 0)
        {
            System.Action reward = () =>
            {
                _result.Clear();

                _dataMgr.Additional.OnWatchedAdsCostumeGacha();

                eCostumeType type = GetRandomCostumeType();
                CostumeGachaResult result = new CostumeGachaResult(_dataMgr.InGame.Costumes,
                                    type, GetRandomCostumeItem(type), GetRandomCostumeCnt());
                _result.Add(result);

                _dataMgr.InGame.Costumes.AddCostume(result.CostumeType, result.ItemKey, result.AddCnt);
                _dataMgr.BothSave();

                OnChangeUIWhenGacha(true);
                _view.CheckBtnShowAdsNotice(_dataMgr.Additional.DailyAdsCostumeRemainingGachaCnt);
            };

            // �������� ����.
            if (_dataMgr.Pay.PurchaseStatus_DeleteAds)
            {
                reward();
            }
            else
            {
                _view.ShowAdsConfirmPopup(_uiMgr, reward);
            }
        }
    }

    void OnClickGachaWithGem()
    {
        if (!GetCheckGachaAble())
        {
            return;
        }

        if (_dataMgr.Wealth.IsEnoughGem(GEM_COST))
        {
            _dataMgr.Wealth.SubtractGem(GEM_COST, null);

            _result.Clear();

            for (int i = 0; i < 3; i++)
            {
                eCostumeType type = GetRandomCostumeType();
                CostumeGachaResult result = new CostumeGachaResult(_dataMgr.InGame.Costumes, type
                    , GetRandomCostumeItem(type), GetRandomCostumeCnt());
                // 3�� �̱⸦ �ϸ� �� ��Ȳ�� ����ĥ �� �ִµ� 1���� �� ��� �ڽ�Ƭ�� �ƽ����� �Ѱ������� �̰� �ΰ��� ���� ���.
                // �װ͵鸸 �������� ġȯ����. �׷��� ���� 100���� ��ü�Ͽ� ����
                if (result.CostumeType == eCostumeType.None)
                {
                    Util.AddItemShowEffect(eAsset.Gem, 100, _resolutionMgr.ScreenMiddle, 1);
                }
                else
                {
                    _result.Add(result);
                    _dataMgr.InGame.Costumes.AddCostume(result.CostumeType, result.ItemKey, result.AddCnt);
                }
            }

            _dataMgr.BothSave();

            OnChangeUIWhenGacha(false);

            FBAnalytics.Send("Shop_Purchase_Costume_Gem");
        }
    }

    void OnClickGacha1Ticket()
    {
        if (!GetCheckGachaAble())
        {
            return;
        }

        if (_dataMgr.Wealth.IsHaveItem(eAsset.CostumeGacha1Ticket))
        {
            _dataMgr.Wealth.ConsumeItem(eAsset.CostumeGacha1Ticket, 1);
            _result.Clear();

            eCostumeType type = GetRandomCostumeType();
            CostumeGachaResult result = new CostumeGachaResult(_dataMgr.InGame.Costumes,
                                    type, GetRandomCostumeItem(type), GetRandomCostumeCnt());
            _result.Add(result);

            _dataMgr.InGame.Costumes.AddCostume(result.CostumeType, result.ItemKey, result.AddCnt);
            _dataMgr.BothSave();

            OnChangeUIWhenGacha(true);
        }
    }

    void OnClickGacha3Ticket()
    {
        if (!GetCheckGachaAble())
        {
            return;
        }

        if (_dataMgr.Wealth.IsHaveItem(eAsset.CostumeGacha3Ticket))
        {
            _dataMgr.Wealth.ConsumeItem(eAsset.CostumeGacha3Ticket, 1);
            _result.Clear();

            for (int i = 0; i < 3; i++)
            {
                eCostumeType type = GetRandomCostumeType();
                CostumeGachaResult result = new CostumeGachaResult(_dataMgr.InGame.Costumes, 
                                        type, GetRandomCostumeItem(type), GetRandomCostumeCnt());
                if (result.CostumeType == eCostumeType.None)
                {
                    Util.AddItemShowEffect(eAsset.Gem, 100, _resolutionMgr.ScreenMiddle, 1);
                }
                else
                {
                    _result.Add(result);
                    _dataMgr.InGame.Costumes.AddCostume(result.CostumeType, result.ItemKey, result.AddCnt);
                }
            }

            _dataMgr.BothSave();

            OnChangeUIWhenGacha(true);

            FBAnalytics.Send("Shop_Purchase_Costume_Gem");
        }
    }

    void OnChangeUIWhenGacha(bool withNotice)
    {
        OnHideUIWhenGacha();

        _popupMgr.SetPossibleClickBackBtn(false);
        _view.PlayChestShakeCor(_result);

        // UI ������ �ٲ�� ���. �����̱�� �������� ������ ������ X
        if (withNotice)
        {
            UpdateVariableUI();
            CheckUniformTabNotice();
            _noticeMgr.Shop.CheckNotice();
        }
    }

    void OnClickConfirm()
    {
        Clean();

        SetClickAbleToggle?.Invoke(true);
        PopupMgr.Instance.SetPossibleClickBackBtn(true);
    }

    bool GetCheckGachaAble()
    {
        if (!_gameMgr.GetNetworkConnect())
        {
            _uiMgr.Popups.ShowConfirmPopup(StrUtil.ToI2("PleaseCheckNetwork"));
            return false;
        }

        if (GetRandomCostumeType() == eCostumeType.None)
        {
            _uiMgr.Messages.ShowDefaultMessage(StrUtil.ToI2("AllUniformMaxDes"));
            return false;
        }
        return true;
    }

    void OnHideUIWhenGacha()
    {
        _view.OnHideGachaInfoUI();
        SetClickAbleToggle?.Invoke(false);
        OnSetActiveBtnClose.Invoke(false);
    }

    void UpdateVariableUI()
    {
        _view.UpdateVariableUI(_dataMgr.Wealth.CostumeGacha3Ticket, _dataMgr.Wealth.CostumeGacha1Ticket
            , _dataMgr.Additional.DailyAdsCostumeRemainingGachaCnt, _dataMgr.Additional.MAX_DAILY_COSTUME_GACHA
            , _dataMgr.Wealth.IsEnoughGem(GEM_COST));
    }

    eCostumeType GetRandomCostumeType()
    {
        List<eCostumeType> type = new List<eCostumeType>();
        if (GetRandomCostumeItem(eCostumeType.eHat) != string.Empty)
        {
            type.Add(eCostumeType.eHat);
        }

        if (GetRandomCostumeItem(eCostumeType.eScarf) != string.Empty)
        {
            type.Add(eCostumeType.eScarf);
        }

        if (GetRandomCostumeItem(eCostumeType.eApron) != string.Empty)
        {
            type.Add(eCostumeType.eApron);
        }

        if (type.Count == 0)
        {
            return eCostumeType.None;
        }
        return type[UnityEngine.Random.Range(0, type.Count)];
    }

    string GetRandomCostumeItem(eCostumeType type)
    {
        CostumesData costumesData = _dataMgr.InGame.Costumes;
        if (type == eCostumeType.eHat)
        {
            List<eHat> hatList = new List<eHat>();
            for (int i = 0; i < (int)eHat.Max; i++)
            {
                if (!costumesData.GetCostume(type, ((eHat)i).ToString()).IsMaxCnt
                    && !costumesData.GetIsNoneGachaCostume(eCostumeType.eHat, i))
                {
                    hatList.Add((eHat)i);
                }
            }

            if (hatList.Count > 0)
            {
                return hatList[UnityEngine.Random.Range(0, hatList.Count)].ToString();
            }
        }
        else if (type == eCostumeType.eScarf)
        {
            List<eScarf> scarfList = new List<eScarf>();
            for (int i = 0; i < (int)eScarf.Max; i++)
            {
                if (!costumesData.GetCostume(type, ((eScarf)i).ToString()).IsMaxCnt
                    && !costumesData.GetIsNoneGachaCostume(eCostumeType.eScarf, i))
                {
                    scarfList.Add((eScarf)i);
                }
            }

            if (scarfList.Count > 0)
            {
                return scarfList[UnityEngine.Random.Range(0, scarfList.Count)].ToString();
            }
        }
        else if (type == eCostumeType.eApron)
        {
            List<eApron> apronList = new List<eApron>();
            for (int i = 0; i < (int)eApron.Max; i++)
            {
                if (!costumesData.GetCostume(type, ((eApron)i).ToString()).IsMaxCnt
                    && !costumesData.GetIsNoneGachaCostume(eCostumeType.eApron, i))
                {
                    apronList.Add((eApron)i);
                }
            }

            if (apronList.Count > 0)
            {
                return apronList[UnityEngine.Random.Range(0, apronList.Count)].ToString();
            }
        }
        return string.Empty;
    }

    uint GetRandomCostumeCnt()
    {
        string[] list = { 9.ToString(), 10.ToString(), 11.ToString(), 12.ToString(), 13.ToString() };
        double[] probability = { 20f, 40f, 20f, 15f, 5f };
        return uint.Parse(Util.GetProbabilityResult(list, probability));
    }
}
