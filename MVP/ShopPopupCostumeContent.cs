using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface IShopPopupCostumeContent
{
    void BtnInitialize();
    void UpdateVariableUI(uint costumeGacha3TicketCnt, uint costumeGacha1TicketCnt
        , int dailyAdsCostumeRemainingGachaCnt, int maxDailyCostumeGachaCnt, bool isEnoughGacha3Cost);
    void Clean();
    void CheckBtnShowAdsNotice(int remainingDailyAdsCostumeGachaCnt);

    void OnHideGachaInfoUI();
    void ShowAdsConfirmPopup(UIMgr uiMgr, Action rewardEvent);
    void PlayChestShakeCor(List<CostumeGachaResult> results);

    event Action OnClickAds;
    event Action OnClickGachaWithGem;
    event Action OnClickGacha1Ticket;
    event Action OnClickGacha3Ticket;
    event Action OnClickConfirm;
}

public class ShopPopupCostumeContent : MonoBehaviour, IShopPopupCostumeContent
{
    // 코스튬이 나오는 상자와 관련.
    public Transform trs_chestParent;
    public Transform trs_costumeSpanwPos;
    public Image img_chest;
    public Sprite spr_chestLock;
    public Sprite spr_chestOpen;
    public GameObject _chestEffectObj;

    // 설명과 버튼들의 부모.
    public GameObject _txtBtnObjs;

    public Button btn_confirm;

    // 광고보고 1회 뽑기 관련
    public TextMeshProUGUI txt_adsCnt;
    public BtnShowAds _btnShowAds;
    public GameObject _btnShowAdsNotice;
    public Button btn_showAdsLock;

    // 1회, 3회 티켓 뽑기 버튼
    public Button btn_gacha3;
    public Button btn_gacha1Ticket;
    public TextMeshProUGUI txt_gacha1Ticket;
    public TextMeshProUGUI txt_gacha3Ticket;

    // 보석으로 3회 뽑기 버튼
    public Button btn_gacha3Ticket;

    // 1회인지 3회인지에 따라서 오브젝트 그룹이 다름(개수가 많으면 프리팹으로 뺐지만 적어서 인스펙터 처리)
    public ShopPopupCostumeContentGachaItem _gacha1Obj;
    public ShopPopupCostumeContentGachaItem[] _gacha3Objs;

    public event Action OnClickAds;
    public event Action OnClickGachaWithGem;
    public event Action OnClickGacha1Ticket;
    public event Action OnClickGacha3Ticket;
    public event Action OnClickConfirm;

    private bool _bInit = false;

    public void BtnInitialize()
    {
        if (_bInit)
            return;

        _bInit = true;

        _btnShowAds.Btn.onClick.AddListener(() =>
        {
            OnClickAds?.Invoke();
        });

        btn_gacha3.onClick.AddListener(() =>
        {
            OnClickGachaWithGem?.Invoke();
        });

        btn_gacha1Ticket.onClick.AddListener(() =>
        {
            OnClickGacha1Ticket?.Invoke();
        });

        btn_gacha3Ticket.onClick.AddListener(() =>
        {
            OnClickGacha3Ticket?.Invoke();
        });

        btn_confirm.onClick.AddListener(() =>
        {
            OnClickConfirm?.Invoke();
        });
    }

    public void Clean()
    {
        _gacha1Obj.gameObject.SetActive(false);
        for (int i = 0; i < _gacha3Objs.Length; i++)
        {
            _gacha3Objs[i].gameObject.SetActive(false);
        }

        img_chest.sprite = spr_chestLock;
        _chestEffectObj.SetActive(false);
        _txtBtnObjs.gameObject.SetActive(true);
        btn_confirm.gameObject.SetActive(false);
    }

