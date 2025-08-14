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
    [Tooltip("이 웨이브의 타입을 선택하세요. (Spread: 꾸준히, Burst: 한 번에)")]
    public SpawnType spawnType; // [추가] 스폰 타입 선택 필드. 이 부분이 없어서 두 번째 에러가 발생했어.

    public string monsterName;
    public int count;

    [Tooltip("SpawnType이 Spread일 때만 사용됩니다. 몬스터를 모두 생성하는 데 걸리는 총 시간입니다.")]
    public float duration = 10f;

    [Tooltip("이 웨이브가 끝난 후 다음 웨이브가 시작되기까지의 대기 시간입니다.")]
    public float delayAfterWave;
}