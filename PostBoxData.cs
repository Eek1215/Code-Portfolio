using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using CodeStage.AntiCheat.Storage;
using Firebase.Database;
using Firebase.Extensions;
using InfiniteValue;
using Newtonsoft.Json;

/* 데이터 코드중에 우편함을 관리하는 데이터 코드입니다 
 * 각각의 우편은 MailItem이란 클래스로 저장되며, 
 * 종료시간, 보상타입, 보상갯수, 우편함에 노출될 텍스트 키값(로컬라이징), 상태(우편함, 받음, 타임오버) 
 * 코드 스타일은 public -> private -> IEnumerator 순으로 보통 짜는 편이며
 * 연관성이 깊고 1회성밖에 안쓰이는 코드는 예외적으로 바로 아래에 같이 두기도 함.
 * 또는 한 스크립트에 여러 기능들이 있을 때 기능별로 region으로 묶기도 함. */
[System.Serializable]
public class PostBoxJsonData
{
    public PostBoxJsonData() { }

    public PostBoxJsonData(PostBoxData data)
    {
        ReceivedMail = new Dictionary<string, MailItem>();
        foreach (var item in data.ReceivedMail)
        {
            ReceivedMail.Add(item.Key, new MailItem(item.Value));
        }

        PersonalPostBox = new Dictionary<string, MailItem>();
        foreach (var item in data.PersonalPostBox)
        {
            PersonalPostBox.Add(item.Key, new MailItem(item.Value));
        }

        LastGetPushReward1_Time = data.LastGetPushReward1_Time.ToString();
        LastGetPushReward2_Time = data.LastGetPushReward2_Time.ToString();
    }

    public Dictionary<string, MailItem> ReceivedMail;
    public Dictionary<string, MailItem> PersonalPostBox;

    public string LastGetPushReward1_Time;
    public string LastGetPushReward2_Time;
}

/* 저장은 개인우편(푸쉬보상 or 서버에서 직접 쏴주는 경우)과 받은 우편들만 저장
 * 공용 우편은 매번 게임을 시작할 때 공용 우편함에서 내가 받은 우편들중에 없는 것들을 읽어옴. */
public class PostBoxData : BaseData, ISubject
{
    // 수령 가능한 메일들 (저장x)
    public Dictionary<string, MailItem> OfficialPostBox { get; private set; }

    // 저장해야 하는 개인우편함 (저장o)
    public Dictionary<string, MailItem> PersonalPostBox { get; private set; }
    // 이미 수령한 메일들 (저장o)
    public Dictionary<string, MailItem> ReceivedMail { get; private set; }

    // 푸쉬보상.
    private readonly TimeSpan PUSH_REWARD1_TIME = new TimeSpan(12, 0, 0);
    private readonly TimeSpan PUSH_REWARD2_TIME = new TimeSpan(18, 0, 0);

    // 12시나 18시에서 추가로 몇시간까지 접속해도 받을 수 있는지.
    private readonly TimeSpan PUSH_RREWARD_PERIOD_TIME = new TimeSpan(5, 0, 0); // 5시간동안 받을 수 있게.

    // 접속 후 다음 푸쉬시간까지 몇초뒤에 받을 수 있는지. 
    private int _remainingTime_PushReward1;
    private int _remainingTime_PushReward2;

    // 마지막으로 받은 푸쉬보상들의 시간.
    public DateTime LastGetPushReward1_Time { get; private set; }
    public DateTime LastGetPushReward2_Time { get; private set; }

    // 하루시간을 초로.
    private readonly int SECOND_PER_DAY = 86400;

    public PostBoxData()
    {
        // 값들은 기본 초기화. 자료구조들은 할당.
        OfficialPostBox = new Dictionary<string, MailItem>();
        ReceivedMail = new Dictionary<string, MailItem>();
        PersonalPostBox = new Dictionary<string, MailItem>();

        LastGetPushReward1_Time = DateTime.MinValue;
        LastGetPushReward2_Time = DateTime.MinValue;
    }

    // 역직렬화를 하게 하는 코드. Json에서 클래스로 되돌리긴 하지만, 저장된 값을 읽어오는 방식을
    // 사용하기 때문에 사용은 하지 않고 에러가 나지 않도록 생성만 해둔 코드.
    [JsonConstructor]
    public PostBoxData(string empty) { }

