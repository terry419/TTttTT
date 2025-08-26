// UIGraphicsDB.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "UIGraphicsDB", menuName = "GameData/UIGraphics Database")]
public class UIGraphicsDB : ScriptableObject
{
    // --- ̱(Singleton)  ---
    private static UIGraphicsDB _instance;
    public static UIGraphicsDB Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<UIGraphicsDB>("UIGraphicsDB");
                if (_instance == null)
                    Debug.LogError("UIGraphicsDB could not be loaded from Resources!");
            }
            return _instance;
        }
    }

    // ---   ---
    [System.Serializable]
    public struct RaritySpriteEntry
    {
        public CardRarity rarity;
        public Sprite sprite;
    }

    public List<RaritySpriteEntry> raritySprites;

    // ---    Լ ---
    private Dictionary<CardRarity, Sprite> raritySpriteDict;

    private void OnEnable()
    {
        // Ʈ ųʸ ȯϿ ˻ ӵ Դϴ.
        raritySpriteDict = raritySprites.ToDictionary(x => x.rarity, x => x.sprite);
    }

    public Sprite GetRaritySprite(CardRarity rarity)
    {
        if (raritySpriteDict != null && raritySpriteDict.TryGetValue(rarity, out Sprite sprite))
        {
            return sprite;
        }
        return null; // شϴ Ʈ  
    }
}