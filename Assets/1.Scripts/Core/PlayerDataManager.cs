// 파일 경로: Assets/1.Scripts/Core/PlayerDataManager.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임의 한 세션(런) 동안 유지되는 플레이어의 모든 데이터를 관리합니다.
/// 이 데이터는 씬 전환 시에도 파괴되지 않습니다. (예: 보유 카드, 스탯 등)
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    public static event System.Action<float, float> OnHealthChanged;

    // --- 데이터 ---
    // [1단계] 체력 및 스탯 데이터
    public BaseStats BaseStats { get; set; }

    private float _currentHealth;
    public float CurrentHealth { get; set; }
    // [2단계] 카드 데이터
    public List<CardInstance> OwnedCards { get; private set; } = new List<CardInstance>();
    public List<CardInstance> EquippedCards { get; private set; } = new List<CardInstance>();

    // [3단계] 유물 데이터
    public List<ArtifactDataSO> OwnedArtifacts { get; private set; } = new List<ArtifactDataSO>();
    public bool IsRunInitialized { get; private set; }


    private void Awake()
    {
        if (!ServiceLocator.IsRegistered<PlayerDataManager>())
        {
            ServiceLocator.Register<PlayerDataManager>(this);
            DontDestroyOnLoad(this.gameObject);
            Initialize(); // 초기화 함수 호출
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // 데이터 초기화 함수
    private void Initialize()
    {
        Debug.Log("[PlayerDataManager] 초기화 완료 및 데이터 영속성 활성화.");
    }

    /// <summary>
    /// 새로운 런(Run)을 시작할 때 모든 데이터를 초기 상태로 리셋합니다.
    /// </summary>
    public void ResetRunData(CharacterDataSO characterData)
    {
        // 1. 스탯 및 체력 초기화
        BaseStats = characterData.baseStats; // 선택한 캐릭터의 기본 스탯으로 설정
        CurrentHealth = BaseStats.baseHealth; // 체력은 최대로

        // 2. 카드 및 유물 초기화
        OwnedCards.Clear();
        EquippedCards.Clear();
        OwnedArtifacts.Clear();

        Debug.Log($"[PlayerDataManager] '{characterData.characterName}'으로 새 런 데이터 초기화 완료.");

        IsRunInitialized = true;
        Debug.Log($"[PlayerDataManager] IsRunInitialized 플래그를 true로 설정했습니다.");

    }
    public void CompleteRunInitialization()
    {
        IsRunInitialized = false;
        Debug.Log($"[PlayerDataManager] IsRunInitialized 플래그를 false로 설정했습니다.");
    }
    public void UpdateHealth(float newHealth)
    {
        // 체력 값을 갱신하고
        CurrentHealth = newHealth;

        // FinalHealth를 계산할 수 있는 CharacterStats를 찾아 이벤트를 발생시킵니다.
        // 이 방식은 임시적이며, 다음 단계에서 더 개선될 수 있습니다.
        var player = ServiceLocator.Get<PlayerController>();
        if (player != null)
        {
            var playerStats = player.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                OnHealthChanged?.Invoke(CurrentHealth, playerStats.FinalHealth);
            }
        }
    }

}