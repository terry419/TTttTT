// 파일 경로: Assets/1.Scripts/Core/SceneAudioLoader.cs

using UnityEngine;

public class SceneAudioLoader : MonoBehaviour
{
    [Header("씬 오디오 컬렉션")]
    [SerializeField]
    private AudioCollection sceneAudioCollection;

    [Header("시작 BGM 이름 (선택 사항)")]
    [SerializeField]
    private string startingBgmName;

    void Start()
    {
        // [수정] ServiceLocator를 통해 AudioManager를 가져옵니다.
        var audioManager = ServiceLocator.Get<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("SceneAudioLoader: AudioManager를 찾을 수 없습니다!");
            return;
        }

        if (sceneAudioCollection != null)
        {
            // [수정] 가져온 audioManager 변수를 사용합니다.
            audioManager.LoadCollection(sceneAudioCollection);
            if (!string.IsNullOrEmpty(startingBgmName))
            {
                audioManager.PlayBgm(startingBgmName);
            }
        }
        else
        {
            Debug.LogWarning("SceneAudioLoader: AudioCollection이 할당되지 않았습니다!", this.gameObject);
        }
    }
}