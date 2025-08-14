// --- ���ϸ�: SceneAudioLoader.cs ---

using UnityEngine;

public class SceneAudioLoader : MonoBehaviour
{
    [Header("�� ����� �÷���")]
    [SerializeField]
    private AudioCollection sceneAudioCollection;

    [Header("���� BGM �̸� (����)")]
    [SerializeField]
    private string startingBgmName;

    void Start()
    {
        // [����] AudioManager�� null�� ��츦 ����� ��� �ڵ� �߰�
        if (AudioManager.Instance == null)
        {
            Debug.LogError("SceneAudioLoader: AudioManager.Instance�� ã�� �� �����ϴ�!");
            return;
        }

        if (sceneAudioCollection != null)
        {
            // [����] �ּ��� �����ؼ� ����� �÷��� �ε� �� BGM ��� ����� ���� �۵��ϵ��� ����
            AudioManager.Instance.LoadCollection(sceneAudioCollection);
            if (!string.IsNullOrEmpty(startingBgmName))
            {
                AudioManager.Instance.PlayBgm(startingBgmName);
            }
        }
        else
        {
            Debug.LogWarning("SceneAudioLoader: AudioCollection�� �Ҵ���� �ʾҽ��ϴ�!", this.gameObject);
        }
    }
}