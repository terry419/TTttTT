// --- ���ϸ�: PrefabProvider.cs (�ű� ����) ---
// ���: Assets/1.Scripts/Gameplay/PrefabProvider.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Gameplay ������ ���Ǵ� ��� �������� ������ �����ͺ��̽� ������ �մϴ�.
/// �� ������Ʈ�� GameplaySystems ������Ʈ�� �����ϸ�, �� ���� �ٸ� �ý��۵鿡��
/// �̸�(string)�� ������� �������� �����մϴ�.
/// </summary>
public class PrefabProvider : MonoBehaviour
{
    public static PrefabProvider Instance { get; private set; }

    [Header("�����÷��� ������ ���")]
    [SerializeField] private List<GameObject> monsterPrefabs;
    [SerializeField] private List<GameObject> bulletPrefabs;
    [SerializeField] private List<GameObject> vfxPrefabs;

    private readonly Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // ���� ���� �� ��� ������ ����Ʈ�� �ϳ��� ��ųʸ��� �����Ͽ�
        // ������ ���� �˻��� �� �ֵ��� �غ��մϴ�.
        PopulatePrefabDict(monsterPrefabs);
        PopulatePrefabDict(bulletPrefabs);
        PopulatePrefabDict(vfxPrefabs);
    }

    private void PopulatePrefabDict(List<GameObject> prefabList)
    {
        if (prefabList == null) return;
        foreach (var prefab in prefabList)
        {
            if (prefab != null && !prefabDictionary.ContainsKey(prefab.name))
            {
                prefabDictionary.Add(prefab.name, prefab);
            }
        }
    }

    /// <summary>
    /// �̸����� �������� ã�� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="name">ã�� �������� �̸�</param>
    /// <returns>ã�� ������. ������ null�� ��ȯ�մϴ�.</returns>
    public GameObject GetPrefab(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        prefabDictionary.TryGetValue(name, out GameObject prefab);
        if (prefab == null)
        {
            Debug.LogError($"[PrefabProvider] ������ ��ųʸ����� '{name}'��(��) ã�� �� �����ϴ�. Inspector�� ������ ��Ͽ� ��ϵǾ����� Ȯ���ϼ���.");
        }
        return prefab;
    }
}
