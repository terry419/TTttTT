using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI 요소 참조")]
    public Button optionsButton;
    public Button startButton;
    public Button codexButton;
    public Button exitButton;
    public TextMeshProUGUI versionInfoText;

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        codexButton.onClick.AddListener(OnCodexButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);

        if (versionInfoText != null)
        {
            versionInfoText.text = "Version: " + Application.version;
        }
    }

    public void OnOptionsButtonClicked() { Debug.Log("옵션 버튼 클릭!"); }
    public void OnStartButtonClicked() { ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.CharacterSelect); }
    public void OnCodexButtonClicked() { ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.Codex); }
    public void OnExitButtonClicked() { Application.Quit(); }
}