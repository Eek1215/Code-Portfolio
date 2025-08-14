using System;
using UnityEngine;

public class ShopPopupPresenter
{
    public event Action OnClosed;

    private IShopPopup _view;

    public ShopPopupPackagePresenter PackagePresenter;
    public ShopPopupGemPresenter GemPresenter;
    public ShopPopupCostumePresenter CostumePresenter;

    private readonly IShopPopupPackageContent _packageContent;
    private readonly IShopPopupGemContent _gemContent;
    private readonly IShopPopupCostumeContent _costumeContent;

    private GameMgr _gameMgr;
    private DataMgr _dataMgr;
    private NoticeMgr _noticeMgr;
    private UIMgr _uiMgr;
    private PopupMgr _popupMgr;
    private IAPMgr _iapMgr;
    private ResolutionMgr _resolutionMgr;
    private SoundMgr _soundMgr;
    private TimeMgr _timeMgr;
    private AdsMgr _adsMgr;

    private eShopPopup _lastOpendTab;
    private bool _bClickAbleTab;

    public ShopPopupPresenter(IShopPopup view, GameMgr gameMgr, DataMgr dataMgr, NoticeMgr noticeMgr, UIMgr uiMgr, PopupMgr popupMgr
        , IAPMgr iapMgr, ResolutionMgr resolutionMgr, SoundMgr soundMgr, TimeMgr timeMgr, AdsMgr adsMgr
        , IShopPopupCostumeContent costumeContent, IShopPopupGemContent gemContent, IShopPopupPackageContent packageContent)
    {
        _gameMgr = gameMgr;
        _dataMgr = dataMgr;
        _noticeMgr = noticeMgr;
        _uiMgr = uiMgr;
        _popupMgr = popupMgr;
        _iapMgr = iapMgr;
        _resolutionMgr = resolutionMgr;
        _soundMgr = soundMgr;
        _timeMgr = timeMgr;
        _adsMgr = adsMgr;

        _bClickAbleTab = true;

        _packageContent = packageContent;
        _gemContent = gemContent;
        _costumeContent = costumeContent;

        _uiMgr.Main.HideMainBtn();

        _view = view;
        // 코스튬 뽑기에서 첫구매혜택을 누르면 닫기가 가능은해서. 그럴경우 다시 킬 때 close가 꺼진상태라 켜줌
        _view.SetBtnCloseActive(true);
        _view.BtnInitialize();
        
        _view.OnClickWealthTab += OnClickWealthTab;
        _view.OnClickPackageTab += OnClickPackageTab;
        _view.OnClickGemTab += OnClickGemTab;
        _view.OnClickCostumeTab += OnClickCostumeTab;

        _view.OnActiveOff += OnDispose;
        _view.OnClickShowConfirmPopup += OnClickPurchaseBonusItem;
        _view.OnClickRestore += OnClickRestore;
        _view.OnClickAdsSkipTicketExchange += OnClickExchange;

        _view.CheckItemTabNotice(_noticeMgr.Shop._bItemOnNotice);
        _view.CheckUniformTabNotice(_noticeMgr.Shop._bUniformOnNotice);
        _view.CheckPurchaseBonus(!dataMgr.InGame.MC.state_skin1);

        _noticeMgr.Shop.OnUpdateNotice += UpdateNotice;

        SetClickAbleToggle(true);
        _view.SetBtnCloseActive(true);
    }

    public void OnDispose()
    {
        _view.OnClickWealthTab -= OnClickWealthTab;
        _view.OnClickPackageTab -= OnClickPackageTab;
        _view.OnClickGemTab -= OnClickGemTab;
        _view.OnClickCostumeTab -= OnClickCostumeTab;

        _view.OnActiveOff -= OnDispose;
        _view.OnClickShowConfirmPopup -= OnClickPurchaseBonusItem;
        _view.OnClickRestore -= OnClickRestore;
        _view.OnClickAdsSkipTicketExchange -= OnClickExchange;

        _noticeMgr.Shop.OnUpdateNotice -= UpdateNotice;

        _uiMgr.Main.ShowMainBtn();

        DisposeLastOpendContent();
        _lastOpendTab = eShopPopup.None;

        _gameMgr = null;
        _dataMgr = null;
        _noticeMgr = null;
        _uiMgr = null;
        _popupMgr = null;
        _iapMgr = null;
        _resolutionMgr = null;
        _soundMgr = null;
        _timeMgr = null;
        _adsMgr = null;

        OnClosed?.Invoke();
    }

    public void OnClickWealthTab()
    {
        if (_bClickAbleTab)
        {
            ShowContent(eShopPopup.Wealth);
            _view.PlayClickBtnSound();
        }
    }

