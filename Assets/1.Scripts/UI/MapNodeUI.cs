using UnityEngine;
using UnityEngine.UI;

// �� ��ũ��Ʈ�� UI ��ư ������Ʈ�� � �� ������(����)��
// ������ �ִ��� �����ϰ� �����ϴ� ������ �մϴ�.
public class MapNodeUI : MonoBehaviour
{
    public MapNode nodeData; // �� ������ ���� (3�ܰ迡�� ���)
    [HideInInspector] public Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }
}