// --- 파일명: DamageText.cs (개선된 버전) ---
// 경로: Assets/1.Scripts/UI/DamageText.cs
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
        poolManager = PoolManager.Instance;
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

        // [수정] 사라질 때의 색상을 미리 계산합니다.
        // RGB 값은 그대로 두고, Alpha(투명도)만 0으로 설정합니다.
        Color endColor = startColor;
        endColor.a = 0f;

        float timer = 0f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + (Vector3.up * moveUpDistance);

        // Pop-up effect
        float popupDuration = 0.1f;
        Vector3 targetScale = Vector3.one * animationScale;

        while (timer < popupDuration)
        {
            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / popupDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;

        // Move up and fade out
        timer = 0f;
        while (timer < animationDuration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timer / animationDuration);
            // [수정] Color.clear 대신 미리 계산해둔 endColor를 사용합니다.
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
