// --- 파일명: Wave.cs ---
using UnityEngine;

// [추가] 스폰 방식을 정의하는 enum. 이 부분이 없어서 첫 번째 에러가 발생했어.
public enum SpawnType
{
    Spread, // 지정된 시간 동안 꾸준히 생성
    Burst   // 한 번에 와르르 생성
}

[System.Serializable]
public class Wave
{
    [Tooltip("이 웨이브에서 스폰할 몬스터의 데이터(SO)를 직접 여기에 연결하세요.")]
    // [수정] string 대신 MonsterDataSO를 직접 참조합니다.
    public MonsterDataSO monsterData;

    [Tooltip("스폰할 몬스터의 수")]
    public int count;

    [Tooltip("SpawnType이 Spread일 때, 첫 몬스터부터 마지막 몬스터까지 스폰되는 데 걸리는 총 시간입니다.")]
    public float duration = 10f;

    [Tooltip("이 웨이브가 끝난 후 다음 웨이브가 시작되기까지의 대기 시간입니다.")]
    public float delayAfterWave;

    [Tooltip("스폰 방식 (Spread: 시간차, Burst: 동시)")]
    public SpawnType spawnType;
}