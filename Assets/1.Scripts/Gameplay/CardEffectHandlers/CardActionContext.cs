using UnityEngine;

public class CardActionContext
{
    public CardInstance CardInstance { get; }
    public EntityStats Caster { get; } // CharacterStats -> EntityStats
    public Transform SpawnPoint { get; }

    public CardActionContext(CardInstance cardInstance, EntityStats caster, Transform spawnPoint) // CharacterStats -> EntityStats
    {
        CardInstance = cardInstance;
        Caster = caster;
        SpawnPoint = spawnPoint;
    }
}