    public void LocalLoad()
    {
        /* 기기에 저장되어 있는 우편함 데이터를 불러오고 그걸 역직렬화하여 푸는 코드.
         * 서버에서 데이터를 불러올 때는 LocalLoad 호출 이전에 서버데이터를 기기에 덮어씌운 뒤 불러옴 */
        PostBoxJsonData data = Newtonsoft.Json.JsonConvert.DeserializeObject<PostBoxJsonData>(
            ObscuredPrefs.Get("PostBoxData", Newtonsoft.Json.JsonConvert.SerializeObject(new PostBoxJsonData(this))));

        if (data != null)
        {
            /* 데이터에서 특정 List나 Dictionary를 null 체크하는 이유는, 업데이트로 새로 추가되는 경우
             * 유저에게서는 데이터가 없었던 것이기 때문에 null 에러가 나옵니다. 
             * 또한 ReferenceEquals(data.ReceivedMail, null)도 속도 측면에서 더 유리할 수 있지만
             * 초기화 시 1회이고 해당 Destroy등을 하는 문제가 없기에 가독성을 위해 == null로 체크를 하였음  */
            if (data.ReceivedMail != null)
            {
                foreach (KeyValuePair<string, MailItem> item in data.ReceivedMail)
                {
                    // 중복으로 들어가는 걸 방지
                    if (!ReceivedMail.ContainsKey(item.Key))
                    {
                        ReceivedMail.Add(item.Key, item.Value);
                    }
                }
            }
            if (data.PersonalPostBox != null)
            {
                foreach (KeyValuePair<string, MailItem> item in data.PersonalPostBox)
                {
                    if (!PersonalPostBox.ContainsKey(item.Key))
                    {
                        PersonalPostBox.Add(item.Key, item.Value);
                    }
                }
            }

            LastGetPushReward1_Time = DateTime.Parse(data.LastGetPushReward1_Time);
            LastGetPushReward2_Time = DateTime.Parse(data.LastGetPushReward2_Time);
        }       

        LoadOfficialPostBoxMail();
    }    

    // 데이터를 저장할 때 아래의 GetJsonData로 현재 클래스를 Json화해서 넘겨준 다음 기기저장.
    public override void SetJsonData(string jsonData)
    {
        // ObscuredPrefs는 에셋중에 PlayerPrefs의 암호화를 한 방식.
        ObscuredPrefs.Set("PostBoxData", jsonData);
    }

