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

/* ���带 �����ϴ� �ڵ�
 * �ڵ��Ģ�� ī��, �Ľ�Į ����� ȥ���Ͽ� ��� */
public class SoundMgr : DontDestroySingleton<SoundMgr>
{
    public AudioMixer AudioMixer;

    // AudioSource�� �����, ȿ����, UIȿ���� ������ ������ ����.
    public AudioSource as_bgm;
    public AudioSource as_sfx;
    public AudioSource as_ui;

    // Enum������ �°� Clip�� ��Ƶ�
    public AudioClip[] ac_bgms;
    public AudioClip[] ac_sfxs;
    public AudioClip[] ac_uiSFXs;

    // ������� ��ü������ ���ư����� Ư�� ������������ ������ ����ø� �ߴ� ����� ����.
    public AudioClip ac_fortuneBG;

    // �����ǽ�ó�� ���ϴ� ��ó�� ������ �� �ִ� ����.
    public ToonyVoices ToonyVoices;

    // ����� �ٲ���� �� ȣ���� ��������Ʈ.
    public delegate void OnEvent();
    public event OnEvent OnBGMChange;

    // ���� �������� ��ݰ� �������. �̸���� ����� ����鼭 �̸���Ⱑ ���� �� ���� �������
    // �ǵ��� �����ϱ� ����.
    public eBGM CurBGM;
    public eBGM CurPreviewBGM;
    public bool IsPlayingPreview;
    private bool _bSoundPlay;

    // IOS�� ��� ���� ��û �� ���尡 �ȳ����� ������ ����.
    // �̸� Audio������ ��Ƶΰ� ���� ��� �� �ٽ� �ʱ�ȭ�Ͽ� ��������� �ʿ䰡 ����
    private AudioConfiguration _audioConfig;
    private float _bgmVolume;
    private bool _bShowedAds;

    public override void Init()
    {
#if UNITY_EDITOR
        // �����͸� �Ҹ��� �ʹ� Ŀ�� ���̴� ��. �����ͼ��� -80~0�� ������ ������ Log10 * 20�� ����ϸ�
        // �ش� ������ ������� ����. ���̿� ���� ���� 0~1f
        AudioMixer.SetFloat("Master", Mathf.Log10(0.2f) * 20);
#endif

        SetVolume(eAudio.BGM, PlayerPrefs.GetFloat("BGM", 0.5f));
        SetVolume(eAudio.SFX, PlayerPrefs.GetFloat("SFX", 0.5f));
        SetVolume(eAudio.UI, PlayerPrefs.GetFloat("UI", 0.5f));
    }

    public void OnLoadCompleteEvent()
    {
        // ó�� ������ ������ �� ��Ʈ�θ� �Ⱥôٸ� ��Ʈ�θ� �� �ڿ� ���;� �ϸ�,
        // Ʃ�丮���� Step�� �ִµ� 0���� ũ�ٴ� �� ��Ʈ�θ� �ôٴ� ����. �� ������ ���尡 ������ �ȵȴ�.
        if (DataMgr.Instance.InGame.TutorialStep > 0)
        {
            _bSoundPlay = true;
        }

        // �������� ������ε� ����� ���
        PlayBGM(DataMgr.Instance.InGame.Facility.BGMList.LastSelectBGM);

        TimeMgr.Instance.AddDecreasePerFrameEvent(CheckBGMFinish);

        _bShowedAds = false;
    }

    // ��Ʈ�� �Ŀ� ���� ��������� �˸�.
    public void BGMPlayFromIntro()
    {
        _bSoundPlay = true;
        PlayBGM(DataMgr.Instance.InGame.Facility.BGMList.LastSelectBGM);
    }

    #region ������ ����
    // ���� ��� �Ŀ� �Ҹ��� �ȳ����� �� �����ϱ� ���� ����� �����
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

    // ���� ��� �Ŀ� �Ҹ��� �ȳ����� �� �����ϱ� ���� ������� ���.
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

