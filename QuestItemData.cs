using System;
using UnityEngine;

public class QuestItemData : BaseData
{
    public string QuestTypeStr;
    public eQuest QuestType
    {
        get { return (eQuest)Enum.Parse(typeof(eQuest), QuestTypeStr); }
    }
    public uint NeedCount;
    public long ProgressCount;
    public string RewardTypeStr;
    public eAsset RewardType
    {
        get { return (eAsset)Enum.Parse(typeof(eAsset), RewardTypeStr); }
    }
    public uint RewardCnt;

    public eQuest QuestIndex
    {
        get { return (eQuest)Enum.Parse(typeof(eQuest), QuestTypeStr); }
    }

    public virtual bool IsEnoughCnt
    {
        get { return ProgressCount >= NeedCount; }
    }

    public delegate void OnEvent();
    public OnEvent OnUpdateValue;

    public virtual void AddEvent()
    {
        /* 이벤트를 직접 호출식이 아닌 delegate 추가하는 이유는. 같은 항목을 가진 퀘스트가 있을 경우에
         * 매번 이벤트를 찾기 힘드므로 이벤트 추가 형식으로하면 빠트리지 않기 때문이다 */
        switch (QuestType)
        {
            case eQuest.ToutCustomer:
                DataMgr.Instance.InGame.Tout.OnToutCustomer += UpdateValue;
                break;

            case eQuest.AutoTout:
                DataMgr.Instance.InGame.Facility.AutoTout.OnTooMuchTout += UpdateValue;
                break;

            case eQuest.PlayPrepIngredients:
                DataMgr.Instance.InGame.Facility.PrepIngredientsGame.OnGameClear += UpdateValue;
                break;

            case eQuest.KillFlyingCash:
                TycoonMgr.Instance.Main._flyingCash._touchObj.OnKillFlyingCash += UpdateValue;
                break;

            case eQuest.FloorStoreLevelUp:
                DataMgr.Instance.InGame.OnFloorStoreLevelUp += UpdateValue;
                break;

            case eQuest.FeedStaff:
                DataMgr.Instance.InGame.Staffs.OnFeedStaff += UpdateValue;
                break;

            case eQuest.PickUpNotPickedUpCash:
                TycoonMgr.Instance.Main.NotPickedUpCashSensor.OnPickUpCash += UpdateValue;
                break;

            case eQuest.PickUpNotPickedUpLeaf:
                TycoonMgr.Instance.Main.NotPickedUpCashSensor.OnPickUpLeaf += UpdateValue;
                break;

            case eQuest.Group1Customer:
            case eQuest.Group2Customer:
            case eQuest.Group3Customer:
            case eQuest.Group4Customer:
            case eQuest.Group5Customer:
                DataMgr.Instance.Additional.RegisterGroupQuest(QuestType, this);
                break;

            case eQuest.ClearDailyQuest:
                DataMgr.Instance.Additional.DailyQuest.OnClearDailyQuest += UpdateValue;
                break;

            case eQuest.BuildSite:
                DataMgr.Instance.InGame.OnBuildSiteEvent += UpdateValue;
                break;

            case eQuest.Attendance:
                DataMgr.Instance.Additional.Attendance.OnAttendance += UpdateValue;
                break;

            case eQuest.ToutChatterCustomer:
                TycoonMgr.Instance.Main.CustomerCtrl.OnToutChatterCustomer += UpdateValue;
                break;


            case eQuest.PlaySnakesLadders:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestSnakesLadders).OnMiniGameClear += UpdateValue;
                break;
            case eQuest.WinCardGameA:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestCardGameA).OnMiniGameClear += UpdateValue;
                break;
            case eQuest.WinCardGameB:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestCardGameB).OnMiniGameClear += UpdateValue;
                break;
            case eQuest.ClearWhackAMole:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestWhackAMole).OnMiniGameClear += UpdateValue;
                break;
            case eQuest.KillDisrupter:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestDisrupter).OnMiniGameClear += UpdateValue;
                break;
            case eQuest.ToutGuide:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestGuide).OnMiniGameClear += UpdateValue;
                break;
            case eQuest.CustomerScreeCapture:
                UIMgr.Instance.Main._screenCapture.OnCaptureWithCustomer += UpdateValue;
                break;
            case eQuest.GiveWaterToCustomer:
                DataMgr.Instance.InGame.Customers.OnClikedSpeechBalloon += UpdateValue;
                break;
            case eQuest.StallLevelUp:
                DataMgr.Instance.InGame.Facility.OnFacilityLevelUp += UpdateValue;
                break;
            case eQuest.StoreTypeLevelUp:
                DataMgr.Instance.InGame.StoreTypes.OnStoreTypeLevelUp += UpdateValue;
                break;
            case eQuest.ClearSpecialQuest:
                DataMgr.Instance.InGame.Facility.SpecialQuestBoard.OnQuestClear += UpdateValue;
                break;

            case eQuest.PickUpTrash:
                DataMgr.Instance.InGame.OnPickUpTrash += UpdateValue;
                break;
            case eQuest.VisitedToutCat:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestToutCat).OnVisited += UpdateValue;
                break;


            case eQuest.GetDailyQuestBonus:
                DataMgr.Instance.Additional.DailyQuest.OnGetDailyQuestBonus += UpdateValue;
                break;
            case eQuest.GetBonusCashBar:
                DataMgr.Instance.InGame.Facility.BonusCashBar.OnGetBonusCash += UpdateValue;
                break;
            case eQuest.ClearGroupReservation:
                DataMgr.Instance.InGame.GroupReservation.OnClearGroupReservation += UpdateValue;
                break;

            case eQuest.VisitedCustomerType1:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType1 += UpdateValue;
                break;
            case eQuest.VisitedCustomerType2:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType2 += UpdateValue;
                break;
            case eQuest.VisitedCustomerType3:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType3 += UpdateValue;
                break;
            case eQuest.VisitedCustomerType4:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType4 += UpdateValue;
                break;
            case eQuest.VisitedCustomerType5:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType5 += UpdateValue;
                break;
        }
    }

    public void RemoveEvent()
    {
        eQuest type = Util.StringToEnum<eQuest>(QuestTypeStr, eQuest.None);

        switch (type)
        {
            case eQuest.ToutCustomer:
                DataMgr.Instance.InGame.Tout.OnToutCustomer -= UpdateValue;
                break;

            case eQuest.AutoTout:
                DataMgr.Instance.InGame.Facility.AutoTout.OnTooMuchTout -= UpdateValue;
                break;

            case eQuest.PlayPrepIngredients:
                DataMgr.Instance.InGame.Facility.PrepIngredientsGame.OnGameClear -= UpdateValue;
                break;

            case eQuest.KillFlyingCash:
                TycoonMgr.Instance.Main._flyingCash._touchObj.OnKillFlyingCash -= UpdateValue;
                break;

            case eQuest.FloorStoreLevelUp:
                DataMgr.Instance.InGame.OnFloorStoreLevelUp -= UpdateValue;
                break;

            case eQuest.FeedStaff:
                DataMgr.Instance.InGame.Staffs.OnFeedStaff -= UpdateValue;
                break;

            case eQuest.PickUpNotPickedUpCash:
                TycoonMgr.Instance.Main.NotPickedUpCashSensor.OnPickUpCash -= UpdateValue;
                break;

            case eQuest.PickUpNotPickedUpLeaf:
                TycoonMgr.Instance.Main.NotPickedUpCashSensor.OnPickUpLeaf -= UpdateValue;
                break;

            case eQuest.Group1Customer:
            case eQuest.Group2Customer:
            case eQuest.Group3Customer:
            case eQuest.Group4Customer:
            case eQuest.Group5Customer:
                DataMgr.Instance.Additional.RemoveGroupQuest(QuestType, this);
                break;

            case eQuest.ClearDailyQuest:
                DataMgr.Instance.Additional.DailyQuest.OnClearDailyQuest -= UpdateValue;
                break;

            case eQuest.BuildSite:
                DataMgr.Instance.InGame.OnBuildSiteEvent -= UpdateValue;
                break;

            case eQuest.Attendance:
                DataMgr.Instance.Additional.Attendance.OnAttendance -= UpdateValue;
                break;

            case eQuest.ToutChatterCustomer:
                TycoonMgr.Instance.Main.CustomerCtrl.OnToutChatterCustomer -= UpdateValue;
                break;

            case eQuest.PlaySnakesLadders:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestSnakesLadders).OnMiniGameClear -= UpdateValue;
                break;
            case eQuest.WinCardGameA:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestCardGameA).OnMiniGameClear -= UpdateValue;
                break;
            case eQuest.WinCardGameB:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestCardGameB).OnMiniGameClear -= UpdateValue;
                break;
            case eQuest.ClearWhackAMole:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestWhackAMole).OnMiniGameClear -= UpdateValue;
                break;
            case eQuest.KillDisrupter:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestDisrupter).OnMiniGameClear -= UpdateValue;
                break;
            case eQuest.ToutGuide:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestGuide).OnMiniGameClear -= UpdateValue;
                break;
            case eQuest.CustomerScreeCapture:
                UIMgr.Instance.Main._screenCapture.OnCaptureWithCustomer -= UpdateValue;
                break;
            case eQuest.GiveWaterToCustomer:
                DataMgr.Instance.InGame.Customers.OnClikedSpeechBalloon -= UpdateValue;
                break;
            case eQuest.StallLevelUp:
                DataMgr.Instance.InGame.Facility.OnFacilityLevelUp -= UpdateValue;
                break;
            case eQuest.StoreTypeLevelUp:
                DataMgr.Instance.InGame.StoreTypes.OnStoreTypeLevelUp -= UpdateValue;
                break;
            case eQuest.ClearSpecialQuest:
                DataMgr.Instance.InGame.Facility.SpecialQuestBoard.OnQuestClear -= UpdateValue;
                break;

            case eQuest.PickUpTrash:
                DataMgr.Instance.InGame.OnPickUpTrash -= UpdateValue;
                break;
            case eQuest.VisitedToutCat:
                DataMgr.Instance.InGame.Customers.GetGuest(eGuest.GuestToutCat).OnVisited -= UpdateValue;
                break;

            case eQuest.GetDailyQuestBonus:
                DataMgr.Instance.Additional.DailyQuest.OnGetDailyQuestBonus -= UpdateValue;
                break;
            case eQuest.GetBonusCashBar:
                DataMgr.Instance.InGame.Facility.BonusCashBar.OnGetBonusCash -= UpdateValue;
                break;
            case eQuest.ClearGroupReservation:
                DataMgr.Instance.InGame.GroupReservation.OnClearGroupReservation -= UpdateValue;
                break;

            case eQuest.VisitedCustomerType1:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType1 -= UpdateValue;
                break;
            case eQuest.VisitedCustomerType2:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType2 -= UpdateValue;
                break;
            case eQuest.VisitedCustomerType3:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType3 -= UpdateValue;
                break;
            case eQuest.VisitedCustomerType4:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType4 -= UpdateValue;
                break;
            case eQuest.VisitedCustomerType5:
                DataMgr.Instance.InGame.Customers.OnVisitedCustomerType5 -= UpdateValue;
                break;
        }
    }

    public virtual void ResetValue(bool bSave)
    {
        if (bSave)
        {
            DataMgr.Instance.LocalSave();
            DataMgr.Instance.FirebaseSave();
        }
    }

    public virtual void UpdateValue(long progressCnt)
    {
        long prevCnt = ProgressCount;
        ProgressCount += progressCnt;
        
        if (ProgressCount >= NeedCount)
        {
            ProgressCount = NeedCount;
            // 값을 올리기전과 최대값에 도달했을 때 같다면 max = max가 된 것이므로 알람을 다시 울리지 않아도됨
            if (prevCnt < ProgressCount)
            {
                OnNotice();
                OnUpdateValue?.Invoke();
            }
            else
            {
                // 최댓값이면 굳이 호출 안해도됨.
            }
        }
        else
        {
            OnUpdateValue?.Invoke();
        }
    }

    public void UpdateValue(eStoreGroup group, eStoreType type)
    {
        if(QuestTypeStr.Contains(group.ToString()))
        {
            UpdateValue(1);
        }
    }

    protected virtual void OnNotice()
    {
        NoticeMgr.Instance.SpecialQuest.OnNotice();
    }

    protected virtual eQuest GetRandomQuestType()
    {
        return eQuest.None;
    }

    protected virtual eAsset GetRewardType()
    {
        return eAsset.None;
    }

    protected virtual uint GetRewardCount()
    {
        return 0;
    }

    protected virtual uint GetNeedCount()
    {
        return 0;
    }
}
