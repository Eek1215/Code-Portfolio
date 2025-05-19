using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public enum eAudio
{
    BGM,
    SFX,
    UI
}

public enum eBGM
{
    None = -1,
    Track1,
    Track2,
    Track3,
    Track4,
    Track5,
    Track6,
    Track7,
    Track8,
    Track9,
    Track10_Winter2024_1,
    Max,
}

public enum eSFX
{
    CreateObj,
    CreateSite,
    ClickSnakesLadders,
    DisrupterHit,
    PrepIngredientsHit,
    CardFlip,
    CardTouch,
    CardTurn,
    ClickExpBalloon,
    CustomerSpawn,
    GuestSpawn,
    ClickSeed,
    ClickRequestBalloon,
    TouristStart,
    TouristGetItem,
    TouristClear,
    TouristResult,    
    CarHoot,
    PickUpTrash,
    PrepIngredientsCreateDust,
    Expand,
    GoUpCoin,
    TouchFlyingCash,
    FlyingCashShake,
    GardenFindItemShovel,
    GardenSpawnSeed,
    GardenGrowthBooster,
    GardenWateringCan,
    SpawnFlyingCash,
}

public enum eUISFX
{
    OnClickBtn,
    GetWealth,
    FloorStoreLevelUp,    
    GetItem,
    CreateBubble,
    ClickBubble,
    TownAbilityLevelUp,
    FruitFinish,
    ScreenshotCapture,
    Screenshot,
    ShowCostumeItem,
    CostumeChestOpen,
    ClickFood,
    EatFood,
    NewCharacter,
    NewStoreType,
    RankUp,
    CapsuleDrop,
    CapsuleOpen,
    FortuneResult,
}

/* 사운드를 관리하는 코드
 * 코드규칙은 카멜, 파스칼 기법을 혼용하여 사용 */
public class SoundMgr : DontDestroySingleton<SoundMgr>
{
    public AudioMixer AudioMixer;

    // AudioSource는 배경음, 효과음, UI효과음 셋으로 나누어 관리.
    public AudioSource as_bgm;
    public AudioSource as_sfx;
    public AudioSource as_ui;

    // Enum순서에 맞게 Clip을 담아둠
    public AudioClip[] ac_bgms;
    public AudioClip[] ac_sfxs;
    public AudioClip[] ac_uiSFXs;

    // 배경음은 전체적으로 돌아가지만 특정 컨텐츠에서는 컨텐츠 입장시만 뜨는 브금이 존재.
    public AudioClip ac_fortuneBG;

    // 동물의숲처럼 말하는 것처럼 연출할 수 있는 에셋.
    public ToonyVoices ToonyVoices;

    // 브금이 바뀌었을 때 호출할 델리게이트.
    public delegate void OnEvent();
    public event OnEvent OnBGMChange;

    // 현재 진행중인 브금과 이전브금. 미리듣기 기능이 생기면서 미리듣기가 끝난 후 이전 브금으로
    // 되돌려 실행하기 위함.
    public eBGM CurBGM;
    public eBGM CurPreviewBGM;
    public bool IsPlayingPreview;
    private bool _bSoundPlay;

    // IOS의 경우 광고를 시청 후 사운드가 안나오는 현상이 존재.
    // 미리 Audio정보를 담아두고 광고 재생 후 다시 초기화하여 실행시켜줄 필요가 있음
    private AudioConfiguration _audioConfig;
    private float _bgmVolume;
    private bool _bShowedAds;

    public override void Init()
    {
#if UNITY_EDITOR
        // 에디터만 소리가 너무 커서 줄이는 것. 오디어믹서는 -80~0의 범위기 때문에 Log10 * 20을 사용하면
        // 해당 범위를 맞출수가 있음. 사이에 들어가는 값은 0~1f
        AudioMixer.SetFloat("Master", Mathf.Log10(0.2f) * 20);
#endif

        SetVolume(eAudio.BGM, PlayerPrefs.GetFloat("BGM", 0.5f));
        SetVolume(eAudio.SFX, PlayerPrefs.GetFloat("SFX", 0.5f));
        SetVolume(eAudio.UI, PlayerPrefs.GetFloat("UI", 0.5f));
    }

    public void OnLoadCompleteEvent()
    {
        // 처음 게임을 실행할 때 인트로를 안봤다면 인트로를 본 뒤에 나와야 하며,
        // 튜토리얼은 Step이 있는데 0보다 크다는 건 인트로를 봤다는 증거. 그 전에는 사운드가 나오면 안된다.
        if (DataMgr.Instance.InGame.TutorialStep > 0)
        {
            _bSoundPlay = true;
        }

        // 마지막에 재생중인데 브금을 재생
        PlayBGM(DataMgr.Instance.InGame.Facility.BGMList.LastSelectBGM);

        TimeMgr.Instance.AddDecreasePerFrameEvent(CheckBGMFinish);

        _bShowedAds = false;
    }

