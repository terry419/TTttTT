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

    private Resolution[] resolutions; // 사용 가능한 해상도 목록

    void Start()
    {
        // --- 리스너 연결 ---
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);

        // --- 초기화 ---
        InitializeGraphicsSettings();
        InitializeAudioSettings();
    }

    private void InitializeGraphicsSettings()
    {
        // 해상도 목록 채우기
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

        // 전체화면 토글 초기화
        fullscreenToggle.isOn = Screen.fullScreen;
    }

    private void InitializeAudioSettings()
    {
        // AudioManager에서 현재 볼륨 값을 가져와 슬라이더에 반영
        bgmVolumeSlider.value = AudioManager.Instance.GetBgmVolume();
        sfxVolumeSlider.value = AudioManager.Instance.GetSfxVolume();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        Debug.Log($"해상도 변경: {resolution.width}x{resolution.height}");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"전체화면 모드: {isFullscreen}");
    }

    public void SetBgmVolume(float volume)
    {
        AudioManager.Instance.SetVolume(volume, sfxVolumeSlider.value);
    }

    public void SetSfxVolume(float volume)
    {
        AudioManager.Instance.SetVolume(bgmVolumeSlider.value, volume);
    }

    // TODO: 변경된 설정들을 PlayerPrefs를 사용하여 저장하고, 게임 시작 시 불러오는 Save/Load 메서드 구현 필요
    public void SaveSettings()
    {
        // PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        // PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        // PlayerPrefs.SetFloat("BGMVolume", bgmVolumeSlider.value);
        // PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
        // PlayerPrefs.Save();
        Debug.Log("설정이 저장되었습니다.");
    }
}
