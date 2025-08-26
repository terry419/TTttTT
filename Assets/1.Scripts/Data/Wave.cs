// --- ϸ: Wave.cs ---
using UnityEngine;

// [߰]   ϴ enum.  κ  ù °  ߻߾.
public enum SpawnType
{
    Spread, //  ð   
    Burst   //   ͸ 
}

[System.Serializable]
public class Wave
{
    [Tooltip(" ̺꿡   (SO)  ⿡ ϼ.")]
    // [] string  MonsterDataSO  մϴ.
    public MonsterDataSO monsterData;

    [Tooltip("  ")]
    public int count;

    [Tooltip("SpawnType Spread , ù ͺ  ͱ Ǵ  ɸ  ðԴϴ.")]
    public float duration = 10f;

    [Tooltip(" ̺갡    ̺갡 ۵Ǳ  ðԴϴ.")]
    public float delayAfterWave;

    [Tooltip("  (Spread: ð, Burst: )")]
    public SpawnType spawnType;

    // ▼▼▼ [3] 이 줄을 추가하세요. ▼▼▼
    [Tooltip("이 웨이브의 몬스터를 몇 마리 미리 생성할지 정합니다. 0으로 두면 'Count' 값을 따릅니다.")]
    public int preloadCount = 50;
}