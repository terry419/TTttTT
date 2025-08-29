using UnityEngine;
using System.Collections.Generic;

// 이 스크립트는 테스트 씬에서만 사용됩니다.
public class TestHarness : MonoBehaviour
{
    [Header("맵 생성기 참조")]
    [Tooltip("테스트 씬에 있는 MapGenerator 컴포넌트를 연결해야 합니다.")]
    [SerializeField] private MapGenerator mapGenerator;

    // 버튼 클릭 시 호출될 함수
    public void StartGameAs(string characterId)
    {
        Debug.Log($"[TestHarness] '{characterId}' 캐릭터로 테스트 시작...");

        // 필수 매니저들을 불러옵니다.
        var gameManager = ServiceLocator.Get<GameManager>();
        var dataManager = ServiceLocator.Get<DataManager>();
        var mapManager = ServiceLocator.Get<MapManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        var sceneTransitionManager = ServiceLocator.Get<SceneTransitionManager>();

        if (mapGenerator == null)
        {
            Debug.LogError("[TestHarness] MapGenerator가 연결되지 않았습니다! 테스트를 진행할 수 없습니다.");
            return;
        }

        // 1. 테스트용 맵 생성 및 초기화
        List<MapNode> mapData = mapGenerator.Generate();
        mapManager.InitializeMap(mapData, mapGenerator.MapWidth, mapGenerator.MapHeight);
        Debug.Log("[TestHarness] 테스트용 맵 데이터 생성 및 초기화 완료.");

        // 2. 캠페인 선택
        campaignManager.SelectRandomCampaign();

        // 3. GameManager에 테스트 데이터 설정
        gameManager.SelectedCharacter = dataManager.GetCharacter(characterId);
        gameManager.AllocatedPoints = 0; // 테스트 시에는 0으로 고정하거나, UI InputField로 값을 받도록 확장할 수 있습니다.
        gameManager.isFirstRound = true;

        // 4. Gameplay 씬으로 전환
        sceneTransitionManager.LoadScene(SceneNames.GamePlay);
    }
}