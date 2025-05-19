using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/* 현재 일반손님을 부를 수 있는지 판단.
 * Success = 부를 수 있음, TooMuch = 손님이 너무 많아서 불가, 
 * NotBuild = 건물을 짓지 않아 불가능 (튜토리얼 이전) */
public enum eCustomerSpawnResult
{
    Success,
    TooMuch,
    NotBuilt,
}

public class CustomerController : MonoBehaviour
{
    /* 캡슐화 문제로 외부 스크립트에서 접근을 하지 않아야 하지만, Inspector에서 할당을 해야 하는 경우는
     * 직렬화를 통하여 접근, 그 외에는 public을 유니티에서 성능문제로 권장하기에 되도록 public 사용 */
    [SerializeField] private BaseCustomer[] _baseCustomerPrefab;
    [SerializeField] private Dictionary<eCustomer, ObjectPool<BaseCustomer>> _customerPool;

    // 일반 손님이 아닌 각종 특수 손님들.
    public Guest_SnakesLadders guest_snakesLadders;
    public Guest_CardGameA guest_cardGameA;
    public Guest_CardGameB guest_cardGameB;
    public Guest_WhackAMole guest_whackAMole;
    public Guest_AdsPig guest_adsPig;
    public Guest_Disrupter guest_disrupter;
    public Guest_Guide guest_guide;
    public Guest_ToutCat guest_toutCat;

    // 타운내에 대기중인 손님 또는 입장한 손님들
    [SerializeField] private List<BaseCustomer> _inTownCustomers;
    // WaitingCustomers의 경우 LinkedList를 사용할 수도 있을거라 판단.
    [SerializeField] private List<BaseCustomer> _waitingCustomers;

    // 타운안에 이용손님이 최대인 경우(대기손님x 순수 이용손님)
    public bool IsInTownFull
    {
        get { return _inTownCustomers.Count >= _maxEnterCustomerCnt; }
    }

    // 현재 최대 들어올 수 있는 손님의 수
    [SerializeField] private int _maxEnterCustomerCnt;
    // 활성화 된 손님의 수
    [SerializeField] private int _activeCustomerCnt;

    // 상위 부모의 스크립트
    private Main _main;
    public Main Main
    {
        get { return _main; }
    }

    public InGameData _inGameData;
    private WealthData _wealthData;

    private Coroutine cor_autoSpawnCustomerProcess;
    private Coroutine cor_spawnGuestProcess;
    private Coroutine cor_townLevelAutoSpawnCustomerProcess;

    // 기다리는 손님이 생겼을 때
    public delegate void OnEvent();
    public event OnEvent OnWaitCustomer;

    // 2인이상 동시입장 손님이 생겼을 때. 퀘스트의 용도
    public delegate void OnUpdateValue(long progressCnt);
    public event OnUpdateValue OnToutChatterCustomer;

    // 마지막으로 스폰된 특수손님. (중복되서 나오지 않도록)
    private eGuest _lastSpawnGuestType;

    private CustomersData _customersData;

    public readonly float DEFAULT_MOVE_SPEED = 2f;
    public float MoveSpeed;

    // 특별퀘스트를 현재 받아도 되는지
    public bool _bCanRequesting;  
    // 특별퀘스트를 갖고 있는 손님의 수 (특별퀘스트는 최대 4마리까지만 동시에 나올 수 있음)
    public int _requestingQuestCustomerCnt;
    // 현재 특별퀘스틀 가진 손님이 최대인지
    public bool IsMaxRequestingCustomer
    {
        get { return _requestingQuestCustomerCnt + _inGameData.Facility.SpecialQuestBoard.GetCurrentQuestCnt() >= 4; }
    }

    // 그룹 의뢰를 현재 받아도 되는지
    public bool _bCanReservation;     

    // 사진찍기 기능을 가진 손님을 담아두기
    private BaseCustomer _customerOnScreenCapture;
    public bool HasScreenCaptureBalloonCustomer
    {
        get { return !ReferenceEquals(_customerOnScreenCapture, null); }
    }

