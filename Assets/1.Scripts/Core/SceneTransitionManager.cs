using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public Image fadeOverlay; // Canvas ���� �ִ� Image ������Ʈ

    private void Awake()
    {
        // ó�� ���� �� ���� ȭ�鿡�� �������� ���̵� ��
        StartCoroutine(FadeIn());
    }

    public void LoadScene(string sceneName)
    {
        // ȣ�� �� ���� �� ���� ȭ������ ���̵� �ƿ� �� �� �ε�
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeIn()
    {
        float t = 1f;
        // ���� ��: ����(1), �� ��: ����(0)
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
        // ���� ��: ����(0), �� ��: ����(1)
        while (t < 1)
        {
            t += Time.deltaTime;
            fadeOverlay.color = new Color(0, 0, 0, t);
            yield return null;
        }
        SceneManager.LoadScene(sceneName);
    }
}
