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

    private readonly Dictionary<string, CardDataSO> cardDataDict = new Dictionary<string, CardDataSO>();
    private readonly Dictionary<string, ArtifactDataSO> artifactDataDict = new Dictionary<string, ArtifactDataSO>();
    private readonly Dictionary<string, CharacterDataSO> characterDict = new Dictionary<string, CharacterDataSO>();
    private readonly Dictionary<string, MonsterDataSO> monsterDataDict = new Dictionary<string, MonsterDataSO>();
    // [추가] v8.0의 새로운 모듈들을 저장할 딕셔너리
    private readonly Dictionary<string, CardEffectSO> moduleDataDict = new Dictionary<string, CardEffectSO>();

    public IEnumerator LoadAllDataAsync()
    {
        Debug.Log("[DataManager] 모든 ScriptableObject 데이터 로드 시작 (via ResourceManager)...");
        var resourceManager = ServiceLocator.Get<ResourceManager>();
        if (resourceManager == null)
        {
            Debug.LogError("[DataManager] ResourceManager를 찾을 수 없습니다! 로드를 진행할 수 없습니다.");
            yield break;
        }

        // 각 데이터 타입별로 로드를 요청하고 핸들을 저장합니다.
        var cardHandle = resourceManager.LoadAllAsync<CardDataSO>("data_card");
        var artifactHandle = resourceManager.LoadAllAsync<ArtifactDataSO>("data_artifact");
        var characterHandle = resourceManager.LoadAllAsync<CharacterDataSO>("data_character");
        var monsterHandle = resourceManager.LoadAllAsync<MonsterDataSO>("data_monster");
        var moduleHandle = resourceManager.LoadAllAsync<CardEffectSO>("data_effect");

        // 모든 핸들이 완료될 때까지 기다립니다.
        yield return cardHandle;
        yield return artifactHandle;
        yield return characterHandle;
        yield return monsterHandle;
        yield return moduleHandle;

        // 로드 결과를 확인하고 딕셔너리에 추가합니다.
        if (cardHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var card in cardHandle.Result) { if (!cardDataDict.ContainsKey(card.cardID)) cardDataDict.Add(card.cardID, card); }
        if (artifactHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var artifact in artifactHandle.Result) { if (!artifactDataDict.ContainsKey(artifact.artifactID)) artifactDataDict.Add(artifact.artifactID, artifact); }
        if (characterHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var character in characterHandle.Result) { if (!characterDict.ContainsKey(character.characterId)) characterDict.Add(character.characterId, character); }
        if (monsterHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var monster in monsterHandle.Result) { if (!monsterDataDict.ContainsKey(monster.monsterID)) monsterDataDict.Add(monster.monsterID, monster); }
        if (moduleHandle.Status == AsyncOperationStatus.Succeeded)
            foreach (var module in moduleHandle.Result) { if (!moduleDataDict.ContainsKey(module.name)) moduleDataDict.Add(module.name, module); }

        Debug.Log($"[DataManager] 모든 ScriptableObject 데이터 로드 완료. (카드: {cardDataDict.Count}, 유물: {artifactDataDict.Count}, 캐릭터: {characterDict.Count}, 몬스터: {monsterDataDict.Count}, 모듈: {moduleDataDict.Count})");

        // 핸들 자체는 ResourceManager가 관리하므로 여기서 Release 하지 않습니다.
    }

    // --- Getter 함수들 ---
    public CardDataSO GetCard(string id) => GetData(id, cardDataDict);
    public ArtifactDataSO GetArtifact(string id) => GetData(id, artifactDataDict);
    public CharacterDataSO GetCharacter(string id) => GetData(id, characterDict);
    public MonsterDataSO GetMonsterData(string id) => GetData(id, monsterDataDict);
    // [추가] 모듈을 이름으로 찾아 반환하는 함수
    public CardEffectSO GetModule(string name) => GetData(name, moduleDataDict);

    private T GetData<T>(string id, Dictionary<string, T> sourceDict) where T : class
    {
        if (string.IsNullOrEmpty(id)) return null;
        sourceDict.TryGetValue(id, out T data);
        if (data == null) Debug.LogWarning($"[DataManager] '{id}' ID/이름으로 {typeof(T).Name} 데이터를 찾을 수 없습니다.");
        return data;
    }

    public List<CardDataSO> GetAllCards() => new List<CardDataSO>(cardDataDict.Values);
    public List<ArtifactDataSO> GetAllArtifacts() => new List<ArtifactDataSO>(artifactDataDict.Values);
}