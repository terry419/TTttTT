// --- 파일명: CampaignManager.cs (수정본) ---
// 경로: Assets/1.Scripts/Core/CampaignManager.cs
using UnityEngine;

public class CampaignManager : MonoBehaviour
{
    // [수정] 이 부분이 누락되어 오류가 발생했습니다.
    public static CampaignManager Instance { get; private set; }

    [Header("현재 진행할 캠페인")]
    [SerializeField] private CampaignDataSO currentCampaign;

    private int currentRoundIndex = 0;

    // [수정] Instance에 자기 자신을 할당하는 Awake 함수가 누락되었습니다.
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

    public RoundDataSO GetNextRoundData()
    {
        if (currentCampaign == null || currentCampaign.rounds.Count == 0)
        {
            Debug.LogError("진행할 캠페인 데이터가 없습니다!");
            return null;
        }

        if (currentRoundIndex < currentCampaign.rounds.Count)
        {
            RoundDataSO nextRound = currentCampaign.rounds[currentRoundIndex];
            currentRoundIndex++;
            Debug.Log($"캠페인 진행: {currentRoundIndex}/{currentCampaign.rounds.Count} 라운드 시작.");
            return nextRound;
        }
        else
        {
            Debug.Log("캠페인의 모든 라운드를 클리어했습니다!");
            ResetCampaign(); // 캠페인 클리어 후 초기화
            return null;
        }
    }

    public void ResetCampaign()
    {
        currentRoundIndex = 0;
    }
}