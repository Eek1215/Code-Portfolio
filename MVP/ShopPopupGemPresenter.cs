using UnityEngine;

public class ShopPopupGemPresenter
{
    private IShopPopupGemContent _view;

    private DataMgr _dataMgr;
    private UIMgr _uiMgr;
    private TimeMgr _timeMgr;
    private AdsMgr _adsMgr;

    public ShopPopupGemPresenter(IShopPopupGemContent view, DataMgr dataMgr, UIMgr uiMgr
            , TimeMgr timeMgr, AdsMgr adsMgr)
    {
        _view = view;
        _dataMgr = dataMgr;
        _uiMgr = uiMgr;
        _timeMgr = timeMgr;
        _adsMgr = adsMgr;

        _view.Initialize(_dataMgr.Pay.PurchaseStatus_DeleteAds);
        if (!CheckAdsCnt())
        {
            _timeMgr.UpdateRemainingResetTimePerSec += UpdateRemainingResetTime;
        }

        _view.OnClickGemWithAds += OnClickAdsGem;
    }

    public void OnDispose()
    {
        _view.OnClickGemWithAds -= OnClickAdsGem;
        _timeMgr.UpdateRemainingResetTimePerSec -= UpdateRemainingResetTime;

        _view = null;
        _dataMgr = null;
        _uiMgr = null;
        _timeMgr = null;
        _adsMgr = null;
    }

    bool CheckAdsCnt()
    {
        return _view.CheckAdsCnt(_dataMgr.Additional.DailyAdsGemRemainingWatchedCnt
            , _dataMgr.Additional.MAX_DAILY_ADS_GEM_WATCHED);
    }

    void UpdateRemainingResetTime(string remainingTime)
    {
        _view.UpdateRemainingResetTime(remainingTime);
    }

    void OnClickAdsGem()
    {
        if (_dataMgr.Additional.DailyAdsGemRemainingWatchedCnt > 0)
        {
            System.Action reward = () =>
            {
                _dataMgr.Additional.OnWatchedAdsGem();
                _view.ShowGetGemEffect();
                CheckAdsCnt();
            };

            if (_dataMgr.Pay.PurchaseStatus_DeleteAds)
            {
                reward();
            }
            else
            {
                _uiMgr.Popups.ShowAdsYesNoPopup(
                StrUtil.ToI2("WatchAnAdGet@").Replace("@", StrUtil.GetIconTxt((int)eIconTxt.Gem) + "10"), false, () =>
                {
                    _adsMgr.ShowAd(() =>
                    {
                        reward();
                    }, eAdsList.WatchAdsGem);
                }, null);
            }
        }
    }
}
