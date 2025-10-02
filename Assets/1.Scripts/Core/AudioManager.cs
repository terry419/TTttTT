// 파일 경로: ./TTttTT/Assets/1.Scripts/Core/AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("오디오 소스 (Audio Sources)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private AudioCollection currentCollection;

    void Awake()
    {
        if (ServiceLocator.IsRegistered<AudioManager>())
        {
            Destroy(gameObject);
            return;
        }

        ServiceLocator.Register<AudioManager>(this);
        DontDestroyOnLoad(gameObject);
    }

    public void LoadCollection(AudioCollection newCollection)
    {
        currentCollection = newCollection;
    }

    public void PlayBgm(string clipName, bool loop = true)
    {
        // [수정] bgmSource가 할당되지 않았으면 함수를 조용히 종료
        if (bgmSource == null || currentCollection == null) return;

        AudioClip clipToPlay = currentCollection.GetBgmClip(clipName);
        if (clipToPlay != null)
        {
            bgmSource.clip = clipToPlay;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }

    public void PlaySfx(string clipName)
    {
        // [수정] sfxSource가 할당되지 않았으면 함수를 조용히 종료
        if (sfxSource == null || currentCollection == null) return;

        AudioClip clipToPlay = currentCollection.GetSfxClip(clipName);
        if (clipToPlay != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }
    }

    public void StopBgm()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }

    public void SetVolume(float bgmVol, float sfxVol)
    {
        if (bgmSource != null) bgmSource.volume = bgmVol;
        if (sfxSource != null) sfxSource.volume = sfxVol;
    }

    // [수정] bgmSource가 없으면 기본값(1.0) 반환
    public float GetBgmVolume() => bgmSource != null ? bgmSource.volume : 1.0f;
    // [수정] sfxSource가 없으면 기본값(1.0) 반환
    public float GetSfxVolume() => sfxSource != null ? sfxSource.volume : 1.0f;
}