    public void Init(Main stage)
    {
        _main = stage;
        _inGameData = DataMgr.Instance.InGame;
        _customersData = _inGameData.Customers;
        _wealthData = DataMgr.Instance.Wealth;

        _inTownCustomers = new List<BaseCustomer>();
        _waitingCustomers = new List<BaseCustomer>();

        _customerPool = new Dictionary<eCustomer, ObjectPool<BaseCustomer>>();

        _activeCustomerCnt = 0;
        _requestingQuestCustomerCnt = 0;

        OnWaitCustomer = null;
        OnToutChatterCustomer = null;

        MoveSpeed = DEFAULT_MOVE_SPEED;
    }

    public void OnLoadCompleteEvent()
    {
        StartCoroutine(GameStartBonusSpawn());
    }
    
    // 손님의 종류에 따라 ObjectPool을 생성     
    void CreatePool(eCustomer type)
    {
        _customerPool.Add(type, new ObjectPool<BaseCustomer>(
            () =>
            {
                return CreateCustomer(type);
            },
            customer =>
            {
                customer.gameObject.SetActive(true);
            },
            customer =>
            {
                customer.gameObject.SetActive(false);
            },
            customer =>
            {
                Destroy(customer.gameObject);
            }));
    }

    BaseCustomer CreateCustomer(eCustomer type)
    {
        //Debug.Log(type.ToString());
        BaseCustomer customer = Instantiate(_baseCustomerPrefab[(int)type], transform);
        customer.SetPool(_customerPool[type]);
        return customer;

    }

    public void UpdateMaxEnterCustomerCnt()
    {
        _maxEnterCustomerCnt = _inGameData.MaxEnterCustomerCnt;
    }

    // 자동으로 손님이 스폰되는 프로세스 가동.
    public void PlayCustomerSpawnProcess()
    {
        if (cor_autoSpawnCustomerProcess == null)
        {
            cor_autoSpawnCustomerProcess = StartCoroutine(AutoSpawnCustomerProcess());
            PlayGuestSpawnProcess(120f);
        }

        if (cor_townLevelAutoSpawnCustomerProcess == null)
        {
            cor_townLevelAutoSpawnCustomerProcess = StartCoroutine(TownLevelAutoSpawnCustomerProcess());
        }
    }

    // 자동으로 특수손님이 스폰되는 프로세스 가동.
    public void PlayGuestSpawnProcess(float delay = 0, bool bRightNow = false)
    {
        if (cor_spawnGuestProcess != null)
        {
            StopCoroutine(cor_spawnGuestProcess);
        }

        if (gameObject.activeInHierarchy)
        {
            cor_spawnGuestProcess = StartCoroutine(SpawnGuestProcess(delay, bRightNow));
        }
    }

    // 손님을 생성가능한지 여부판단.
    public eCustomerSpawnResult GetSpawnableCustomerState()
    {
        if (!_inGameData.Sites[0].Floors[0].IsStoreNotNone)
        {
            return eCustomerSpawnResult.NotBuilt;
        }

        // 손님홍보의 이용제한 수
        if (_activeCustomerCnt >= _maxEnterCustomerCnt + 30)//_queueSignData.WaitableCustomerCnt)
        {
            return eCustomerSpawnResult.TooMuch;
        }

        return eCustomerSpawnResult.Success;
    }

    public BaseCustomer SpawnCustomer(bool isAuto)
    {
        // 자동홍보나 자동생성으로 나왔을 때.
        if (isAuto)
        {
            if (!_customersData.IsAnyCustomerSpawnable || GetSpawnableCustomerState() == eCustomerSpawnResult.TooMuch)
            {
                // 맨처음에 아무 손님도 등장한 적이 없고 오토스폰일 때 토끼가 등장하면 안되기 때문
                return null;
            }
        }
        else
        {
            // 낮은 확률로 일반손님처럼 나올 수 있는 게스트가 나감.
            if (_customersData.GetGuest(eGuest.GuestGuide).IsSpawnAble
            && Random.Range(0, 100f) <= 2f && !guest_guide.gameObject.activeSelf)
            {
                guest_guide.gameObject.SetActive(true);
                guest_guide.SetCreatedInfo(_customersData.GetGuest(eGuest.GuestGuide), GetTownBottom()
                    , new Vector3(_main.GetTownEntranceMiddle(), _main.trs_townEntranceLeftTop.position.y), this);

                return null;
            }
        }
                
        // Auto = true 면 자동으로는 새로운 손님을 꺼내진 않는다.
        CustomerData random = _customersData.GetRandomOpendCustomer(isAuto);
        if (!_customerPool.ContainsKey(random.Info.Type))
        {
            CreatePool(random.Info.Type);
        }

        BaseCustomer customer = _customerPool[random.Info.Type].Get();

        customer.SetCreatedInfo(random, GetTownBottom(), GetTownTop(), _main, this);

        _activeCustomerCnt++;

        return customer;
    }

