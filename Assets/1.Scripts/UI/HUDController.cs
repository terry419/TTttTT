using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI killCountText;

    public void UpdateTimer(float time)
    {
        if (timerText == null) return;
        time = Mathf.Max(0, time);
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        // [수정] 텍스트를 영어로 변경
        timerText.text = $"Time : {minutes:00}:{seconds:00}";
    }

    public void UpdateKillCount(int currentKills, int goalKills)
    {
        if (killCountText != null)
        {
            // [수정] 텍스트를 영어로 변경
            killCountText.text = $"Kills: {currentKills} / {goalKills}";
        }
    }
}