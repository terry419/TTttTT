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
        Debug.Log("[DataManager] 모든 ScriptableObject 데이터 비동기 로드 시작...");

        // [추가] "data_effect" 레이블을 가진 모든 CardEffectSO 에셋을 로드하는 핸들 추가
        var cardHandle = Addressables.LoadAssetsAsync<CardDataSO>("data_card", null);
        var artifactHandle = Addressables.LoadAssetsAsync<ArtifactDataSO>("data_artifact", null);
        var characterHandle = Addressables.LoadAssetsAsync<CharacterDataSO>("data_character", null);
        var monsterHandle = Addressables.LoadAssetsAsync<MonsterDataSO>("data_monster", null);
        var moduleHandle = Addressables.LoadAssetsAsync<CardEffectSO>("data_effect", null);

        // [수정] 모든 핸들을 그룹으로 묶어 한번에 처리
        var groupHandle = Addressables.ResourceManager.CreateGenericGroupOperation(
            new List<AsyncOperationHandle> { cardHandle, artifactHandle, characterHandle, monsterHandle, moduleHandle }, true);

        yield return groupHandle;

        if (groupHandle.Status == AsyncOperationStatus.Succeeded)
        {
            // 기존 데이터 로딩
            foreach (var card in cardHandle.Result) { if (!cardDataDict.ContainsKey(card.cardID)) cardDataDict.Add(card.cardID, card); }
            foreach (var artifact in artifactHandle.Result) { if (!artifactDataDict.ContainsKey(artifact.artifactID)) artifactDataDict.Add(artifact.artifactID, artifact); }
            foreach (var character in characterHandle.Result) { if (!characterDict.ContainsKey(character.characterId)) characterDict.Add(character.characterId, character); }
            foreach (var monster in monsterHandle.Result) { if (!monsterDataDict.ContainsKey(monster.monsterID)) monsterDataDict.Add(monster.monsterID, monster); }

            // [추가] 새로 로드한 모듈들을 딕셔너리에 저장
            foreach (var module in moduleHandle.Result) { if (!moduleDataDict.ContainsKey(module.name)) moduleDataDict.Add(module.name, module); }

            Debug.Log($"[DataManager] 모든 ScriptableObject 데이터 로드 완료. (카드: {cardDataDict.Count}, 유물: {artifactDataDict.Count}, 캐릭터: {characterDict.Count}, 몬스터: {monsterDataDict.Count}, 모듈: {moduleDataDict.Count})");
        }
        else
        {
            // [수정] 5단계 목표에 맞게 오류 모니터링 강화
            Debug.LogError("[DataManager] CRITICAL ERROR: 하나 이상의 핵심 데이터 그룹 로딩에 실패했습니다! Addressables Groups 창에서 각 레이블('data_card', 'data_artifact', 'data_character', 'data_monster', 'data_effect')에 에셋이 올바르게 할당되었는지 확인하세요.");
        }

        // 핸들 메모리 해제
        Addressables.Release(cardHandle);
        Addressables.Release(artifactHandle);
        Addressables.Release(characterHandle);
        Addressables.Release(monsterHandle);
        Addressables.Release(moduleHandle);
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