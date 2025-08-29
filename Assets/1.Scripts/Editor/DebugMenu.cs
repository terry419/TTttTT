using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

// 이 스크립트는 Unity 에디터에서만 동작합니다.
public class DebugMenu
{
    // 메뉴 경로 "Tools/Debug/Start as Warrior"에 메뉴 아이템을 추가합니다.
    [MenuItem("Tools/Debug/Start as Warrior (맵 생성 후)")]
    private static void StartGameAsWarrior()
    {
        // 에디터가 플레이 모드가 아닐 경우 경고 메시지를 띄우고 실행을 중단합니다.
        if (!EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("오류", "이 메뉴는 에디터가 플레이 모드일 때만 사용할 수 있습니다.", "확인");
            return;
        }

        // 필수 매니저들을 ServiceLocator를 통해 안전하게 가져옵니다.
        var gameManager = ServiceLocator.Get<GameManager>();
        var dataManager = ServiceLocator.Get<DataManager>();
        var mapManager = ServiceLocator.Get<MapManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        var sceneTransitionManager = ServiceLocator.Get<SceneTransitionManager>();

        // 모든 매니저가 존재하는지 확인합니다. (Initializer 씬부터 정상 실행했다면 존재해야 합니다.)
        if (gameManager == null || dataManager == null || mapManager == null || campaignManager == null || sceneTransitionManager == null)
        {
            EditorUtility.DisplayDialog("오류", "필수 매니저를 찾을 수 없습니다. Initializer 씬부터 게임을 시작했는지 확인하세요.", "확인");
            return;
        }

        // 1. 맵 생성: MapGenerator는 프리팹으로 존재해야 찾을 수 있으므로, 임시로 로드하거나 씬에 배치해야 합니다.
        //    가장 간단한 방법은 PointAllocation 씬에 있는 MapGenerator를 참조하는 것입니다.
        //    현재는 즉시 실행을 위해 임시 MapGenerator를 생성합니다.
        GameObject tempGeneratorObj = new GameObject("TempMapGenerator");
        MapGenerator mapGenerator = tempGeneratorObj.AddComponent<MapGenerator>();

        List<MapNode> mapData = mapGenerator.Generate();
        mapManager.InitializeMap(mapData, mapGenerator.MapWidth, mapGenerator.MapHeight);
        Object.Destroy(tempGeneratorObj); // 사용 후 즉시 파괴
        Debug.Log("[DebugMenu] 테스트용 맵 생성 및 초기화 완료.");

        // 2. 캠페인 선택
        campaignManager.SelectRandomCampaign();

        // 3. GameManager에 데이터 설정 (직접 변수 접근 대신, 새로 만든 메서드 사용)
        CharacterDataSO warriorData = dataManager.GetCharacter(CharacterIDs.Warrior);
        gameManager.SetupForTest(warriorData, 0);

        // 4. Gameplay 씬으로 전환
        sceneTransitionManager.LoadScene(SceneNames.GamePlay);
    }
}