using UnityEngine;

public class CardActionContext
{
    public CardInstance CardInstance { get; } // SO ��� �ν��Ͻ� ��ü�� ����
    public CharacterStats Caster { get; }
    public Transform SpawnPoint { get; }

    public CardActionContext(CardInstance cardInstance, CharacterStats caster, Transform spawnPoint)
    {
        CardInstance = cardInstance;
        Caster = caster;
        SpawnPoint = spawnPoint;
    }
}