    public void UpdateVariableUI(uint costumeGacha3TicketCnt, uint costumeGacha1TicketCnt
        , int dailyAdsCostumeRemainingGachaCnt, int maxDailyCostumeGachaCnt, bool isEnoughGacha3Cost)
    {
        // 3회 뽑기. 티켓 유뮤
        if (costumeGacha3TicketCnt > 0)
        {
            btn_gacha3.gameObject.SetActive(false);
            btn_gacha3Ticket.gameObject.SetActive(true);
            txt_gacha3Ticket.text = costumeGacha3TicketCnt.ToString();
            btn_gacha3.GetComponent<DoPunchObj>().OffPunch();
        }
        else
        {
            btn_gacha3.gameObject.SetActive(true);
            btn_gacha3Ticket.gameObject.SetActive(false);
            btn_gacha3.interactable = isEnoughGacha3Cost;
            if (btn_gacha3.interactable)
            {
                btn_gacha3.GetComponent<DoPunchObj>().OnPunch();
            }
            else
            {
                btn_gacha3.GetComponent<DoPunchObj>().OffPunch();
            }
        }

        // 1뽑기 티켓유무
        if (costumeGacha1TicketCnt > 0)
        {
            btn_gacha1Ticket.gameObject.SetActive(true);
            txt_gacha1Ticket.text = costumeGacha1TicketCnt.ToString();

            _btnShowAds.gameObject.SetActive(false);
            btn_showAdsLock.gameObject.SetActive(false);
            _btnShowAds.GetComponent<DoPunchObj>().OffPunch();
        }
        else
        {
            btn_gacha1Ticket.gameObject.SetActive(false);
            if (dailyAdsCostumeRemainingGachaCnt == 0)//DataMgr.Instance.Additional.DailyAdsCostumeRemainingGachaCnt
            {
                _btnShowAds.gameObject.SetActive(false);
                btn_showAdsLock.gameObject.SetActive(true);

                _btnShowAds.GetComponent<DoPunchObj>().OffPunch();
            }
            else
            {
                _btnShowAds.gameObject.SetActive(true);
                btn_showAdsLock.gameObject.SetActive(false);
                txt_adsCnt.text = StrUtil.GetColorStr("#7FFF6E", dailyAdsCostumeRemainingGachaCnt.ToString())
                                                + " / " + maxDailyCostumeGachaCnt;// DataMgr.Instance.Additional.MAX_DAILY_COSTUME_GACHA

                _btnShowAds.GetComponent<DoPunchObj>().OnPunch();
            }
        }
    }

    public void OnHideGachaInfoUI()
    {
        _txtBtnObjs.gameObject.SetActive(false);
        btn_confirm.gameObject.SetActive(false);
    }

    public void ShowAdsConfirmPopup(UIMgr uiMgr, Action rewardEvent)
    {
        uiMgr.Popups.ShowAdsYesNoPopup(StrUtil.ToI2("WatchAnAdGetUniform"), _btnShowAds.IsWillUseTicket, () =>
        {
            _btnShowAds.OnClick(eAdsList.CostumeFree, () =>
            {
                rewardEvent();
            });
        }, null);
    }

    public void PlayChestShakeCor(List<CostumeGachaResult> results)
    {
        StartCoroutine(ChestShake(results));
    }

    // 상자가 흔들린 뒤에 보여주는 연출.
    IEnumerator ChestShake(List<CostumeGachaResult> results, float during = 1f, float power = 20f)
    {
        Vector3 originPos = img_chest.transform.position;

        img_chest.transform.DOScale(new Vector3(1.2f, 0.7f, 0), 0.2f).SetEase(Ease.OutQuad);
        img_chest.transform.DOScale(Vector3.one, 0.1f).SetDelay(0.3f);
        trs_chestParent.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.2f, 7).SetDelay(0.3f);

        yield return new WaitForSeconds(0.3f);
        img_chest.sprite = spr_chestOpen;
        _chestEffectObj.SetActive(true);
        SoundMgr.Instance.PlayUISFX(eUISFX.CostumeChestOpen);
        yield return new WaitForSeconds(0.2f);

        if (results.Count == 1)
        {
            _gacha1Obj.gameObject.SetActive(true);
            _gacha1Obj.ShowItem(trs_costumeSpanwPos.position, results[0]);
        }
        else
        {
            for (int i = 0; i < results.Count; i++)
            {
                _gacha3Objs[i].gameObject.SetActive(true);
                _gacha3Objs[i].ShowItem(trs_costumeSpanwPos.position, results[i]);
                yield return new WaitForSeconds(0.2f);
            }
        }

        yield return new WaitForSeconds(2f);

        btn_confirm.gameObject.SetActive(true);
    }

    public void CheckBtnShowAdsNotice(int remainingDailyAdsCostumeGachaCnt)
    {
        _btnShowAdsNotice.SetActive(remainingDailyAdsCostumeGachaCnt > 0);
    }
}
