// 파일명: CampaignManager.cs (리팩토링 완료)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CampaignManager : MonoBehaviour
{
    [Header("캠페인 목록")]
    [Tooltip("여기에 캠페인으로 사용될 모든 캠페인 SO 목록을 추가하세요.")]
    public List<CampaignDataSO> availableCampaigns;

    private CampaignDataSO currentCampaign;

    void Awake()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        ServiceLocator.Register<CampaignManager>(this);
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// [주 함수] 사용 가능한 캠페인 중 하나를 무작위로 선택합니다.
    /// </summary>
    public CampaignDataSO SelectRandomCampaign()
    {
        if (availableCampaigns == null || availableCampaigns.Count == 0)
        {
            Debug.LogError("[CampaignManager] 사용 가능한 캠페인이 없습니다!");
            currentCampaign = null;
            return null;
        }

        currentCampaign = availableCampaigns[Random.Range(0, availableCampaigns.Count)];
        Debug.Log($"[CampaignManager] 새로운 캠페인 '{currentCampaign.name}'이(가) 선택되었습니다. (아직 시작은 안 함)");
        return currentCampaign;
    }

    public CampaignDataSO GetCurrentCampaign()
    {
        return currentCampaign;
    }

    /// <summary>
    /// 노드의 Y좌표(인덱스)에 해당하는 라운드 데이터를 반환합니다.
    /// </summary>
    public RoundDataSO GetRoundDataForNode(MapNode node)
    {
        if (currentCampaign == null)
        {
            Debug.LogError("### 오류 ### GetRoundDataForNode 호출: currentCampaign이 null입니다! 인스펙터에 캠페인이 등록되었는지 확인하세요.");
            return null;
        }

        if (node == null) return null;

        int roundIndex = node.Position.y;

        if (roundIndex >= 0 && roundIndex < currentCampaign.rounds.Count)
        {
            Debug.Log($"요청한 노드(Y:{roundIndex})에 맞는 라운드 데이터 '{currentCampaign.rounds[roundIndex].name}'을 반환합니다.");
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

    private void OnDestroy()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - OnDestroy() 시작. (프레임: {Time.frameCount})");
    }
}