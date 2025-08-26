using UnityEngine;
using System.Collections;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("ִϸ̼ ")]
    [Tooltip("ؽƮ Ƣ  ִϸ̼ ü ũ⸦ մϴ. (1 = 100%)")]
    public float animationScale = 1.0f;
    [Tooltip("ִϸ̼ Ǵ ðԴϴ.")]
    public float animationDuration = 0.8f;
    [Tooltip("ؽƮ  ö󰡴 ŸԴϴ.")]
    public float moveUpDistance = 1.5f;

    private TextMeshProUGUI textMesh;
    private PoolManager poolManager;

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError("DamageText: ڽ Ʈ TextMeshProUGUI Ʈ ã  ϴ!");
        }
    }

    void Start()
    {
        // --- [] ServiceLocator  PoolManager ãƿɴϴ. ---
        poolManager = ServiceLocator.Get<PoolManager>();
    }

    public void ShowDamage(float damageAmount)
    {
        if (textMesh == null) return;

        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        transform.SetParent(null, true);
        transform.localScale = Vector3.zero;

        Color startColor = textMesh.color;
        startColor.a = 1f;
        textMesh.color = startColor;

        Color endColor = startColor;
        endColor.a = 0f;

        float timer = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (Vector3.up * moveUpDistance);

        float popupDuration = 0.1f;
        Vector3 targetScale = Vector3.one * animationScale;

        while (timer < popupDuration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / popupDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;

        timer = 0f;
        while (timer < animationDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timer / animationDuration);
            textMesh.color = Color.Lerp(startColor, endColor, timer / animationDuration);
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