// --- 파일명: SceneAudioLoader.cs ---

using UnityEngine;

public class SceneAudioLoader : MonoBehaviour
{
    [Header("씬 오디오 컬렉션")]
    [SerializeField]
    private AudioCollection sceneAudioCollection;

    [Header("시작 BGM 이름 (선택)")]
    [SerializeField]
    private string startingBgmName;

    void Start()
    {
        // [수정] AudioManager가 null일 경우를 대비한 방어 코드 추가
        if (AudioManager.Instance == null)
        {
            Debug.LogError("SceneAudioLoader: AudioManager.Instance를 찾을 수 없습니다!");
            return;
        }

        if (sceneAudioCollection != null)
        {
            // [수정] 주석을 해제해서 오디오 컬렉션 로드 및 BGM 재생 기능이 정상 작동하도록 수정
            AudioManager.Instance.LoadCollection(sceneAudioCollection);
            if (!string.IsNullOrEmpty(startingBgmName))
            {
                AudioManager.Instance.PlayBgm(startingBgmName);
            }
        }
        else
        {
            Debug.LogWarning("SceneAudioLoader: AudioCollection이 할당되지 않았습니다!", this.gameObject);
        }
    }
}