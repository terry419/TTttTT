//  : Assets/1.Scripts/Core/SceneAudioLoader.cs

using UnityEngine;

public class SceneAudioLoader : MonoBehaviour
{
    [Header("  ÷")]
    [SerializeField]
    private AudioCollection sceneAudioCollection;

    [Header(" BGM ̸ ( )")]
    [SerializeField]
    private string startingBgmName;

    void Start()
    {
        // [] ServiceLocator  AudioManager ɴϴ.
        var audioManager = ServiceLocator.Get<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("SceneAudioLoader: AudioManager ã  ϴ!");
            return;
        }

        if (sceneAudioCollection != null)
        {
            // 오디오 컬렉션을 AudioManager에 로드합니다.
            audioManager.LoadCollection(sceneAudioCollection);
            if (!string.IsNullOrEmpty(startingBgmName))
            {
                audioManager.PlayBgm(startingBgmName);
            }
        }
        else
        {
            Debug.LogWarning("SceneAudioLoader: AudioCollection Ҵ ʾҽϴ!", this.gameObject);
        }
    }
}