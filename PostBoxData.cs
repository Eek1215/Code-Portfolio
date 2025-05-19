using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using CodeStage.AntiCheat.Storage;
using Firebase.Database;
using Firebase.Extensions;
using InfiniteValue;
using Newtonsoft.Json;

/* ������ �ڵ��߿� �������� �����ϴ� ������ �ڵ��Դϴ� 
 * ������ ������ MailItem�̶� Ŭ������ ����Ǹ�, 
 * ����ð�, ����Ÿ��, ���󰹼�, �����Կ� ����� �ؽ�Ʈ Ű��(���ö���¡), ����(������, ����, Ÿ�ӿ���) 
 * �ڵ� ��Ÿ���� public -> private -> IEnumerator ������ ���� ¥�� ���̸�
 * �������� ��� 1ȸ���ۿ� �Ⱦ��̴� �ڵ�� ���������� �ٷ� �Ʒ��� ���� �α⵵ ��.
 * �Ǵ� �� ��ũ��Ʈ�� ���� ��ɵ��� ���� �� ��ɺ��� region���� ���⵵ ��. */
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

/* ������ ���ο���(Ǫ������ or �������� ���� ���ִ� ���)�� ���� ����鸸 ����
 * ���� ������ �Ź� ������ ������ �� ���� �����Կ��� ���� ���� ������߿� ���� �͵��� �о��. */
public class PostBoxData : BaseData, ISubject
{
    // ���� ������ ���ϵ� (����x)
    public Dictionary<string, MailItem> OfficialPostBox { get; private set; }

    // �����ؾ� �ϴ� ���ο����� (����o)
    public Dictionary<string, MailItem> PersonalPostBox { get; private set; }
    // �̹� ������ ���ϵ� (����o)
    public Dictionary<string, MailItem> ReceivedMail { get; private set; }

    // Ǫ������.
    private readonly TimeSpan PUSH_REWARD1_TIME = new TimeSpan(12, 0, 0);
    private readonly TimeSpan PUSH_REWARD2_TIME = new TimeSpan(18, 0, 0);

    // 12�ó� 18�ÿ��� �߰��� ��ð����� �����ص� ���� �� �ִ���.
    private readonly TimeSpan PUSH_RREWARD_PERIOD_TIME = new TimeSpan(5, 0, 0); // 5�ð����� ���� �� �ְ�.

    // ���� �� ���� Ǫ���ð����� ���ʵڿ� ���� �� �ִ���. 
    private int _remainingTime_PushReward1;
    private int _remainingTime_PushReward2;

    // ���������� ���� Ǫ��������� �ð�.
    public DateTime LastGetPushReward1_Time { get; private set; }
    public DateTime LastGetPushReward2_Time { get; private set; }

    // �Ϸ�ð��� �ʷ�.
    private readonly int SECOND_PER_DAY = 86400;

    public PostBoxData()
    {
        // ������ �⺻ �ʱ�ȭ. �ڷᱸ������ �Ҵ�.
        OfficialPostBox = new Dictionary<string, MailItem>();
        ReceivedMail = new Dictionary<string, MailItem>();
        PersonalPostBox = new Dictionary<string, MailItem>();

        LastGetPushReward1_Time = DateTime.MinValue;
        LastGetPushReward2_Time = DateTime.MinValue;
    }

    // ������ȭ�� �ϰ� �ϴ� �ڵ�. Json���� Ŭ������ �ǵ����� ������, ����� ���� �о���� �����
    // ����ϱ� ������ ����� ���� �ʰ� ������ ���� �ʵ��� ������ �ص� �ڵ�.
    [JsonConstructor]
    public PostBoxData(string empty) { }

