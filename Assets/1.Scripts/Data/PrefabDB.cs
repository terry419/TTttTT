using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PrefabDB", menuName = "GameData/PrefabDB")]
public class PrefabDB : ScriptableObject
{
    [Header("몬스터 프리팹")]
    public List<GameObject> monsterPrefabs;

    [Header("총알 프리팹")]
    public List<GameObject> bulletPrefabs;

    [Header("시각 효과 (VFX) 프리팹")]
    public List<GameObject> vfxPrefabs;
}
