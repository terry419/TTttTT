// --- 파일명: Wave.cs ---
// 경로: Assets/1.Scripts/Data/Wave.cs

using UnityEngine;

public enum SpawnType
{
    Spread, // 시간 간격을 두고 몬스터를 순차적으로 스폰합니다.
    Burst   // 설정된 개수만큼 한 번에 몬스터를 스폰합니다.
}

[System.Serializable]
public class Wave
{
    [Tooltip("스폰할 몬스터 데이터(SO)를 지정합니다.")]
    public MonsterDataSO monsterData;

    [Tooltip("스폰할 몬스터의 개수를 설정합니다.")]
    public int count;

    [Tooltip("Spread 타입일 때 총 스폰 기간(초)을 설정합니다.")]
    public float duration = 10f;

    [Tooltip("웨이브 완료 후 다음 웨이브까지 대기할 시간(초)입니다.")]
    public float delayAfterWave;

    [Tooltip("이 웨이브의 스폰 방식을 선택합니다 (Spread: 시간 간격, Burst: 일괄 스폰).")]
    public SpawnType spawnType;

    [Tooltip("웨이브 시작 전에 미리 생성할 몬스터 수를 설정합니다. 0으로 두면 'count' 값을 사용합니다.")]
    public int preloadCount = 50;
}