    // �ȵ���̵��� ��� �̾����� ����ϴ� �� ����� ��¹���� �ٲ�� �Ҹ��� �ȳ��� 0���� �ʱ�ȭ�Ǵ� ������ �ֱ� ������
    // ��� ������ �ٲ���� �� �Ҹ��� �������� ����.
    public void OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        SetVolume(eAudio.BGM, PlayerPrefs.GetFloat("BGM", 0.5f));
        SetVolume(eAudio.SFX, PlayerPrefs.GetFloat("SFX", 0.5f));
        SetVolume(eAudio.UI, PlayerPrefs.GetFloat("UI", 0.5f));
    }

    /// <param name="bgm"></param>
    /// <param name="bForce">������ ����� �ٲ�� �ϴ� ���.���������Ʈ���� ������ ���ϴ� ���� ���� ���</param>
    public void PlayBGM(eBGM bgm, bool bForce = false)
    {
        // ���ϴ� �� ����� ������ ������ ���� ������ �ʿ����. ���� ������� �ֽ��̱� ����
        if(bForce && IsPlayingPreview)
        {
            IsPlayingPreview = false;
            CurPreviewBGM = eBGM.None;
        }

        /* ���� ���������̸鼭 �������ΰ� Ư�� �˾����� �̺�Ʈ ������ ������ �ִ� ����̴�.
         * �̷� ���� ������ �ٽ� ������ �� ���� ������ �Ѿ�� �⺻ �ý����� �������� �ʵ���
         * ���⼭ ������ѹ����� ��. */
        if (IsPlayingPreview && as_bgm.loop)
        {
            return;
        }

        // ���������Ʈ�� ������ ��ȸ�ϸ� ó������ ���ƿͼ� �ٽ� ���.
        if((int)bgm >= ac_bgms.Length)
        {
            bgm = eBGM.Track1;
        }

        // �⺻������ �� ���� ������ �������� �����ϱ� ������ loop�� ����.
        as_bgm.loop = false;
        as_bgm.clip = ac_bgms[(int)bgm];
        OnBGMChange?.Invoke();
        CurBGM = bgm;
        if (_bSoundPlay)
        {
            as_bgm.Play();
        }
    }

    // ����� �̸����
    public void PlayBGMPreview(eBGM bgm, bool bLoop)
    {
        // ���� ���� �̸����� �ȵ�.
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

    // �̸���� ����
    public void RevertBGM()
    {
        if (IsPlayingPreview)
        {
            IsPlayingPreview = false;
            CurPreviewBGM = eBGM.None;
            PlayBGM(CurBGM);
        }
    }

    /* TimeMgr���� PerSec�� �ƴ϶� frame���� �پ��鼭 üũ�ϴ� ��ƾ�� �ִµ�
     * �ش� ��ƾ���� frame���� deltaTime�� �����־� �ð��� �帣�� �� 1�ʴ������� �� ������ üũ�� �� �ְ� �� */
    void CheckBGMFinish(float deltaTime)
    {
        if (!_bSoundPlay)
            return;

        /* �⺻ ������� ������ loop�� ���Ѵٰ� �Ͽ��µ� ���⼭ ȿ���� �����Ѵ�.
         * ����� ������� audioclip�� length�� ���������� ���̸� �� �� �ֱ⿡
         * �ش� �ð���ŭ �ڷ�ƾ���� ������ ����� �ְ�����, �ڷ�ƾ�� �ø��⺸�� ���ư��� ���μ����� �߰��Ͽ�
         * �ϰ�ó�� �Ǵ� ����� ä�� (�ڷ�ƾ�� ������ ���ư��� ���������� ������ ���� �� �ְ�
         * ������ �� �� �ִ� ������ ���̴� ���� ������ ���õ� �ڵ尡 ���� ������ ����x)
         * */
        if(!as_bgm.isPlaying)
        {
            // �̸����� �̸���⸦ �����ϰ� ������ ��������� ���ư���
            if (IsPlayingPreview)
            {
                RevertBGM();
            }
            // ����� ���� �÷������̸� ���� ������ ��ȸ
            else
            {
                BGMListData data = DataMgr.Instance.InGame.Facility.BGMList;
                data.SelectBGM(data.GetNextPlayableBGM());
                PlayBGM(data.LastSelectBGM);
            }
        }
    }

    // ȿ���� �ڵ�� 1ȸ�� ����ؾ� �ϴ��� �������������� ���� �޶���.
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
                /* ������ �ڵ���� ���������� Coroutine�� �Ҵ��ϰ�
                 * cor_playSFXLoop = StartCoroutine(PlaySFXLoop(sfx, cnt, interval));
                 * ��ó�� �߰����� �ش� ���� ���� ���� ���� �Ҹ��ϴ� ���� ������ �ڵ�⿡ �Ҵ����� ����.
                 * �ڷ�ƾ�� ��ȸ���� �ƴ϶�� �ǵ��� �Ҵ��ϰ� �����ϴ� �� �ٶ���. */
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

    // ���� ������ �����ϴ� �ͺ��� �����̸� �ְ� �����Ű�� �� ���� �Ǵ��Ͽ� �����ε�.
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