    // 인트로 후에 사운드 재생가능을 알림.
    public void BGMPlayFromIntro()
    {
        _bSoundPlay = true;
        PlayBGM(DataMgr.Instance.InGame.Facility.BGMList.LastSelectBGM);
    }

    #region 아이폰 전용
    // 광고 재생 후에 소리가 안나오는 걸 방지하기 위해 브금을 재실행
    public void SoundReset()
    {
        if (_bShowedAds)
        {
            AudioSettings.Reset(_audioConfig);
            as_bgm.volume = _bgmVolume;
            as_bgm.Play();
            _bShowedAds = false;
        }
    }

    // 광고 재생 후에 소리가 안나오는 걸 방지하기 위해 브금정보 기억.
    public void SetCurConfig()
    {
        _bShowedAds = true;
        _audioConfig = AudioSettings.GetConfiguration();
        _bgmVolume = as_bgm.volume;
    }
    #endregion

    private void OnEnable()
    {
        AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
    }

    private void OnDisable()
    {
        AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
    }

    // 안드로이드의 경우 이어폰을 사용하는 등 기기의 출력방식이 바뀌면 소리가 안나고 0으로 초기화되는 문제가 있기 때문에
    // 기기 정보가 바뀌었을 때 소리가 나오도록 설정.
    public void OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        SetVolume(eAudio.BGM, PlayerPrefs.GetFloat("BGM", 0.5f));
        SetVolume(eAudio.SFX, PlayerPrefs.GetFloat("SFX", 0.5f));
        SetVolume(eAudio.UI, PlayerPrefs.GetFloat("UI", 0.5f));
    }

    /// <param name="bgm"></param>
    /// <param name="bForce">강제로 브금을 바꿔야 하는 경우.배경음리스트에서 강제로 원하는 곡을 누른 경우</param>
    public void PlayBGM(eBGM bgm, bool bForce = false)
    {
        // 원하는 곡 재생을 눌렀기 때문에 이전 정보가 필요없음. 현재 재생곡이 최신이기 때문
        if(bForce && IsPlayingPreview)
        {
            IsPlayingPreview = false;
            CurPreviewBGM = eBGM.None;
        }

        /* 현재 프리뷰중이면서 루프중인건 특정 팝업에서 이벤트 음악이 나오고 있는 경우이다.
         * 이런 경우는 음악이 다시 끝났을 때 다음 곡으로 넘어가는 기본 시스템이 동작하지 않도록
         * 여기서 종료시켜버리는 것. */
        if (IsPlayingPreview && as_bgm.loop)
        {
            return;
        }

        // 배경음리스트를 끝까지 순회하면 처음으로 돌아와서 다시 재생.
        if((int)bgm >= ac_bgms.Length)
        {
            bgm = eBGM.Track1;
        }

        // 기본적으로 한 곡이 끝나면 다음곡을 실행하기 때문에 loop는 꺼둠.
        as_bgm.loop = false;
        as_bgm.clip = ac_bgms[(int)bgm];
        OnBGMChange?.Invoke();
        CurBGM = bgm;
        if (_bSoundPlay)
        {
            as_bgm.Play();
        }
    }

    // 배경음 미리듣기
    public void PlayBGMPreview(eBGM bgm, bool bLoop)
    {
        // 같은 곡을 미리듣기는 안됨.
        if(as_bgm.clip == ac_bgms[(int)bgm])
        {
            return;
        }

        IsPlayingPreview = true;
        CurPreviewBGM = bgm;

        as_bgm.loop = bLoop;
        as_bgm.clip = ac_bgms[(int)bgm];
        OnBGMChange?.Invoke();
        if (_bSoundPlay)
        {
            as_bgm.Play();
        }
    }

    public void PlayFortuneBGMPreview()
    {
        if (as_bgm.clip == ac_fortuneBG)
        {
            return;
        }

        IsPlayingPreview = true;
        CurPreviewBGM = eBGM.None;

        as_bgm.loop = true;
        as_bgm.clip = ac_fortuneBG;
        OnBGMChange?.Invoke();
        if (_bSoundPlay)
        {
            as_bgm.Play();
        }
    }

    // 미리듣기 종료
    public void RevertBGM()
    {
        if (IsPlayingPreview)
        {
            IsPlayingPreview = false;
            CurPreviewBGM = eBGM.None;
            PlayBGM(CurBGM);
        }
    }

    /* TimeMgr에서 PerSec뿐 아니라 frame별로 줄어들면서 체크하는 루틴이 있는데
     * 해당 루틴에서 frame간의 deltaTime을 보내주어 시간이 흐르는 걸 1초단위보다 더 빠르게 체크할 수 있게 함 */
    void CheckBGMFinish(float deltaTime)
    {
        if (!_bSoundPlay)
            return;

        /* 기본 배경음은 위에서 loop를 안한다고 하였는데 여기서 효력을 발휘한다.
         * 비슷한 방법으로 audioclip의 length로 사운드파일의 길이를 알 수 있기에
         * 해당 시간만큼 코루틴으로 돌리는 방법도 있겠으나, 코루틴을 늘리기보다 돌아가는 프로세스에 추가하여
         * 일괄처리 되는 방안을 채택 (코루틴이 여러개 돌아가서 오버헤드등의 문제가 생길 수 있고
         * 단점이 될 수 있는 순서가 꼬이는 등의 문제는 관련된 코드가 없기 때문에 문제x)
         * */
        if(!as_bgm.isPlaying)
        {
            // 미리듣기면 미리듣기를 종료하고 원래의 배경음으로 돌아가기
            if (IsPlayingPreview)
            {
                RevertBGM();
            }
            // 배경음 순차 플레이중이면 다음 곡으로 순회
            else
            {
                BGMListData data = DataMgr.Instance.InGame.Facility.BGMList;
                data.SelectBGM(data.GetNextPlayableBGM());
                PlayBGM(data.LastSelectBGM);
            }
        }
    }

    // 효과음 코드는 1회만 재생해야 하는지 여러차례인지에 따라 달라짐.
    public void PlaySFX(eSFX sfx, int cnt = 1, float interval = 0.05f)
    {
        if (_bSoundPlay)
        {
            if (cnt == 1)
            {
                as_sfx.PlayOneShot(ac_sfxs[(int)sfx]);
            }
            else
            {
                /* 보통의 코드들은 전역변수로 Coroutine을 할당하고
                 * cor_playSFXLoop = StartCoroutine(PlaySFXLoop(sfx, cnt, interval));
                 * 위처럼 했겠지만 해당 경우는 멈출 일이 없고 소멸하는 것이 목적인 코드기에 할당하지 않음.
                 * 코루틴은 일회성이 아니라면 되도록 할당하고 관리하는 게 바람직. */
                StartCoroutine(PlaySFXLoop(sfx, cnt, interval));
            }            
        }
    }

    public void PlayUISFX(eUISFX ui, int cnt = 1, float interval = 0.05f)
    {
        if (_bSoundPlay)
        {
            if (cnt == 1)
            {
                as_ui.PlayOneShot(ac_uiSFXs[(int)ui]);
            }
            else
            {
                StartCoroutine(PlayUISFXLoop(ui, cnt, interval));
            }
        }
    }

    // 사운드 파일을 수정하는 것보다 딜레이를 주고 실행시키는 게 낫다 판단하여 오버로딩.
    public void PlayUISFX(eUISFX ui, float delay)
    {
        if (_bSoundPlay)
        {
            StartCoroutine(PlayUISFXLoop(ui, delay));
        }
    }

    public void SoundPause()
    {
        _bSoundPlay = false;
        as_bgm.Stop();
        as_sfx.Stop();
        as_ui.Stop();
    }

    public void SoundRestart()
    {
        _bSoundPlay = true;
        as_bgm.Play();
    }

    /// <summary>
    /// </summary>
    /// <param name="value">0 ~ 1</param>
    public void SetVolume(eAudio type, float value)
    {
        float volume = value == 0 ? -80f : Mathf.Log10(value) * 20;
        switch (type)
        {
            case eAudio.BGM:
                AudioMixer.SetFloat("BGM", volume);
                PlayerPrefs.SetFloat("BGM", value);
                break;
            case eAudio.SFX:
                AudioMixer.SetFloat("SFX", volume);
                PlayerPrefs.SetFloat("SFX", value);
                break;
            case eAudio.UI:
                AudioMixer.SetFloat("UI", volume);
                PlayerPrefs.SetFloat("UI", value);
                break;
        }
    }

    IEnumerator PlaySFXLoop(eSFX sfx, int cnt, float interval)
    {
        WaitForSeconds delay = new WaitForSeconds(interval);
        while (cnt > 0)
        {
            cnt--;
            PlaySFX(sfx);
            yield return delay;
        }
    }

    IEnumerator PlayUISFXLoop(eUISFX sfx, int cnt, float interval)
    {
        WaitForSeconds delay = new WaitForSeconds(interval);
        while(cnt > 0)
        {
            cnt--;
            PlayUISFX(sfx);
            yield return delay;
        }
    }

    IEnumerator PlayUISFXLoop(eUISFX sfx, float delay)
    {
        WaitForSeconds wfs = new WaitForSeconds(delay);
        yield return wfs;
        PlayUISFX(sfx);
    }
}
