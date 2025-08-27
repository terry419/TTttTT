using UnityEngine;

public class UIFeedbackManager : MonoBehaviour
{
    void OnEnable()
    {
        MonsterController.OnMonsterDamaged += HandleMonsterDamaged;
    }

    void OnDisable()
    {
        MonsterController.OnMonsterDamaged -= HandleMonsterDamaged;
    }

    private async void HandleMonsterDamaged(float damageAmount, Vector3 position)
    {
        Debug.Log($"[UIFeedbackManager] HandleMonsterDamaged called. Damage: {damageAmount}, Position: {position}");

        if (ServiceLocator.Get<PoolManager>() == null)
        {
            Debug.LogError("[UIFeedbackManager] PoolManager is null in HandleMonsterDamaged.");
            return;
        }

                string key = PrefabKeys.DamageTextCanvas; // PrefabKeys에서 키 사용
        GameObject textGO = await ServiceLocator.Get<PoolManager>().GetAsync(key);

        if (textGO == null)
        {
            Debug.LogError($"[UIFeedbackManager] PoolManager.GetAsync returned null for key: {key}");
            return;
        }
        Debug.Log($"[UIFeedbackManager] Successfully got GameObject from pool. Name: {textGO.name}, Active: {textGO.activeSelf}");
        
        textGO.transform.position = position + Vector3.up * 0.5f;

        DamageText damageTextComponent = textGO.GetComponent<DamageText>();
        if (damageTextComponent != null)
        {
            Debug.Log("[UIFeedbackManager] DamageText component found. Calling ShowDamage.");
            damageTextComponent.ShowDamage(damageAmount);
        }
        else
        {
            Debug.LogError("[UIFeedbackManager] DamageText component NOT found on pooled GameObject!");
        }
    }
}
