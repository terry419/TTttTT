using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "BossStageData", menuName = "Data/Boss Stage Data", order = 1)]
public class BossStageDataSO : ScriptableObject
{
    [Header("Boss Configuration")]
    [Tooltip("The boss character's data asset.")]
    public CharacterDataSO bossCharacterData;

    [Tooltip("The boss character's prefab to be spawned.")]
    public AssetReferenceGameObject bossPrefab;

    [Header("Monster Waves")]
    [Tooltip("The sequence of monster waves to be spawned during the boss stage.")]
    public List<Wave> waves;

    [Header("Interactive Spawn Rules")]
    [Tooltip("주기적으로 킬 수를 정산하고 상대에게 몬스터를 보낼 시간 간격 (초)")]
    public float reinforcementInterval = 10f;

    [Tooltip("상대방이 기록한 킬 수에 비례하여 보낼 몬스터의 비율 (예: 0.1은 10%)")]
    [Range(0, 1)] public float reinforcementRatio = 0.1f;

    [Tooltip("이 누적 킬 수를 달성할 때마다 상대에게 특수 몬스터를 보냅니다.")]
    public int milestoneKillTarget = 50;
    
    [Tooltip("주기적 증원으로 보낼 몬스터의 종류 목록입니다. 목록에서 무작위로 선택됩니다.")]
    public List<MonsterDataSO> reinforcementMonsters;

    [Tooltip("마일스톤 달성 시 보낼 특수 몬스터의 데이터 (ScriptableObject)")]
    public MonsterDataSO specialMonsterData;
}
