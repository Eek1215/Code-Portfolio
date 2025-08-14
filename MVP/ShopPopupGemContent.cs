using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Mono.Cecil;

public interface IShopPopupGemContent
{
    void Initialize(bool isPurchasedDeleteAds);
    bool CheckAdsCnt(int hasRemainingAdsCnt, int dailyMaxAdsCnt);
    void UpdateRemainingResetTime(string remainingTime);
    void ShowGetGemEffect();

    event Action OnClickGemWithAds;
}

public class ShopPopupGemContent : MonoBehaviour, IShopPopupGemContent
{
    public RectTransform rt_adsGem;
    public Image img_movieIconBalloon;
    public TextMeshProUGUI txt_adsGemRemainingCnt;

    // 보석상품들
    public ShopPopupGemContentItem[] _items;

    public Button btn_gemWithAds;
    public event Action OnClickGemWithAds;

    private bool _bInit;

    public void Initialize(bool isPurchasedDeleteAds)
    {
        if (!_bInit)
        {
            _bInit = true;
            _items[0].Init(eProductID.gem_1);
            _items[1].Init(eProductID.gem_2);
            _items[2].Init(eProductID.gem_3);
            _items[3].Init(eProductID.gem_4);
            _items[4].Init(eProductID.gem_5);

            btn_gemWithAds.onClick.AddListener(() =>
            {
                OnClickGemWithAds?.Invoke();
            });
        }
        else
        {
            for (int i = 0; i < _items.Length; i++)
            {
                _items[i].SetPriceTxt();
            }
        }

        img_movieIconBalloon.transform.localScale = Vector3.one;
        
        if(isPurchasedDeleteAds)
        {
            img_movieIconBalloon.sprite = SpriteMgr.Instance.UI.spr_deleteAds;
        }
    }

    public bool CheckAdsCnt(int hasRemainingAdsCnt, int dailyMaxAdsCnt)
    {
        if (hasRemainingAdsCnt > 0)
        {
            txt_adsGemRemainingCnt.text = StrUtil.GetColorStr("red", hasRemainingAdsCnt.ToString())
                                                + " / " + dailyMaxAdsCnt;
            return true;
        }
        else
        {
            txt_adsGemRemainingCnt.text = string.Empty;
            return false;
        }
    }

    public void UpdateRemainingResetTime(string remainingTime)
    {
        txt_adsGemRemainingCnt.text = remainingTime;
    }

    public void ShowGetGemEffect()
    {
        Util.AddItemShowEffect(eAsset.Gem, 10, rt_adsGem.position, 5);
    }
}
