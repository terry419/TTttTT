using UnityEngine;
using System.Collections.Generic;

// �� ��ũ��Ʈ�� �׽�Ʈ �������� ���˴ϴ�.
public class TestHarness : MonoBehaviour
{
    [Header("�� ������ ����")]
    [Tooltip("�׽�Ʈ ���� �ִ� MapGenerator ������Ʈ�� �����ؾ� �մϴ�.")]
    [SerializeField] private MapGenerator mapGenerator;

    // ��ư Ŭ�� �� ȣ��� �Լ�
    public void StartGameAs(string characterId)
    {
        Debug.Log($"[TestHarness] '{characterId}' ĳ���ͷ� �׽�Ʈ ����...");

        // �ʼ� �Ŵ������� �ҷ��ɴϴ�.
        var gameManager = ServiceLocator.Get<GameManager>();
        var dataManager = ServiceLocator.Get<DataManager>();
        var mapManager = ServiceLocator.Get<MapManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        var sceneTransitionManager = ServiceLocator.Get<SceneTransitionManager>();

        if (mapGenerator == null)
        {
            Debug.LogError("[TestHarness] MapGenerator�� ������� �ʾҽ��ϴ�! �׽�Ʈ�� ������ �� �����ϴ�.");
            return;
        }

        // 1. �׽�Ʈ�� �� ���� �� �ʱ�ȭ
        List<MapNode> mapData = mapGenerator.Generate();
        mapManager.InitializeMap(mapData, mapGenerator.MapWidth, mapGenerator.MapHeight);
        Debug.Log("[TestHarness] �׽�Ʈ�� �� ������ ���� �� �ʱ�ȭ �Ϸ�.");

        // 2. ķ���� ����
        campaignManager.SelectRandomCampaign();

        // 3. GameManager�� �׽�Ʈ ������ ����
        gameManager.SelectedCharacter = dataManager.GetCharacter(characterId);
        gameManager.AllocatedPoints = 0; // �׽�Ʈ �ÿ��� 0���� �����ϰų�, UI InputField�� ���� �޵��� Ȯ���� �� �ֽ��ϴ�.
        gameManager.isFirstRound = true;

        // 4. Gameplay ������ ��ȯ
        sceneTransitionManager.LoadScene(SceneNames.GamePlay);
    }
}