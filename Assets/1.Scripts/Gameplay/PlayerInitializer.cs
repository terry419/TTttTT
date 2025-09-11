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
        var playerController = GetComponent<PlayerController>();
        var gameManager = ServiceLocator.Get<GameManager>();
        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();

        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter(CharacterIDs.Warrior);
        if (characterToLoad == null)
        {
            Debug.LogError("[INIT-DEBUG] CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다!");
            return;
        }

        // 외형 변경 로직 ... (기존과 동일)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && characterToLoad.illustration != null)
        {
            spriteRenderer.sprite = characterToLoad.illustration;
            Debug.Log($"[PlayerInitializer] 캐릭터 외형을 {characterToLoad.characterName}의 것으로 변경했습니다.");
        }

        // --- [핵심 수정 순서] ---
        // 1. 기본 스탯을 먼저 할당합니다.
        playerStats.stats = characterToLoad.baseStats;

        var cardManager = ServiceLocator.Get<CardManager>();
        var artifactManager = ServiceLocator.Get<ArtifactManager>();

        if (playerDataManager.IsRunInitialized)
        {
            // 2. 카드/유물 매니저를 연결하고 모든 보너스 스탯을 적용합니다.
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);

            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

            // 3. 시작 아이템을 지급합니다. (이 과정에서 스탯이 다시 계산됩니다)
            List<NewCardDataSO> cardsToEquip = characterToLoad.startingCards;
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
                    if (artifact != null) artifactManager.EquipArtifact(artifact);
                }
            }
            playerDataManager.CompleteRunInitialization();
        }
        else
        {
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);
        }

        // 4. 모든 스탯 설정이 끝난 후, CharacterStats의 초기화를 직접 호출합니다.
        playerStats.Initialize();

        // 5. 카드 자동 선택 루프를 시작합니다.
        if (cardManager != null)
        {
            cardManager.StartCardSelectionLoop();
        }
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"--- 플레이어({gameObject.name}) 스탯 초기화 완료 ---");
        sb.AppendLine($"체력: {playerStats.GetCurrentHealth():F1} / {playerStats.FinalHealth:F1}");
        sb.AppendLine($"공격력 보너스: {playerStats.FinalDamageBonus:F2}%");
        sb.AppendLine($"공격 속도: {playerStats.FinalAttackSpeed:F2}");
        sb.AppendLine($"이동 속도: {playerStats.FinalMoveSpeed:F2}");
        sb.AppendLine($"치명타 확률: {playerStats.FinalCritRate:F2}%");
        sb.AppendLine($"치명타 피해: {playerStats.FinalCritDamage:F2}%");
        Debug.Log(sb.ToString());
    }
}