// --- 파일명: DataManager.cs (v8.0 모듈 로딩 기능 추가) ---
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Linq;

public class DataManager : MonoBehaviour
{

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<DataManager>())
        {
            ServiceLocator.Register<DataManager>(this);
            DontDestroyOnLoad(gameObject);
            // [수정] Awake에서 바로 로딩을 시작하지 않고, 외부(GameInitializer)에서 호출하도록 변경될 수 있으므로 일단 대기합니다.
            // StartCoroutine(LoadAllDataAsync()); // -> GameInitializer가 호출하도록 역할을 이전하는 것이 더 안정적입니다.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private readonly Dictionary<string, NewCardDataSO> newCardDataDict = new Dictionary<string, NewCardDataSO>();
    private readonly Dictionary<string, ArtifactDataSO> artifactDataDict = new Dictionary<string, ArtifactDataSO>();
    private readonly Dictionary<string, CharacterDataSO> characterDict = new Dictionary<string, CharacterDataSO>();
    private readonly Dictionary<string, MonsterDataSO> monsterDataDict = new Dictionary<string, MonsterDataSO>();
    private readonly Dictionary<string, CardEffectSO> moduleDataDict = new Dictionary<string, CardEffectSO>();

    public IEnumerator LoadAllDataAsync()
    {
        var resourceManager = ServiceLocator.Get<ResourceManager>();
        if (resourceManager == null) yield break;

        // [수정] CardDataSO 로딩 핸들 삭제, NewCardDataSO 핸들 추가
        var newCardHandle = resourceManager.LoadAllAsync<NewCardDataSO>("data_card_new"); // NewCardDataSO용 레이블
        var artifactHandle = resourceManager.LoadAllAsync<ArtifactDataSO>("data_artifact");
        var characterHandle = resourceManager.LoadAllAsync<CharacterDataSO>("data_character");
        var monsterHandle = resourceManager.LoadAllAsync<MonsterDataSO>("data_monster");
        var moduleHandle = resourceManager.LoadAllAsync<CardEffectSO>("data_effect");

        yield return newCardHandle;
        yield return artifactHandle;
        yield return characterHandle;
        yield return monsterHandle;
        yield return moduleHandle;

        // [수정] NewCardDataSO 딕셔너리 채우기
        if (newCardHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var card in newCardHandle.Result) { if (!newCardDataDict.ContainsKey(card.basicInfo.cardID)) newCardDataDict.Add(card.basicInfo.cardID, card); }

        if (artifactHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var artifact in artifactHandle.Result) { if (!artifactDataDict.ContainsKey(artifact.artifactID)) artifactDataDict.Add(artifact.artifactID, artifact); }

        if (characterHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var character in characterHandle.Result) { if (!characterDict.ContainsKey(character.characterId)) characterDict.Add(character.characterId, character); }

        if (monsterHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var monster in monsterHandle.Result) { if (!monsterDataDict.ContainsKey(monster.monsterID)) monsterDataDict.Add(monster.monsterID, monster); }

        if (moduleHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var module in moduleHandle.Result) { if (!moduleDataDict.ContainsKey(module.name)) moduleDataDict.Add(module.name, module); }
    }

    public NewCardDataSO GetNewCard(string id) => GetData(id, newCardDataDict);
    public List<NewCardDataSO> GetAllNewCards() => new List<NewCardDataSO>(newCardDataDict.Values);

    public ArtifactDataSO GetArtifact(string id) => GetData(id, artifactDataDict);
    public List<ArtifactDataSO> GetAllArtifacts() => new List<ArtifactDataSO>(artifactDataDict.Values);
    public CharacterDataSO GetCharacter(string id) => GetData(id, characterDict);
    public MonsterDataSO GetMonsterData(string id) => GetData(id, monsterDataDict);
    public CardEffectSO GetModule(string name) => GetData(name, moduleDataDict);

    private T GetData<T>(string id, Dictionary<string, T> sourceDict) where T : class
    {
        if (string.IsNullOrEmpty(id)) return null;
        sourceDict.TryGetValue(id, out T data);
        return data;
    }
}