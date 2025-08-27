// 파일 경로: Assets/1/Scripts/UI/RouteSelectionController.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// 맵 경로 선택 씬의 전체적인 흐름을 제어하는 '조율자(Coordinator)' 클래스입니다.
/// MapManager로부터 맵 데이터를 받아 MapView에 그리도록 지시하고,
/// RewardManager의 상태에 따라 UI 포커스와 버튼 활성화 여부를 결정합니다.
/// </summary>
public class RouteSelectionController : MonoBehaviour
{
    // --- Inspector-Visible Fields --- //
    [Header("제어할 UI 패널")]
    [SerializeField] private GameObject routeSelectPanel;

    [Header("전문 컴포넌트 참조")]
    [SerializeField] private MapView mapView;
    [SerializeField] private AutoFocusScroller autoFocusScroller;

    [Header("포커스 대상 버튼")]
    public Button rewardPageButton;

    // --- Unity Lifecycle Methods --- //
    void Awake()
    {
        ServiceLocator.Register<RouteSelectionController>(this);
    }

    void Start()
    {
        if (routeSelectPanel == null || mapView == null || autoFocusScroller == null)
        {
            Debug.LogError($"[{GetType().Name}] 필요한 UI 컴포넌트가 Inspector에 연결되지 않았습니다!", this.gameObject);
            return;
        }
        mapView.OnNodeSelected += OnNodeClicked;
        routeSelectPanel.SetActive(false); // 시작 시에는 비활성화 상태
    }

    // --- Public Methods --- //

    /// <summary>
    /// 맵 선택 패널을 활성화하고 관련된 모든 UI를 최신 상태로 업데이트합니다.
    /// 다른 UI 관리자(예: CardRewardUIManager)에 의해 호출됩니다.
    /// </summary>
    public void Show()
    {
        Debug.Log($"[{GetType().Name}] Show()가 호출되었습니다. 맵 선택 UI를 활성화합니다.");

        // 맵 뷰를 생성하기 전에, MapManager가 유효한 맵 데이터를 가지고 있는지 다시 확인합니다.
        GenerateMapView();

        routeSelectPanel.SetActive(true);
        UpdateNodeInteractability();
        mapView.SetupAllNodeNavigations(rewardPageButton); // 컨트롤러 네비게이션 설정
        StartCoroutine(SetFocusRoutine()); // 포커스 설정
    }

    /// <summary>
    /// 맵 선택 패널을 비활성화합니다.
    /// </summary>
    public void Hide()
    {
        routeSelectPanel.SetActive(false);
    }

    /// <summary>
    /// '보상 페이지로' 버튼을 클릭했을 때, 다시 카드 보상 UI로 돌아갑니다.
    /// </summary>
    public void GoBackToCardReward()
    {
        Debug.Log($"[{GetType().Name}] '보상 페이지로' 버튼이 클릭되었습니다. 카드 보상 UI로 돌아갑니다.");
        Hide();

        var cardRewardUI = ServiceLocator.Get<CardRewardUIManager>();
        if (cardRewardUI != null)
        {
            cardRewardUI.Show();
        }
    }

    // --- Private Helper Methods --- //

    /// <summary>
    /// MapManager의 데이터를 기반으로 MapView에 맵을 그리도록 지시합니다.
    /// </summary>
    private void GenerateMapView()
    {
        var mapManager = ServiceLocator.Get<MapManager>();

        // [디버깅] 맵이 그려지지 않는 문제의 원인을 찾기 위한 핵심 확인 지점입니다.
        if (mapManager == null || !mapManager.IsMapInitialized)
        {
            Debug.LogError($"[{GetType().Name}] CRITICAL: MapManager를 찾을 수 없거나, 맵 데이터가 초기화되지 않았습니다! (IsMapInitialized: {(mapManager != null ? mapManager.IsMapInitialized.ToString() : "N/A")}). 맵이 그려지지 않을 것입니다.");
            return;
        }

        Debug.Log($"[{GetType().Name}] 유효한 MapManager를 찾았습니다. MapView 생성을 시작합니다.");
        mapView.GenerateMapView(mapManager.AllNodes, mapManager.MapWidth, mapManager.MapHeight);
    }

