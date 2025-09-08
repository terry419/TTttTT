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
    [SerializeField] private InventoryManager inventoryManager;

    [Header("전문 컴포넌트 참조")]
    [SerializeField] private MapView mapView;
    [SerializeField] private AutoFocusScroller autoFocusScroller;

        [Header("포커스 대상 버튼")]
    [SerializeField] private Button rewardPageButton;
    [SerializeField] private Button inventoryButton;

    // --- Unity Lifecycle Methods --- //
    void Awake()
    {
        if (ServiceLocator.IsRegistered<RouteSelectionController>())
        {
            Debug.LogWarning($"[{GetType().Name}] 중복 생성되어 파괴됩니다.", this.gameObject);
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Register<RouteSelectionController>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<RouteSelectionController>(this);
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
        // 직접 내용을 실행하는 대신 코루틴을 시작하도록 변경
        StartCoroutine(ShowRoutine());
    }

    // UI 활성화와 네비게이션 설정을 분리하기 위한 코루틴
    private IEnumerator ShowRoutine()
    {
        Debug.Log($"[패널 제어 디버그] ShowRoutine 시작. RouteSelectPanel을 활성화합니다. (Frame: {Time.frameCount})");
        routeSelectPanel.SetActive(true);

        yield return null;

        Debug.Log($"[패널 제어 디버그] 한 프레임 대기 완료. 네비게이션 설정을 시작합니다. (Frame: {Time.frameCount})");
        GenerateMapView();
        UpdateNodeInteractability();
        mapView.SetupAllNodeNavigations(rewardPageButton, inventoryButton);

        StartCoroutine(SetFocusRoutine());
    }

    /// <summary>
    /// 맵 선택 패널을 비활성화합니다.
    /// </summary>
    public void Hide()
    {
        Debug.Log($"[패널 제어 디버그] Hide 호출됨. RouteSelectPanel을 비활성화합니다. (Frame: {Time.frameCount})");
        routeSelectPanel.SetActive(false);
    }
    public void ShowInventory()
    {
        if (inventoryManager != null)
        {
            // 인벤토리를 열고, "닫혔을 때 실행할 행동"을 콜백으로 넘겨줍니다.
            inventoryManager.Show(() =>
            {
                // 이 패널(RouteSelectPanel)을 다시 활성화하고,
                this.routeSelectPanel.SetActive(true);
                // 포커스를 원래의 인벤토리 버튼으로 되돌립니다.
                EventSystem.current.SetSelectedGameObject(this.inventoryButton.gameObject);
            });

            // 콜백을 넘겨준 후, 이 패널은 비활성화합니다.
            this.routeSelectPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[RouteSelectionController] inventoryManager가 할당되지 않았습니다.");
        }
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
    private void OnNodeClicked(MapNode node)
    {
        var rewardManager = ServiceLocator.Get<RewardManager>();
        // 보상 선택이 완료되지 않았다면 노드 클릭 무시
        if (rewardManager != null && !rewardManager.IsRewardSelectionComplete)
        {
            Debug.Log($"[{GetType().Name}] 보상 선택이 완료되지 않아 노드 클릭을 무시합니다.");
            return;
        }
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
        yield return null;
        mapView.SetupAllNodeNavigations(rewardPageButton, inventoryButton);

        EventSystem.current.SetSelectedGameObject(null);
        GameObject targetObjectToFocus = null;

        var rewardManager = ServiceLocator.Get<RewardManager>();
        bool isRewardSelectionComplete = (rewardManager == null || rewardManager.IsRewardSelectionComplete);

        // (2,0) 노드를 한 번만 찾아서 재사용합니다.
        GameObject targetNodeObject = mapView.GetNodeUIObject(new Vector2Int(2, 0));
        Button targetNodeButton = (targetNodeObject != null) ? targetNodeObject.GetComponent<Button>() : null;

        // '보상 페이지로' 버튼 설정
        if (rewardPageButton != null)
        {
            rewardPageButton.gameObject.SetActive(!isRewardSelectionComplete);

            if (!isRewardSelectionComplete && targetNodeButton != null)
            {
                Navigation nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = targetNodeButton,
                    selectOnDown = targetNodeButton,
                    selectOnLeft = targetNodeButton,
                    selectOnRight = targetNodeButton
                };
                rewardPageButton.navigation = nav;
            }
        }

        // '인벤토리' 버튼 설정 (새로 추가된 로직)
        if (inventoryButton != null && targetNodeButton != null)
        {
            Navigation nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = targetNodeButton,
                selectOnDown = targetNodeButton,
                selectOnLeft = targetNodeButton,
                selectOnRight = targetNodeButton
            };
            inventoryButton.navigation = nav;
        }

        // 초기 포커스 설정
        if (isRewardSelectionComplete)
        {
            targetObjectToFocus = mapView.FindLeftmostAvailableNode(ServiceLocator.Get<MapManager>().GetReachableNodes());
        }
        else
        {
            if (rewardPageButton != null && rewardPageButton.gameObject.activeInHierarchy)
            {
                targetObjectToFocus = rewardPageButton.gameObject;
            }
        }

        if (targetObjectToFocus != null)
        {
            EventSystem.current.SetSelectedGameObject(targetObjectToFocus);
        }
    }
}