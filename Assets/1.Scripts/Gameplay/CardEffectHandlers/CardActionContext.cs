using UnityEngine;

// ī�� �׼� ���࿡ �ʿ��� ��� ������ ��� Ŭ����
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