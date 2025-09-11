using System.Collections.Generic;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [Header("v8.0 테스트용 시작 카드 목록")]
    [SerializeField] private List<NewCardDataSO> testStartingNewCards;

    void Start()
    {
        var playerStats = GetComponent<CharacterStats>();
        GetComponent<SpriteRenderer>().sortingOrder = 20;
        var gameManager = ServiceLocator.Get<GameManager>();
        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();
        var cardManager = ServiceLocator.Get<CardManager>();
        var artifactManager = ServiceLocator.Get<ArtifactManager>();

        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter(CharacterIDs.Warrior);
        if (characterToLoad == null)
        {
            Debug.LogError("[PlayerInitializer] CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다!");
            return;
        }

        // --- 공통 초기화 로직 ---
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && characterToLoad.illustration != null)
        {
            spriteRenderer.sprite = characterToLoad.illustration;
        }
        playerStats.stats = characterToLoad.baseStats;
        if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
        if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);


        // --- 첫 라운드와 이후 라운드 로직 분리 ---
        if (playerDataManager.IsRunInitialized)
        {
            // [첫 라운드] Point Allocation 직후에만 실행되는 블록
            Debug.Log("[PlayerInitializer] 새 게임 런 초기화를 시작합니다 (첫 라운드).");

            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

            // 사용할 카드 목록 결정 (SO 우선, 없으면 폴백)
            List<NewCardDataSO> cardsToEquip = characterToLoad.startingCards;
            if (cardsToEquip == null || cardsToEquip.Count == 0)
            {
                Debug.Log($"[PlayerInitializer] '{characterToLoad.name}' SO에 시작 카드가 없어, 프리팹에 설정된 폴백 카드를 사용합니다.");
                cardsToEquip = testStartingNewCards;
            }

            // 결정된 시작 아이템 지급
            if (cardsToEquip != null && cardManager != null)
            {
                foreach (var cardData in cardsToEquip)
                {
                    if (cardData != null)
                    {
                        CardInstance instance = cardManager.AddCard(cardData);
                        if (instance != null) cardManager.Equip(instance);
                    }
                }
            }
            if (characterToLoad.startingArtifacts != null && artifactManager != null)
            {
                foreach (var artifact in characterToLoad.startingArtifacts)
                {
                    artifactManager.EquipArtifact(artifact);
                }
            }
            playerDataManager.CompleteRunInitialization(); // 초기화 완료 플래그를 false로 변경
        }
        else
        {
            // [이후 라운드 또는 테스트] PlayerDataManager에 이미 데이터가 있으므로, 아이템을 새로 지급하지 않습니다.
            Debug.Log("[PlayerInitializer] 기존 런 데이터를 사용하여 초기화를 진행합니다 (2라운드 이후 또는 테스트).");
            // [추가] 저장된 카드와 유물 정보로부터 스탯을 다시 계산합니다.
            playerStats.RecalculateAllModifiers();
        }

        // 최종 스탯 계산 및 체력 설정은 항상 마지막에 호출합니다.
        playerStats.Initialize();

        if (cardManager != null)
        {
            cardManager.StartCardSelectionLoop();
        }
    }
}