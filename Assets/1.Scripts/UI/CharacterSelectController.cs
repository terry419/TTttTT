using UnityEngine;

/// <summary>
/// 캐릭터 선택 씬의 전체적인 흐름과 상태를 관리하는 컨트롤러입니다.
/// 각 UI 패널(캐릭터 선택, 포인트 분배, 결과)을 제어하고,
/// 선택된 캐릭터 데이터와 분배된 포인트를 다음 씬으로 전달할 준비를 합니다.
/// </summary>
public class CharacterSelectController : MonoBehaviour
{
    public CharacterDataSO SelectedCharacter { get; private set; }
    public int AllocatedPoints { get; private set; }

    void Start()
    {
        // 씬이 시작되면, CharacterSelectUI 스크립트가 붙어있는 패널이 활성화됩니다.
    }

    public void OnCharacterSelected(CharacterDataSO characterData)
    {
        SelectedCharacter = characterData;
        Debug.Log($"[CharSelectController] 캐릭터 선택됨: {SelectedCharacter.characterName}");
    }

    public void ProceedToPointAllocation()
    {
        if (SelectedCharacter == null)
        {
            Debug.LogWarning("[CharSelectController] 캐릭터가 선택되지 않았습니다. 포인트 분배를 진행할 수 없습니다.");
            return;
        }

        GameManager.Instance.SelectedCharacter = this.SelectedCharacter;
        // GameManager의 PointAllocation 상태로 전환하도록 수정
        GameManager.Instance.ChangeState(GameManager.GameState.PointAllocation);
    }
}