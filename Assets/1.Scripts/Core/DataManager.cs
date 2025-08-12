using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

// DataManager: 게임 데이터를 로드, 관리하고 제공하는 중앙 관리자입니다.
// 이 클래스는 싱글톤으로 구현되어 게임 전체에서 접근 가능합니다.
public class DataManager : MonoBehaviour
{
    // DataManager의 싱글톤 인스턴스입니다.
    public static DataManager Instance { get; private set; }

    // CardDataSO와 ArtifactDataSO를 ID로 빠르게 찾기 위한 딕셔너리 캐시입니다.
    private readonly Dictionary<string, CardDataSO> cardDict = new Dictionary<string, CardDataSO>();
    private readonly Dictionary<string, ArtifactDataSO> artifactDict = new Dictionary<string, ArtifactDataSO>();
    private readonly Dictionary<string, CharacterDataSO> characterDict = new Dictionary<string, CharacterDataSO>();

    void Awake()
    {
        // DataManager의 인스턴스가 하나만 존재하도록 보장합니다.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // 씬이 변경되어도 이 GameObject가 파괴되지 않도록 설정합니다.
        DontDestroyOnLoad(gameObject);

        // 관리자가 활성화되면 모든 게임 데이터를 로드합니다.
        LoadAll();
    }

    // Resources 폴더에서 모든 CardDataSO와 ArtifactDataSO를 로드하고 캐시합니다.
    // 이 메서드는 로드된 데이터의 유효성 검사 및 중복 ID 검사를 수행합니다.
    public void LoadAll()
    {
        cardDict.Clear();
        artifactDict.Clear();

        // Resources 폴더 내 "CardData" 경로에서 모든 CardDataSO를 로드합니다.
        // 예: Assets/Resources/CardData/MyCard.asset
        CardDataSO[] loadedCardData = Resources.LoadAll<CardDataSO>("CardData");
        foreach (var cd in loadedCardData)
        {
            // 데이터 무결성을 위해 중복된 카드 ID를 확인합니다.
            if (cardDict.ContainsKey(cd.cardID))
            {
                Debug.LogError($"[DataManager] 중복된 카드 ID 추가: {cd.cardID}. 데이터 무결성을 위해 이 항목은 건너뜁니다.");
                continue; // 중복된 항목은 건너뜁니다.
            }
            // 로드된 카드 데이터의 유효성을 검사합니다.
            ValidateCardData(cd);
            // 유효성 검사를 통과한 카드 데이터를 딕셔너리에 추가합니다.
            cardDict[cd.cardID] = cd;
        }

        // Resources 폴더 내 "ArtifactData" 경로에서 모든 ArtifactDataSO를 로드합니다.
        // 예: Assets/Resources/ArtifactData/MyArtifact.asset
        ArtifactDataSO[] loadedArtifactData = Resources.LoadAll<ArtifactDataSO>("ArtifactData");
        foreach (var ad in loadedArtifactData)
        {
            // 중복된 유물 ID를 확인합니다.
            if (artifactDict.ContainsKey(ad.artifactID))
            {
                Debug.LogError($"[DataManager] 중복된 유물 ID 추가: {ad.artifactID}. 데이터 무결성을 위해 이 항목은 건너뜁니다.");
                continue; // 중복된 항목은 건너뜁니다.
            }
            // 로드된 유물 데이터의 유효성을 검사합니다.
            ValidateArtifactData(ad);
            // 유효성 검사를 통과한 유물 데이터를 딕셔너리에 추가합니다.
            artifactDict[ad.artifactID] = ad;
        }

        // Resources 폴더 내 "CharacterData" 경로에서 모든 CharacterDataSO를 로드합니다.
        CharacterDataSO[] loadedCharacterData = Resources.LoadAll<CharacterDataSO>("CharacterData");
        Debug.Log($"[DataManager] 로드된 CharacterDataSO 에셋 개수: {loadedCharacterData.Length}");
        foreach (var charData in loadedCharacterData)
        {
            Debug.Log($"[DataManager] 로드된 캐릭터: ID={charData.characterId}, Name={charData.characterName}");
            if (characterDict.ContainsKey(charData.characterId))
            {
                Debug.LogError($"[DataManager] 중복된 캐릭터 ID 추가: {charData.characterId}. 이 항목은 건너뜁니다.");
                continue;
            }
            characterDict[charData.characterId] = charData;
        }
    }

    // 특정 ID로 CardDataSO를 검색합니다.
    // 카드를 찾지 못하면 null을 반환하고 경고 로그를 출력합니다.
    public CardDataSO GetCard(string id)
    {
        if (!cardDict.TryGetValue(id, out var cd))
            Debug.LogWarning($"[DataManager] 존재하지 않는 카드 ID를 검색하려 시도합니다: {id}.");
        return cd;
    }

