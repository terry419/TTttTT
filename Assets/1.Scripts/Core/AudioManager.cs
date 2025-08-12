using UnityEngine;

/// <summary>
/// 게임의 모든 사운드(배경음악, 효과음) 재생을 관리하는 싱글톤 클래스입니다.
/// BGM과 SFX를 위한 별도의 AudioSource를 사용하여 독립적인 볼륨 제어 및 재생이 가능하도록 합니다.
/// OptionsController 등 다른 시스템과 연동하여 사운드 설정을 관리합니다.
/// </summary>
[RequireComponent(typeof(AudioSource), typeof(AudioSource))] // BGM, SFX용 2개 필요
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource bgmSource; // 배경음악 재생기
    private AudioSource sfxSource; // 효과음 재생기

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // AudioSource 컴포넌트들을 가져와 각각 할당합니다.
        AudioSource[] sources = GetComponents<AudioSource>();
        bgmSource = sources[0];
        sfxSource = sources[1];

        // BGM은 보통 루프 재생됩니다.
        bgmSource.loop = true;
    }

    /// <summary>
    /// 지정된 배경음악을 재생합니다.
    /// </summary>
    /// <param name="bgmClip">재생할 오디오 클립</param>
    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmClip == null || bgmSource.clip == bgmClip) return;

        bgmSource.clip = bgmClip;
        bgmSource.Play();
        Debug.Log($"[Audio] BGM 재생: {bgmClip.name}");
    }

    /// <summary>
    /// 배경음악 재생을 중지합니다.
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 지정된 효과음을 한 번 재생합니다. 여러 효과음이 겹쳐서 재생될 수 있습니다.
    /// </summary>
    /// <param name="sfxClip">재생할 오디오 클립</param>
    public void PlaySFX(AudioClip sfxClip)
    { 
        if (sfxClip == null) return;
        // PlayOneShot은 기존 재생 중인 효과음을 멈추지 않고 새로 재생합니다.
        sfxSource.PlayOneShot(sfxClip);
    }

    /// <summary>
    /// BGM과 SFX의 볼륨을 조절합니다. OptionsController에서 호출됩니다.
    /// </summary>
    /// <param name="bgmVolume">BGM 볼륨 (0.0 ~ 1.0)</param>
    /// <param name="sfxVolume">SFX 볼륨 (0.0 ~ 1.0)</param>
    public void SetVolume(float bgmVolume, float sfxVolume)
    {
        bgmSource.volume = Mathf.Clamp01(bgmVolume);
        sfxSource.volume = Mathf.Clamp01(sfxVolume);

        // TODO: 변경된 볼륨 설정을 PlayerPrefs 등에 저장하는 로직 필요
    }

    /// <summary>
    /// 현재 BGM 볼륨을 가져옵니다.
    /// </summary>
    public float GetBgmVolume()
    {
        return bgmSource.volume;
    }

    /// <summary>
    /// 현재 SFX 볼륨을 가져옵니다.
    /// </summary>
    public float GetSfxVolume()
    {
        return sfxSource.volume;
    }
}
