// [수정] CampaignManager.cs 전체 코드입니다.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CampaignManager : MonoBehaviour
{
    public static CampaignManager Instance { get; private set; }

    [Header("캠페인 목록")]
    [Tooltip("여기에 게임에서 사용될 모든 캠페인 SO 파일을 연결하세요.")]
    public List<CampaignDataSO> availableCampaigns; // [수정] 여러 캠페인을 담을 리스트

    private CampaignDataSO currentCampaign; // [수정] 이번 게임에서 플레이할 캠페인

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

    /// <summary>
    /// [새 함수] 게임 시작 시 등록된 캠페인 중 하나를 랜덤으로 선택합니다.
    /// </summary>
    public void SelectAndStartRandomCampaign()
    {
        if (availableCampaigns == null || availableCampaigns.Count == 0)
        {
            Debug.LogError("[CampaignManager] 사용 가능한 캠페인이 없습니다! 인스펙터에서 캠페인을 등록해주세요.");
            currentCampaign = null;
            return;
        }

        // 리스트에서 랜덤으로 캠페인 하나를 선택하여 현재 캠페인으로 설정
        currentCampaign = availableCampaigns[Random.Range(0, availableCampaigns.Count)];
        ResetCampaign(); // 라운드 인덱스 초기화
        Debug.Log($"[CampaignManager] 새로운 캠페인 '{currentCampaign.name}'이(가) 랜덤으로 선택되었습니다.");
    }

    /// <summary>
    /// 노드의 Y좌표(인덱스)에 해당하는 라운드 데이터를 반환합니다.
    /// </summary>
    public RoundDataSO GetRoundDataForNode(MapNode node)
    {
        if (currentCampaign == null)
        {
            Debug.LogError("### 디버그 ### GetRoundDataForNode 실패: currentCampaign이 null입니다! 인스펙터에 캠페인이 등록되었는지 확인하세요.");
            return null;
        }
        // [수정] currentCampaign이 선택되었는지 확인하는 방어 코드 추가
        if (currentCampaign == null)
        {
            Debug.LogError("[CampaignManager] 현재 진행할 캠페인이 선택되지 않았습니다!");
            return null;
        }
        if (node == null) return null;

        int roundIndex = node.Position.y;

        if (roundIndex >= 0 && roundIndex < currentCampaign.rounds.Count)
        {
            Debug.Log($"요청된 노드(Y:{roundIndex})에 맞는 라운드 데이터 '{currentCampaign.rounds[roundIndex].name}'을 반환합니다.");
            return currentCampaign.rounds[roundIndex];
        }
        else
        {
            Debug.LogError($"잘못된 라운드 인덱스({roundIndex})가 요청되었습니다!");
            return null;
        }
    }

    public void ResetCampaign()
    {
    }
}