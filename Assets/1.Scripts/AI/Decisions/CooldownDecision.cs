// 경로: ./TTttTT/Assets/1/Scripts/AI/Decisions/CooldownDecision.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// [신규 결정 부품] 특정 행동의 재사용 대기시간(쿨타임)을 관리하는 '센서'입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Cooldown")]
public class CooldownDecision : Decision
{
    [Tooltip("재사용 대기시간(초)입니다.")]
    public float cooldown = 8f;

    public override bool Decide(MonsterController monster)
    {
        // 1. 몬스터의 개인 쿨타임 보관함을 확인합니다.
        // 이 Decision 에셋(this)에 대한 알람이 맞춰져 있는지 찾아봅니다.
        if (monster.cooldownTimers.TryGetValue(this, out float lastUseTime))
        {
            // 2. 알람이 맞춰져 있다면, 현재 시간과 마지막 사용 시간을 비교합니다.
            if (Time.time - lastUseTime >= cooldown)
            {
                // 3. 쿨타임이 다 지났다면, "사용 가능!"(true)을 반환하고,
                //    즉시 다음 쿨타임을 위해 현재 시간을 새로 기록합니다.
                monster.cooldownTimers[this] = Time.time;
                return true;
            }
            else
            {
                // 4. 아직 쿨타임이 돌고 있다면, "사용 불가!"(false)를 반환합니다.
                return false;
            }
        }
        else
        {
            // 5. 이 스킬을 한 번도 쓴 적이 없다면, 당연히 "사용 가능!"(true)을 반환하고,
            //    최초 사용 시간을 기록합니다.
            monster.cooldownTimers.Add(this, Time.time);
            return true;
        }
    }
}