using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IShopPopupPackageContent
{
    void BtnInitialize(SoundMgr soundMgr);
    void MoveToPackage(eProductID product);
    void SetAdsSkipTicketPrice(string price);
    void ResetContentPos();
    void SetDeleteAdsProductState(bool isPurchasedDeleteAds);

    void SetInfinityBuffProductState(bool isPurchasedInfinityBuff);

    void SetFSFeeBoosterProductState(bool isPurchasedFSFeeBooster);

    void SetTipObjBoosterProductState(bool isPurchasedTipObjBooster);

    void SetCleanerPackageProductState(bool isPurchasedCleanerPackage);

    void SetAutoPrepIngredientsProductState(bool isPurchasedAutoPrepIngredients
        , bool isBuiltPrepIngredients);

    void SetAutoKillFlyingCashProductState(bool isPurchasedAutoKillFlyingCash);

    void SetAutoScreenCaptureProductState(bool isPurchasedAutoScreenCapture);

    void SetUniformPackageState(bool isUniformPack1PurchaseStatus, int purchasedCnt
        , int maxPurchaseAbleCnt, string price);

    event Action OnClickPurchaseBeginning;
    event Action OnClickPurchaseDeleteAds;
    event Action OnClickPurchaseInfinityBuff;
    event Action OnClickPurchaseFSFeeBooster;
    event Action OnClickPurchaseTipObjBooster;
    event Action OnClickPurchaseCleanerPackage;
    event Action OnClickPurchaseAutoPrepIngredients;
    event Action OnClickPurchaseAutoKillFlyingCash;
    event Action OnClickPurchaseAutoScreenCapture;
    event Action OnClickPurchaseUniformPack1;
}

public class ShopPopupPackageContent : MonoBehaviour, IShopPopupPackageContent
{
    public RectTransform rt_content;

    [Header("Delete Ads")]
    public RectTransform rt_deleteAds;
    public GameObject _deleteAdsPurchased;

    [Header("Infinity")]
    public RectTransform rt_infinityBuff;
    public GameObject _infinityBuffPurchased;

    [Header("FSFeeBooster")]
    public RectTransform rt_fsFeeBooster;
    public GameObject _fsFeeBoosterPurchased;

    [Header("TipObjBooster")]
    public RectTransform rt_tipObjBooster;
    public GameObject _tipObjBoosterPurchased;

    [Header("Cleaner")]
    public RectTransform rt_cleaner;
    public GameObject _cleanerPurchased;

    [Header("Auto Prep Ingredients")]
    public RectTransform rt_autoPrepIngredients;
    public GameObject _autoPrepIngredientsPurchased;

    [Header("Auto Kill Flying Cash")]
    public RectTransform rt_autoKillFlyingCash;
    public GameObject _autoKillFlyingCashPurchased;

    [Header("Auto Screen Capture")]
    public RectTransform rt_autoScreenCapture;
    public GameObject _autoScreenCapturePurchased;

    [Header("Uniform Pack1")]
    public RectTransform rt_uniformPack1;
    public GameObject _uniformPack1Purchased;
    public TextMeshProUGUI txt_uniformPack1PurchaseCnt;
    public TextMeshProUGUI txt_uniformPack1Price;

    public TextMeshProUGUI txt_adsSkipTicketPrice;

    [Header("Button")]
    public Button btn_purchaseBeginning;
    public Button btn_purchaseDeleteAds;
    public Button btn_purchaseInfinityBuff;
    public Button btn_purchaseFSFeeBooster;
    public Button btn_purchaseTipObjBooster;
    public Button btn_purchaseCleanerPackage;
    public Button btn_purchaseAutoPrepIngredients;
    public Button btn_purchaseAutoKillFlyingCash;
    public Button btn_purchaseAutoScreenCapture;
    public Button btn_purchaseUniformPack1;

    public event Action OnClickPurchaseBeginning;
    public event Action OnClickPurchaseDeleteAds;
    public event Action OnClickPurchaseInfinityBuff;
    public event Action OnClickPurchaseFSFeeBooster;
    public event Action OnClickPurchaseTipObjBooster;
    public event Action OnClickPurchaseCleanerPackage;
    public event Action OnClickPurchaseAutoPrepIngredients;
    public event Action OnClickPurchaseAutoKillFlyingCash;
    public event Action OnClickPurchaseAutoScreenCapture;
    public event Action OnClickPurchaseUniformPack1;

