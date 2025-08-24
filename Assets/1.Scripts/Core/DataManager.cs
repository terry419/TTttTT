// --- 파일명: DataManager.cs (역할 축소) ---
// 경로: Assets/1.Scripts/Core/DataManager.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

public class DataManager : MonoBehaviour
{
    void Awake()
    {
        if (!ServiceLocator.IsRegistered<DataManager>())
        {
            ServiceLocator.Register<DataManager>(this);
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadAllDataAsync());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // [삭제] 프리팹 관련 필드 모두 삭제

    private readonly Dictionary<string, CardDataSO> cardDataDict = new Dictionary<string, CardDataSO>();
    private readonly Dictionary<string, ArtifactDataSO> artifactDataDict = new Dictionary<string, ArtifactDataSO>();
    private readonly Dictionary<string, CharacterDataSO> characterDict = new Dictionary<string, CharacterDataSO>();
    private readonly Dictionary<string, MonsterDataSO> monsterDataDict = new Dictionary<string, MonsterDataSO>();

    // ▼▼▼ [3] 기존 InitializeDataSOs 함수를 아래 내용으로 완전히 교체합니다. ▼▼▼
    public IEnumerator LoadAllDataAsync()
    {
        Debug.Log("[DataManager] 모든 ScriptableObject 데이터 비동기 로드 시작...");

        var cardHandle = Addressables.LoadAssetsAsync<CardDataSO>("data_card", null);
        var artifactHandle = Addressables.LoadAssetsAsync<ArtifactDataSO>("data_artifact", null);
        var characterHandle = Addressables.LoadAssetsAsync<CharacterDataSO>("data_character", null);
        var monsterHandle = Addressables.LoadAssetsAsync<MonsterDataSO>("data_monster", null);

        var groupHandle = Addressables.ResourceManager.CreateGenericGroupOperation(
            new List<AsyncOperationHandle> { cardHandle, artifactHandle, characterHandle, monsterHandle }, true);

        yield return groupHandle;

        if (groupHandle.Status == AsyncOperationStatus.Succeeded)
        {
            foreach (var card in cardHandle.Result) { if (!cardDataDict.ContainsKey(card.cardID)) cardDataDict.Add(card.cardID, card); }
            foreach (var artifact in artifactHandle.Result) { if (!artifactDataDict.ContainsKey(artifact.artifactID)) artifactDataDict.Add(artifact.artifactID, artifact); }
            foreach (var character in characterHandle.Result) { if (!characterDict.ContainsKey(character.characterId)) characterDict.Add(character.characterId, character); }
            foreach (var monster in monsterHandle.Result) { if (!monsterDataDict.ContainsKey(monster.monsterID)) monsterDataDict.Add(monster.monsterID, monster); }
            
            Debug.Log("[DataManager] 모든 ScriptableObject 데이터 로드 완료.");
        }
        else
        {
            Debug.LogError("[DataManager] ScriptableObject 데이터 로딩 실패!");
        }
        
        Addressables.Release(cardHandle);
        Addressables.Release(artifactHandle);
        Addressables.Release(characterHandle);
        Addressables.Release(monsterHandle);
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
