using System.Collections.Generic;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [Header("v8.0 테스트용 시작 카드 목록")]
    [SerializeField] private List<NewCardDataSO> testStartingNewCards;

    void Start()
    {
        var playerStats = GetComponent<CharacterStats>();
        var playerController = GetComponent<PlayerController>();
        var gameManager = ServiceLocator.Get<GameManager>();

        // 캐릭터 데이터 로드 및 기본 스탯 설정 (이전과 동일)
        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter(CharacterIDs.Warrior);
        if (characterToLoad == null) { Debug.LogError("적용할 캐릭터 데이터를 찾을 수 없습니다!"); return; }
        playerStats.stats = characterToLoad.baseStats;

        var cardManager = ServiceLocator.Get<CardManager>();
        var artifactManager = ServiceLocator.Get<ArtifactManager>();

        if (gameManager.isFirstRound)
        {
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);

            // 영구 스탯 및 분배 포인트 적용 (이전과 동일)
            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

            // [수정] 신규 카드 장착 로직만 남깁니다.
            if (testStartingNewCards != null && testStartingNewCards.Count > 0 && cardManager != null)
            {
                foreach (var newCard in testStartingNewCards)
                {
                    if (newCard != null)
                    {
                        cardManager.AddCard(newCard);
                        cardManager.Equip(newCard);
                    }
                }
            }

            // 유물 장착 로직 (이전과 동일)
            if (characterToLoad.startingArtifacts != null && artifactManager != null)
            {
                foreach (var artifact in characterToLoad.startingArtifacts)
                {
                    if (artifact != null) artifactManager.EquipArtifact(artifact);
                }
            }
            gameManager.isFirstRound = false;
        }
        else
        {
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);
        }

        playerStats.CalculateFinalStats();
        playerStats.currentHealth = playerStats.FinalHealth;
        if (cardManager != null) cardManager.StartCardSelectionLoop();
        if (playerController != null) playerController.StartAutoAttackLoop();
    }
}