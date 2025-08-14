using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class ShopPopupPackagePresenter
{
    private IShopPopupPackageContent _view;
    private DataMgr _dataMgr;
    private UIMgr _uiMgr;
    private PayData _payData;
    private IAPMgr _iapMgr;

    public ShopPopupPackagePresenter(IShopPopupPackageContent view, DataMgr dataMgr, UIMgr uiMgr
        , IAPMgr iapMgr, SoundMgr soundMgr)
    {
        _view = view;
        _dataMgr = dataMgr;
        _uiMgr = uiMgr;
        _payData = _dataMgr.Pay;
        _iapMgr = iapMgr;

        _view.OnClickPurchaseBeginning += OnClickPurchaseBeginning;
        _view.OnClickPurchaseDeleteAds += OnClickPurchaseDeleteAds;
        _view.OnClickPurchaseInfinityBuff += OnClickPurchaseInfinityBuff;
        _view.OnClickPurchaseFSFeeBooster += OnClickPurchaseFSFeeBooster;
        _view.OnClickPurchaseTipObjBooster += OnClickPurchaseTipObjBooster;
        _view.OnClickPurchaseCleanerPackage += OnClickPurchaseCleanerPackage;
        _view.OnClickPurchaseAutoPrepIngredients += OnClickPurchaseAutoPrepIngredients;
        _view.OnClickPurchaseAutoKillFlyingCash += OnClickPurchaseAutoKillFlyingCash;
        _view.OnClickPurchaseAutoScreenCapture += OnClickPurchaseAutoScreenCapture;
        _view.OnClickPurchaseUniformPack1 += OnClickPurchaseUniformPack1;

        UpdateVariableUI();
        _view.BtnInitialize(soundMgr);
        _view.SetAdsSkipTicketPrice(StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.adsskipticketpackage)));
        _view.ResetContentPos();
    }

    public void OnDispose()
    {
        _view.OnClickPurchaseBeginning -= OnClickPurchaseBeginning;
        _view.OnClickPurchaseDeleteAds -= OnClickPurchaseDeleteAds;
        _view.OnClickPurchaseInfinityBuff -= OnClickPurchaseInfinityBuff;
        _view.OnClickPurchaseFSFeeBooster -= OnClickPurchaseFSFeeBooster;
        _view.OnClickPurchaseTipObjBooster -= OnClickPurchaseTipObjBooster;
        _view.OnClickPurchaseCleanerPackage -= OnClickPurchaseCleanerPackage;
        _view.OnClickPurchaseAutoPrepIngredients -= OnClickPurchaseAutoPrepIngredients;
        _view.OnClickPurchaseAutoKillFlyingCash -= OnClickPurchaseAutoKillFlyingCash;
        _view.OnClickPurchaseAutoScreenCapture -= OnClickPurchaseAutoScreenCapture;
        _view.OnClickPurchaseUniformPack1 -= OnClickPurchaseUniformPack1;

        _view = null;
        _dataMgr = null;
        _uiMgr = null;
        _payData = null;
        _iapMgr = null;
    }

    public void MoveToPackage(eProductID product)
    {
        _view.MoveToPackage(product);
    }

    public void UpdateVariableUI()
    {
        // 다른곳에서 배너로 구입을 하면 여기를 못들어왔기 때문.
        if (_payData == null)
        {
            return;
        }

        _view.SetDeleteAdsProductState(_payData.PurchaseStatus_DeleteAds);
        _view.SetInfinityBuffProductState(_payData.PurchaseStatus_InfinityBuff);
        _view.SetFSFeeBoosterProductState(_payData.PurchaseStatus_FSFeeBooster);
        _view.SetTipObjBoosterProductState(_payData.PurchaseStatus_TipObjBooster);
        _view.SetCleanerPackageProductState(_payData.PurchaseStatus_CleanerPackage);
        _view.SetAutoPrepIngredientsProductState(_payData.PurchaseStatus_AutoPrepIngredients
                                        , _dataMgr.InGame.Facility.PrepIngredientsGame.IsBuilt);
        _view.SetAutoKillFlyingCashProductState(_payData.PurchaseStatus_AutoKillFlyingCash);
        _view.SetAutoScreenCaptureProductState(_payData.PurchaseStatus_AutoScreenCapture);
        _view.SetUniformPackageState(_payData.IsUniformPack1PurchaseStatus, _payData.UniformPackage1PurchasedCnt
            , _payData.MAX_REMAININGUNIFORMPACK1PURCHASE_CNT, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.uniformpackage1)));
    }

    void OnClickPurchaseBeginning()
    {
        if (!_dataMgr.Pay.PurchaseStatus_Beginning)
        {
            _iapMgr.IsUniformPackage1OpendFromPopup = false;
            _uiMgr.Popups.ShowPurchaseProductConfirmPopup(eProductID.beginningpackage);
        }
        else
        {
            _uiMgr.Popups.ShowProductBannerPopup(eProductID.beginningpackage, string.Empty
                , string.Empty);
        }
    }

    void OnClickPurchaseDeleteAds()
    {
        _uiMgr.Popups.ShowProductBannerPopup(eProductID.deleteads, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.deleteads))
            , StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.deleteads), 3m));
    }

    void OnClickPurchaseInfinityBuff()
    {
        _uiMgr.Popups.ShowProductBannerPopup(eProductID.infinitybuffpackage, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.infinitybuffpackage))
            , StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.infinitybuffpackage), 3m));
    }

    void OnClickPurchaseFSFeeBooster()
    {
        _uiMgr.Popups.ShowProductBannerPopup(eProductID.fsfeeboosterpackage, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.fsfeeboosterpackage))
            , StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.fsfeeboosterpackage), 2m));
    }

    void OnClickPurchaseTipObjBooster()
    {
        _uiMgr.Popups.ShowProductBannerPopup(eProductID.tipobjboosterpackage, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.tipobjboosterpackage))
            , StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.tipobjboosterpackage), 2.2m));
    }

    void OnClickPurchaseCleanerPackage()
    {
        _uiMgr.Popups.ShowProductBannerPopup(eProductID.cleanerpackage, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.cleanerpackage))
            , StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.cleanerpackage), 3m));
    }

    void OnClickPurchaseAutoPrepIngredients()
    {
        _uiMgr.Popups.ShowProductBannerPopup(eProductID.autoprepingredients, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.autoprepingredients))
            , StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.autoprepingredients), 3m));
    }

    void OnClickPurchaseAutoKillFlyingCash()
    {
        _uiMgr.Popups.ShowProductBannerPopup(eProductID.autokillflyingcash, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.autokillflyingcash))
            , StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.autokillflyingcash), 2.23m));
    }

    void OnClickPurchaseAutoScreenCapture()
    {
        _uiMgr.Popups.ShowProductBannerPopup(eProductID.autoscreencapture, StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.autoscreencapture))
            , StrUtil.GetLocalizePrice(_iapMgr.GetProduct(eProductID.autoscreencapture), 2.45m));
    }

    void OnClickPurchaseUniformPack1()
    {
        if (_payData.IsUniformPack1PurchaseStatus)
        {
            _uiMgr.Popups.ShowPurchaseProductConfirmPopup(eProductID.uniformpackage1, 4m);
        }
    }
}
