using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Globalization;

/* 게임의 전체적인 첫 프로세스를 관리하는 코드입니다.
 * 모바일의 경우 LoadScene에서 로그인에 관련 된 코드들과 상호작용을 하고
 * 화면에 보여지는 정보와 데이터 로드 순서를 결정짓습니다.
 * 싱글턴 패턴에서 오브젝트가 Scene을 바뀌어도 파괴되지 않도록 DontDestroySingleton라는
 * 클래스를 상속합니다. */
public class GameMgr : DontDestroySingleton<GameMgr>
{
    private readonly string DATABASE_URL = "https://주소는_포트폴리오기_때문에_가리겠습니다/";
    private bool _bPriorInitComplete = false;

    public bool IsAgreeTermsAndCondition;
    public bool IsAgreePushNotice;

    public bool IsInitFinish { get; private set; }

    public delegate void OnEvent();
    public event OnEvent OnChangeLanguageEvent;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    public void LoadSceneInit()
    {
        IsInitFinish = false;

        // 로그인을 위한 밑작업(모바일)
        AccountMgr.Instance.Init();
        AdsMgr.Instance.Init();
#if UNITY_ANDROID || UNITY_IOS
        SetEnvironment();
        PriorInit();
#endif
    }

    public override void Init() { }
    public void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1)
        {
            _bPriorInitComplete = false;

            PopupMgr.Instance.SceneLoaded();
            TimeMgr.Instance.SceneLoaded();
            AdsMgr.Instance.SceneLoaded();

            UIMgr.Instance.Main.SetScreenBlind(true);

#if UNITY_EDITOR
            // 로그인을 위한 밑작업(에디터용)
            AccountMgr.Instance.Init();
            AdsMgr.Instance.Init();
            SetEnvironment();
            PriorInit();
#endif
            StartCoroutine(MainSceneInitProcess());
        }
    }

    void PriorInit()
    {
        TimeMgr.Instance.Init();
        StartCoroutine(WaitTimeMgrInitProcess());
    }

    public void PlayLoginProcess()
    {
        // 데이터의 기본값을 설정. 그 후 게임에서 요구하는 권한들을 미리 불러오기.
        DataMgr.Instance.Init();
        DataMgr.Instance.LoadTermFromLocalData();

        /* CompareCloudData는 클라우드에 데이터가 있는데 기기에 데이터가 없는지 확인.
         * 새로운 기기등으로 깔았을 때 클라우드 데이터를 기기에다 덮어쓰는 게 필요한지 작업.
         * 체크 후 모바일의 경우 로딩신에서 버전이 최신업데이트가 필요한지 한다. */
        Action checkVersion = () =>
        {
            if (LoadScene.Instance != null)
            {
                LoadScene.Instance.CheckUpdate();
            }

            _bPriorInitComplete = true;
        };

        // 네트워크 연결 여부 필요(시간조작을 막기 위함)
        if (GetNetworkConnect())
        {
            DataMgr.Instance.CompareCloudData(() =>
            {                
                checkVersion();
            });
        }
        // 이미 버전을 로드한 상태인데 버전 로드와 해당 코드 사이에서 인터넷이 끊긴 경우를 위해 버전체크를 한다.
        else
        {
            checkVersion();
        }
    }

    IEnumerator WaitTimeMgrInitProcess()
    {
        WaitForSeconds loadDelay = new WaitForSeconds(0.1f);
        // MainProcess를 실행하기 전 현재시간이 초기화되기를 기다리는 중.
        while (TimeMgr.Instance.CurrentTime == DateTime.MinValue)
        {
#if UNITY_EDITOR
            Debug.Log("Time loading...");
#endif
            yield return loadDelay;
        }
        
        // 로그인을 이미 한적이 있다면 그냥 로그인을 실행
        if (PlayerPrefs.GetInt("AuthType", -1) == 1)
        {
            // 로그인 경험이 없었거나 구글 또는 게임센터로 로그인했던 사람은
            // 구글 또는 게임센터로 로그인을 한다.
            if (LoadScene.Instance != null)
            {
                LoadScene.Instance.HideLoginBtn();
            }

            AccountMgr.Instance.Login(() =>
            {
                PlayLoginProcess();
            });
        }
        // 익명으로 플레이하던 경우는 익명으로 로그인을 시킨다.
        else if (PlayerPrefs.GetInt("AuthType", -1) == 0)
        {
            if (LoadScene.Instance != null)
            {
                LoadScene.Instance.HideLoginBtn();
            }

            AccountMgr.Instance.AnonymousLogin(() =>
            {
                PlayLoginProcess();
            });
        }
        // 이 경우는 한번도 로그인을 안한 경우.
        else if(PlayerPrefs.GetInt("AuthType", -1) == -1)
        {
#if UNITY_EDITOR
            AccountMgr.Instance.Login(() =>
            {
                PlayLoginProcess();
            });
#else
            LoadScene.Instance.ShowLoginBtn();
#endif
        }
    }

    IEnumerator MainSceneInitProcess()
    {
        ResolutionMgr.Instance.Init(() => CameraMgr.Instance.Init());
#if UNITY_EDITOR
        // 모바일은 로그인 뒤에 해당 코드가 들어오지만, 에디터는 게임신에서 시작할 수 있도록
        // 초기화와 로드를 동시에 시작하고 초기화가 되면 프로세스를 시작.
        while(!_bPriorInitComplete)
        {
            yield return null;
        }
#else
        yield return null;
#endif
        ConvertUnit.Init();
        InfoTableMgr.Instance.Init();
        SpriteMgr.Instance.Init();
        ParticleMgr.Instance.Init();

        DataMgr dataMgr = DataMgr.Instance;
        dataMgr.LocalLoad();

        // 순서 변경 X
        TycoonMgr.Instance.Init();
        UIMgr.Instance.Init();
        NoticeMgr.Instance.Init();
        SoundMgr.Instance.Init();
        PopupMgr.Instance.Init();
        LightMgr.Instance.Init();
        ConversationMgr.Instance.Init();
        FirebaseTriggerMgr.Instance.Init();
        IAPMgr.Instance.Init();
        AppsflyerMgr.Instance.Init();
        FacebookMgr.Instance.Init();

        // 서버의 데이터가 있는 경우
        if (DataMgr.Instance.CloudData_Auth != null && DataMgr.Instance.Auth != null
            && DataMgr.Instance.CloudData_InGame != null)
        {
            // 서버의 데이터가 안전하게 있는 경우(테스트 도중 불안정하게 데이터가 저장되었을 때를 주로 대비)
            if (DataMgr.Instance.CloudData_Auth.TotalPlayTime != null)
            {
                // 기기데이터와 서버데이터가 2분이상 차이가 날 경우 둘의 데이터가 다르며
                // 한쪽 데이터를 선택하는 팝업이 뜨는지 결정.
                if (Mathf.Abs((float)((TimeSpan.Parse(DataMgr.Instance.CloudData_Auth.TotalPlayTime) -
                        DataMgr.Instance.Auth.TotalPlayTime).TotalMinutes)) > 2)
                {
                    UIMgr.Instance.Main.SetScreenBlind(false);
                    UIMgr.Instance.Popups.ShowCollisionDataPopup(OnLoadCompleteEvent);
                }
                // 기기데이터와 서버데이터가 같다면 바로 게임내부를 초기화하는 프로세스 가동
                else
                {
                    OnLoadCompleteEvent();
                }
            }
            // 서버의 데이터가 불안정한 경우.
            else
            {
                OnLoadCompleteEvent();
            }
        }
        // 서버의 데이터가 없는 경우 바로 시작이 가능.
        else
        {
            OnLoadCompleteEvent();
        }
    }

    void OnLoadCompleteEvent()
    {
        // 전반적으로 다른 코드에 관여를 안하는 것들을 위로.
        DataMgr dataMgr = DataMgr.Instance;
        dataMgr.OnLoadCompleteEvent();

        // 일일초기화가 되는 시간 계산 및 현재시간과 날짜가 일일초기화가 필요한지 판단.
        TimeMgr.Instance.CalculateDailyResetTime();
        TimeMgr.Instance.CheckDailyResetOnStart();

        // 현재시간을 조작하는지 시간을 갱신하는 프로세스 동작.
        TimeMgr.Instance.StartTimeCorrection();

        CameraMgr.Instance.OnLoadCompleteEvent();

        // 출석여부에 따라 출석 팝업 오픈. 튜토리얼중에는 X
        if (!dataMgr.Additional.Attendance.IsTodayAttendance
            && dataMgr.InGame.IsTutorialClear)
        {
            UIMgr.Instance.Popups.ShowAttendancePopup();
        }

        // 특정 상품 팝업을 띄웠으나 효율이 떨어져 주석으로 가려둔 상태.
        //if (!dataMgr.Pay.IsTodayShowBeginningPackage
        //    && !dataMgr.Pay.PurchaseStatus_Beginning
        //    && dataMgr.InGame.Sites[0].BuiltFloorNum >= 3)
        //{
        //    IAPMgr.Instance.AddEventAfterInit(() =>
        //    {
        //        UIMgr.Instance.Popups.ShowBeginningPackagePopup();
        //        dataMgr.Pay.SetShowedBeginningPackage();
        //    });
        //}

        // 특정 상품구매 조건이 가능하면 띄우는 팝업.
        if (!dataMgr.Pay.IsTodayShowBabyProductsPopup
            && dataMgr.Pay.IsAnyBabyProductPurchaseAble
            && dataMgr.InGame.Facility.PrepIngredientsGame.IsBuilt)
        {
            // 결제 프로세스의 초기화가 끝난 뒤 동작.(결제 금액 등 정보 초기화가 우선시 되기 때문)
            IAPMgr.Instance.AddEventAfterInit(() =>
            {
                UIMgr.Instance.Popups.ShowBabyProductsPopup();
                dataMgr.Pay.SetShowedBabyProductsPopup();
            });
        }

        // 오프라인 보상이 열려야 하는지 확인.
        dataMgr.Additional.OfflineReward.CheckOfflineReward();

        UIMgr.Instance.OnLoadCompleteEvent();
        ConversationMgr.Instance.OnLoadCompleteEvent();
        TycoonMgr.Instance.OnLoadCompleteEvent();
        InfoTableMgr.Instance.OnLoadCompleteEvent();
        NoticeMgr.Instance.OnLoadCompleteEvent();

        UIMgr.Instance.Main.SetScreenBlind(false);

        // 해당유저가 서버에서 벤 처리를 했다면 플레이하지 못하도록 팝업을 띄움.
        if (dataMgr.Auth.IsBannedAccount)
        {
            UIMgr.Instance.Popups.ShowBannedAccountPopup();
        }

        IsInitFinish = true;
    }

    void SetEnvironment()
    {
        SetFrame(PlayerPrefs.GetInt("Frame", 60));
        
        // format을 일반화함 이 코드 전엔 2023-06-08 PM 1:50:51 -> 06/08/2023 13:51:13
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Firebase.FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(DATABASE_URL);
    }


    public void SetFrame(int frame)
    {
        Application.targetFrameRate = frame;
        PlayerPrefs.SetInt("Frame", frame);
    }

    public NetworkReachability GetNetworkState()
    {
        return Application.internetReachability;
    }

    public bool GetNetworkConnect()
    {
        NetworkReachability state = GetNetworkState();
        return state == NetworkReachability.ReachableViaCarrierDataNetwork
            || state == NetworkReachability.ReachableViaLocalAreaNetwork;
    }

    public void OnChangedLanguage()
    {
        OnChangeLanguageEvent?.Invoke();
    }
}
