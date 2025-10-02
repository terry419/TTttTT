// ���� ���: ./TTttTT/Assets/1.Scripts/UI/DisplaySettings.cs
using UnityEngine;

public class DisplaySettings : MonoBehaviour
{
    public void SetFullScreen()
    {
        Debug.Log("[DisplaySettings] '��ü ȭ��' ��ư Ŭ����. Screen.fullScreenMode�� 'ExclusiveFullScreen'���� ���� �õ�.");
        // �����Ϳ����� �������� �ʴ� ���� �����Դϴ�.
#if !UNITY_EDITOR
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
#else
        Debug.LogWarning("[DisplaySettings] ������ ȯ�濡���� ExclusiveFullScreen ��带 �������� �ʽ��ϴ�.");
#endif
    }

    public void SetBorderlessWindow()
    {
        Debug.Log("[DisplaySettings] '�׵θ� ���� â' ��ư Ŭ����. Screen.fullScreenMode�� 'FullScreenWindow'�� ���� �õ�.");
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    public void SetWindowed()
    {
        Debug.Log("[DisplaySettings] 'â ���' ��ư Ŭ����. Screen.SetResolution(1280x720) ȣ�� �õ�.");
        // �������� Game �䰡 'Free Aspect'�� �ƴϸ� ũ�Ⱑ ������ �� �ֽ��ϴ�.
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }
}