using UnityEngine;


/// <summary>
/// 카드 및 유물 효과 실행을 담당하는 클래스입니다.
/// </summary>
public class EffectExecutor : MonoBehaviour
{
    public static EffectExecutor Instance { get; private set; }

    // DataManager 참조 (ID 기반 SO 조회를 위해)
    private DataManager dataManager;
    // PoolManager 참조 (오브젝트 풀링을 위해)
    private PoolManager poolManager;
    // AudioManager 참조 (사운드/파티클 동기화를 위해)
    private AudioManager audioManager;

    // PlayerController 참조 (흡혈 효과 등에서 사용)
    private PlayerController playerController;

    [Header("이펙트 프리팹 참조")]
    [SerializeField] private GameObject bulletPrefab; // 다중 공격 등을 위한 총알 프리팹
    [SerializeField] private GameObject waveEffectPrefab; // 파동 효과 프리팹 (예시)
    [SerializeField] private GameObject spiralEffectPrefab; // 나선형 효과 프리팹 (예시)


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 필요한 참조들을 FindObjectOfType 또는 다른 방식으로 초기화
        dataManager = FindObjectOfType<DataManager>();
        poolManager = FindObjectOfType<PoolManager>();
        audioManager = FindObjectOfType<AudioManager>();
        playerController = FindObjectOfType<PlayerController>(); // PlayerController 참조 초기화

