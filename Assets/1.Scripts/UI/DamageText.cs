using UnityEngine;
using System.Collections;
using TMPro;

public class DamageText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private PoolManager poolManager;

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    void Start()
    {
        poolManager = PoolManager.Instance;
    }

    public void ShowDamage(float damageAmount)
    {
        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        transform.localScale = Vector3.zero;
        Color startColor = textMesh.color;
        startColor.a = 1f;
        textMesh.color = startColor;

        float duration = 0.5f;
        float timer = 0f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * 1.5f;

        // Pop-up effect
        float popupDuration = 0.1f;
        while (timer < popupDuration)
        {
            transform.localScale = Vector3.one * (timer / popupDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;

        // Move up and fade out
        timer = 0f;
        while (timer < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timer / duration);
            textMesh.color = Color.Lerp(startColor, Color.clear, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        if (poolManager != null)
        {
            poolManager.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
