using UnityEngine;

/// <summary>
/// 캐릭터 선택 씬의 전체적인 흐름과 상태를 관리하는 컨트롤러입니다.
/// 각 UI 패널(캐릭터 선택, 포인트 분배, 결과)을 제어하고,
/// 선택된 캐릭터 데이터와 분배된 포인트를 다음 씬으로 전달할 준비를 합니다.
/// </summary>
public class CharacterSelectController : MonoBehaviour
{
    // 현재 선택된 캐릭터의 데이터를 저장할 변수입니다.
    // 이 데이터는 DataManager로부터 받아옵니다.
    public CharacterDataSO SelectedCharacter { get; private set; }

    // 플레이어가 분배한 포인트를 저장할 변수입니다.
    public int AllocatedPoints { get; private set; }

    void Start()
    {
        // 씬이 시작되면, CharacterSelectUI 스크립트가 붙어있는 패널이 활성화됩니다。
        // 이 씬에는 CharacterSelectPanel만 존재하므로 별도의 SetActive(true) 호출은 필요하지 않습니다。
    }

    /// <summary>
    /// 사용자가 캐릭터를 선택했을 때 CharacterSelectUI에서 호출됩니다。
    /// </summary>
    /// <param name="characterData">선택된 캐릭터의 ScriptableObject 데이터</param>
    public void OnCharacterSelected(CharacterDataSO characterData)
    {
        SelectedCharacter = characterData;
        Debug.Log($"[CharSelectController] 캐릭터 선택됨: {SelectedCharacter.characterName}");
        // 이 단계에서는 아직 UI를 변경하지 않습니다。
        // '게임 시작' 버튼을 눌렀을 때 다음 단계로 진행합니다。
    }

    /// <summary>
    /// '게임 시작' 버튼 클릭 시, 포인트 분배 씬으로 전환합니다。
    /// CharacterSelectUI에서 호출됩니다。
    /// </summary>
    public void ProceedToPointAllocation()
    {
        if (SelectedCharacter == null)
        {
            Debug.LogWarning("[CharSelectController] 캐릭터가 선택되지 않았습니다. 포인트 분배를 진행할 수 없습니다.");
            // TODO: 사용자에게 "캐릭터를 먼저 선택하세요"와 같은 알림을 띄우는 로직 추가
            return;
        }

        // GameManager에 선택된 캐릭터 정보를 저장하고 포인트 분배 씬으로 전환합니다。
        GameManager.Instance.SelectedCharacter = this.SelectedCharacter;
        GameManager.Instance.ChangeState(GameManager.GameState.Allocation); // Allocation 상태로 변경하여 PointAllocation 씬 로드
    }
}