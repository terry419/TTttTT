using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class SequentialPayload
{
    [Tooltip("이 효과가 발동될 바운스 횟수. 0은 최초 피격, 1은 첫 번째 튕김을 의미합니다.")]
    public int onBounceNumber;
    [Tooltip("해당 순서에 발동시킬 CardEffectSO 에셋의 Addressable ID (에셋 파일명)")]
    public string effectID;
}

[CreateAssetMenu(fileName = "Projectile_", menuName = "GameData/Card Effects/Projectile Effect")]
public class ProjectileEffectSO : CardEffectSO
{
    [Header("투사체 기본 설정")]
    [Tooltip("발사할 투사체 프리팹")]
    public GameObject bulletPrefab;
    [Tooltip("투사체 속도")]
    public float speed = 10f;
    [Tooltip("투사체 기본 피해량")]
    public float baseDamage = 10f;

    [Header("특수 기능")]
    [Tooltip("관통 횟수")]
    public int pierceCount = 0;
    [Tooltip("리코셰(튕김) 횟수")]
    public int ricochetCount = 0;
    [Tooltip("true이면 이미 맞춘 적에게 다시 튕길 수 있습니다 (단, 연속으로는 안됨)")]
    public bool canRicochetToSameTarget = false;
    [Tooltip("true이면 가장 가까운 적을 추적합니다.")]
    public bool isTracking = false;

    [Header("피격 시 연쇄 효과 (Payload)")]
    [Tooltip("투사체가 적에게 명중했을 때 순차적으로 발동할 효과 목록")]
    public List<SequentialPayload> sequentialPayloads;

    [Header("시각 효과 (VFX)")]
    [Tooltip("피격 시 재생할 VFX의 ID")]
    public string onHitVFXKey;
    [Tooltip("치명타 피격 시 재생할 VFX의 ID")]
    public string onCritVFXKey;
    [Tooltip("수명이 다해 소멸 시 재생할 VFX의 ID")]
    public string onExpireVFXKey;

    public override void Execute(EffectContext context)
    {
        // 이 로직은 6단계(EffectExecutor) 및 7단계(BulletController)에서 최종 구현됩니다.
        Debug.Log($"<color=lime>[ProjectileEffect]</color> '{this.name}' 실행. (로직 구현 대기중)");
    }
}