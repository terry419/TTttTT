// --- 파일명: DamageText.cs ---
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
        if (textMesh == null)
        {
            Debug.LogError("DamageText: TextMeshProUGUI 컴포넌트를 찾을 수 없습니다!");
            return;
        }
        textMesh.text = Mathf.RoundToInt(damageAmount).ToString();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        // [수정] 애니메이션 도중 다른 Transform의 영향을 받지 않도록 부모를 잠시 해제
        transform.SetParent(null, true);

        transform.localScale = Vector3.zero;
        Color startColor = textMesh.color;
        startColor.a = 1f;
        textMesh.color = startColor;

        float duration = 0.8f; // 애니메이션 시간을 조금 늘려서 확인하기 쉽게
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