    public void BtnInitialize(SoundMgr soundMgr)
    {
        btn_purchaseBeginning.onClick.AddListener(()=>
        {
            OnClickPurchaseBeginning?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseDeleteAds.onClick.AddListener(() =>
        {
            OnClickPurchaseDeleteAds?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseInfinityBuff.onClick.AddListener(() =>
        {
            OnClickPurchaseInfinityBuff?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseFSFeeBooster.onClick.AddListener(() =>
        {
            OnClickPurchaseFSFeeBooster?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseTipObjBooster.onClick.AddListener(() =>
        {
            OnClickPurchaseTipObjBooster?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseCleanerPackage.onClick.AddListener(() =>
        {
            OnClickPurchaseCleanerPackage?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseAutoPrepIngredients.onClick.AddListener(() =>
        {
            OnClickPurchaseAutoPrepIngredients?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseAutoKillFlyingCash.onClick.AddListener(() =>
        {
            OnClickPurchaseAutoKillFlyingCash?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseAutoScreenCapture.onClick.AddListener(() =>
        {
            OnClickPurchaseAutoScreenCapture?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });

        btn_purchaseUniformPack1.onClick.AddListener(() =>
        {
            OnClickPurchaseUniformPack1?.Invoke();
            soundMgr.PlayUISFX(eUISFX.OnClickBtn);
        });
    }

    void OnDisable()
    {
        btn_purchaseBeginning.onClick.RemoveAllListeners();
        btn_purchaseDeleteAds.onClick.RemoveAllListeners();
        btn_purchaseInfinityBuff.onClick.RemoveAllListeners();
        btn_purchaseFSFeeBooster.onClick.RemoveAllListeners();
        btn_purchaseTipObjBooster.onClick.RemoveAllListeners();
        btn_purchaseCleanerPackage.onClick.RemoveAllListeners();
        btn_purchaseAutoPrepIngredients.onClick.RemoveAllListeners();
        btn_purchaseAutoKillFlyingCash.onClick.RemoveAllListeners();
        btn_purchaseAutoScreenCapture.onClick.RemoveAllListeners();
        btn_purchaseUniformPack1.onClick.RemoveAllListeners();
    }

    public void SetAdsSkipTicketPrice(string price)
    {
        txt_adsSkipTicketPrice.text = price;
    }

    public void ResetContentPos()
    {
        rt_content.anchoredPosition = Vector3.zero;
    }

    public void MoveToPackage(eProductID product)
    {
        float size = 0;
        RectTransform rt;

        switch (product)
        {
            case eProductID.fsfeeboosterpackage:
                for (int i = 0; i < rt_content.childCount; i++)
                {
                    rt = rt_content.GetChild(i).GetComponent<RectTransform>();
                    if (rt == rt_fsFeeBooster)
                    {
                        break;
                    }
                    size += rt.sizeDelta.x;
                }
                rt_content.anchoredPosition = new Vector3(-size, 0, 0);
                break;
            case eProductID.tipobjboosterpackage:
                for (int i = 0; i < rt_content.childCount; i++)
                {
                    rt = rt_content.GetChild(i).GetComponent<RectTransform>();
                    if (rt == rt_tipObjBooster)
                    {
                        break;
                    }
                    size += rt.sizeDelta.x;
                }
                rt_content.anchoredPosition = new Vector3(-size, 0, 0);
                break;
        }
    }

    public void SetDeleteAdsProductState(bool isPurchasedDeleteAds)
    {
        _deleteAdsPurchased.SetActive(isPurchasedDeleteAds);
    }

    public void SetInfinityBuffProductState(bool isPurchasedInfinityBuff)
    {
        _infinityBuffPurchased.SetActive(isPurchasedInfinityBuff);
        if (isPurchasedInfinityBuff)
        {
            rt_infinityBuff.SetAsLastSibling();
        }
    }

    public void SetFSFeeBoosterProductState(bool isPurchasedFSFeeBooster)
    {
        _fsFeeBoosterPurchased.SetActive(isPurchasedFSFeeBooster);
        if (isPurchasedFSFeeBooster)
        {
            rt_fsFeeBooster.SetAsLastSibling();
        }
    }

    public void SetTipObjBoosterProductState(bool isPurchasedTipObjBooster)
    {
        _tipObjBoosterPurchased.SetActive(isPurchasedTipObjBooster);
        if (isPurchasedTipObjBooster)
        {
            rt_tipObjBooster.SetAsLastSibling();
        }
    }

    public void SetCleanerPackageProductState(bool isPurchasedCleanerPackage)
    {
        _cleanerPurchased.SetActive(isPurchasedCleanerPackage);
        if (isPurchasedCleanerPackage)
        {
            rt_cleaner.SetAsLastSibling();
        }
    }

    public void SetAutoPrepIngredientsProductState(bool isPurchasedAutoPrepIngredients
        , bool isBuiltPrepIngredients)
    {
        // 짓지 않았으면 배너자체가 가려져 있음.
        rt_autoPrepIngredients.gameObject.SetActive(isBuiltPrepIngredients);
        if (isBuiltPrepIngredients)
        {
            _autoPrepIngredientsPurchased.SetActive(isPurchasedAutoPrepIngredients);
            if (isPurchasedAutoPrepIngredients)
            {
                rt_autoPrepIngredients.SetAsLastSibling();
            }
        }
    }

    public void SetAutoKillFlyingCashProductState(bool isPurchasedAutoKillFlyingCash)
    {
        _autoKillFlyingCashPurchased.SetActive(isPurchasedAutoKillFlyingCash);
        if (isPurchasedAutoKillFlyingCash)
        {
            rt_autoKillFlyingCash.SetAsLastSibling();
        }
    }

    public void SetAutoScreenCaptureProductState(bool isPurchasedAutoScreenCapture)
    {
        _autoScreenCapturePurchased.SetActive(isPurchasedAutoScreenCapture);
        if (isPurchasedAutoScreenCapture)
        {
            rt_autoScreenCapture.SetAsLastSibling();
        }
    }

    public void SetUniformPackageState(bool isUniformPack1PurchaseStatus, int purchasedCnt
                                     , int maxPurchaseAbleCnt, string price)
    {
        _uniformPack1Purchased.SetActive(!isUniformPack1PurchaseStatus);

        txt_uniformPack1PurchaseCnt.text = string.Format("({0} / {1})", purchasedCnt, maxPurchaseAbleCnt);
        txt_uniformPack1Price.text = price;
    }
}
