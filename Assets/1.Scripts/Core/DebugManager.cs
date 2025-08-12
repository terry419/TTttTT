using UnityEngine;

/// <summary>
/// 개발 및 테스트 편의를 위한 디버그 기능을 관리하는 싱글톤 클래스입니다.
/// F1키로 디버그 UI를 토글하고, 게임 내 주요 변수(플레이어 능력치, 몬스터 스폰 등)를
/// 실시간으로 확인하고 조작하는 기능을 제공합니다. 최종 빌드에서는 비활성화되어야 합니다.
/// </summary>
public class DebugManager : MonoBehaviour
{
    public static DebugManager Instance { get; private set; }

    [Header("디버그 UI 참조")]
    [SerializeField] private GameObject debugPanel; // 디버그 UI의 최상위 패널

    private bool isDebugModeEnabled = false;

    // --- 실시간 조작을 위한 참조 변수들 ---
    private CharacterStats playerStats;
    private MonsterSpawner monsterSpawner;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // 디버그 패널은 기본적으로 비활성화
        if (debugPanel != null) debugPanel.SetActive(false);

        // 참조할 컴포넌트들을 찾습니다. 씬에 해당 오브젝트가 없을 수도 있으므로 null 체크가 필요합니다.
        // playerStats = FindObjectOfType<CharacterStats>();
        // monsterSpawner = FindObjectOfType<MonsterSpawner>();
    }

    void Update()
    {
        // F1 키를 눌러 디버그 모드를 토글합니다.
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebugMode();
        }

        // 디버그 모드가 활성화된 상태에서만 UI를 업데이트합니다.
        if (isDebugModeEnabled && debugPanel != null)
        {
            UpdateDebugInfo();
        }
    }

    public void ToggleDebugMode()
    {
        isDebugModeEnabled = !isDebugModeEnabled;
        if (debugPanel != null) debugPanel.SetActive(isDebugModeEnabled);
        Debug.Log($"디버그 모드: {(isDebugModeEnabled ? "활성화" : "비활성화")}");

        // 디버그 모드가 활성화되면 게임 시간을 느리게 하거나, 추가 정보를 로깅할 수 있습니다.
        // Time.timeScale = isDebugModeEnabled ? 0.1f : 1f;
    }

    /// <summary>
    /// 디버그 패널의 정보를 실시간으로 업데이트합니다.
    /// </summary>
    private void UpdateDebugInfo()
    {
        // TODO: 디버그 패널의 Text UI 요소들에 아래 정보를 표시하는 로직 구현
        // if (playerStats != null)
        // {
        //     string playerInfo = $"Player Final Attack: {playerStats.finalDamage}";
        //     // ... 기타 플레이어 스탯 정보
        // }
        // if (monsterSpawner != null)
        // {
        //     string spawnerInfo = $"Monster Spawn Interval: {monsterSpawner.spawnInterval}";
        //     // ... 기타 스포너 정보
        // }
    }

    // --- 디버그 UI의 버튼/슬라이더와 연결될 메서드들 ---

    public void GodMode(bool isOn)
    {
        // TODO: 플레이어 무적 모드 적용 로직
        Debug.Log($"갓 모드: {isOn}");
    }

    public void AddPlayerHealth(float amount)
    {
        // TODO: 플레이어 체력 증가 로직
        // if(playerStats != null) playerStats.Heal(amount);
    }

    public void KillAllMonsters()
    {
        // TODO: 현재 씬의 모든 몬스터를 제거하는 로직
        // MonsterController[] monsters = FindObjectsOfType<MonsterController>();
        // foreach(var monster in monsters) { monster.Die(); }
    }
}
