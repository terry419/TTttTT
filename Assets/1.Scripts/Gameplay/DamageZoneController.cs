// 경로: Assets/1.Scripts/Gameplay/DamageZoneController.cs
using UnityEngine;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// [v6] 핵심 버그(ID 없으면 작동안함) 수정 및 크기 자동 계산 기능이 추가된 최종 버전입니다.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class DamageZoneController : MonoBehaviour
{
    private CreateZoneEffectSO zoneData;
    private CharacterStats playerCaster;
    private float lifeTimer;
    private CircleCollider2D zoneCollider;
    private Transform visualsTransform;
    private float baseSpriteDiameter = 1f; // 스프라이트의 월드 유닛 기준 기본 지름
    private readonly Dictionary<GameObject, StatusEffectInstance> appliedEffects = new Dictionary<GameObject, StatusEffectInstance>();

    void Awake()
    {
        zoneCollider = GetComponent<CircleCollider2D>();
        zoneCollider.isTrigger = true;

        visualsTransform = transform.Find("Visuals");
        if (visualsTransform != null)
        {
            SpriteRenderer visualsRenderer = visualsTransform.GetComponent<SpriteRenderer>();
            if (visualsRenderer != null && visualsRenderer.sprite != null)
            {
                baseSpriteDiameter = visualsRenderer.sprite.bounds.size.x;
                if (baseSpriteDiameter == 0) baseSpriteDiameter = 1f; // 0일 경우의 오류 방지
            }
        }
    }

    public void Initialize(CreateZoneEffectSO data, CharacterStats caster)
    {
        this.zoneData = data;
        this.playerCaster = caster;
        lifeTimer = 0f;

        zoneCollider.radius = data.Radius;

        if (visualsTransform != null)
        {
            float requiredScale = (data.Radius * 2f) / baseSpriteDiameter;
            visualsTransform.localScale = Vector3.one * requiredScale;
        }

        appliedEffects.Clear();
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (zoneData == null) return;
        lifeTimer += Time.deltaTime;
        if (lifeTimer >= zoneData.ZoneDuration)
        {
            ServiceLocator.Get<PoolManager>()?.Release(gameObject);
        }
    }

    void OnDisable()
    {
        foreach (var instance in appliedEffects.Values)
        {
            ServiceLocator.Get<StatusEffectManager>()?.RemoveStatusEffect(instance);
        }
        appliedEffects.Clear();
        zoneData = null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (zoneData == null || appliedEffects.ContainsKey(other.gameObject)) return;

        if (other.CompareTag(Tags.Player))
        {
            ApplyPlayerEffects(other.gameObject);
        }
        else if (other.CompareTag(Tags.Monster))
        {
            ApplyEnemyEffects(other.gameObject);
        }
    }

    private void ApplyPlayerEffects(GameObject target)
    {
        var settings = zoneData.PlayerEffects;
        if (settings == null) return;

        bool hasAnyEffect = settings.DamageAmount != 0 || settings.HealAmount != 0 || settings.AttackBonus != 0 || settings.AttackSpeedBonus != 0 || settings.MoveSpeedBonus != 0 || settings.CritRateBonus != 0 || settings.CritMultiplierBonus != 0;
        if (!hasAnyEffect) return;

        var bonuses = new Dictionary<StatType, float>();
        if (settings.AttackBonus != 0) bonuses.Add(StatType.Attack, settings.AttackBonus);
        if (settings.AttackSpeedBonus != 0) bonuses.Add(StatType.AttackSpeed, settings.AttackSpeedBonus);
        if (settings.MoveSpeedBonus != 0) bonuses.Add(StatType.MoveSpeed, settings.MoveSpeedBonus);
        if (settings.CritRateBonus != 0) bonuses.Add(StatType.CritRate, settings.CritRateBonus);
        if (settings.CritMultiplierBonus != 0) bonuses.Add(StatType.CritMultiplier, settings.CritMultiplierBonus);

        string effectId = string.IsNullOrEmpty(settings.StatusEffectID) ? $"TempZoneEffect_Player_{GetInstanceID()}" : settings.StatusEffectID;
        var effectInstance = new StatusEffectInstance(target, effectId, settings.AppliedEffectDuration, bonuses, settings.DamageAmount, DamageType.Flat, false, settings.HealAmount, 1f, HealType.Flat, StackingBehavior.NoStack, playerCaster, null, null, null);
        
        ApplyAndLogEffect(target, "Player", effectInstance, bonuses, settings.HealAmount);
    }

    private void ApplyEnemyEffects(GameObject target)
    {
        var settings = zoneData.EnemyEffects;
        if (settings == null) return;

        bool hasAnyEffect = settings.DamageAmount != 0 || settings.HealAmount != 0 || settings.AttackBonus != 0 || settings.MoveSpeedBonus != 0 || settings.DamageTakenBonus != 0;
        if (!hasAnyEffect) return;

        var bonuses = new Dictionary<StatType, float>();
        if (settings.AttackBonus != 0) bonuses.Add(StatType.Attack, settings.AttackBonus);
        if (settings.MoveSpeedBonus != 0) bonuses.Add(StatType.MoveSpeed, settings.MoveSpeedBonus);
        if (settings.DamageTakenBonus != 0) bonuses.Add(StatType.DamageTaken, settings.DamageTakenBonus);

        string effectId = string.IsNullOrEmpty(settings.StatusEffectID) ? $"TempZoneEffect_Enemy_{GetInstanceID()}" : settings.StatusEffectID;
        var effectInstance = new StatusEffectInstance(target, effectId, settings.AppliedEffectDuration, bonuses, settings.DamageAmount, DamageType.Flat, false, settings.HealAmount, 1f, HealType.Flat, StackingBehavior.NoStack, playerCaster, null, null, null);

        ApplyAndLogEffect(target, "Enemy", effectInstance, bonuses, settings.HealAmount);
    }

    private void ApplyAndLogEffect(GameObject target, string targetType, StatusEffectInstance effectInstance, Dictionary<StatType, float> bonuses, float healAmount)
    {
        ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(target, effectInstance);
        appliedEffects.Add(target, effectInstance);

        var logBuilder = new StringBuilder();
        logBuilder.Append($"[DamageZone] '{target.name}' ({targetType}) entered. Applying effects from '{zoneData.name}'.");
        logBuilder.Append($" | ID: {effectInstance.EffectId}");
        if (effectInstance.DamagePerSecond > 0) logBuilder.Append($", DMG/s: {effectInstance.DamagePerSecond}");
        if (healAmount > 0) logBuilder.Append($", Heal/s: {healAmount}");
        
        if (bonuses.Count > 0)
        {
            logBuilder.Append(" | Bonuses: ");
            foreach (var bonus in bonuses)
            {
                logBuilder.Append($"{bonus.Key}: {bonus.Value}%, ");
            }
            logBuilder.Length -= 2;
        }

        Debug.Log(logBuilder.ToString());
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (appliedEffects.TryGetValue(other.gameObject, out var effectInstance))
        {
            if (effectInstance.RemainingDuration <= 0)
            {
                ServiceLocator.Get<StatusEffectManager>()?.RemoveStatusEffect(effectInstance);
            }
            appliedEffects.Remove(other.gameObject);
        }
    }
}
