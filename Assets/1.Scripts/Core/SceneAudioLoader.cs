using UnityEngine;

/// <summary>
/// 씬이 시작될 때 지정된 AudioCollection을 AudioManager에 로드하고,
/// 시작 BGM을 재생하는 역할을 합니다.
/// </summary>
public class SceneAudioLoader : MonoBehaviour
{
    [Header("이 씬에서 사용할 오디오 컬렉션")]
    [SerializeField]
    private AudioCollection sceneAudioCollection;

    [Header("씬 시작 시 재생할 BGM 이름 (선택 사항)")]
    [SerializeField]
    private string startingBgmName;

    void Start()
    {
        if (sceneAudioCollection != null)
        {
            // AudioManager.Instance.LoadCollection(sceneAudioCollection); // AudioManager에 LoadCollection 함수 구현 필요
            if (!string.IsNullOrEmpty(startingBgmName))
            {
                // AudioManager.Instance.PlayBgm(startingBgmName); // AudioManager에 PlayBgm 함수 구현 필요
            }
        }
        else
        {
            Debug.LogWarning("SceneAudioLoader: AudioCollection이 할당되지 않았습니다!", this.gameObject);
        }
    }
}