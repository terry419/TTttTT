// 추천 경로: Assets/1.Scripts/Data/Shared/StatModifiers.cs
using UnityEngine;
using System;

/// <summary>
/// 카드 장착 시 플레이어에게 적용되는 지속적인 스탯 보너스를 정의하는 공용 클래스입니다.
/// </summary>
[Serializable]
public class StatModifiers
{
    [Tooltip("공격력에 더해지는 % 배율")]
    public float damageMultiplier;
    [Tooltip("공격 속도에 더해지는 % 배율")]
    public float attackSpeedMultiplier;
    [Tooltip("이동 속도에 더해지는 % 배율")]
    public float moveSpeedMultiplier;
    [Tooltip("최대 체력에 더해지는 % 배율")]
    public float healthMultiplier;
    [Tooltip("치명타 확률에 더해지는 % 배율")]
    public float critRateMultiplier;
    [Tooltip("치명타 피해량에 더해지는 % 배율")]
    public float critDamageMultiplier;
}