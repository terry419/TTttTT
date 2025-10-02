using UnityEngine;

public class DisplaySettings : MonoBehaviour
{
    // 1. ��üȭ�� ���
    public void SetFullScreen()
    {
        // ���� �Ϲ����� ��üȭ�� ����Դϴ�.
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Debug.Log("��üȭ������ ����");
    }

    // 2. �׵θ� ���� â ���
    public void SetBorderlessWindow()
    {
        // ���� ����� �ػ󵵿� �� ���� �׵θ� ���� â ����Դϴ�.
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Debug.Log("�׵θ� ���� â ���� ����");
    }

    // 3. â ��� (1280x720)
    public void SetWindowed()
    {
        // ��õ�ص帰 1280x720 ũ���� â ���� �����մϴ�.
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        Debug.Log("â ��� (1280x720)���� ����");
    }
}