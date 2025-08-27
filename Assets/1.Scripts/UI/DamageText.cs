using UnityEngine;
using System.Collections;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("데미지 텍스트 애니메이션 설정")]
    [Tooltip("텍스트 팝업 시 최종 크기를 조절합니다. (1 = 100%)")]
    public float animationScale = 1.0f;
    [Tooltip("텍스트가 사라지는 총 시간입니다.")]
    public float animationDuration = 0.8f;
    [Tooltip("텍스트가 위로 이동하는 거리입니다.")]
    public float moveUpDistance = 1.5f;

    private TextMeshProUGUI textMesh;
    private PoolManager poolManager;

    private void Awake()
    {
        Debug.Log("[DamageText] Awake called.");
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError("DamageText: TextMeshProUGUI component NOT found in children!");
        }
        else
        {
            Debug.Log("[DamageText] TextMeshProUGUI component found.");
        }
    }

    void Start()
    {
        Debug.Log("[DamageText] Start called.");
        poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null)
        {
            Debug.LogError("[DamageText] PoolManager is null in Start.");
        }
    }

    public void ShowDamage(float damageAmount)
    {
        Debug.Log($"[DamageText] ShowDamage called with amount: {damageAmount}");
        if (textMesh == null)
        {
            Debug.LogError("[DamageText] textMesh is null in ShowDamage. Cannot set text.");
            return;
        }

        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        Debug.Log($"[DamageText] Setting text to: {textMesh.text}");
        StartCoroutine(Animate());
        Debug.Log("[DamageText] Animate coroutine started.");
    }

    private IEnumerator Animate()
    {
        Debug.Log("[DamageText] Animate coroutine started.");
        transform.SetParent(null, true); // This line is still suspicious for UI elements
        transform.localScale = Vector3.zero;
        Debug.Log($"[DamageText] Initial scale set to: {transform.localScale}");

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
        Debug.Log($"[DamageText] Popup animation complete. Final scale: {transform.localScale}");

        timer = 0f;
        while (timer < animationDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timer / animationDuration);
            textMesh.color = Color.Lerp(startColor, endColor, timer / animationDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        Debug.Log("[DamageText] Fade and move animation complete.");

        if (poolManager != null)
        {
            Debug.Log("[DamageText] Releasing GameObject to pool.");
            poolManager.Release(gameObject);
        }
        else
        {
            Debug.LogWarning("[DamageText] PoolManager is null. Destroying GameObject.");
            Destroy(gameObject);
        }
    }
}