// 파일 경로: Assets/1.Scripts/UI/PlayerHealthBar.cs

using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider healthBarSlider;

    // 특정 스탯 컴포넌트가 아닌, 범용 EntityStats를 참조하도록 변경
    private EntityStats entityStats;

    // Awake에서 자신과 같은 게임 오브젝트에 있는 부모 클래스(EntityStats)를 찾아둡니다.
    void Awake()
    {
        entityStats = GetComponentInParent<EntityStats>();
    }

    private void OnEnable()
    {
        if (entityStats != null)
        {
            // EntityStats의 체력 변경 이벤트를 구독합니다.
            entityStats.OnHealthChanged += HandleHealthChanged;
            // 활성화될 때 현재 체력으로 UI를 즉시 업데이트합니다.
            HandleHealthChanged(entityStats.CurrentHealth, entityStats.FinalHealth);
        }
    }

    private void OnDisable()
    {
        if (entityStats != null)
        {
            // 구독을 해제합니다.
            entityStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthBarSlider != null && maxHealth > 0)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }
}