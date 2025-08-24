using UnityEngine;
using System.Collections;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [Tooltip("텍스트가 튀어 오르는 애니메이션의 전체적인 크기를 조절합니다. (1 = 100%)")]
    public float animationScale = 1.0f;
    [Tooltip("애니메이션이 재생되는 시간입니다.")]
    public float animationDuration = 0.8f;
    [Tooltip("텍스트가 위로 올라가는 거리입니다.")]
    public float moveUpDistance = 1.5f;

    private TextMeshProUGUI textMesh;
    private PoolManager poolManager;

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
        if (textMesh == null)
        {
            Debug.LogError("DamageText: 자식 오브젝트에서 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다!");
        }
    }

    void Start()
    {
        // --- [수정] ServiceLocator를 통해 PoolManager를 찾아옵니다. ---
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