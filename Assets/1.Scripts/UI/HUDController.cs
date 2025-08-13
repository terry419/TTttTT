using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 인게임 화면의 HUD(Heads-Up Display)를 관리합니다.
/// 체력, 라운드 타이머, 킬 카운트 등 주요 정보를 표시하고 업데이트합니다.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("UI 요소 참조")]
    [SerializeField] private Slider healthBar; // 체력 바
    [SerializeField] private TextMeshProUGUI timerText; // 남은 시간 텍스트
    [SerializeField] private TextMeshProUGUI killCountText; // 킬 카운트 텍스트
    [SerializeField] private TextMeshProUGUI remainingEnemiesText; // 남은 적 수 (기획서 내용)

    // 플레이어의 CharacterStats 참조 (체력 업데이트를 위해)
    private CharacterStats playerStats;

    void Start()
    {
        // 플레이어를 찾아 체력 정보와 연결합니다.
        if (PlayerController.Instance != null)
        {
            playerStats = PlayerController.Instance.GetComponent<CharacterStats>();
        }
        else
        {
            Debug.LogError("PlayerController 인스턴스를 찾을 수 없습니다! HUD가 플레이어 정보를 가져올 수 없습니다.");
        }
    }

    void Update()
    {
        if (playerStats != null)
        {
            UpdateHealthBar(playerStats.GetCurrentHealth(), playerStats.finalHealth);
        }
    }

    /// <summary>
    /// 타이머 텍스트를 업데이트합니다.
    /// </summary>
    public void UpdateTimer(float time)
    {
        if (timerText == null) return;

        time = Mathf.Max(0, time);
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// 킬 카운트 텍스트와 남은 적 수를 업데이트합니다.
    /// </summary>
    public void UpdateKillCount(int currentKills, int goalKills)
    {
        if (killCountText != null) killCountText.text = $"Kills: {currentKills} / {goalKills}";
        if (remainingEnemiesText != null) remainingEnemiesText.text = $"남은 적: {goalKills - currentKills}";
    }

    /// <summary>
    /// 체력 바를 업데이트합니다.
    /// </summary>
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBar == null) return;

        if (maxHealth > 0)
        {
            healthBar.value = currentHealth / maxHealth;
        }
        else
        {
            healthBar.value = 0;
        }
    }

    // CharacterStats에 GetCurrentHealth() 메서드가 필요합니다.
    // 예시: public float GetCurrentHealth() { return currentHealth; }
}
