using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 옵션 패널의 UI와 상호작용을 담당합니다.
/// 해상도, 창 모드, 볼륨, 언어, 폰트 크기 등 게임의 각종 설정을 변경하고,
/// 변경된 내용을 AudioManager나 다른 시스템에 적용하며 영구적으로 저장합니다.
/// </summary>
public class OptionsController : MonoBehaviour
{
    [Header("그래픽 설정 참조")]
    [SerializeField] private TMP_Dropdown resolutionDropdown; // 해상도 드롭다운
    [SerializeField] private Toggle fullscreenToggle; // 전체화면 토글

    [Header("오디오 설정 참조")]
    [SerializeField] private Slider bgmVolumeSlider; // BGM 볼륨 슬라이더
    [SerializeField] private Slider sfxVolumeSlider; // SFX 볼륨 슬라이더

    [Header("언어 및 접근성 참조")]
    [SerializeField] private TMP_Dropdown languageDropdown; // 언어 드롭다운
    [SerializeField] private Slider fontSizeSlider; // 폰트 크기 슬라이더
    [SerializeField] private CanvasScaler mainCanvasScaler; // 메인 UI Canvas의 CanvasScaler

    private Resolution[] resolutions; // 사용 가능한 해상도 목록

    void Start()
    {
        // --- 리스너 연결 ---
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        languageDropdown.onValueChanged.AddListener(SetLanguage);
        fontSizeSlider.onValueChanged.AddListener(SetFontSize);

        // --- 초기화 및 설정 로드 ---
        InitializeGraphicsSettings();
        InitializeAudioSettings();
        InitializeAccessibilitySettings();
        LoadSettings(); // 저장된 설정 불러오기
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
        bgmVolumeSlider.value = AudioManager.Instance.GetBgmVolume();
        sfxVolumeSlider.value = AudioManager.Instance.GetSfxVolume();
    }

    private void InitializeAccessibilitySettings()
    {
        // 언어 드롭다운 옵션 설정 (예시)
        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(new List<string> { "한국어", "English" });
        languageDropdown.value = 0; // 기본값 한국어
        languageDropdown.RefreshShownValue();

        // 폰트 크기 슬라이더 초기값 설정 (예시)
        fontSizeSlider.minValue = 0.5f;
        fontSizeSlider.maxValue = 1.5f;
        fontSizeSlider.value = 1.0f; // 기본값
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
        AudioManager.Instance.SetVolume(volume, sfxVolumeSlider.value);
        SaveSettings();
    }

    public void SetSfxVolume(float volume)
    {
        AudioManager.Instance.SetVolume(bgmVolumeSlider.value, volume);
        SaveSettings();
    }

    public void SetLanguage(int languageIndex)
    {
        string languageCode = languageDropdown.options[languageIndex].text; // 선택된 언어 텍스트
        Debug.Log($"언어 변경: {languageCode}");
        // TODO: LocalizationManager.ChangeLanguage(languageCode); // 실제 로컬라이제이션 매니저 호출
        SaveSettings();
    }

    public void SetFontSize(float scale)
    {
        if (mainCanvasScaler != null)
        {
            // CanvasScaler의 dynamicPixelsPerUnit을 조절하여 폰트 크기 변경
            // 기본값 1.0f를 기준으로 스케일 적용
            mainCanvasScaler.dynamicPixelsPerUnit = 1.0f * scale;
            Debug.Log($"폰트 크기 변경: {scale}");
        }
        else
        {
            Debug.LogWarning("mainCanvasScaler가 할당되지 않았습니다. 폰트 크기를 변경할 수 없습니다.");
        }
        SaveSettings();
    }

    /// <summary>
    /// 현재 설정들을 PlayerPrefs에 저장합니다.
    /// </summary>
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

    /// <summary>
    /// PlayerPrefs에서 저장된 설정들을 불러와 적용합니다.
    /// </summary>
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex");
            resolutionDropdown.RefreshShownValue();
            SetResolution(resolutionDropdown.value); // 설정 적용
        }
        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen") == 1;
            SetFullscreen(fullscreenToggle.isOn); // 설정 적용
        }
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume");
            SetBgmVolume(bgmVolumeSlider.value); // 설정 적용
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
            SetSfxVolume(sfxVolumeSlider.value); // 설정 적용
        }
        if (PlayerPrefs.HasKey("LanguageIndex"))
        {
            languageDropdown.value = PlayerPrefs.GetInt("LanguageIndex");
            languageDropdown.RefreshShownValue();
            SetLanguage(languageDropdown.value); // 설정 적용
        }
        if (PlayerPrefs.HasKey("FontSizeScale"))
        {
            fontSizeSlider.value = PlayerPrefs.GetFloat("FontSizeScale");
            SetFontSize(fontSizeSlider.value); // 설정 적용
        }
        Debug.Log("설정을 불러왔습니다.");
    }
}
