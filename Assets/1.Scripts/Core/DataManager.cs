// --- 파일명: DataManager.cs ---

using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("데이터베이스")]
    [SerializeField] private PrefabDB prefabDB;

    private readonly Dictionary<string, GameObject> monsterPrefabDict = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> bulletPrefabDict = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> vfxPrefabDict = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, CardDataSO> cardDataDict = new Dictionary<string, CardDataSO>();
    private readonly Dictionary<string, ArtifactDataSO> artifactDataDict = new Dictionary<string, ArtifactDataSO>();
    private readonly Dictionary<string, CharacterDataSO> characterDict = new Dictionary<string, CharacterDataSO>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeData();
    }

    private void InitializeData()
    {
        if (prefabDB != null)
        {
            LoadPrefabs(prefabDB.monsterPrefabs, monsterPrefabDict);
            LoadPrefabs(prefabDB.bulletPrefabs, bulletPrefabDict);
            LoadPrefabs(prefabDB.vfxPrefabs, vfxPrefabDict);
        }

        CardDataSO[] allCards = Resources.LoadAll<CardDataSO>("CardData");
        foreach (var card in allCards) { if (!cardDataDict.ContainsKey(card.cardID)) cardDataDict.Add(card.cardID, card); }

        ArtifactDataSO[] allArtifacts = Resources.LoadAll<ArtifactDataSO>("ArtifactData");
        foreach (var artifact in allArtifacts) { if (!artifactDataDict.ContainsKey(artifact.artifactID)) artifactDataDict.Add(artifact.artifactID, artifact); }

        CharacterDataSO[] allCharacters = Resources.LoadAll<CharacterDataSO>("CharacterData");
        foreach (var character in allCharacters) { if (!characterDict.ContainsKey(character.characterId)) characterDict.Add(character.characterId, character); }
    }

    private void LoadPrefabs(List<GameObject> prefabList, Dictionary<string, GameObject> targetDict)
    {
        if (prefabList == null) return;
        foreach (var prefab in prefabList)
        {
            if (prefab != null && !targetDict.ContainsKey(prefab.name)) targetDict.Add(prefab.name, prefab);
        }
    }

    public GameObject GetMonsterPrefab(string name) => GetPrefab(name, monsterPrefabDict);
    public GameObject GetBulletPrefab(string name) => GetPrefab(name, bulletPrefabDict);
    public GameObject GetVfxPrefab(string name) => GetPrefab(name, vfxPrefabDict);

    public CardDataSO GetCard(string id) => GetData(id, cardDataDict);
    public ArtifactDataSO GetArtifact(string id) => GetData(id, artifactDataDict);
    public CharacterDataSO GetCharacter(string id) => GetData(id, characterDict);

    private T GetData<T>(string id, Dictionary<string, T> sourceDict) where T : class
    {
        sourceDict.TryGetValue(id, out T data);
        return data;
    }

    private GameObject GetPrefab(string name, Dictionary<string, GameObject> sourceDict)
    {
        sourceDict.TryGetValue(name, out GameObject prefab);
        return prefab;
    }

    public List<CardDataSO> GetAllCards() => new List<CardDataSO>(cardDataDict.Values);

    // [추가] CodexController에서 호출할 GetAllArtifacts 메서드를 추가했어.
    public List<ArtifactDataSO> GetAllArtifacts() => new List<ArtifactDataSO>(artifactDataDict.Values);
}