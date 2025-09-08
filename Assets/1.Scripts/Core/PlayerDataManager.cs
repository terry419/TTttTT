using UnityEngine;
using System.Collections.Generic;

// 이 클래스들은 프로젝트에 이미 존재하거나 만들어져야 합니다.
// public class CardInstance { /* ... */ }
// public class CharacterStats { /* ... */ }

/// <summary>
/// 게임의 한 세션(런) 동안 유지되는 플레이어의 모든 데이터를 관리합니다.
/// 이 데이터는 씬 전환 시에도 파괴되지 않습니다. (예: 보유 카드, 스탯 등)
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    // --- 데이터 ---
    // 다른 곳에서 함부로 리스트를 바꾸지 못하도록 public { get; private set; }으로 설정
    public List<CardInstance> OwnedCards { get; private set; }
    public CharacterStats PlayerStats { get; private set; }

    private void Awake()
    {
        // GameManager, DataManager와 완벽히 동일한 싱글톤 및 영속성 처리
        if (!ServiceLocator.IsRegistered<PlayerDataManager>())
        {
            ServiceLocator.Register<PlayerDataManager>(this);
            DontDestroyOnLoad(this.gameObject);
            Initialize();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // 데이터 초기화 함수
    private void Initialize()
    {
        OwnedCards = new List<CardInstance>();
        // PlayerStats = new CharacterStats(); // 캐릭터 기본 스탯으로 초기화하는 로직 필요
        Debug.Log("[PlayerDataManager] 초기화 완료 및 데이터 영속성 활성화.");
    }

    // --- 데이터 조작을 위한 공용 함수 (API) ---

    public void AddCard(CardInstance newCard)
    {
        if (newCard != null)
        {
            OwnedCards.Add(newCard);
        }
    }

    public void ResetRunData()
    {
        OwnedCards.Clear();
        // PlayerStats = new CharacterStats();
        Debug.Log("[PlayerDataManager] 플레이어의 런(Run) 데이터가 초기화되었습니다.");
    }

    // 여기에 스탯 변경, 아티팩트 추가 등 다양한 데이터 관리 함수를 추가할 수 있습니다.
}
