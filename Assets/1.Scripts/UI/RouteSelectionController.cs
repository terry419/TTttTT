using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 맵 경로 선택 씬의 전체적인 흐름을 제어하는 '조율자(Coordinator)' 클래스입니다.
/// 맵 데이터 생성, UI 표시, 자동 스크롤 등의 실제 작업은 각각의 전문 컴포넌트(MapView, AutoFocusScroller)에 위임합니다.
/// </summary>
public class RouteSelectionController : MonoBehaviour
{
    public static RouteSelectionController Instance { get; private set; }

    [Header("제어할 UI 패널")]
    [SerializeField] private GameObject routeSelectPanel;

    [Header("전문 컴포넌트 참조")]
    [SerializeField] private MapView mapView;
    [SerializeField] private AutoFocusScroller autoFocusScroller;

    [Header("포커스 대상 버튼")]
    public Button rewardPageButton;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (routeSelectPanel == null || mapView == null || autoFocusScroller == null)
        {
            Debug.LogError("[RouteSelectionController] 필요한 컴포넌트가 연결되지 않았습니다!");
            return;
        }
        mapView.OnNodeSelected += OnNodeClicked;
        routeSelectPanel.SetActive(false);
    }

    /// <summary>
    /// [수정됨] MapManager의 데이터를 기반으로 MapView를 생성하거나 업데이트합니다.
    /// </summary>
    private void GenerateMapView()
    {
        // [추가됨] mapView 참조가 null인지 확인하는 디버그 로그
        if (mapView == null)
        {
            Debug.LogError("[RouteSelectionController] MapView 참조가 설정되지 않았습니다! Inspector에서 연결해주세요.");
            return;
        }

        if (MapManager.Instance == null || !MapManager.Instance.IsMapInitialized)
        {
            Debug.LogError("[RouteSelectionController] MapManager가 초기화되지 않아 MapView를 생성할 수 없습니다!");
            return;
        }

        Debug.Log("[RouteSelectionController] MapManager의 데이터를 기반으로 MapView를 생성합니다.");

        mapView.GenerateMapView(
            MapManager.Instance.AllNodes,
            MapManager.Instance.MapWidth,
            MapManager.Instance.MapHeight
        );
    }

    private void OnNodeClicked(MapNode node)
    {
        Debug.Log($"[RouteSelectionController] 노드 클릭 감지: {node.Position}, 타입: {node.NodeType}");
        if (MapManager.Instance != null)
        {
            MapManager.Instance.MoveToNode(node);
            Hide();
        }
    }

    /// <summary>
    /// [수정됨] 맵 선택 패널을 활성화하고 맵 뷰를 업데이트합니다.
    /// </summary>
    public void Show()
    {
        GenerateMapView();

        routeSelectPanel.SetActive(true);
        UpdateNodeInteractability();
        mapView.SetupAllNodeNavigations(rewardPageButton);
        StartCoroutine(SetFocusRoutine());
    }

    private void UpdateNodeInteractability()
    {
        if (MapManager.Instance == null) return;
        List<MapNode> reachableNodes = MapManager.Instance.GetReachableNodes();

        mapView.UpdateNodeInteractability(reachableNodes);
    }

    private IEnumerator SetFocusRoutine()
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        GameObject targetObjectToFocus = null;

        bool isRewardSelected = (RewardManager.Instance != null && RewardManager.Instance.IsRewardSelectionComplete);

        if (rewardPageButton != null)
        {
            rewardPageButton.gameObject.SetActive(!isRewardSelected);
        }

        if (isRewardSelected)
        {
            targetObjectToFocus = mapView.FindLeftmostAvailableNode(MapManager.Instance.GetReachableNodes());

            if (rewardPageButton != null && targetObjectToFocus != null)
            {
                Navigation nav = rewardPageButton.navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnRight = targetObjectToFocus.GetComponent<Button>();
                rewardPageButton.navigation = nav;
            }
        }
        else
        {
            if (rewardPageButton != null)
            {
                targetObjectToFocus = rewardPageButton.gameObject;
            }
        }

        if (targetObjectToFocus != null)
        {
            EventSystem.current.SetSelectedGameObject(targetObjectToFocus);
        }
        else
        {
            Debug.LogWarning("[Focus] 포커스를 설정할 대상을 찾지 못했습니다.");
        }
    }

    public void Hide()
    {
        routeSelectPanel.SetActive(false);
    }

    public void GoBackToCardReward()
    {
        Hide();
        if (CardRewardUIManager.Instance != null)
        {
            CardRewardUIManager.Instance.Show();
        }
    }
}