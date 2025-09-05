// 경로: ./TTttTT/Assets/1/Scripts/AI/Decisions/AllyCountDecision.cs
using UnityEngine;

/// <summary>
/// [신규 결정 부품] 주변의 아군 몬스터 숫자를 기준으로 판단하는 '센서'입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Decisions/Ally Count")]
public class AllyCountDecision : Decision
{
    [Tooltip("아군을 탐지할 주변 반경입니다.")]
    public float checkRadius = 10f;

    [Tooltip("판단 기준이 되는 아군의 숫자입니다.")]
    public int allyCountThreshold = 2;

    [Tooltip("체크: 아군이 기준치 '미만'일 때 참 / 체크 해제: 아군이 기준치 '이상'일 때 참")]
    public bool triggerWhenBelow = true;

    // 주변 몬스터를 담을 임시 배열 (매번 새로 생성하지 않아 성능에 유리합니다)
    private static Collider2D[] _monsterColliders = new Collider2D[50];

    public override bool Decide(MonsterController monster)
    {
        // Physics2D.OverlapCircleNonAlloc은 지정된 위치에 있는 모든 콜라이더를 찾아 배열에 담아주고, 그 숫자를 반환합니다.
        int hitCount = Physics2D.OverlapCircleNonAlloc(monster.transform.position, checkRadius, _monsterColliders, LayerMask.GetMask("Monster"));

        // 찾은 숫자에서 자기 자신(1)을 뺀 것이 순수한 주변 아군의 숫자입니다.
        int allyCount = hitCount - 1;

        if (triggerWhenBelow)
        {
            // 주변 아군이 기준치 '미만'일 때 true를 반환합니다.
            return allyCount < allyCountThreshold;
        }
        else
        {
            // 주변 아군이 기준치 '이상'일 때 true를 반환합니다.
            return allyCount >= allyCountThreshold;
        }
    }
}