    /// <summary>
    /// MapView에서 특정 노드가 클릭되었을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    // ▼▼▼▼▼ OnNodeClicked 함수를 아래 내용으로 교체 ▼▼▼▼▼
    private void OnNodeClicked(MapNode node)
    {
        // StartCoroutine을 제거하고 async 메소드를 직접 호출합니다.
        // UniTask의 결과를 기다리지 않고 다음으로 넘어가므로,
        // C# 7.0 이상에서는 아래와 같이 `_ = ` 를 붙여주는 것이 좋습니다. (의도적으로 결과를 무시한다는 의미)
        _ = SelectNodeAndPreload(node);
    }
    /// <summary>
    /// 노드 선택 후, 다음 라운드에 필요한 에셋을 프리로드하고 씬을 전환하는 코루틴입니다.
    /// </summary>
    private async UniTask SelectNodeAndPreload(MapNode node)
    {
        Debug.Log($"[{GetType().Name}] 노드 {node.Position} 선택됨. 다음 라운드 프리로딩을 시작합니다.");
        mapView.UpdateNodeInteractability(new List<MapNode>());

        var gameManager = ServiceLocator.Get<GameManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        var mapManager = ServiceLocator.Get<MapManager>();

        RoundDataSO nextRoundData = campaignManager.GetRoundDataForNode(node);

        // [수정] yield return StartCoroutine(...) -> await gameManager.PreloadAssetsForRound(...)
        if (nextRoundData != null)
        {
            await gameManager.PreloadAssetsForRound(nextRoundData);
        }

        mapManager.MoveToNode(node);
        Hide();
        gameManager.ChangeState(GameManager.GameState.Gameplay);
    }

    /// <summary>
    /// 현재 플레이어가 이동할 수 있는 노드만 활성화하도록 MapView에 지시합니다.
    /// </summary>
    // ▼▼▼▼▼▼▼▼▼▼▼ [이 함수 전체를 아래 코드로 교체하세요] ▼▼▼▼▼▼▼▼▼▼▼
    private void UpdateNodeInteractability()
    {
        var mapManager = ServiceLocator.Get<MapManager>();
        if (mapManager == null) return;

        List<MapNode> reachableNodes = mapManager.GetReachableNodes();
        mapView.UpdateNodeInteractability(reachableNodes);
    }

    /// <summary>
    /// UI가 활성화된 후, 현재 상황에 맞는 버튼에 자동으로 포커스를 설정하는 코루틴입니다.
    /// </summary>
    private IEnumerator SetFocusRoutine()
    {
        yield return null; // UI가 완전히 활성화될 때까지 한 프레임 대기
        EventSystem.current.SetSelectedGameObject(null);
        GameObject targetObjectToFocus = null;

        // [핵심 로직] RewardManager의 상태를 직접 가져와 '보상 페이지로' 버튼의 활성화 여부를 결정합니다.
        var rewardManager = ServiceLocator.Get<RewardManager>();
        // RewardManager가 없거나, 있더라도 보상 선택이 완료된 상태라면 true가 됩니다.
        bool isRewardSelectionComplete = (rewardManager == null || rewardManager.IsRewardSelectionComplete);

        Debug.Log($"[{GetType().Name}] 포커스 설정 중... RewardManager의 보상 선택 완료 상태: {isRewardSelectionComplete}");

        if (rewardPageButton != null)
        {
            // 보상 선택이 완료되지 않았을 때만 '보상 페이지로' 버튼을 활성화합니다.
            rewardPageButton.gameObject.SetActive(!isRewardSelectionComplete);
        }

        if (isRewardSelectionComplete)
        {
            // 보상 선택이 끝났다면, 선택 가능한 맵 노드 중 가장 왼쪽에 있는 노드에 포커스를 줍니다.
            targetObjectToFocus = mapView.FindLeftmostAvailableNode(ServiceLocator.Get<MapManager>().GetReachableNodes());
            Debug.Log($"[{GetType().Name}] 포커스 대상: 맵 노드 ({(targetObjectToFocus != null ? targetObjectToFocus.name : "없음")})");
        }
        else
        {
            // 아직 보상 선택이 끝나지 않았다면, '보상 페이지로' 버튼에 포커스를 줍니다.
            if (rewardPageButton != null)
            {
                targetObjectToFocus = rewardPageButton.gameObject;
                Debug.Log($"[{GetType().Name}] 포커스 대상: '보상 페이지로' 버튼");
            }
        }

        // 최종적으로 결정된 대상에 포커스를 설정합니다.
        if (targetObjectToFocus != null)
        {
            EventSystem.current.SetSelectedGameObject(targetObjectToFocus);
        }
    }
}