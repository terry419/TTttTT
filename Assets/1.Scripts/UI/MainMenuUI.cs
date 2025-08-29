using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI 요소 참조")]
    public Button optionsButton;
    public Button startButton;
    public Button codexButton;
    public Button exitButton;
    public TextMeshProUGUI versionInfoText;
    private GameObject lastSelected; // 마지막으로 포커스된 오브젝트를 추적하기 위한 변수


    void Start()
    {
        Debug.Log("[INPUT TRACE ⑦] MainMenuUI.Start: 시작 및 버튼 리스너 등록.");
        startButton.onClick.AddListener(OnStartButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        codexButton.onClick.AddListener(OnCodexButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);

        StartCoroutine(SetInitialFocus());
    }

    private IEnumerator SetInitialFocus()
    {
        // EventSystem이 준비될 때까지 한 프레임 대기합니다.
        yield return null;

        // 모든 포커스를 해제한 뒤, 시작 버튼에 강제로 포커스를 맞춥니다.
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        Debug.Log("[INPUT TRACE] MainMenuUI: 초기 포커스를 'StartButton'으로 설정 완료.");
    }

    void Update()
    {
        // EventSystem의 현재 상태를 매 프레임 추적합니다.
        if (EventSystem.current == null) return;

        GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
        if (currentSelected != lastSelected)
        {
            Debug.Log($"[INPUT TRACE] EventSystem 포커스 변경: '{(lastSelected != null ? lastSelected.name : "없음")}' -> '{(currentSelected != null ? currentSelected.name : "없음")}'");
            lastSelected = currentSelected;
        }
    }

    public void OnOptionsButtonClicked() { Debug.Log("옵션 버튼 클릭!"); }
    public void OnStartButtonClicked() 
    {
        Debug.Log("[INPUT TRACE ⑧] MainMenuUI: 'Start' 버튼 클릭 이벤트 정상 처리됨.");
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.CharacterSelect); 
    }
    public void OnCodexButtonClicked() { ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.Codex); }
    public void OnExitButtonClicked() { Application.Quit(); }
}