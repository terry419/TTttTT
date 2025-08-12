using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public Image fadeOverlay; // Canvas 위에 있는 Image 컴포넌트

    private void Awake()
    {
        // 처음 진입 시 검은 화면에서 투명으로 페이드 인
        StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        // 호출 시 투명 → 검은 화면으로 페이드 아웃 후 씬 로드
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeIn()
    {
        float t = 1f;
        // 시작 색: 검정(1), 끝 색: 투명(0)
        while (t > 0)
        {
            t -= Time.deltaTime;
            fadeOverlay.color = new Color(0, 0, 0, t);
            yield return null;
        }
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        float t = 0f;
        // 시작 색: 투명(0), 끝 색: 검정(1)
        while (t < 1)
        {
            t += Time.deltaTime;
            fadeOverlay.color = new Color(0, 0, 0, t);
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }
}
