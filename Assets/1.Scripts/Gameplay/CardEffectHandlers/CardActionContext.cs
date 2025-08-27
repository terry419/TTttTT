using UnityEngine;

// 카드 액션 실행에 필요한 모든 정보를 담는 클래스
public class CardActionContext
{
    public CardDataSO SourceCard { get; }
    public CharacterStats Caster { get; }
    public Transform SpawnPoint { get; }

    public CardActionContext(CardDataSO sourceCard, CharacterStats caster, Transform spawnPoint)
    {
        SourceCard = sourceCard;
        Caster = caster;
        SpawnPoint = spawnPoint;
    }
}