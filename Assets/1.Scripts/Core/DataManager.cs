// --- 파일명: DataManager.cs (역할 축소) ---
// 경로: Assets/1.Scripts/Core/DataManager.cs
using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    void Awake()
    {
        ServiceLocator.Register<DataManager>(this);
        DontDestroyOnLoad(gameObject);
        InitializeDataSOs();
    }

    // [삭제] 프리팹 관련 필드 모두 삭제

    private readonly Dictionary<string, CardDataSO> cardDataDict = new Dictionary<string, CardDataSO>();
    private readonly Dictionary<string, ArtifactDataSO> artifactDataDict = new Dictionary<string, ArtifactDataSO>();
    private readonly Dictionary<string, CharacterDataSO> characterDict = new Dictionary<string, CharacterDataSO>();
    private readonly Dictionary<string, MonsterDataSO> monsterDataDict = new Dictionary<string, MonsterDataSO>();

    private void InitializeDataSOs()
    {
        // 데이터 SO 로드는 Resources 폴더를 그대로 사용합니다.
        CardDataSO[] allCards = Resources.LoadAll<CardDataSO>("CardData");
        foreach (var card in allCards) { if (!cardDataDict.ContainsKey(card.cardID)) cardDataDict.Add(card.cardID, card); }

        ArtifactDataSO[] allArtifacts = Resources.LoadAll<ArtifactDataSO>("ArtifactData");
        foreach (var artifact in allArtifacts) { if (!artifactDataDict.ContainsKey(artifact.artifactID)) artifactDataDict.Add(artifact.artifactID, artifact); }

        CharacterDataSO[] allCharacters = Resources.LoadAll<CharacterDataSO>("CharacterData");
        foreach (var character in allCharacters) { if (!characterDict.ContainsKey(character.characterId)) characterDict.Add(character.characterId, character); }

        MonsterDataSO[] allMonsters = Resources.LoadAll<MonsterDataSO>("MonsterData");
        foreach (var monster in allMonsters) { if (!monsterDataDict.ContainsKey(monster.monsterID)) monsterDataDict.Add(monster.monsterID, monster); }

        Debug.Log("[DataManager] 모든 ScriptableObject 데이터 로드 완료.");
    }

    // [삭제] Get...Prefab 메서드들 삭제

    public CardDataSO GetCard(string id) => GetData(id, cardDataDict);
    public ArtifactDataSO GetArtifact(string id) => GetData(id, artifactDataDict);
    public CharacterDataSO GetCharacter(string id) => GetData(id, characterDict);
    public MonsterDataSO GetMonsterData(string id) => GetData(id, monsterDataDict);

    private T GetData<T>(string id, Dictionary<string, T> sourceDict) where T : class
    {
        sourceDict.TryGetValue(id, out T data);
        return data;
    }

    public List<CardDataSO> GetAllCards() => new List<CardDataSO>(cardDataDict.Values);
    public List<ArtifactDataSO> GetAllArtifacts() => new List<ArtifactDataSO>(artifactDataDict.Values);
}
