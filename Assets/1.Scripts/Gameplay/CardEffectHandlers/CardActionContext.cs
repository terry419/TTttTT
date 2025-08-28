using UnityEngine;

public class CardActionContext
{
    public CardInstance CardInstance { get; } // SO 대신 인스턴스 자체를 전달
    public CharacterStats Caster { get; }
    public Transform SpawnPoint { get; }

    public CardActionContext(CardInstance cardInstance, CharacterStats caster, Transform spawnPoint)
    {
        CardInstance = cardInstance;
        Caster = caster;
        SpawnPoint = spawnPoint;
    }
}