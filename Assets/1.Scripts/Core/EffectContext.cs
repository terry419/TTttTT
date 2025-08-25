using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 카드 효과가 실행될 때 필요한 모든 맥락 정보를 담는 데이터 클래스입니다.
/// 이 객체는 EffectExecutor가 생성하여 각 CardEffectSO의 Execute 메소드에 전달합니다.
/// </summary>
public class EffectContext
{
    /// <summary>
    /// 효과를 발동시킨 주체(플레이어)의 CharacterStats입니다.
    /// </summary>
    public CharacterStats Caster;

    /// <summary>
    /// 효과가 시작되는 월드 좌표입니다. (예: 플레이어의 총구, 몬스터의 피격 위치)
    /// </summary>
    public Transform SpawnPoint;

    /// <summary>
    /// 최초 발사 시의 목표 대상입니다. (타겟팅 시스템에 의해 결정됨)
    /// </summary>
    public MonsterController InitialTarget;

    /// <summary>
    /// 효과가 적중한 월드 좌표입니다. (OnHit, OnCrit 등의 트리거에서 사용)
    /// </summary>
    public Vector3 HitPosition;

    /// <summary>
    /// 해당 효과 연쇄 반응에서 발생한 피해량입니다. (흡혈 등에서 사용)
    /// </summary>
    public float DamageDealt;

    /// <summary>
    /// 효과를 발동시킨 피격이 치명타였는지 여부입니다.
    /// </summary>
    public bool IsCritical;
    
    /// <summary>
    /// 효과를 발동시킨 피격으로 대상이 사망했는지 여부입니다.
    /// </summary>
    public bool IsKill;

    /// <summary>
    /// ShotgunPatternSO 같은 '수정' 옵션이 계산한 발사 궤적 목록입니다.
    /// 이 목록이 비어있지 않으면 ProjectileEffectSO는 이 궤적을 사용합니다.
    /// </summary>
    public List<Vector2> FiringDirections = new List<Vector2>();
}