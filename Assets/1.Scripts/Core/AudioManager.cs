// 파일명: AudioManager.cs (리팩토링 완료)
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("오디오 소스 (Audio Sources)")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    private AudioCollection currentCollection;

    void Awake()
    {
        // ServiceLocator에 자기 자신을 등록합니다.
        ServiceLocator.Register<AudioManager>(this);
    }

    public void LoadCollection(AudioCollection newCollection)
    {
        currentCollection = newCollection;
    }

    public void PlayBgm(string clipName, bool loop = true)
    {
        if (currentCollection == null)
        {
            Debug.LogWarning("AudioManager: 재생할 AudioCollection이 로드되지 않았습니다!");
            return;
        }

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
        if (currentCollection == null)
        {
            Debug.LogWarning("AudioManager: 재생할 AudioCollection이 로드되지 않았습니다!");
            return;
        }

        AudioClip clipToPlay = currentCollection.GetSfxClip(clipName);
        if (clipToPlay != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }
    }

    public void StopBgm()
    {
        bgmSource.Stop();
    }

    public void SetVolume(float bgmVol, float sfxVol)
    {
        bgmSource.volume = bgmVol;
        sfxSource.volume = sfxVol;
    }

    public float GetBgmVolume() => bgmSource.volume;
    public float GetSfxVolume() => sfxSource.volume;
}