    public void SpawnChatterCustomer(bool isAuto, int cnt)
    {
        /* 여러 마리가 동시에 나올 때 겹치지 않도록 같은 세로선상에서 가로로 조금씩 떨어트려 나오도록
         * 1.6~2.2이 정중앙일때 2마리기준. (cnt - 1)은 첫 기준점의 가로좌표. 거기서부터 xPos를 더해서 떨어트림 */
        float xPos = _main.GetTownEntranceMiddle(Random.Range(1.6f, 2.2f) * (cnt - 1));
        for (int i = 0; i < cnt; i++)
        {
            CustomerData random = _customersData.GetRandomOpendCustomer(isAuto);
            if (!_customerPool.ContainsKey(random.Info.Type))
            {
                CreatePool(random.Info.Type);
            }

            BaseCustomer customer = _customerPool[random.Info.Type].Get();

            Vector3 bottom = new Vector3(xPos + (i * 1.2f), _main.trs_townEntranceRightBottom.position.y, 0);
            Vector3 top = new Vector3(xPos + (i * 1.2f), _main.trs_townEntranceLeftTop.position.y, 0);
            customer.SetCreatedInfo(random, bottom, top, _main, this);
            
            if (i == 0)
            {
                _main.ChatterObjPool.SetObjToCustomer(customer.trs_posHeart, cnt, false);
                customer.SetRight();
            }

            if(i == cnt - 1)
            {
                _main.ChatterObjPool.SetObjToCustomer(customer.trs_posHeart, cnt, true);
                customer.SetLeft();
            }

            _activeCustomerCnt++;
        }

        OnToutChatterCustomer?.Invoke(1);
    }

    public BaseCustomer SpawnCustomerByGuide(Vector3 guidePos)
    {
        CustomerData random = _customersData.GetRandomOpendCustomer(true);
        if (!_customerPool.ContainsKey(random.Info.Type))
        {
            CreatePool(random.Info.Type);
        }

        BaseCustomer customer = _customerPool[random.Info.Type].Get();

        Vector3 addPos = new Vector3(Random.Range(-1f, 1f), Random.Range(0, -1.5f), 0);
        customer.SetCreatedInfo(random, GetTownBottom(), guidePos + addPos, _main, this);

        _activeCustomerCnt++;

        return customer;
    }

    /// <param name="customer">예약시스템을 받은 동물로 필수로 꺼내기.</param>
    /// <returns></returns>
    public void PlaySpawnCustomerByGroupReservation(eCustomer customerType, int customerCnt)
    {
        CustomerData random = _customersData.GetCustomer(customerType);
        if (!_customerPool.ContainsKey(random.Info.Type))
        {
            CreatePool(random.Info.Type);
        }

        BaseCustomer customer = _customerPool[random.Info.Type].Get();
        customer.SetCreatedInfo(random, GetTownBottom(), GetTownTop(), _main, this);
        customer.ShowBalloon(eCustomerBalloon.ComeGroupReservation, 4f);

        _activeCustomerCnt++;

        StartCoroutine(GroupReservationSpawnCustomerProcess(customerCnt));
    }

