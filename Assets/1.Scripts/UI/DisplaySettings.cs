// 파일 경로: ./TTttTT/Assets/1.Scripts/UI/DisplaySettings.cs
using UnityEngine;

public class DisplaySettings : MonoBehaviour
{
    public void SetFullScreen()
    {
        Debug.Log("[DisplaySettings] '전체 화면' 버튼 클릭됨. Screen.fullScreenMode를 'ExclusiveFullScreen'으로 설정 시도.");
        // 에디터에서는 동작하지 않는 것이 정상입니다.
#if !UNITY_EDITOR
        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
#else
        Debug.LogWarning("[DisplaySettings] 에디터 환경에서는 ExclusiveFullScreen 모드를 지원하지 않습니다.");
#endif
    }

    public void SetBorderlessWindow()
    {
        Debug.Log("[DisplaySettings] '테두리 없는 창' 버튼 클릭됨. Screen.fullScreenMode를 'FullScreenWindow'로 설정 시도.");
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
    }

    public void SetWindowed()
    {
        Debug.Log("[DisplaySettings] '창 모드' 버튼 클릭됨. Screen.SetResolution(1280x720) 호출 시도.");
        // 에디터의 Game 뷰가 'Free Aspect'가 아니면 크기가 고정될 수 있습니다.
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }
}