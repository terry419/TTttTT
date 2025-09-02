// Assets/1.Scripts/Data/UIGraphicsDB.cs

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "UIGraphicsDB", menuName = "GameData/UIGraphics Database")]
public class UIGraphicsDB : ScriptableObject
{
    [Header("기본값")]
    public Sprite defaultSprite;
    public Color defaultColor = Color.grey;

    [Header("등급별 색상")]
    public List<RarityColorEntry> rarityColors;

    [Header("속성별 아이콘")]
    public List<AttributeSpriteEntry> attributeSprites;

    [System.Serializable]
    public struct AttributeSpriteEntry
    {
        public CardType attributeType; // CardType을 사용
        public Sprite sprite;
    }

    [System.Serializable]
    public struct RarityColorEntry
    {
        public CardRarity rarity;
        public Color color;
    }

    private Dictionary<CardRarity, Color> rarityColorDict;
    private Dictionary<CardType, Sprite> attributeSpriteDict; // 속성 딕셔너리 추가

    private void OnEnable()
    {
        if (rarityColors != null)
        {
            rarityColorDict = rarityColors.ToDictionary(x => x.rarity, x => x.color);
        }

        if (attributeSprites != null)
        {
            attributeSpriteDict = attributeSprites.ToDictionary(x => x.attributeType, x => x.sprite);
        }
    }

    public Color GetRarityColor(CardRarity rarity)
    {
        if (rarityColorDict == null) OnEnable();

        if (rarityColorDict != null && rarityColorDict.TryGetValue(rarity, out Color color))
        {
            return color;
        }
        return defaultColor;
    }

    public Sprite GetAttributeSprite(CardType type)
    {
        if (attributeSpriteDict == null) OnEnable();

        if (attributeSpriteDict != null && attributeSpriteDict.TryGetValue(type, out Sprite sprite))
        {
            return sprite;
        }
        return defaultSprite;
    }
}