    void SpawnGuest()
    {
        List<eGuest> list = new List<eGuest>(_customersData.OpendGuestCustomers);
        if (list.Count >= 2 && list.Contains(_lastSpawnGuestType))
        {
            list.Remove(_lastSpawnGuestType);
        }

        // Guide는 일반 홍보에서 나옴
        if (list.Contains(eGuest.GuestGuide))
        {
            list.Remove(eGuest.GuestGuide);
        }

        Guest guest = null;
        eGuest ranType = list[Random.Range(0, list.Count)];
        switch (ranType)
        {
            case eGuest.GuestDisrupter:
                guest = guest_disrupter;
                break;
            case eGuest.GuestSnakesLadders:
                guest = guest_snakesLadders;
                break;
            case eGuest.GuestCardGameA:
                guest = guest_cardGameA;
                break;
            case eGuest.GuestAdsPig:
                guest = guest_adsPig;
                break;
            case eGuest.GuestCardGameB:
                guest = guest_cardGameB;
                break;
            case eGuest.GuestWhackAMole:
                guest = guest_whackAMole;
                break;
            case eGuest.GuestToutCat:
                guest = guest_toutCat;
                break;

        }

        _lastSpawnGuestType = ranType;

        guest.gameObject.SetActive(true);
        guest.SetCreatedInfo(_customersData.GetGuest(ranType), GetTownBottom(), GetTownTop(), this);
    }

    public void WaitCustomer(BaseCustomer customer)
    {
        _waitingCustomers.Add(customer);
        OnWaitCustomer?.Invoke();
    }

    public void EnterCustomer(BaseCustomer customer)
    {
        _inTownCustomers.Add(customer);
    }

    // 타운에 빈자리가 생겼다고 알림을 받는 것
    public void HaveVacant(BaseCustomer customer)
    {
        _activeCustomerCnt--;
        _inTownCustomers.Remove(customer);
        AdmitWaitingCustomer();
    }

    // 기다리던 손님의 입장을 허락.
    public void AdmitWaitingCustomer()
    {
        if (_waitingCustomers.Count > 0)
        {
            BaseCustomer customer = _waitingCustomers[0];
            _waitingCustomers.RemoveAt(0);
            customer.CheckEnterEvent();
        }
    }

    public void RemoveCustomer(BaseCustomer customer)
    {        
        customer.gameObject.SetActive(false);
    }

    // 손님이 퀘스트를 가질 수 있도록 플래그에 변화.
    public void PermitRequest()
    {
        _bCanRequesting = true;
    }

    // 손님이 퀘스트를 가졌다면 이후로는 가질 수 없기에 바로 변환.
    public void OnRequesting()
    {
        _bCanRequesting = false;
        _requestingQuestCustomerCnt++;
    }

    public void PermitReservation(float delay = 0)
    {
        if (DataMgr.Instance.InGame.Sites[0].BuiltFloorNum >= 3)
        {
            StartCoroutine(PermitReservationAfterDelay(delay));
        }
    }

    public void OnReservation()
    {
        _bCanReservation = false;
    }

    public void OffRequesting()
    {
        _requestingQuestCustomerCnt--;
    }

    public void SetMoveSpeed()
    {
        MoveSpeed = DEFAULT_MOVE_SPEED
            + 1 * (1 + (float)(DataMgr.Instance.Additional.AdsBuff.CustomerMoveSpeedBuff) * 0.01f) - 1;

        for (int i = 0; i < _inTownCustomers.Count; i++)
        {
            _inTownCustomers[i].SetMoveSpeed();
        }

        for (int i = 0; i < _waitingCustomers.Count; i++)
        {
            _waitingCustomers[i].SetMoveSpeed();
        }
    }

    /* 타운안에 들어와있는 손님들 중에 말풍선이 없는 손님에게
     * 스크린샷 기능 말풍선을 부여. 미리 할당을 해서 차후에 제거시에도 용이 */
    public bool ShowRandomScreenCaptureBalloon()
    {
        if (_inTownCustomers.Count == 0)
            return false;

        for (int i = 0; i < _inTownCustomers.Count; i++)
        {
            if (!_inTownCustomers[i].Balloon.gameObject.activeSelf)
            {
                BaseCustomer customer = _inTownCustomers[i];
                customer.ShowBalloon(eCustomerBalloon.Camera, 60f, ()=> 
                {
                    customer.RestartCameraProcess();
                });
                _customerOnScreenCapture = customer;
                TycoonMgr.Instance.Main.StaffCtrl._autoScreenCaptureStaff.PlayWorkProcess();
                return true;
            }
        }
        return false;
    }

    public void SetScreenCaptureCustomerNull()
    {
        _customerOnScreenCapture = null;
    }

