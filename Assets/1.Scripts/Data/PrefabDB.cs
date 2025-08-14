using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PrefabDB", menuName = "GameData/PrefabDB")]
public class PrefabDB : ScriptableObject
{
    public List<GameObject> monsterPrefabs;
    public List<GameObject> bulletPrefabs;
    public List<GameObject> effectPrefabs; // ������ �ؽ�Ʈ �� ����Ʈ��
}