    public override string GetJsonData()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(new PostBoxJsonData(this));
    }

    public void OnLoadCompleteEvent()
    {
        // 개인우편함을 모두 불러온 뒤 푸쉬알림 여부를 체크.
        LoadPersonalPostBoxMail(() =>
        {
            if (CheckTodayPushReward(LastGetPushReward1_Time, PUSH_REWARD1_TIME, GetPushReward1()))
            {
                LastGetPushReward1_Time = TimeMgr.Instance.CurrentTime;
            }

            if (CheckTodayPushReward(LastGetPushReward2_Time, PUSH_REWARD2_TIME, GetPushReward2()))
            {
                LastGetPushReward2_Time = TimeMgr.Instance.CurrentTime;
            }

            CalculatePushRewardTime();

            // 앱의 Focus가 나갔다가 돌아오거나 시간이 흘렀을 때 푸쉬보상을 체크.
            TimeMgr.Instance.OnBreakApplicationUnpause += CalculatePushRewardTime;
            TimeMgr.Instance.DecreaseTimePerSec += DecreasePerSec;

            DataMgr.Instance.BothSave();
        });

        CheckNotice();
    }

    #region 비동기 우편함 Load
    public async void LoadPersonalPostBoxMail(Action completeAction = null)
    {
        await DataMgr.Instance.UserRef.Child(eDataIndex.PostBox.ToString()).Child("PersonalPostBox").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    return;
                }

                DataSnapshot snapshot = task.Result;
                //Debug.Log("Personal Cnt before : " + PersonalPostBox.Count);
                foreach (DataSnapshot item in snapshot.Children)
                {
                    //Debug.Log(item.Key + " : " + PersonalPostBox.ContainsKey(item.Key) + " : " + ReceivedMail.ContainsKey(item.Key));
                    if (!PersonalPostBox.ContainsKey(item.Key) && !ReceivedMail.ContainsKey(item.Key))
                    {
                        PersonalPostBox.Add(item.Key, JsonConvert.DeserializeObject<MailItem>(item.GetRawJsonValue()));
                    }
                }
                //Debug.Log("Personal Cnt after : " + PersonalPostBox.Count);
                CheckReceiveTime();
                CheckNotice();

                completeAction?.Invoke();
            });
    }

    public async void LoadOfficialPostBoxMail(Action completeAction = null)
    {
        await DataMgr.Instance.Database.Child(eDBRoot.Official.ToString())
            .Child(eDBOfficial.PostBox.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    return;
                }

                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot item in snapshot.Children)
                {
                    if (!OfficialPostBox.ContainsKey(item.Key) && !ReceivedMail.ContainsKey(item.Key))
                    {
                        OfficialPostBox.Add(item.Key, Newtonsoft.Json.JsonConvert.DeserializeObject<MailItem>(item.GetRawJsonValue()));
                    }
                }

                CheckReceiveTime();
                CheckNotice();

                completeAction?.Invoke();
            });
    }

    public async void LoadAllPostBoxMail(Action completeAction = null)
    {
        await DataMgr.Instance.UserRef.Child(eDataIndex.PostBox.ToString()).Child("PersonalPostBox").GetValueAsync()
         .ContinueWithOnMainThread(task =>
         {
             if (task.IsFaulted || task.IsCanceled)
             {
                 return;
             }

             DataSnapshot snapshot = task.Result;
             foreach (DataSnapshot item in snapshot.Children)
             {
                 if (!PersonalPostBox.ContainsKey(item.Key) && !ReceivedMail.ContainsKey(item.Key))
                 {
                     PersonalPostBox.Add(item.Key, JsonConvert.DeserializeObject<MailItem>(item.GetRawJsonValue()));
                 }
             }
         });

        await DataMgr.Instance.Database.Child(eDBRoot.Official.ToString())
            .Child(eDBOfficial.PostBox.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    return;
                }

                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot item in snapshot.Children)
                {
                    if (!OfficialPostBox.ContainsKey(item.Key) && !ReceivedMail.ContainsKey(item.Key))
                    {
                        OfficialPostBox.Add(item.Key, Newtonsoft.Json.JsonConvert.DeserializeObject<MailItem>(item.GetRawJsonValue()));
                    }
                }

                CheckReceiveTime();
                CheckNotice();

                completeAction?.Invoke();
            });
    }
    #endregion

    public void AddPersonalMail(string key, MailItem item)
    {
        if (!PersonalPostBox.ContainsKey(key))
        {
            PersonalPostBox.Add(key, item);

            CheckNotice();
        }
    }

    // 우편함의 내용물을 받은 경우.
    // 공용우편 예시. O_20250319_162535 = 우편타입_종료날짜_종료되는시간
    // 시간을 키로 저장사용하였으며, 초 단위기 때문에 한 번에 여러개를 보내지 않으면 가능.
    // 한 번에 여러개를 보낸다면 시간뒤에 추가로 인덱스나 랜덤키를 부여하는 방식 채택.
    public void ReceiveMail(string key, MailItem item, Vector3 clickPos)
    {
        bool shouldSave = false;
        string[] keySplit = key.Split('_');
        // 앞자리가 P면 개인우편, O면 공용 우편.
        if (keySplit[0].Equals("P"))
        {
            if (PersonalPostBox.ContainsKey(key))
            {
                PersonalPostBox.Remove(key);
                shouldSave = true;
            }
        }
        else
        {
            if (OfficialPostBox.ContainsKey(key))
            {
                OfficialPostBox.Remove(key);
                shouldSave = true;
            }
            // 유저에게 직접 발송하여 넣어준건 앞이 O_으로 시작하지만 직접 PersonalPostBox에 들어간다.
            // 전체유저에게 발송한 건 OfficialPostBox로 들어감.
            else if (PersonalPostBox.ContainsKey(key))
            {
                PersonalPostBox.Remove(key);
                shouldSave = true;
            }
        }

        if (shouldSave)
        {
            // 받은 시간 정보입력 후 받은 메일함에 추가 and 저장.
            item.MailState = eMailState.Received.ToString();
            item.ReceviedTime = TimeMgr.Instance.CurrentTime.ToString();
            ReceivedMail.Add(key, item);

            ReceiveReward(item, clickPos);

            CheckNotice();

            LocalSave();
            ObscuredSave();
            DataMgr.Instance.FirebaseSave();
        }
    }

    public void ReceiveAllMail()
    {
        foreach (KeyValuePair<string, MailItem> item in OfficialPostBox.ToList())
        {
            OfficialPostBox.Remove(item.Key);
            item.Value.MailState = eMailState.Received.ToString();
            item.Value.ReceviedTime = TimeMgr.Instance.CurrentTime.ToString();
            ReceivedMail.Add(item.Key, item.Value);

            ReceiveReward(item.Value, ResolutionMgr.Instance.ScreenMiddle);
        }

        foreach (KeyValuePair<string, MailItem> item in PersonalPostBox.ToList())
        {
            PersonalPostBox.Remove(item.Key);
            item.Value.MailState = eMailState.Received.ToString();
            item.Value.ReceviedTime = TimeMgr.Instance.CurrentTime.ToString();
            ReceivedMail.Add(item.Key, item.Value);

            ReceiveReward(item.Value, ResolutionMgr.Instance.ScreenMiddle);
        }

        CheckNotice();

        LocalSave();
        ObscuredSave();
        DataMgr.Instance.FirebaseSave();
    }

    // 현재 우편들이 남은시간이 초과되버려서 없어져야 하는지.
    public void CheckReceiveTime()
    {
        bool isUpdate = false;
        // 우편함을 매개변수로 받아 우편함 내에 모든 우편들의 시간을 검토하는 람다식.
        Action<Dictionary<string, MailItem>> remove = dic =>
        {
            foreach (KeyValuePair<string, MailItem> item in dic.ToList())
            {
                // 받을 수 있는 시간이 지나버리면 해당 PostBox에서 지워주고 TimeOver라고 처리 후 받은메일함으로 저장.
                if ((DateTime.Parse(item.Value.EndTime) - TimeMgr.Instance.CurrentTime).TotalSeconds <= 0)
                {
                    dic.Remove(item.Key);
                    item.Value.MailState = eMailState.TimeOver.ToString();
                    item.Value.ReceviedTime = TimeMgr.Instance.CurrentTime.ToString();
                    ReceivedMail.Add(item.Key, item.Value);

                    // 변동된 사항이 하나라도 있을 경우만 저장.
                    isUpdate = true;
                }
            }
        };

        remove(OfficialPostBox);
        remove(PersonalPostBox);

        if(isUpdate)
        {
            LocalSave();
            ObscuredSave();
            DataMgr.Instance.FirebaseSave();
        }
    }

    // TimeMgr이라는 코드에서 1초마다 돌아가는 코드에서 접속 시 계산해놓은 다음 푸쉬보상 받을 시간을
    // 점점 줄여서 0초가 되면 시간이 되었다고 판단. (중간에 다른 앱을 하고 오거나 했을 경우 보정작업 ApplicationPause에서 함)
    void DecreasePerSec()
    {
        _remainingTime_PushReward1 -= 1;
        _remainingTime_PushReward2 -= 1;

        // int기 때문에 과감하게 0으로 입력. float double도 가능하지만 그런경우 < 0를 더 선호.
        if (_remainingTime_PushReward1 == 0)
        {
            DateTime curTime = TimeMgr.Instance.CurrentTime;
            DateTime resetTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0);
            resetTime += PUSH_REWARD1_TIME;

            _remainingTime_PushReward1 = SECOND_PER_DAY;
            LastGetPushReward1_Time = TimeMgr.Instance.CurrentTime;
            AddPersonalMail("P_" + Util.GetMailKey(TimeMgr.Instance.CurrentTime.AddMinutes(5)), GetPushReward1());

            DataMgr.Instance.BothSave();
        }

        if (_remainingTime_PushReward2 == 0)
        {
            DateTime curTime = TimeMgr.Instance.CurrentTime;
            DateTime resetTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0);
            resetTime += PUSH_REWARD2_TIME;

            _remainingTime_PushReward2 = SECOND_PER_DAY;
            LastGetPushReward2_Time = TimeMgr.Instance.CurrentTime;
            AddPersonalMail("P_" + Util.GetMailKey(TimeMgr.Instance.CurrentTime.AddMinutes(5)), GetPushReward2());
            
            DataMgr.Instance.BothSave();
        }
    }

    #region 푸쉬 보상 관련
    // 현재시간 기준 푸쉬보상 시간까지 남은 시간 계산.
    public void CalculatePushRewardTime()
    {
        DateTime curTime = TimeMgr.Instance.CurrentTime;
        DateTime resetTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0);

        resetTime += PUSH_REWARD1_TIME;
        // 2023.01.01 접속 기준. 2023.01.01 12:00:00 에 푸쉬보상을 받을 수 있으며, 현재시간보다 적다는 건 
        // 이미 오늘의 푸쉬시간을 지났기 때문에 다음날의 푸쉬시간을 기다려야 함
        // (푸쉬 보상을 지급하는 건 LoadCompleteEvent에서 했음)
        if ((resetTime - curTime).TotalSeconds < 0)
        {
            resetTime = resetTime.AddDays(1);
        }

        // 남은시간을 초로 계산하여, 1초마다 시간을 체크하는 Time코드에서 체킹함.
        _remainingTime_PushReward1 = (int)(resetTime - curTime).TotalSeconds;

        // 오후 푸쉬보상도 동일.
        resetTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0);
        resetTime += PUSH_REWARD2_TIME;
        if ((resetTime - curTime).TotalSeconds < 0)
        {
            // 다음날 푸쉬보상 시간.
            resetTime = resetTime.AddDays(1);
        }

        _remainingTime_PushReward2 = (int)(resetTime - curTime).TotalSeconds;
    }

    bool CheckTodayPushReward(DateTime lastGetPushReward, TimeSpan rewardTime, MailItem item)
    {
        bool isShouldSave = false;

        DateTime curTime = TimeMgr.Instance.CurrentTime;
        DateTime todayRewardTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0);
        todayRewardTime += rewardTime;

        /* 현재시간이 보상받을 수 있는 시간 FloatTime 범주내에 있으며
         * 현재시간 - 보상시간이 0보다 크다는 건. 보상시간 이후에 접속했으며,
         * 보상시간 - 마지막받은 보상시간이 0보다 크다는 건, 이전날 받았단 의미 */
        if ((curTime - todayRewardTime).TotalSeconds <= PUSH_RREWARD_PERIOD_TIME.TotalSeconds
            && (curTime - todayRewardTime).TotalSeconds >= 0
            && (todayRewardTime - lastGetPushReward).TotalSeconds > 0
            && (curTime - lastGetPushReward).TotalDays > 0) // 1일이상 차이나야.
        {
            AddPersonalMail("P_" + Util.GetMailKey(curTime.AddMinutes(5)), item);
            isShouldSave = true;
        }
        return isShouldSave;
    }
    #endregion

    void CheckNotice()
    {
        NoticeMgr.Instance.Function.SetPostBoxPersonalNotice(PersonalPostBox.Count > 0);
        NoticeMgr.Instance.Function.SetPostBoxOfficialNotice(OfficialPostBox.Count > 0);
    }

    /// <param name="item"></param>
    /// <param name="clickPos">우편함에서 받기버튼을 클릭한 좌표. 해당 좌표에서 이펙트연출</param>
    void ReceiveReward(MailItem item, Vector3 clickPos)
    {
        // Util에 String을 Enum으로 파싱하는 코드. 제네릭을 사용하였고
        // eAsset.Cash 부분을 보면 eAsset 부분이 제네릭변수기 때문에 따로 <eAsset>을 넣어주지 않아도 됨.
        eAsset itemType = Util.StringToEnum(item.AssetType, eAsset.Cash);

        // Util에서 아이템을 지급하면서 화면에 효과가 나타나거나 팝업을 띄워주는 기능. 모든 재화가 가능.
        Util.AddItemShowEffect(itemType, uint.Parse(item.Cnt), clickPos);
    }

    // 점심 보상에 대한 정보
    MailItem GetPushReward1()
    {
        // 종료시각, 주는 아이템의 타입, 개수, 설명키(I2로 로컬라이징 되어있음)
        MailItem item = new MailItem(TimeMgr.Instance.CurrentTime.AddDays(1).ToString()
            , eAsset.AutoToutTicket.ToString()
            , 1.ToString()
            , "LunchDailyDes", eMailState.InPostBox.ToString());
        return item;
    }

    // 위와 같으며 저녁 시간 보상.
    MailItem GetPushReward2()
    {
        MailItem item = new MailItem(TimeMgr.Instance.CurrentTime.AddDays(1).ToString()
            , eAsset.CashRewardTicket_A.ToString()
            , 1.ToString()
            , "EveningDailyDes", eMailState.InPostBox.ToString());
        return item;
    }

    // 게임 시작 시 지급하는 보상.
    public void AddGameStartReward()
    {
        AddPersonalMail("P_" + Util.GetMailKey(TimeMgr.Instance.CurrentTime.AddMinutes(5))
            , new MailItem(TimeMgr.Instance.CurrentTime.AddDays(7).ToString()
            , eAsset.Gem.ToString()
            , 100.ToString()
            , "InstallMsg", eMailState.InPostBox.ToString()));

        AddPersonalMail("P_" + Util.GetMailKey(TimeMgr.Instance.CurrentTime.AddMinutes(6))
            , new MailItem(TimeMgr.Instance.CurrentTime.AddDays(7).ToString()
            , eAsset.AdsSkipTicket.ToString()
            , 1.ToString()
            , "InstallMsg", eMailState.InPostBox.ToString()));

        DataMgr.Instance.BothSave();
    }
}
