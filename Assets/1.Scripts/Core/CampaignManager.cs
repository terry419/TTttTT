// [����] CampaignManager.cs ��ü �ڵ��Դϴ�.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CampaignManager : MonoBehaviour
{
    public static CampaignManager Instance { get; private set; }

    [Header("ķ���� ���")]
    [Tooltip("���⿡ ���ӿ��� ���� ��� ķ���� SO ������ �����ϼ���.")]
    public List<CampaignDataSO> availableCampaigns; // [����] ���� ķ������ ���� ����Ʈ

    private CampaignDataSO currentCampaign; // [����] �̹� ���ӿ��� �÷����� ķ����

    void Awake()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// [�� �Լ�] ���� ���� �� ��ϵ� ķ���� �� �ϳ��� �������� �����մϴ�.
    /// </summary>
    public void SelectAndStartRandomCampaign()
    {
        if (availableCampaigns == null || availableCampaigns.Count == 0)
        {
            Debug.LogError("[CampaignManager] ��� ������ ķ������ �����ϴ�! �ν����Ϳ��� ķ������ ������ּ���.");
            currentCampaign = null;
            return;
        }

        // ����Ʈ���� �������� ķ���� �ϳ��� �����Ͽ� ���� ķ�������� ����
        currentCampaign = availableCampaigns[Random.Range(0, availableCampaigns.Count)];
        ResetCampaign(); // ���� �ε��� �ʱ�ȭ
        Debug.Log($"[CampaignManager] ���ο� ķ���� '{currentCampaign.name}'��(��) �������� ���õǾ����ϴ�.");
    }

    /// <summary>
    /// ����� Y��ǥ(�ε���)�� �ش��ϴ� ���� �����͸� ��ȯ�մϴ�.
    /// </summary>
    public RoundDataSO GetRoundDataForNode(MapNode node)
    {
        if (currentCampaign == null)
        {
            Debug.LogError("### ����� ### GetRoundDataForNode ����: currentCampaign�� null�Դϴ�! �ν����Ϳ� ķ������ ��ϵǾ����� Ȯ���ϼ���.");
            return null;
        }
        // [����] currentCampaign�� ���õǾ����� Ȯ���ϴ� ��� �ڵ� �߰�
        if (currentCampaign == null)
        {
            Debug.LogError("[CampaignManager] ���� ������ ķ������ ���õ��� �ʾҽ��ϴ�!");
            return null;
        }
        if (node == null) return null;

        int roundIndex = node.Position.y;

        if (roundIndex >= 0 && roundIndex < currentCampaign.rounds.Count)
        {
            Debug.Log($"��û�� ���(Y:{roundIndex})�� �´� ���� ������ '{currentCampaign.rounds[roundIndex].name}'�� ��ȯ�մϴ�.");
            return currentCampaign.rounds[roundIndex];
        }
        else
        {
            Debug.LogError($"�߸��� ���� �ε���({roundIndex})�� ��û�Ǿ����ϴ�!");
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