    // 특정 ID로 ArtifactDataSO를 검색합니다.
    // 유물을 찾지 못하면 null을 반환하고 경고 로그를 출력합니다.
    public ArtifactDataSO GetArtifact(string id)
    {
        if (!artifactDict.TryGetValue(id, out var ad))
            Debug.LogWarning($"[DataManager] 존재하지 않는 유물 ID를 검색하려 시도합니다: {id}.");
        return ad;
    }

    // 특정 ID로 CharacterDataSO를 검색합니다.
    public CharacterDataSO GetCharacter(string id)
    {
        if (!characterDict.TryGetValue(id, out var charData))
            Debug.LogWarning($"[DataManager] 존재하지 않는 캐릭터 ID를 검색하려 시도합니다: {id}.");
        return charData;
    }

    // 개별 CardDataSO의 일반적인 유효성을 검사합니다.
    // 치명적인 오류가 발견되면 게임을 종료하고 애플리케이션을 종료합니다.
    public void ValidateCardData(CardDataSO cd)
    {
        if (cd == null)
        {
            Debug.LogError("[DataManager] 유효성 검사를 위한 카드 데이터가 null입니다. 애플리케이션을 종료합니다.");
            Application.Quit(); // 치명적인 오류: 게임 데이터 로드 실패.
            return;
        }
        if (string.IsNullOrEmpty(cd.cardID))
        {
            Debug.LogError($"[DataManager] 카드 ID가 비어 있습니다: {cd.name}. 애플리케이션을 종료합니다.");
            Application.Quit(); // 치명적인 오류: 게임 종료.
            return;
        }
        if (string.IsNullOrEmpty(cd.cardName))
        {
            Debug.LogError($"[DataManager] 카드 이름이 비어 있습니다: {cd.cardID}. 애플리케이션을 종료합니다.");
            Application.Quit(); // 치명적인 오류: 게임 종료.
            return;
        }
        // 모든 능력치 배율은 음수가 아니어야 합니다.
        if (cd.damageMultiplier < 0 ||
            cd.attackSpeedMultiplier < 0 ||
            cd.moveSpeedMultiplier < 0 ||
            cd.healthMultiplier < 0 ||
            cd.critRateMultiplier < 0 ||
            cd.critDamageMultiplier < 0 ||
            cd.lifestealPercentage < 0)
        {
            Debug.LogError($"[DataManager] 카드 능력치 배율 중 음수가 있습니다: {cd.cardID}. 애플리케이션을 종료합니다.");
            Application.Quit(); // 치명적인 오류: 게임 종료.
            return;
        }
    }

    // 개별 ArtifactDataSO의 일반적인 유효성을 검사합니다.
    // 치명적인 오류가 발견되면 게임을 종료하고 애플리케이션을 종료합니다.
    public void ValidateArtifactData(ArtifactDataSO ad)
    {
        if (ad == null)
        {
            Debug.LogError("[DataManager] 유효성 검사를 위한 유물 데이터가 null입니다. 애플리케이션을 종료합니다.");
            Application.Quit(); // 치명적인 오류: 게임 데이터 로드 실패.
            return;
        }
        if (string.IsNullOrEmpty(ad.artifactID))
        {
            Debug.LogError($"[DataManager] 유물 ID가 비어 있습니다: {ad.name}. 애플리케이션을 종료합니다.");
            Application.Quit(); // 치명적인 오류: 게임 종료.
            return;
        }
        // ArtifactDataSO에 정의된 각 부스트 비율이 음수가 아닌지 확인합니다.
        if (ad.attackBoostRatio < 0 || ad.healthBoostRatio < 0 || ad.moveSpeedBoostRatio < 0 ||
            ad.critChanceBoostRatio < 0 || ad.critDamageBoostRatio < 0 || ad.lifestealBoostRatio < 0)
        {
            Debug.LogError($"[DataManager] 유물 효과 비율 중 음수가 있습니다: {ad.artifactID}. 애플리케이션을 종료합니다.");
            Application.Quit(); // 치명적인 오류: 게임 종료.
            return;
        }
    }

    // 로드된 모든 CardDataSO 목록을 반환합니다.
    public List<CardDataSO> GetAllCards()
    {
        return new List<CardDataSO>(cardDict.Values);
    }

    // 로드된 모든 ArtifactDataSO 목록을 반환합니다.
    public List<ArtifactDataSO> GetAllArtifacts()
    {
        return new List<ArtifactDataSO>(artifactDict.Values);
    }
}