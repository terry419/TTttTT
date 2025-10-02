using UnityEngine;

public class DisplaySettings : MonoBehaviour
{
    // 1. 전체화면 모드
    public void SetFullScreen()
    {
        // 가장 일반적인 전체화면 모드입니다.
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        Debug.Log("전체화면으로 변경");
    }

    // 2. 테두리 없는 창 모드
    public void SetBorderlessWindow()
    {
        // 현재 모니터 해상도에 꽉 차는 테두리 없는 창 모드입니다.
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Debug.Log("테두리 없는 창 모드로 변경");
    }

    // 3. 창 모드 (1280x720)
    public void SetWindowed()
    {
        // 추천해드린 1280x720 크기의 창 모드로 설정합니다.
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        Debug.Log("창 모드 (1280x720)으로 변경");
    }
}