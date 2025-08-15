// --- ���ϸ�: CampaignManager.cs (������) ---
// ���: Assets/1.Scripts/Core/CampaignManager.cs
using UnityEngine;

public class CampaignManager : MonoBehaviour
{
    // [����] �� �κ��� �����Ǿ� ������ �߻��߽��ϴ�.
    public static CampaignManager Instance { get; private set; }

    [Header("���� ������ ķ����")]
    [SerializeField] private CampaignDataSO currentCampaign;

    private int currentRoundIndex = 0;

    // [����] Instance�� �ڱ� �ڽ��� �Ҵ��ϴ� Awake �Լ��� �����Ǿ����ϴ�.
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
            Debug.LogError("������ ķ���� �����Ͱ� �����ϴ�!");
            return null;
        }

        if (currentRoundIndex < currentCampaign.rounds.Count)
        {
            RoundDataSO nextRound = currentCampaign.rounds[currentRoundIndex];
            currentRoundIndex++;
            Debug.Log($"ķ���� ����: {currentRoundIndex}/{currentCampaign.rounds.Count} ���� ����.");
            return nextRound;
        }
        else
        {
            Debug.Log("ķ������ ��� ���带 Ŭ�����߽��ϴ�!");
            ResetCampaign(); // ķ���� Ŭ���� �� �ʱ�ȭ
            return null;
        }
    }

    public void ResetCampaign()
    {
        currentRoundIndex = 0;
    }
}