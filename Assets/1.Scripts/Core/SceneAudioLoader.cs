using UnityEngine;

/// <summary>
/// ���� ���۵� �� ������ AudioCollection�� AudioManager�� �ε��ϰ�,
/// ���� BGM�� ����ϴ� ������ �մϴ�.
/// </summary>
public class SceneAudioLoader : MonoBehaviour
{
    [Header("�� ������ ����� ����� �÷���")]
    [SerializeField]
    private AudioCollection sceneAudioCollection;

    [Header("�� ���� �� ����� BGM �̸� (���� ����)")]
    [SerializeField]
    private string startingBgmName;

    void Start()
    {
        if (sceneAudioCollection != null)
        {
            // AudioManager.Instance.LoadCollection(sceneAudioCollection); // AudioManager�� LoadCollection �Լ� ���� �ʿ�
            if (!string.IsNullOrEmpty(startingBgmName))
            {
                // AudioManager.Instance.PlayBgm(startingBgmName); // AudioManager�� PlayBgm �Լ� ���� �ʿ�
            }
        }
        else
        {
            Debug.LogWarning("SceneAudioLoader: AudioCollection�� �Ҵ���� �ʾҽ��ϴ�!", this.gameObject);
        }
    }
}