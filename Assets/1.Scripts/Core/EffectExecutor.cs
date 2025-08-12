using UnityEngine;

/// <summary>
/// 카드 및 유물 효과 실행을 담당하는 클래스입니다.
/// </summary>
public class EffectExecutor : MonoBehaviour
{
    public static EffectExecutor Instance { get; private set; }

    // DataManager 참조 (ID 기반 SO 조회를 위해)
    // private DataManager dataManager; // TODO: 실제 DataManager 구현 시 활성화

    // PoolManager 참조 (오브젝트 풀링을 위해)
    // private PoolManager poolManager; // TODO: 실제 PoolManager 구현 시 활성화

    // AudioManager 참조 (사운드/파티클 동기화를 위해)
    // private AudioManager audioManager; // TODO: 실제 AudioManager 구현 시 활성화

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // TODO: 필요한 참조들을 FindObjectOfType 또는 다른 방식으로 초기화
        // dataManager = FindObjectOfType<DataManager>();
        // poolManager = FindObjectOfType<PoolManager>();
        // audioManager = FindObjectOfType<AudioManager>();
    }

    /// <summary>
    /// 카드 데이터를 기반으로 효과를 실행합니다.
    /// </summary>
    /// <param name="cardData">실행할 카드의 데이터 (CardDataSO)</param>
    public void Execute(CardDataSO cardData)
    {
        Debug.Log($"Executing card effect: {cardData.cardName} (ID: {cardData.cardID})");

        // TODO: AudioManager가 구현되면 활성화
        // if (audioManager != null && cardData.effectSound != null)
        // {
        //     audioManager.PlaySFX(cardData.effectSound);
        // }

        // TriggerType에 따라 다른 이펙트 로직 분기
        switch (cardData.triggerType)
        {
            case TriggerType.Interval:
                // 주기적 발동 효과 (예: 특정 시간마다 데미지, 버프 등)
                Debug.Log($"Interval effect for {cardData.cardName}");
                break;
            case TriggerType.OnHit:
                // 적중 시 발동 효과 (예: 추가 데미지, 상태 이상 적용)
                Debug.Log($"OnHit effect for {cardData.cardName}");

                // 흡혈 효과 (Lifesteal)
                if (cardData.lifestealPercentage > 0)
                {
                    // TODO: 실제 데미지량과 플레이어의 CharacterStats 또는 PlayerController를 통해 체력 회복 로직 구현
                    // float actualDamageDealt = /* 실제 적에게 가한 데미지 */;
                    // float healAmount = actualDamageDealt * cardData.lifestealPercentage;
                    // PlayerController.Instance.Heal(healAmount); // 예시
                    Debug.Log($"Lifesteal effect: {cardData.lifestealPercentage * 100}% of damage converted to health.");
                }

                // 다중 공격 (Multi-attack) - 예시: 특정 카드 ID에 따라 분열 효과 발동
                // project_plan.md에 '분열' 카드가 언급되어 있으므로, cardID를 통해 구분할 수 있습니다.
                if (cardData.cardID == "card_split_001") // 예시 카드 ID
                {
                    ExecuteSplitEffect(cardData);
                }
                else
                {
                    // 일반적인 OnHit 데미지 적용 로직
                    // TODO: 데미지 계산 로직에 damageMultiplier 적용
                }
                break;
            case TriggerType.OnCrit:
                // 치명타 시 발동 효과 (예: 추가 효과, 스킬 쿨타임 감소)
                Debug.Log($"OnCrit effect for {cardData.cardName}");
                break;
            case TriggerType.OnSkillUse:
                // 스킬 사용 시 발동 효과 (예: 특정 스킬 강화, 자원 회복)
                Debug.Log($"OnSkillUse effect for {cardData.cardName}");
                break;
            // TODO: project_plan.md에 언급된 OnLowHealth 등 추가 TriggerType 처리
            default:
                Debug.LogWarning($"Unhandled TriggerType: {cardData.triggerType} for card {cardData.cardName}");
                break;
        }

        // TODO: 이펙트 패턴(Spiral, Wave 등)에 대한 구체적인 구현 호출
        // 이펙트 패턴은 cardData의 effectDescription이나 다른 필드를 기반으로 결정될 수 있습니다.
        // 예: if (cardData.cardID == "warrior_spiral_001") ExecuteSpiralEffect(cardData);
    }

    /// <summary>
    /// 유물 데이터를 기반으로 효과를 실행합니다.
    /// </summary>
    /// <param name="artifactData">실행할 유물의 데이터 (ArtifactDataSO)</param>
    public void Execute(ArtifactDataSO artifactData)
    {
        Debug.Log($"Executing artifact effect: {artifactData.artifactName} (ID: {artifactData.artifactID})");

        // TODO: AudioManager가 구현되면 활성화
        // if (audioManager != null && artifactData.effectSound != null)
        // {
        //     audioManager.PlaySFX(artifactData.effectSound);
        // }

        // TODO: artifactData의 효과에 따라 다른 이펙트 로직 구현
        // 예: 체력 부스팅, 유리 대포 등
        // artifactData의 필드를 사용하여 능력치 변경, 게임 규칙 변경 등을 처리합니다.
    }

    /// <summary>
    /// '분열' 카드 효과를 실행합니다. (다중 공격의 한 형태)
    /// project_plan.md: n발, 360°/n 간격 직선 발사
    /// </summary>
    /// <param name="cardData">분열 효과를 가진 카드 데이터</param>
    private void ExecuteSplitEffect(CardDataSO cardData)
    {
        Debug.Log("Executing Split Effect (Multi-attack)");
        // TODO: CardDataSO에 splitCount 필드 추가 제안
        // int splitCount = 3; // 임시 값, 실제로는 cardData에서 가져와야 함

        // TODO: PoolManager를 사용하여 프리팹 인스턴스화 및 설정
        // GameObject bulletPrefab = /* 적절한 총알 프리팹 */;
        // Vector3 spawnPosition = /* 플레이어 또는 발사 지점 위치 */;

        // for (int i = 0; i < splitCount; i++)
        // {
        //     float angle = i * (360f / splitCount);
        //     Quaternion rotation = Quaternion.Euler(0, 0, angle);
        //     GameObject bullet = poolManager.Get(bulletPrefab); // 예시
        //     bullet.transform.position = spawnPosition;
        //     bullet.transform.rotation = rotation;
        //     bullet.SetActive(true);
        //     // TODO: 총알에 힘을 가하거나 이동 로직 시작
        // }
    }

    // TODO: 다른 이펙트 패턴(Spiral, Wave 등)에 대한 구체적인 메서드 구현
    // private void ExecuteSpiralEffect(CardDataSO cardData) { ... }
    // private void ExecuteWaveEffect(CardDataSO cardData) { ... }
}

// CardDataSO와 ArtifactDataSO는 ScriptableObject로 정의되어야 합니다.
// TriggerType 열거형은 project_plan.md에 정의되어 있습니다.
// 이 파일들이 먼저 존재해야 EffectExecutor가 정상적으로 컴파일됩니다.
