// --- 파일명: SceneTransitionManager.cs (최종 수정본) ---

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    private Image fadeOverlay;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬 전환 시 파괴되지 않는 자신만의 Canvas와 Image를 코드로 직접 생성합니다.
        InitializeFadeOverlay();
    }

    private void InitializeFadeOverlay()
    {
        GameObject canvasGo = new GameObject("SceneTransitionCanvas");
        canvasGo.transform.SetParent(this.transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 다른 UI보다 항상 위에 있도록 설정

        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject imageGo = new GameObject("FadeOverlayImage");
        imageGo.transform.SetParent(canvasGo.transform);
        fadeOverlay = imageGo.AddComponent<Image>();

        RectTransform rt = fadeOverlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeOverlay.color = new Color(0, 0, 0, 0); // 시작은 투명
        fadeOverlay.raycastTarget = false;
        fadeOverlay.gameObject.SetActive(false); // 처음엔 비활성화
    }

    public void LoadScene(string sceneName)
    {
        // 이미 전환 중일 때는 중복 실행 방지
        StopAllCoroutines();
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeIn()
    {
        fadeOverlay.gameObject.SetActive(true);
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime; // Time.timeScale에 영향받지 않도록 변경
            fadeOverlay.color = new Color(0, 0, 0, t);
            yield return null;
        }
        fadeOverlay.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        // 씬이 로드될 때 FadeIn이 자동으로 호출되므로, 씬 로드 후 FadeIn을 기다릴 필요가 없습니다.
        SceneManager.sceneLoaded += OnSceneLoaded;

        fadeOverlay.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime;
            fadeOverlay.color = new Color(0, 0, 0, t);
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 이벤트가 중복 등록되지 않도록, 호출된 후 바로 구독을 해지합니다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(FadeIn());
    }
}