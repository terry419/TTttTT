using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsUIController : MonoBehaviour
{
    [Header("UI ���")]
    [Tooltip("���� ���۵� �� ���� ���� ���õ� ��ư")]
    [SerializeField] private GameObject firstSelectedButton;
    [Tooltip("���� �޴��� ���ư� ��ư")]
    [SerializeField] private Button backButton;

    [Header("����� ����")]
    [Tooltip("BGM ���� ������ InteractiveSlider")]
    [SerializeField] private InteractiveSlider bgmVolumeSlider;
    [Tooltip("SFX ���� ������ InteractiveSlider")]
    [SerializeField] private InteractiveSlider sfxVolumeSlider;

    private AudioManager audioManager;

    void Start()
    {
        // ServiceLocator�� ���� AudioManager �ν��Ͻ��� �����ɴϴ�.
        audioManager = ServiceLocator.Get<AudioManager>();

        // �� ��ư�� �����̴��� �̺�Ʈ �����ʸ� �����մϴ�.
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

        // �ʱ� ��Ŀ���� �����մϴ�.
        StartCoroutine(SetInitialFocus());

        // AudioManager�� �ִٸ�, ���� ���� ������ �����̴��� �ʱ�ȭ�մϴ�.
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
        Debug.Log($"[Audio] BGM ���� ����: {volume:P0}"); // P0 = �����(�Ҽ��� ����)
    }

    private void SetSfxVolume(float volume)
    {
        if (audioManager == null) return;
        audioManager.SetVolume(bgmVolumeSlider.value, volume);

        // (���� ����) ���� ���� �� ȿ������ ����Ͽ� ��� �ǵ���� �ݴϴ�.
        // �� ����� ����Ϸ��� "UI_SFX_Test" ��� �̸��� ����� Ŭ���� AudioCollection�� �־�� �մϴ�.
        audioManager.PlaySfx("UI_SFX_Test");
        Debug.Log($"[Audio] SFX ���� ����: {volume:P0}");
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