        if (dataManager == null) Debug.LogError("[EffectExecutor] DataManager를 찾을 수 없습니다.");
        if (poolManager == null) Debug.LogError("[EffectExecutor] PoolManager를 찾을 수 없습니다.");
        if (audioManager == null) Debug.LogError("[EffectExecutor] AudioManager를 찾을 수 없습니다.");
        if (playerController == null) Debug.LogWarning("[EffectExecutor] PlayerController를 찾을 수 없습니다. 일부 효과가 작동하지 않을 수 있습니다.");
    }

    /// <summary>
    /// 카드 데이터를 기반으로 효과를 실행합니다.
    /// </summary>
    /// <param name="cardData">실행할 카드의 데이터 (CardDataSO)</param>
    /// <param name="actualDamageDealt">OnHit 효과의 경우, 실제로 적에게 가한 데미지량 (흡혈 등 계산에 사용)</param>
    public void Execute(CardDataSO cardData, float actualDamageDealt = 0f)
    {
        Debug.Log($"Executing card effect: {cardData.cardName} (ID: {cardData.cardID})");

        // AudioManager가 구현되면 활성화
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
                if (cardData.lifestealPercentage > 0 && actualDamageDealt > 0)
                {
                    float healAmount = actualDamageDealt * cardData.lifestealPercentage;
                    if (playerController != null) // PlayerController.Instance 대신 playerController 사용
                    {
                        playerController.Heal(healAmount);
                        Debug.Log($"Lifesteal effect: Healed {healAmount} HP from {actualDamageDealt} damage.");
                    }
                    else
                    {
                        Debug.LogWarning("PlayerController 참조가 없어 흡혈 효과를 적용할 수 없습니다.");
                    }
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

        // CardEffectType에 따라 특수 이펙트 로직 분기
        switch (cardData.effectType)
        {
            case CardEffectType.SplitShot:
                ExecuteSplitEffect(cardData, 5); // 5발 발사
                break;
            case CardEffectType.Wave:
                ExecuteWaveEffect(cardData);
                break;
            case CardEffectType.Spiral:
                ExecuteSpiralEffect(cardData);
                break;
            case CardEffectType.Lightning:
                // ExecuteLightningEffect(cardData); // 번개 효과 구현 필요
                Debug.Log($"Lightning effect for {cardData.cardName}");
                break;
            case CardEffectType.None:
                // 특수 효과 없음
                break;
            default:
                Debug.LogWarning($"Unhandled CardEffectType: {cardData.effectType} for card {cardData.cardName}");
                break;
        }
    }

    /// <summary>
    /// 유물 데이터를 기반으로 효과를 실행합니다.
    /// </summary>
    /// <param name="artifactData">실행할 유물의 데이터 (ArtifactDataSO)</param>
    public void Execute(ArtifactDataSO artifactData)
    {
        Debug.Log($"Executing artifact effect: {artifactData.artifactName} (ID: {artifactData.artifactID})");

        // AudioManager가 구현되면 활성화
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
    /// <param name="splitCount">발사할 총알의 개수</param>
    private void ExecuteSplitEffect(CardDataSO cardData, int splitCount)
    {
        Debug.Log($"Executing Split Effect (Multi-attack) with {splitCount} projectiles.");

        if (poolManager == null || bulletPrefab == null)
        {
            Debug.LogError("[EffectExecutor] PoolManager 또는 bulletPrefab이 할당되지 않아 분열 효과를 실행할 수 없습니다.");
            return;
        }

        // 플레이어의 위치에서 총알 발사 (임시, 실제로는 플레이어의 발사 지점에서 나와야 함)
        Vector3 spawnPosition = playerController != null ? playerController.transform.position : Vector3.zero; // PlayerController.Instance 대신 playerController 사용
        if (playerController == null)
        {
            Debug.LogWarning("[EffectExecutor] PlayerController 참조를 찾을 수 없어 (0,0,0)에서 총알을 발사합니다.");
        }

        for (int i = 0; i < splitCount; i++)
        {
            float angle = i * (360f / splitCount);
            // 총알이 플레이어의 앞 방향을 기준으로 회전하도록 설정
            Quaternion rotation = Quaternion.Euler(0, 0, angle); // 2D 게임이므로 Z축 회전

            GameObject bullet = poolManager.Get(bulletPrefab);
            if (bullet != null)
            {
                bullet.transform.position = spawnPosition;
                bullet.transform.rotation = rotation;
                bullet.SetActive(true);
                // TODO: 총알에 힘을 가하거나 이동 로직 시작 (BulletController에서 처리)
                // BulletController bulletController = bullet.GetComponent<BulletController>();
                // if (bulletController != null)
                // {
                //     bulletController.Initialize(cardData.damageMultiplier); // 예시: 카드 데미지 배율 전달
                // }
            }
        }
    }

    /// <summary>
    /// '파동' 카드 효과를 실행합니다.
    /// project_plan.md: 반경300px, 속도600px/s, 지속2초
    /// </summary>
    /// <param name="cardData">파동 효과를 가진 카드 데이터</param>
    private void ExecuteWaveEffect(CardDataSO cardData)
    {
        Debug.Log($"Executing Wave Effect for {cardData.cardName}.");

        if (poolManager == null || waveEffectPrefab == null)
        {
            Debug.LogError("[EffectExecutor] PoolManager 또는 waveEffectPrefab이 할당되지 않아 파동 효과를 실행할 수 없습니다.");
            return;
        }

        // 플레이어의 위치에서 파동 효과 생성
        Vector3 spawnPosition = playerController != null ? playerController.transform.position : Vector3.zero; // PlayerController.Instance 대신 playerController 사용
        if (playerController == null)
        {
            Debug.LogWarning("[EffectExecutor] PlayerController 참조를 찾을 수 없어 (0,0,0)에서 파동 효과를 생성합니다.");
        }

        GameObject wave = poolManager.Get(waveEffectPrefab);
        if (wave != null)
        {
            wave.transform.position = spawnPosition;
            wave.SetActive(true);
            // TODO: 파동 효과의 크기, 속도, 지속 시간 설정 로직 구현
            // 예: wave.GetComponent<WaveEffectController>().Initialize(300f, 600f, 2f);
        }
    }

    /// <summary>
    /// '나선형' 카드 효과를 실행합니다.
    /// </summary>
    /// <param name="cardData">나선형 효과를 가진 카드 데이터</param>
    private void ExecuteSpiralEffect(CardDataSO cardData)
    {
        Debug.Log($"Executing Spiral Effect for {cardData.cardName}.");

        if (poolManager == null || spiralEffectPrefab == null)
        {
            Debug.LogError("[EffectExecutor] PoolManager 또는 spiralEffectPrefab이 할당되지 않아 나선형 효과를 실행할 수 없습니다.");
            return;
        }

        // 플레이어의 위치에서 나선형 총알 발사
        Vector3 spawnPosition = playerController != null ? playerController.transform.position : Vector3.zero; // PlayerController.Instance 대신 playerController 사용
        if (playerController == null)
        {
            Debug.LogWarning("[EffectExecutor] PlayerController 참조를 찾을 수 없어 (0,0,0)에서 나선형 효과를 생성합니다.");
        }

        // 임시 값: 나선형으로 발사할 총알 개수
        int numberOfSpiralProjectiles = 8;
        // 임시 값: 나선형 발사 간의 각도 증가량
        float angleIncrement = 360f / numberOfSpiralProjectiles;

        for (int i = 0; i < numberOfSpiralProjectiles; i++)
        {
            float angle = i * angleIncrement;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject bullet = poolManager.Get(bulletPrefab); // 나선형도 총알 프리팹 사용
            if (bullet != null)
            {
                bullet.transform.position = spawnPosition;
                bullet.transform.rotation = rotation;
                bullet.SetActive(true);
                // TODO: 총알에 힘을 가하거나 이동 로직 시작 (BulletController에서 처리)
            }
        }
    }

    // TODO: 다른 이펙트 패턴(Lightning 등)에 대한 구체적인 메서드 구현
    // private void ExecuteLightningEffect(CardDataSO cardData) { ... }
}
