// 파일 경로: Assets/1.Scripts/Gameplay/BossStats.cs

using UnityEngine;

// EntityStats를 상속받아 보스 전용 기능을 구현합니다.
public class BossStats : EntityStats
{
    // Die() 추상 메서드를 보스에 맞게 구현합니다.
    protected override void Die()
    {
        Debug.Log($"[BossStats] 보스({gameObject.name})가 사망했습니다.");
        // 향후 여기에 보스 사망 시의 로직(예: 게임 승리)을 추가할 수 있습니다.
        gameObject.SetActive(false);
    }
}