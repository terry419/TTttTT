using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsUIController : MonoBehaviour
{
    [Header("UI 요소")]
    [Tooltip("씬이 시작될 때 가장 먼저 선택될 버튼")]
    [SerializeField] private GameObject firstSelectedButton;
    [Tooltip("메인 메뉴로 돌아갈 버튼")]
    [SerializeField] private Button backButton;

    [Header("오디오 설정")]
    [Tooltip("BGM 볼륨 조절용 InteractiveSlider")]
    [SerializeField] private InteractiveSlider bgmVolumeSlider;
    [Tooltip("SFX 볼륨 조절용 InteractiveSlider")]
    [SerializeField] private InteractiveSlider sfxVolumeSlider;

    private AudioManager audioManager;

    void Start()
    {
        // ServiceLocator를 통해 AudioManager 인스턴스를 가져옵니다.
        audioManager = ServiceLocator.Get<AudioManager>();

        // 각 버튼과 슬라이더에 이벤트 리스너를 연결합니다.
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        if (bgmVolumeSlider != null)
        {
            bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        // 초기 포커스를 설정합니다.
        StartCoroutine(SetInitialFocus());

        // AudioManager가 있다면, 현재 볼륨 값으로 슬라이더를 초기화합니다.
        if (audioManager != null)
        {
            bgmVolumeSlider.value = audioManager.GetBgmVolume();
            sfxVolumeSlider.value = audioManager.GetSfxVolume();
        }
    }

    private void SetBgmVolume(float volume)
    {
        if (audioManager == null) return;
        audioManager.SetVolume(volume, sfxVolumeSlider.value);
        Debug.Log($"[Audio] BGM 볼륨 변경: {volume:P0}"); // P0 = 백분율(소수점 없음)
    }

    private void SetSfxVolume(float volume)
    {
        if (audioManager == null) return;
        audioManager.SetVolume(bgmVolumeSlider.value, volume);

        // (선택 사항) 볼륨 조절 시 효과음을 재생하여 즉시 피드백을 줍니다.
        // 이 기능을 사용하려면 "UI_SFX_Test" 라는 이름의 오디오 클립이 AudioCollection에 있어야 합니다.
        audioManager.PlaySfx("UI_SFX_Test");
        Debug.Log($"[Audio] SFX 볼륨 변경: {volume:P0}");
    }

    public void OnBackButtonClicked()
    {
        ServiceLocator.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
    }

    private IEnumerator SetInitialFocus()
    {
        yield return null;
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
}