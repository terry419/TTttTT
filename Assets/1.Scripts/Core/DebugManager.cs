// 파일명: DebugManager.cs (리팩토링 완료)
using UnityEngine;
using TMPro;
using System.Text;

/// <summary>
/// 개발 및 테스트 편의를 위한 디버그 기능을 관리하는 클래스입니다.
/// F1키로 디버그 UI를 토글하고, 게임 내 주요 변수를 실시간으로 확인하고 조작하는 기능을 제공합니다.
/// </summary>
public class DebugManager : MonoBehaviour
{
    [Header("디버그 UI 참조")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("디버그 기능 설정")]
    [SerializeField] private float healthToAdd = 50f;

    private bool isDebugModeEnabled = false;
    private CharacterStats playerStats; // 외부에서 등록받을 플레이어 스탯 참조
    private StringBuilder infoBuilder = new StringBuilder();

    void Awake()
    {
        if (!ServiceLocator.IsRegistered<DebugManager>())
        {
            ServiceLocator.Register<DebugManager>(this);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (debugPanel != null) debugPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ToggleDebugMode();
        }
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
    }

    // --- 외부 컴포넌트 등록/해제 메서드 ---

    /// <summary>
    /// PlayerStats 컴포넌트를 디버그 매니저에 등록합니다.
    /// </summary>
    public void RegisterPlayer(CharacterStats player)
    {
        playerStats = player;
        Debug.Log("[DebugManager] PlayerStats 등록됨.");
    }

    /// <summary>
    /// 등록된 PlayerStats 컴포넌트를 해제합니다.
    /// </summary>
    public void UnregisterPlayer()
    {
        playerStats = null;
        Debug.Log("[DebugManager] PlayerStats 등록 해제됨.");
    }

    private void UpdateDebugInfo()
    {
        if (infoText == null) return;
        infoBuilder.Clear();

        if (playerStats != null)
        {
            infoBuilder.AppendLine("--- Player Stats ---");
            infoBuilder.AppendLine($"Health: {playerStats.currentHealth:F1} / {playerStats.FinalHealth:F1}");
            infoBuilder.AppendLine($"Is Invulnerable: {playerStats.isInvulnerable}");
            infoBuilder.AppendLine($"Damage: {playerStats.FinalDamage:F2}");
            infoBuilder.AppendLine($"Attack Speed: {playerStats.FinalAttackSpeed:F2}");
            infoBuilder.AppendLine($"Move Speed: {playerStats.FinalMoveSpeed:F2}");
            infoBuilder.AppendLine($"Crit Rate: {playerStats.FinalCritRate:P2}");
            infoBuilder.AppendLine($"Crit Damage: {playerStats.FinalCritDamage:P2}");
        }
        else
        {
            infoBuilder.AppendLine("PlayerStats not registered.");
        }

        infoBuilder.AppendLine("\n--- Game Info ---");
        infoBuilder.AppendLine($"Active Monsters: {FindObjectsOfType<MonsterController>().Length}");
        infoText.text = infoBuilder.ToString();
    }

    // --- 디버그 UI의 버튼/슬라이더와 연결될 메서드들 ---

    public void GodMode(bool isOn)
    {
        if (playerStats != null)
        {
            playerStats.isInvulnerable = isOn;
            Debug.Log($"갓 모드: {isOn}");
        }
    }

    public void AddPlayerHealth()
    {
        if (playerStats != null)
        {
            playerStats.Heal(healthToAdd);
            Debug.Log($"플레이어 체력 {healthToAdd} 증가");
        }
    }

    public void KillAllMonsters()
    {
        MonsterController[] monsters = FindObjectsOfType<MonsterController>();
        foreach (var monster in monsters)
        {
            monster.TakeDamage(monster.currentHealth);
        }
        Debug.Log($"{monsters.Length}마리의 몬스터를 처치했습니다.");
    }
}