using UnityEngine;

/// <summary>
/// BGM, SFX 등 오디오 클립들을 그룹으로 묶어 관리하는 ScriptableObject.
/// 각 씬이나 상황에 맞는 오디오 세트를 구성하는 데 사용됩니다.
/// </summary>
[CreateAssetMenu(fileName = "NewAudioCollection", menuName = "9th/Audio/Audio Collection")]
public class AudioCollection : ScriptableObject
{
    [Header("배경음악 (BGM)")]
    public AudioClip[] bgmClips;

    [Header("효과음 (SFX)")]
    public AudioClip[] sfxClips;

    /// <summary>
    /// 이름으로 BGM 오디오 클립을 찾습니다.
    /// </summary>
    public AudioClip GetBgmClip(string clipName)
    {
        foreach (var clip in bgmClips)
        {
            if (clip.name == clipName)
            {
                return clip;
            }
        }
        Debug.LogWarning($"'{name}' 컬렉션에 '{clipName}' BGM이 없습니다.");
        return null;
    }

    /// <summary>
    /// 이름으로 SFX 오디오 클립을 찾습니다.
    /// </summary>
    public AudioClip GetSfxClip(string clipName)
    {
        foreach (var clip in sfxClips)
        {
            if (clip.name == clipName)
            {
                return clip;
            }
        }
        Debug.LogWarning($"'{name}' 컬렉션에 '{clipName}' SFX가 없습니다.");
        return null;
    }
}
