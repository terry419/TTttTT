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
}