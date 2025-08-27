using UnityEngine;

// [����] NewCardDataSO�� �޵��� ����
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