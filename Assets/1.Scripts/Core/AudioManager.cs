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
        // ServiceLocator에 이미 등록된 인스턴스가 있는지 확인
        if (ServiceLocator.IsRegistered<AudioManager>())
        {
            // 이미 있다면 나는 중복이므로 스스로 파괴
            Destroy(gameObject);
            return;
        }
        
        // 최초의 인스턴스일 경우, 등록하고 파괴되지 않도록 설정
        ServiceLocator.Register<AudioManager>(this);
        DontDestroyOnLoad(gameObject);
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