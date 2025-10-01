using UnityEngine;
using TMPro;

public class BossStageHUDController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI elapsedTimeText;
    [SerializeField] private TextMeshProUGUI playerKillCountText;
    [SerializeField] private TextMeshProUGUI bossKillCountText;

    void OnEnable()
    {
        // BossStageManager가 보내는 이벤트에 구독
        BossStageManager.OnElapsedTimeChanged += UpdateElapsedTime;
        BossStageManager.OnKillCountsChanged += UpdateKillCounts;
    }

    void OnDisable()
    {
        // 구독 해제
        BossStageManager.OnElapsedTimeChanged -= UpdateElapsedTime;
        BossStageManager.OnKillCountsChanged -= UpdateKillCounts;
    }

    private void UpdateElapsedTime(float time)
    {
        if (elapsedTimeText == null) return;

        time = Mathf.Max(0, time);
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        elapsedTimeText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateKillCounts(int playerKills, int bossKills)
    {
        if (playerKillCountText != null)
        {
            playerKillCountText.text = $"Player Kills: {playerKills}";
        }

        if (bossKillCountText != null)
        {
            bossKillCountText.text = $"Boss Kills: {bossKills}";
        }
    }
}
