using UnityEngine;

// [수정] NewCardDataSO를 받도록 변경
public class CardActionContext
{
    public NewCardDataSO SourceCard { get; }
    public CharacterStats Caster { get; }
    public Transform SpawnPoint { get; }

    public CardActionContext(NewCardDataSO sourceCard, CharacterStats caster, Transform spawnPoint)
    {
        SourceCard = sourceCard;
        Caster = caster;
        SpawnPoint = spawnPoint;
    }
}