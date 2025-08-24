// 파일명: SceneTransitionManager.cs (리팩토링 완료)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    private Image fadeOverlay;

    private void Awake()
    {
        ServiceLocator.Register<SceneTransitionManager>(this);
        DontDestroyOnLoad(gameObject);
        InitializeFadeOverlay();
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

    public void LoadScene(string sceneName)
    {
        StopAllCoroutines();
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

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