    // 레서판다라는 직원에 의해 자동으로 스크린샷이 찍힐 경우.
    public void ScreenCaptureWithLesserPanda()
    {
        if(HasScreenCaptureBalloonCustomer)
        {
            // 이 곳외에 전처리기가 필요하지 않아서 위에서 선언하지 않음.
            InfiniteValue.InfVal reward = UIMgr.Instance.Main._screenCapture.GetRewardValue();
            UIMgr.Instance.Main._goUpWealthEffect.ShowEffectToCash(5, 
                _customerOnScreenCapture._uiBalloonItem.transform.position, reward);
            _customerOnScreenCapture.OnScreenCaptureEvent();
        }
    }

    Vector3 GetTownBottom()
    {
        return new Vector3(_main.GetTownEntranceRangeX() 
                    , _main.trs_townEntranceRightBottom.position.y, 0);
    }

    Vector3 GetTownTop()
    {
        float middleY = (_main.trs_townEntranceLeftTop.position.y + _main.trs_townEntranceRightBottom.position.y)
            / 2;
        return new Vector3(_main.GetTownEntranceRangeX()
            , Random.Range(middleY, _main.trs_townEntranceLeftTop.position.y), 0);
    }

    IEnumerator GameStartBonusSpawn()
    {
        if (_inGameData.Sites[0].BuiltFloorNum >= 2)
        {
            WaitForSeconds delay = new WaitForSeconds(2f);
            for (int i = 0; i < 20; i++)
            {
                SpawnCustomer(true);
                yield return delay;
            }
        }
    }

    IEnumerator AutoSpawnCustomerProcess()
    {
        while(true)
        {
            int cnt = 1;
            float ran = Random.Range(0, 100f);
            if (_inGameData.Sites[0].BuiltFloorNum >= 4 && ran >= 60f)
            {
                cnt = 2;
            }
            else if(_inGameData.Sites[1].BuiltFloorNum >= 3 && ran >= 90f)
            {
                cnt = 3;
            }

            for (int i = 0; i < cnt; i++)
            {
                SpawnCustomer(true);
                yield return new WaitForSeconds(0.2f);
            }
            
            yield return new WaitForSeconds(_customersData.GetAutoToutDelay());
        }
    }

    IEnumerator TownLevelAutoSpawnCustomerProcess()
    {
        WaitForSeconds delay = new WaitForSeconds(1.5f);
        yield return new WaitForSeconds(5f);
        while (true)
        {
            if (_wealthData.TownLevel >= 2)
            {
                for (int i = 0; i < _wealthData.TownLevelAutoToutCustomerCnt; i++)
                {
                    SpawnCustomer(true);
                    yield return delay;
                }
            }
            yield return new WaitForSeconds(_wealthData.TownLevelAutoToutDelay);
        }
    }

    IEnumerator GroupReservationSpawnCustomerProcess(int cnt)
    {
        WaitForSeconds delay = new WaitForSeconds(0.5f);
        for (int i = 0; i < cnt; i++)
        {
            CustomerData random = _customersData.GetRandomOpendCustomer(false);
            if (!_customerPool.ContainsKey(random.Info.Type))
            {
                CreatePool(random.Info.Type);
            }

            BaseCustomer customer = _customerPool[random.Info.Type].Get();

            customer.SetCreatedInfo(random, GetTownBottom(), GetTownTop(), _main, this);

            _activeCustomerCnt++;

            yield return delay;
        }
    }

    IEnumerator SpawnGuestProcess(float delay, bool bRightNow)
    {
        if (!bRightNow)
        {
            if(delay == 0)
            {
                delay = Random.Range(100f, 180f)
                    - DataMgr.Instance.InGame.Facility.DailyFortune.GetFortuneValue(eFortune.IncGuestVisit);
            }
            
            yield return new WaitForSeconds(delay);
        }

        // 아직 나올 수 없는 조건(오픈이 안된 1호실만 지어진 상태)라면 계속 넘겨야함
        if (_customersData.OpendGuestCustomers.Count == 0)
        {
            PlayGuestSpawnProcess();
        }
        else
        {
            SpawnGuest();
            cor_spawnGuestProcess = null;
        }
    }

    IEnumerator PermitReservationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _bCanReservation = true;
    }
}
