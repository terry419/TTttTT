// 파일 경로: Assets/1.Scripts/UI/PlayerHealthBar.cs (수정)
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider healthBarSlider;

    // UI 오브젝트가 활성화될 때 이벤트를 구독합니다.
    private void OnEnable()
    {
        // PlayerDataManager의 static 이벤트에 HandleHealthChanged 함수를 직접 연결합니다.
        PlayerDataManager.OnHealthChanged += HandleHealthChanged;
        Debug.Log("[PlayerHealthBar] OnHealthChanged 이벤트 구독 시작.");
    }

    // UI 오브젝트가 비활성화될 때 이벤트 구독을 해제합니다. (메모리 누수 방지)
    private void OnDisable()
    {
        PlayerDataManager.OnHealthChanged -= HandleHealthChanged;
        Debug.Log("[PlayerHealthBar] OnHealthChanged 이벤트 구독 해제.");
    }

    // 이벤트가 발생했을 때만 호출되는 함수
    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthBarSlider != null && maxHealth > 0)
        {
            healthBarSlider.value = currentHealth / maxHealth;
            Debug.Log($"[PlayerHealthBar] 체력 UI 업데이트: {currentHealth:F1}/{maxHealth:F1}");
        }
    }
}