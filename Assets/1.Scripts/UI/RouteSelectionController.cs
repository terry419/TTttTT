using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Linq;

public class RouteSelectionController : MonoBehaviour
{
    [Header("제어할 UI 패널")]
    [SerializeField] private GameObject routeSelectPanel;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("전문 컴포넌트 참조")]
    [SerializeField] private MapView mapView;
    [SerializeField] private AutoFocusScroller autoFocusScroller;
    [Header("포커스 대상 버튼")]
    [SerializeField] private Button rewardPageButton;
    [SerializeField] private Button inventoryButton;

    void Awake()
    {
        if (ServiceLocator.IsRegistered<RouteSelectionController>())
        {
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
        routeSelectPanel.SetActive(false);
    }

    public void Show()
    {
        StartCoroutine(ShowRoutine());
    }

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

    public void Hide()
    {
        Debug.Log($"[패널 제어 디버그] Hide 호출됨. RouteSelectPanel을 비활성화합니다. (Frame: {Time.frameCount})");
        routeSelectPanel.SetActive(false);
    }

    // ▼▼▼ [수정] ShowInventory 메서드에 디버그 로그 추가 ▼▼▼
    public void ShowInventory()
    {
        if (inventoryManager != null)
        {
            Debug.Log("[RouteSelectionController] ShowInventory 호출. 인벤토리를 열고 닫기 콜백을 등록합니다.");
            // 인벤토리를 열고, "닫혔을 때 실행할 행동"을 콜백으로 넘겨줍니다.
            inventoryManager.Show(() =>
            {
                // 이 패널(RouteSelectPanel)을 다시 활성화하고,
                Debug.Log("<color=lime>[RouteSelectionController] 인벤토리 닫힘 콜백 실행! RouteSelectPanel을 다시 활성화합니다.</color>");
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

    // (GenerateMapView, OnNodeClicked 등 다른 메서드는 기존과 동일하게 유지)
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

    private void OnNodeClicked(MapNode node)
    {
        var rewardManager = ServiceLocator.Get<RewardManager>();
        if (rewardManager != null && !rewardManager.IsRewardSelectionComplete)
        {
            Debug.Log($"[{GetType().Name}] 보상 선택이 완료되지 않아 노드 클릭을 무시합니다.");
            return;
        }
        _ = SelectNodeAndPreload(node);
    }

    private async UniTask SelectNodeAndPreload(MapNode node)
    {
        Debug.Log($"[{GetType().Name}] 노드 {node.Position} 선택됨. 다음 라운드 프리로딩을 시작합니다.");
        mapView.UpdateNodeInteractability(new List<MapNode>());

        var gameManager = ServiceLocator.Get<GameManager>();
        var campaignManager = ServiceLocator.Get<CampaignManager>();
        var mapManager = ServiceLocator.Get<MapManager>();

        RoundDataSO nextRoundData = campaignManager.GetRoundDataForNode(node);
        if (nextRoundData != null)
        {
            await gameManager.PreloadAssetsForRound(nextRoundData);
        }

        mapManager.MoveToNode(node);
        Hide();
        gameManager.ChangeState(GameManager.GameState.Gameplay);
    }

    private void UpdateNodeInteractability()
    {
        var mapManager = ServiceLocator.Get<MapManager>();
        if (mapManager == null) return;

        List<MapNode> reachableNodes = mapManager.GetReachableNodes();
        mapView.UpdateNodeInteractability(reachableNodes);
    }

    private IEnumerator SetFocusRoutine()
    {
        yield return null;
        mapView.SetupAllNodeNavigations(rewardPageButton, inventoryButton);

        EventSystem.current.SetSelectedGameObject(null);
        GameObject targetObjectToFocus = null;

        var rewardManager = ServiceLocator.Get<RewardManager>();
        bool isRewardSelectionComplete = (rewardManager == null || rewardManager.IsRewardSelectionComplete);

        GameObject targetNodeObject = mapView.GetNodeUIObject(new Vector2Int(2, 0));
        Button targetNodeButton = (targetNodeObject != null) ?
        targetNodeObject.GetComponent<Button>() : null;

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