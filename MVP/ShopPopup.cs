using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eShopPopup
{
    None = -1,
    Package,
    Gem,
    Wealth,
    Costume,
}

public interface IShopPopup
{
    void BtnInitialize();
    void OpenContent(eShopPopup content);
    void CheckItemTabNotice(bool on);
    void CheckUniformTabNotice(bool on);
    void CheckPurchaseBonus(bool on);
    void SetBtnCloseActive(bool on);

    void PlayClickBtnSound();
    event Action OnActiveOff;

    event Action OnClickPackageTab;
    event Action OnClickGemTab;
    event Action OnClickWealthTab;
    event Action OnClickCostumeTab;

    event Action<Sprite> OnClickShowConfirmPopup;
    event Action OnClickRestore;
    event Action OnClickAdsSkipTicketExchange;
}

public class ShopPopup : Popup, IShopPopup
{
    public ShopPopupWealthContent _wealthContent;
    public ShopPopupGemContent _gemContent;
    public ShopPopupPackageContent _packageContent;
    public ShopPopupCostumeContent _costumeContent;

    public Image img_BG;
    public Image img_pattern;

    public Button btn_close;

    public Color color_BG1;
    public Color color_BG2;
    public Color color_pattern1;
    public Color color_pattern2;

    public Button btn_packageTab;
    public Button btn_gemTab;
    public Button btn_wealthTab;
    public Button btn_costumeTab;
    public GameObject[] _tabBtnBlinds;

    public GameObject _itemTabNotice;
    public GameObject _uniformTabNotice;

    public Button btn_restore;
    public Button btn_exchange; // 티켓 -> 보석

    // 첫결제 보너스 관련
    public Button btn_purchaseBonusItem;
    public Image img_purchaseBonusItem;

    private Button btn_lastSelected;
    private GameObject _lastSelectedBlind;

    public event Action OnClickPackageTab;
    public event Action OnClickGemTab;
    public event Action OnClickWealthTab;
    public event Action OnClickCostumeTab;

    public event Action OnActiveOff;
    public event Action<Sprite> OnClickShowConfirmPopup;
    public event Action OnClickRestore;
    public event Action OnClickAdsSkipTicketExchange;
    public event Action OnClickGacha1Ticket;

    private bool _bInit = false;

    public void BtnInitialize()
    {
        if (_bInit)
            return;

        _bInit = true;

        btn_packageTab.onClick.AddListener(() =>
        {
            OnClickPackageTab?.Invoke();
        });

        btn_gemTab.onClick.AddListener(() =>
        {
            OnClickGemTab?.Invoke();
        });

        btn_wealthTab.onClick.AddListener(() =>
        {
            OnClickWealthTab?.Invoke();
        });

        btn_costumeTab.onClick.AddListener(() =>
        {
            OnClickCostumeTab?.Invoke();
        });

        btn_purchaseBonusItem.onClick.AddListener(() =>
        {
            OnClickShowConfirmPopup?.Invoke(img_purchaseBonusItem.sprite);
        });

        btn_restore.onClick.AddListener(() =>
        {
            OnClickRestore?.Invoke();
        });

        btn_exchange.onClick.AddListener(() =>
        {
            OnClickAdsSkipTicketExchange?.Invoke();
        });
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        OnActiveOff?.Invoke();
    }

    public void OpenContent(eShopPopup content)
    {
        _wealthContent.gameObject.SetActive(false);
        _gemContent.gameObject.SetActive(false);
        _packageContent.gameObject.SetActive(false);
        _costumeContent.gameObject.SetActive(false);

        if (btn_lastSelected != null)
        {
            btn_lastSelected.interactable = true;
            btn_lastSelected.transform.localScale = Vector3.one;

            _lastSelectedBlind.gameObject.SetActive(true);
        }

        // ios는 복원버튼을 켜야 하는데 우선 켰다가 코스튬일때만 끄기 위함.
#if UNITY_IOS
        btn_restore.gameObject.SetActive(true);
#else
        btn_restore.gameObject.SetActive(false);
#endif
        btn_exchange.gameObject.SetActive(DataMgr.Instance.Pay.PurchaseStatus_DeleteAds);

        switch (content)
        {
            case eShopPopup.Package:
                _packageContent.gameObject.SetActive(true);
                btn_lastSelected = btn_packageTab;
                _lastSelectedBlind = _tabBtnBlinds[0];
                img_BG.color = color_BG1;
                img_pattern.color = color_pattern1;
                break;

            case eShopPopup.Gem:
                _gemContent.gameObject.SetActive(true);
                btn_lastSelected = btn_gemTab;
                _lastSelectedBlind = _tabBtnBlinds[1];
                img_BG.color = color_BG1;
                img_pattern.color = color_pattern1;
                break;

            case eShopPopup.Wealth:
                _wealthContent.gameObject.SetActive(true);
                btn_lastSelected = btn_wealthTab;
                _lastSelectedBlind = _tabBtnBlinds[2];
                img_BG.color = color_BG1;
                img_pattern.color = color_pattern1;
                break;

            case eShopPopup.Costume:
#if UNITY_IOS
                btn_restore.gameObject.SetActive(false);
#endif
                btn_exchange.gameObject.SetActive(false);

                _costumeContent.gameObject.SetActive(true);
                btn_lastSelected = btn_costumeTab;
                _lastSelectedBlind = _tabBtnBlinds[3];
                img_BG.color = color_BG2;
                img_pattern.color = color_pattern2;
                break;
        }

        btn_lastSelected.interactable = false;
        btn_lastSelected.transform.DOScale(1.1f, 0.1f).OnComplete(() =>
        {
            btn_lastSelected.transform.DOScale(1.1f, 0.1f).OnComplete(() =>
            {
                btn_lastSelected.transform.localScale = Vector3.one * 1.1f;
            });
        });

        _lastSelectedBlind.SetActive(false);
    }

    public void OnSetBlindFlag(bool blindOn)
    {
        _wasActiveOffBlind.SetBlockFlag(blindOn);
    }

    public void PlayClickBtnSound()
    {
        SoundMgr.Instance.PlayUISFX(eUISFX.OnClickBtn);
    }

    public void CheckItemTabNotice(bool on)
    {
        _itemTabNotice.SetActive(on);
    }

    public void CheckUniformTabNotice(bool on)
    {
        _uniformTabNotice.SetActive(on);
    }

    public void CheckPurchaseBonus(bool on)
    {
        btn_purchaseBonusItem.gameObject.SetActive(on);
    }

    public void SetBtnCloseActive(bool on)
    {
        btn_close.gameObject.SetActive(on);
    }
}
