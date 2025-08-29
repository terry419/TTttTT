using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// �� ��ũ��Ʈ�� Unity �����Ϳ����� �����մϴ�.
public class DebugMenu
{
    // �޴� ��� "Tools/Debug/Start as Warrior"�� �޴� �������� �߰��մϴ�.
    [MenuItem("Tools/Debug/Start as Warrior (�� ���� ��)")]
    private static void StartGameAsWarrior()
    {
        // �����Ͱ� �÷��� ��尡 �ƴ� ��� ��� �޽����� ���� ������ �ߴ��մϴ�.
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("����", "�� �޴��� �����Ͱ� �÷��� ����� ���� ����� �� �ֽ��ϴ�.", "Ȯ��");
            return;
        }

        // �ʼ� �Ŵ������� ServiceLocator�� ���� �����ϰ� �����ɴϴ�.
        var gameManager = ServiceLocator.Get<GameManager>();
        var dataManager = ServiceLocator.Get<DataManager>();
        var mapManager = ServiceLocator.Get<MapManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        var sceneTransitionManager = ServiceLocator.Get<SceneTransitionManager>();

        // ��� �Ŵ����� �����ϴ��� Ȯ���մϴ�. (Initializer ������ ���� �����ߴٸ� �����ؾ� �մϴ�.)
        if (gameManager == null || dataManager == null || mapManager == null || campaignManager == null || sceneTransitionManager == null)
        {
            EditorUtility.DisplayDialog("����", "�ʼ� �Ŵ����� ã�� �� �����ϴ�. Initializer ������ ������ �����ߴ��� Ȯ���ϼ���.", "Ȯ��");
            return;
        }

        // 1. �� ����: MapGenerator�� ���������� �����ؾ� ã�� �� �����Ƿ�, �ӽ÷� �ε��ϰų� ���� ��ġ�ؾ� �մϴ�.
        //    ���� ������ ����� PointAllocation ���� �ִ� MapGenerator�� �����ϴ� ���Դϴ�.
        //    ����� ��� ������ ���� �ӽ� MapGenerator�� �����մϴ�.
        GameObject tempGeneratorObj = new GameObject("TempMapGenerator");
        MapGenerator mapGenerator = tempGeneratorObj.AddComponent<MapGenerator>();

        List<MapNode> mapData = mapGenerator.Generate();
        mapManager.InitializeMap(mapData, mapGenerator.MapWidth, mapGenerator.MapHeight);
        Object.Destroy(tempGeneratorObj); // ��� �� ��� �ı�
        Debug.Log("[DebugMenu] �׽�Ʈ�� �� ���� �� �ʱ�ȭ �Ϸ�.");

        // 2. ķ���� ����
        campaignManager.SelectRandomCampaign();

        // 3. GameManager�� ������ ���� (���� ���� ���� ���, ���� ���� �޼��� ���)
        CharacterDataSO warriorData = dataManager.GetCharacter(CharacterIDs.Warrior);
        gameManager.SetupForTest(warriorData, 0);

        // 4. Gameplay ������ ��ȯ
        sceneTransitionManager.LoadScene(SceneNames.GamePlay);
    }
}