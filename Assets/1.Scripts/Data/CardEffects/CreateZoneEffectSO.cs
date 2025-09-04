// 경로: Assets/1.Scripts/Data/CardEffects/CreateZoneEffectSO.cs

using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

// --- [신규] 효과 설정 클래스들 ---

[System.Serializable]
public class PlayerZoneEffectSettings
{
    [Tooltip("부여할 상태 효과의 고유 ID. 없으면 빈칸으로 두세요.")]
    public string StatusEffectID = "ZoneDebuff_Player";
    [Tooltip("대상이 장판을 벗어난 후 효과가 지속되는 시간 (초).")]
    public float AppliedEffectDuration = 0f;

    [Header("지속 피해/회복")]
    [Tooltip("초당 가하는 피해량입니다.")]
    public float DamageAmount;
    [Tooltip("초당 회복량입니다.")]
    public float HealAmount;

    [Header("능력치 변경 (Stat Bonuses, % 단위)")]
    [Tooltip("공격력(%)을 변경합니다.")]
    public float AttackBonus;
    [Tooltip("공격 속도(%)를 변경합니다.")]
    public float AttackSpeedBonus;
    [Tooltip("이동 속도(%)를 변경합니다.")]
    public float MoveSpeedBonus;
    [Tooltip("치명타 확률(%)을 변경합니다.")]
    public float CritRateBonus;
    [Tooltip("치명타 피해량(%)을 변경합니다.")]
    public float CritMultiplierBonus;
}

[System.Serializable]
public class EnemyZoneEffectSettings
{
    [Tooltip("부여할 상태 효과의 고유 ID. 없으면 빈칸으로 두세요.")]
    public string StatusEffectID = "ZoneBuff_Enemy";
    [Tooltip("대상이 장판을 벗어난 후 효과가 지속되는 시간 (초).")]
    public float AppliedEffectDuration = 0f;

    [Header("지속 피해/회복")]
    [Tooltip("초당 가하는 피해량입니다.")]
    public float DamageAmount;
    [Tooltip("초당 회복량입니다.")]
    public float HealAmount;

    [Header("능력치 변경 (Stat Bonuses, % 단위)")]
    [Tooltip("공격력(%)을 변경합니다.")]
    public float AttackBonus;
    [Tooltip("이동 속도(%)를 변경합니다.")]
    public float MoveSpeedBonus;
    [Tooltip("받는 피해량(%)을 변경합니다.")]
    public float DamageTakenBonus;
}


/// <summary>
/// [최종] 인스펙터에서 장판의 모든 효과(플레이어/적)를 대상에 맞게 설정할 수 있는 카드 효과 데이터입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_CreateZone_", menuName = "GameData/CardData/Modules/CreateZoneEffect")]
public class CreateZoneEffectSO : CardEffectSO
{
    [Header("--- 장판 공통 설정 ---")]
    [Tooltip("반드시 DamageZoneController.cs 컴포넌트를 가진 '장판' 프리팹이어야 합니다.")]
    public AssetReferenceGameObject ZonePrefabRef;

    [Tooltip("장판 전체가 유지되는 시간 (초).")]
    public float ZoneDuration = 10f;
    [Tooltip("장판의 효과 범위 (반지름).")]
    public float Radius = 5f;

    [Header("--- 범위 내 플레이어에게 부여할 효과 ---")]
    public PlayerZoneEffectSettings PlayerEffects;

    [Header("--- 범위 내 적에게 부여할 효과 ---")]
    public EnemyZoneEffectSettings EnemyEffects;


    public override void Execute(EffectContext context)
    {
        if (ZonePrefabRef == null || !ZonePrefabRef.RuntimeKeyIsValid())
        {
            Debug.LogError($"[{name}] 장판 프리팹이 유효하게 설정되지 않았습니다!");
            return;
        }
        CreateZoneAsync(context).Forget();
    }

    private async UniTaskVoid CreateZoneAsync(EffectContext context)
    {
        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        GameObject zoneGO = await poolManager.GetAsync(ZonePrefabRef.AssetGUID);

        if (zoneGO != null && zoneGO.TryGetComponent<DamageZoneController>(out var zoneController))
        {
            Vector3 spawnPosition = (context.HitTarget != null) ? context.HitTarget.transform.position : context.Caster.transform.position;
            zoneGO.transform.position = spawnPosition;

            zoneController.Initialize(this, context.Caster);
        }
    }
}