    public void OnClickGemTab()
    {
        if (_bClickAbleTab)
        {
            ShowContent(eShopPopup.Gem);
            _view.PlayClickBtnSound();
        }
    }

    public void OnClickPackageTab()
    {
        if (_bClickAbleTab)
        {
            ShowContent(eShopPopup.Package);
            _view.PlayClickBtnSound();
        }
    }

    public void OnClickCostumeTab()
    {
        if (_bClickAbleTab)
        {
            ShowContent(eShopPopup.Costume);
            _view.PlayClickBtnSound();
        }
    }

    void DisposeLastOpendContent()
    {
        switch (_lastOpendTab)
        {
            case eShopPopup.Package:
                if (PackagePresenter != null)
                {
                    PackagePresenter.OnDispose();
                    PackagePresenter = null;
                }
                break;
            case eShopPopup.Gem:
                if (GemPresenter != null)
                {
                    GemPresenter.OnDispose();
                    GemPresenter = null;
                }
                break;
            case eShopPopup.Costume:
                if (CostumePresenter != null)
                {
                    CostumePresenter.OnDispose();
                    CostumePresenter = null;
                }
                break;
        }
    }

    public void ShowContent(eShopPopup content)
    {
        DisposeLastOpendContent();

        switch (content)
        {
            case eShopPopup.Package:
                PackagePresenter = new ShopPopupPackagePresenter(_packageContent, _dataMgr, _uiMgr, _iapMgr, _soundMgr);
                break;
            case eShopPopup.Gem:
                GemPresenter = new ShopPopupGemPresenter(_gemContent, _dataMgr, _uiMgr, _timeMgr, _adsMgr);
                break;
            case eShopPopup.Costume:
                CostumePresenter = new ShopPopupCostumePresenter(_costumeContent, _gameMgr, _dataMgr, _uiMgr, _noticeMgr, _resolutionMgr, _popupMgr);
                CostumePresenter.OnSetActiveBtnClose += (on) => _view.SetBtnCloseActive(on);
                CostumePresenter.SetClickAbleToggle += (able) => SetClickAbleToggle(able);
                CostumePresenter.CheckUniformTabNotice += () => _view.CheckUniformTabNotice(_noticeMgr.Shop._bUniformOnNotice);
                break;
        }

        _view.OpenContent(content);
        _view.CheckItemTabNotice(_noticeMgr.Shop._bItemOnNotice);

        _lastOpendTab = content;
    }

    public void OnClickPurchaseBonusItem(Sprite spr)
    {
        _uiMgr.Popups.ShowConfirmPopup(spr,
            StrUtil.ToI2("FirstPurchaseBonusDes").Replace("@", "<color=#FB3533>").Replace("$", "</color>")
            + "\n" +
            StrUtil.GetColorStr("#B41FC3", "(" + StrUtil.ToI2("PassiveEffects") + " : " + StrUtil.ToI2("IncInfoDeskTime").Replace("@", "20") + ")"));
    }

    public void OnClickRestore()
    {
        _iapMgr.OnClickRestore();
    }

    public void OnClickExchange()
    {
        if (_dataMgr.Wealth.IsHaveItem(eAsset.AdsSkipTicket))
        {
            _uiMgr.Popups.ShowYesNoPopup(StrUtil.ToI2("QuestionTicketExchange@#")
                .Replace("@", StrUtil.GetIconTxt((int)eIconTxt.AdsSkipTicket) + " " + _dataMgr.Wealth.AdsSkipTicket.ToString())
                .Replace("#", StrUtil.GetIconTxt((int)eIconTxt.Gem) + " " + (_dataMgr.Wealth.AdsSkipTicket * _dataMgr.Pay.TicketToGemValue).ToString())
                .Replace("(", "<color=#D43000>")
                .Replace(")", "</color>"), () =>
                {
                    _popupMgr.PopupClean();
                    _dataMgr.Wealth.AddGem(_dataMgr.Wealth.AdsSkipTicket * _dataMgr.Pay.TicketToGemValue);
                    _uiMgr.Popups.ShowExchangeTicketToGem(_dataMgr.Wealth.AdsSkipTicket, _dataMgr.Wealth.AdsSkipTicket * _dataMgr.Pay.TicketToGemValue);
                    _dataMgr.Wealth.ConsumeItem(eAsset.AdsSkipTicket, _dataMgr.Wealth.AdsSkipTicket);
                });
        }
        else
        {
            _uiMgr.Messages.ShowDefaultMessage(StrUtil.ToI2("NoneAdsSkipTicket"));
        }
    }

    public void SetClickAbleToggle(bool able)
    {
        _bClickAbleTab = able;
    }

    void UpdateNotice(bool on)
    {
        _view.CheckItemTabNotice(on);
    }
}