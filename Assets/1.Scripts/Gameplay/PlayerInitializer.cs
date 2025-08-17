// --- 파일 위치: Assets/1.Scripts/Gameplay/PlayerInitializer.cs ---
// --- 참고: 이 파일은 기존 PlayerInitializer.cs를 대체해야 합니다. ---

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 게임 시작 시 플레이어 오브젝트를 초기화하는 역할을 담당합니다. (리팩토링 버전)
/// 이 클래스는 필요한 데이터와 컴포넌트를 가져와서, 각 컴포넌트가 스스로를 초기화하도록
/// 메시지를 전달하는 '지휘자(Coordinator)'의 역할에 집중합니다.
/// </summary>
public class PlayerInitializer : MonoBehaviour
{
    [Header("테스트용 시작 카드 목록")]
    [Tooltip("캐릭터 데이터(SO)에 시작 카드가 설정되어 있으면, 이 목록은 무시됩니다.")]
    [SerializeField] private List<CardDataSO> testStartingCards;

    void Start()
    {

        EventSystem.current.SetSelectedGameObject(null);

        // 1. 핵심 컴포넌트 및 매니저 참조 가져오기
        var playerStats = GetComponent<CharacterStats>();
        var playerSpriteRenderer = GetComponent<SpriteRenderer>();
        var playerController = GetComponent<PlayerController>();
        var gameManager = GameManager.Instance;
        var progressionManager = ProgressionManager.Instance;

        // 2. DebugManager에 플레이어 등록
        if (DebugManager.Instance != null)
        {
            DebugManager.Instance.RegisterPlayer(playerStats);
        }

        // 3. 캐릭터 데이터 로드
        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? DataManager.Instance.GetCharacter("warrior");
        if (characterToLoad == null)
        {
            Debug.LogError("CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다!");
            return;
        }

        // 4. 기본 정보 적용 (스탯, 외형)
        playerStats.stats = characterToLoad.baseStats;
        playerSpriteRenderer.sprite = characterToLoad.illustration;

        // 5. 스탯 계산 위임 (영구 스탯, 분배 포인트)
        // PlayerInitializer가 직접 계산하지 않고, CharacterStats에 계산을 위임합니다.
        CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
        playerStats.ApplyPermanentStats(permanentStats);
        playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

        // 6. 시작 아이템 장착
        EquipStartingItems(characterToLoad);

        // 7. 최종 스탯 계산 및 체력 초기화
        playerStats.CalculateFinalStats();
        playerStats.currentHealth = playerStats.finalHealth;

        // 8. 게임플레이 루프 시작
        if (CardManager.Instance != null) CardManager.Instance.StartCardSelectionLoop();
        if (playerController != null) playerController.StartAutoAttackLoop();
    }

    /// <summary>
    /// 캐릭터의 시작 카드와 유물을 장착합니다.
    /// </summary>
    private void EquipStartingItems(CharacterDataSO characterData)
    {
        // 시작 카드 결정 및 장착
        List<CardDataSO> cardsToEquip = new List<CardDataSO>();
        if (testStartingCards != null && testStartingCards.Count > 0)
        {
            cardsToEquip.AddRange(testStartingCards); // 1순위: 테스트용 카드
        }
        else if (characterData.startingCard != null)
        {
            cardsToEquip.Add(characterData.startingCard); // 2순위: 캐릭터 데이터의 시작 카드
        }

        if (cardsToEquip.Count > 0 && CardManager.Instance != null)
        {
            foreach (var card in cardsToEquip)
            {
                if (card != null)
                {
                    CardManager.Instance.AddCard(card);
                    CardManager.Instance.Equip(card);
                }
            }
            Debug.Log($"[PlayerInitializer] {cardsToEquip.Count}개의 시작 카드를 장착했습니다.");
        }
        else
        {
            Debug.LogWarning("[PlayerInitializer] 설정된 시작 카드가 없어, 장착된 카드 없이 시작합니다.");
        }

        // 시작 유물 장착
        if (characterData.startingArtifacts != null && characterData.startingArtifacts.Count > 0)
        {
            if (ArtifactManager.Instance != null)
            {
                foreach (var artifact in characterData.startingArtifacts)
                {
                    if (artifact != null) ArtifactManager.Instance.EquipArtifact(artifact);
                }
            }
        }
    }
}