    public void LocalLoad()
    {
        /* ��⿡ ����Ǿ� �ִ� ������ �����͸� �ҷ����� �װ� ������ȭ�Ͽ� Ǫ�� �ڵ�.
         * �������� �����͸� �ҷ��� ���� LocalLoad ȣ�� ������ ���������͸� ��⿡ ����� �� �ҷ��� */
        PostBoxJsonData data = Newtonsoft.Json.JsonConvert.DeserializeObject<PostBoxJsonData>(
            ObscuredPrefs.Get("PostBoxData", Newtonsoft.Json.JsonConvert.SerializeObject(new PostBoxJsonData(this))));

        if (data != null)
        {
            /* �����Ϳ��� Ư�� List�� Dictionary�� null üũ�ϴ� ������, ������Ʈ�� ���� �߰��Ǵ� ���
             * �������Լ��� �����Ͱ� ������ ���̱� ������ null ������ ���ɴϴ�. 
             * ���� ReferenceEquals(data.ReceivedMail, null)�� �ӵ� ���鿡�� �� ������ �� ������
             * �ʱ�ȭ �� 1ȸ�̰� �ش� Destroy���� �ϴ� ������ ���⿡ �������� ���� == null�� üũ�� �Ͽ���  */
            if (data.ReceivedMail != null)
            {
                foreach (KeyValuePair<string, MailItem> item in data.ReceivedMail)
                {
                    // �ߺ����� ���� �� ����
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

    // �����͸� ������ �� �Ʒ��� GetJsonData�� ���� Ŭ������ Jsonȭ�ؼ� �Ѱ��� ���� �������.
    public override void SetJsonData(string jsonData)
    {
        // ObscuredPrefs�� �����߿� PlayerPrefs�� ��ȣȭ�� �� ���.
        ObscuredPrefs.Set("PostBoxData", jsonData);
    }

    public override string GetJsonData()
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(new PostBoxJsonData(this));
    }

    public void OnLoadCompleteEvent()
    {
        // ���ο������� ��� �ҷ��� �� Ǫ���˸� ���θ� üũ.
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

            // ���� Focus�� �����ٰ� ���ƿ��ų� �ð��� �귶�� �� Ǫ�������� üũ.
            TimeMgr.Instance.OnBreakApplicationUnpause += CalculatePushRewardTime;
            TimeMgr.Instance.DecreaseTimePerSec += DecreasePerSec;

            DataMgr.Instance.BothSave();
        });

        CheckNotice();
    }

    #region �񵿱� ������ Load
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

    // �������� ���빰�� ���� ���.
    // ������� ����. O_20250319_162535 = ����Ÿ��_���ᳯ¥_����Ǵ½ð�
    // �ð��� Ű�� �������Ͽ�����, �� ������ ������ �� ���� �������� ������ ������ ����.
    // �� ���� �������� �����ٸ� �ð��ڿ� �߰��� �ε����� ����Ű�� �ο��ϴ� ��� ä��.
    public void ReceiveMail(string key, MailItem item, Vector3 clickPos)
    {
        bool shouldSave = false;
        string[] keySplit = key.Split('_');
        // ���ڸ��� P�� ���ο���, O�� ���� ����.
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
            // �������� ���� �߼��Ͽ� �־��ذ� ���� O_���� ���������� ���� PersonalPostBox�� ����.
            // ��ü�������� �߼��� �� OfficialPostBox�� ��.
            else if (PersonalPostBox.ContainsKey(key))
            {
                PersonalPostBox.Remove(key);
                shouldSave = true;
            }
        }

        if (shouldSave)
        {
            // ���� �ð� �����Է� �� ���� �����Կ� �߰� and ����.
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

    // ���� ������� �����ð��� �ʰ��ǹ����� �������� �ϴ���.
    public void CheckReceiveTime()
    {
        bool isUpdate = false;
        // �������� �Ű������� �޾� ������ ���� ��� ������� �ð��� �����ϴ� ���ٽ�.
        Action<Dictionary<string, MailItem>> remove = dic =>
        {
            foreach (KeyValuePair<string, MailItem> item in dic.ToList())
            {
                // ���� �� �ִ� �ð��� ���������� �ش� PostBox���� �����ְ� TimeOver��� ó�� �� �������������� ����.
                if ((DateTime.Parse(item.Value.EndTime) - TimeMgr.Instance.CurrentTime).TotalSeconds <= 0)
                {
                    dic.Remove(item.Key);
                    item.Value.MailState = eMailState.TimeOver.ToString();
                    item.Value.ReceviedTime = TimeMgr.Instance.CurrentTime.ToString();
                    ReceivedMail.Add(item.Key, item.Value);

                    // ������ ������ �ϳ��� ���� ��츸 ����.
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

    // TimeMgr�̶�� �ڵ忡�� 1�ʸ��� ���ư��� �ڵ忡�� ���� �� ����س��� ���� Ǫ������ ���� �ð���
    // ���� �ٿ��� 0�ʰ� �Ǹ� �ð��� �Ǿ��ٰ� �Ǵ�. (�߰��� �ٸ� ���� �ϰ� ���ų� ���� ��� �����۾� ApplicationPause���� ��)
    void DecreasePerSec()
    {
        _remainingTime_PushReward1 -= 1;
        _remainingTime_PushReward2 -= 1;

        // int�� ������ �����ϰ� 0���� �Է�. float double�� ���������� �׷���� < 0�� �� ��ȣ.
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

    #region Ǫ�� ���� ����
    // ����ð� ���� Ǫ������ �ð����� ���� �ð� ���.
    public void CalculatePushRewardTime()
    {
        DateTime curTime = TimeMgr.Instance.CurrentTime;
        DateTime resetTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0);

        resetTime += PUSH_REWARD1_TIME;
        // 2023.01.01 ���� ����. 2023.01.01 12:00:00 �� Ǫ�������� ���� �� ������, ����ð����� ���ٴ� �� 
        // �̹� ������ Ǫ���ð��� ������ ������ �������� Ǫ���ð��� ��ٷ��� ��
        // (Ǫ�� ������ �����ϴ� �� LoadCompleteEvent���� ����)
        if ((resetTime - curTime).TotalSeconds < 0)
        {
            resetTime = resetTime.AddDays(1);
        }

        // �����ð��� �ʷ� ����Ͽ�, 1�ʸ��� �ð��� üũ�ϴ� Time�ڵ忡�� üŷ��.
        _remainingTime_PushReward1 = (int)(resetTime - curTime).TotalSeconds;

        // ���� Ǫ������ ����.
        resetTime = new DateTime(curTime.Year, curTime.Month, curTime.Day, 0, 0, 0);
        resetTime += PUSH_REWARD2_TIME;
        if ((resetTime - curTime).TotalSeconds < 0)
        {
            // ������ Ǫ������ �ð�.
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

        /* ����ð��� ������� �� �ִ� �ð� FloatTime ���ֳ��� ������
         * ����ð� - ����ð��� 0���� ũ�ٴ� ��. ����ð� ���Ŀ� ����������,
         * ����ð� - ���������� ����ð��� 0���� ũ�ٴ� ��, ������ �޾Ҵ� �ǹ� */
        if ((curTime - todayRewardTime).TotalSeconds <= PUSH_RREWARD_PERIOD_TIME.TotalSeconds
            && (curTime - todayRewardTime).TotalSeconds >= 0
            && (todayRewardTime - lastGetPushReward).TotalSeconds > 0
            && (curTime - lastGetPushReward).TotalDays > 0) // 1���̻� ���̳���.
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
    /// <param name="clickPos">�����Կ��� �ޱ��ư�� Ŭ���� ��ǥ. �ش� ��ǥ���� ����Ʈ����</param>
    void ReceiveReward(MailItem item, Vector3 clickPos)
    {
        // Util�� String�� Enum���� �Ľ��ϴ� �ڵ�. ���׸��� ����Ͽ���
        // eAsset.Cash �κ��� ���� eAsset �κ��� ���׸������� ������ ���� <eAsset>�� �־����� �ʾƵ� ��.
        eAsset itemType = Util.StringToEnum(item.AssetType, eAsset.Cash);

        // Util���� �������� �����ϸ鼭 ȭ�鿡 ȿ���� ��Ÿ���ų� �˾��� ����ִ� ���. ��� ��ȭ�� ����.
        Util.AddItemShowEffect(itemType, uint.Parse(item.Cnt), clickPos);
    }

    // ���� ���� ���� ����
    MailItem GetPushReward1()
    {
        // ����ð�, �ִ� �������� Ÿ��, ����, ����Ű(I2�� ���ö���¡ �Ǿ�����)
        MailItem item = new MailItem(TimeMgr.Instance.CurrentTime.AddDays(1).ToString()
            , eAsset.AutoToutTicket.ToString()
            , 1.ToString()
            , "LunchDailyDes", eMailState.InPostBox.ToString());
        return item;
    }

    // ���� ������ ���� �ð� ����.
    MailItem GetPushReward2()
    {
        MailItem item = new MailItem(TimeMgr.Instance.CurrentTime.AddDays(1).ToString()
            , eAsset.CashRewardTicket_A.ToString()
            , 1.ToString()
            , "EveningDailyDes", eMailState.InPostBox.ToString());
        return item;
    }

    // ���� ���� �� �����ϴ� ����.
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
