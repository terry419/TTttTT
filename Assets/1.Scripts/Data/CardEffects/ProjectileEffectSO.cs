using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;

/// <summary>
/// 피격 시 순차적으로 발동할 연쇄 효과를 정의하는 클래스입니다.
/// </summary>
[Serializable]
public class SequentialPayload
{
    [Tooltip("이 효과가 발동될 튕김 횟수입니다. (0 = 최초 피격 시)")]
    public int onBounceNumber = 0;

    [Tooltip("발동시킬 효과 모듈입니다.")]
    public AssetReferenceT<CardEffectSO> effectToTrigger;
}

/// <summary>
/// 투사체에 관통, 튕김, 추적 등의 특수 능력을 부여하는 모듈입니다.
/// </summary>
[CreateAssetMenu(fileName = "Module_Projectile_", menuName = "GameData/v8.0/Modules/ProjectileEffect")]
public class ProjectileEffectSO : CardEffectSO, IPreloadable
{
    [Header("발사체 기본 설정")]
    [Tooltip("타겟팅 방식 (전방, 가장 가까운 적 등)")]
    public TargetingType targetingType = TargetingType.Nearest;

    [Tooltip("발사할 투사체 프리팹의 Addressable 참조")]
    public AssetReferenceGameObject bulletPrefabReference;

    [Tooltip("투사체 속도. 플랫폼의 baseSpeed에 곱해질 배율입니다. (1 = 100%)")]
    public float speed = 1f;

    [Header("투사체 특수 능력")]
    [Tooltip("관통 횟수")]
    public int pierceCount = 0;

    [Tooltip("튕김 횟수")]
    public int ricochetCount = 0;

    [Tooltip("이미 맞춘 적에게 다시 튕길 수 있는지 여부")]
    public bool canRicochetToSameTarget = false;

    [Tooltip("가장 가까운 적을 추적하는 기능")]
    public bool isTracking = false;

    [Header("VFX 설정 (Addressable Key)")]
    [Tooltip("피격 시 재생할 VFX의 Addressable 참조")]
    public AssetReferenceGameObject onHitVFXRef;

    [Tooltip("치명타 피격 시 재생할 VFX의 Addressable 참조")]
    public AssetReferenceGameObject onCritVFXRef;

    [Tooltip("투사체 소멸 시 재생할 VFX의 Addressable 참조")]
    public AssetReferenceGameObject onExpireVFXRef;

    [Header("연쇄 효과 (Sequential Payloads)")]
    [Tooltip("피격 또는 튕김 시 순차적으로 발동할 효과 목록")]
    public List<SequentialPayload> sequentialPayloads;

    public ProjectileEffectSO()
    {
        // 이 모듈은 발사 시점에 투사체를 생성하는 역할을 하므로 OnFire가 기본값입니다.
        trigger = EffectTrigger.OnFire;
    }

    public override void Execute(EffectContext context)
    {
        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행.");
        // 이 모듈은 데이터를 제공하는 역할이 핵심입니다.
        // 실제 투사체 발사 로직은 이 데이터를 읽어갈 EffectExecutor에서 처리됩니다.
    }

    // [추가] IPreloadable 인터페이스 구현
    public IEnumerable<AssetReferenceGameObject> GetPrefabsToPreload()
    {
        if (bulletPrefabReference != null && bulletPrefabReference.RuntimeKeyIsValid())
            yield return bulletPrefabReference;
        
        if (onHitVFXRef != null && onHitVFXRef.RuntimeKeyIsValid())
            yield return onHitVFXRef;

        if (onCritVFXRef != null && onCritVFXRef.RuntimeKeyIsValid())
            yield return onCritVFXRef;

        if (onExpireVFXRef != null && onExpireVFXRef.RuntimeKeyIsValid())
            yield return onExpireVFXRef;

        // The user's request did not include onExpireVFXRef, so I will not add it.
    }
}