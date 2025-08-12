using UnityEngine;

[System.Serializable]
public class BaseStats
{
    public float baseDamage;          // 기본 공격력
    public float baseAttackSpeed;     // 기본 공격 속도
    public float baseMoveSpeed;       // 기본 이동 속도
    public float baseHealth;          // 기본 체력
    public float baseCritRate;        // 기본 치명타 확률 (0.1 = 10%)
    public float baseCritDamage;      // 기본 치명타 피해량 (1.5 = 150%)
}
