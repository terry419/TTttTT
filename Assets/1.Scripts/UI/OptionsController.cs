using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class OptionsController : MonoBehaviour
{
    [Header("그래픽 설정 참조")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("오디오 설정 참조")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("언어 및 접근성 참조")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Slider fontSizeSlider;
    [SerializeField] private CanvasScaler mainCanvasScaler;

    private Resolution[] resolutions;

    // --- [추가] AudioManager 인스턴스를 저장할 변수 ---
    private AudioManager audioManager;

    void Start()
    {
        // --- [추가] ServiceLocator를 통해 AudioManager를 찾아옵니다. ---
        audioManager = ServiceLocator.Get<AudioManager>();

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        languageDropdown.onValueChanged.AddListener(SetLanguage);
        fontSizeSlider.onValueChanged.AddListener(SetFontSize);

        InitializeGraphicsSettings();
        InitializeAudioSettings();
        InitializeAccessibilitySettings();
        LoadSettings();
    }

    private void InitializeGraphicsSettings()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;
    }

    private void InitializeAudioSettings()
    {
        // --- [수정] 찾아온 audioManager 인스턴스를 사용합니다. ---
        if (audioManager != null)
        {
            bgmVolumeSlider.value = audioManager.GetBgmVolume();
            sfxVolumeSlider.value = audioManager.GetSfxVolume();
        }
    }

    private void InitializeAccessibilitySettings()
    {
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string> { "한국어", "English" });
        languageDropdown.value = 0;
        languageDropdown.RefreshShownValue();

        fontSizeSlider.minValue = 0.5f;
        fontSizeSlider.maxValue = 1.5f;
        fontSizeSlider.value = 1.0f;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log($"해상도 변경: {resolution.width}x{resolution.height}");
        SaveSettings();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"전체화면 모드: {isFullscreen}");
        SaveSettings();
    }

    public void SetBgmVolume(float volume)
    {
        // --- [수정] 찾아온 audioManager 인스턴스를 사용합니다. ---
        if (audioManager != null)
        {
            audioManager.SetVolume(volume, sfxVolumeSlider.value);
        }
        SaveSettings();
    }

    public void SetSfxVolume(float volume)
    {
        // --- [수정] 찾아온 audioManager 인스턴스를 사용합니다. ---
        if (audioManager != null)
        {
            audioManager.SetVolume(bgmVolumeSlider.value, volume);
        }
        SaveSettings();
    }

    public void SetLanguage(int languageIndex)
    {
        string languageCode = languageDropdown.options[languageIndex].text;
        Debug.Log($"언어 변경: {languageCode}");
        SaveSettings();
    }

    public void SetFontSize(float scale)
    {
        if (mainCanvasScaler != null)
        {
            mainCanvasScaler.dynamicPixelsPerUnit = 1.0f * scale;
            Debug.Log($"폰트 크기 변경: {scale}");
        }
        else
        {
            Debug.LogWarning("mainCanvasScaler가 할당되지 않았습니다. 폰트 크기를 변경할 수 없습니다.");
        }
        SaveSettings();
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("BGMVolume", bgmVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        PlayerPrefs.SetInt("LanguageIndex", languageDropdown.value);
        PlayerPrefs.SetFloat("FontSizeScale", fontSizeSlider.value);
        PlayerPrefs.Save();
        Debug.Log("설정이 저장되었습니다.");
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex");
            resolutionDropdown.RefreshShownValue();
            SetResolution(resolutionDropdown.value);
        }
        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen") == 1;
            SetFullscreen(fullscreenToggle.isOn);
        }
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume");
            SetBgmVolume(bgmVolumeSlider.value);
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
            SetSfxVolume(sfxVolumeSlider.value);
        }
        if (PlayerPrefs.HasKey("LanguageIndex"))
        {
            languageDropdown.value = PlayerPrefs.GetInt("LanguageIndex");
            languageDropdown.RefreshShownValue();
            SetLanguage(languageDropdown.value);
        }
        if (PlayerPrefs.HasKey("FontSizeScale"))
        {
            fontSizeSlider.value = PlayerPrefs.GetFloat("FontSizeScale");
            SetFontSize(fontSizeSlider.value);
        }
        Debug.Log("설정을 불러왔습니다.");
    }
}