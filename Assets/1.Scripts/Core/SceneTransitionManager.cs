// SceneTransitionManager.cs - 수정된 최종본

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic; // Stack 사용을 위해 추가

public class SceneTransitionManager : MonoBehaviour
{
    private Image fadeOverlay;
    private readonly Stack<string> sceneStack = new Stack<string>(); // 씬 기록을 위한 스택

    private void Awake()
    {
        if (!ServiceLocator.IsRegistered<SceneTransitionManager>())
        {
            ServiceLocator.Register<SceneTransitionManager>(this);
            DontDestroyOnLoad(gameObject);
            InitializeFadeOverlay();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFadeOverlay()
    {
        GameObject canvasGo = new GameObject("SceneTransitionCanvas");
        canvasGo.transform.SetParent(this.transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        GameObject imageGo = new GameObject("FadeOverlayImage");
        imageGo.transform.SetParent(canvasGo.transform);
        fadeOverlay = imageGo.AddComponent<Image>();

        RectTransform rt = fadeOverlay.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        fadeOverlay.color = new Color(0, 0, 0, 0);
        fadeOverlay.raycastTarget = false;
        fadeOverlay.gameObject.SetActive(false);
    }

    // --- 기존 함수 수정 ---
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneTransition] Loading new scene: {sceneName}");
        sceneStack.Clear(); // 씬을 완전히 새로 로드하므로, 스택을 비웁니다.
        StopAllCoroutines();
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    // --- 새 함수 추가 ---
    public void LoadSceneAdditive(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        sceneStack.Push(sceneName);
        Debug.Log($"[SceneTransition] Loaded '{sceneName}' additively. Stack count: {sceneStack.Count}");
    }

    public void UnloadTopScene()
    {
        if (sceneStack.Count > 0)
        {
            string sceneToUnload = sceneStack.Pop();
            SceneManager.UnloadSceneAsync(sceneToUnload);
            Debug.Log($"[SceneTransition] Unloaded '{sceneToUnload}'. Stack count: {sceneStack.Count}");
        }
        else
        {
            Debug.LogWarning("[SceneTransition] Scene stack is empty. Cannot go back.");
        }
    }

    // --- 기존 코루틴 ---
    private IEnumerator FadeIn()
    {
        fadeOverlay.gameObject.SetActive(true);
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            fadeOverlay.color = new Color(0, 0, 0, t);
            yield return null;
        }
        fadeOverlay.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
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
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(FadeIn());
    }
}
