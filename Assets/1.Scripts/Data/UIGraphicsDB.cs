// UIGraphicsDB.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "UIGraphicsDB", menuName = "GameData/UIGraphics Database")]
public class UIGraphicsDB : ScriptableObject
{
    [Tooltip("요청한 등급의 스프라이트를 찾지 못했을 때 반환할 기본 이미지입니다.")]
    public Sprite defaultSprite;


    [System.Serializable]
    public struct RaritySpriteEntry
    {
        public CardRarity rarity;
        public Sprite sprite;
    }

    public List<RaritySpriteEntry> raritySprites;
    private Dictionary<CardRarity, Sprite> raritySpriteDict;

    private void OnEnable()
    {
        raritySpriteDict = raritySprites.ToDictionary(x => x.rarity, x => x.sprite);
    }
    public Sprite GetRaritySprite(CardRarity rarity)
    {
        // 딕셔너리가 준비되었고, 요청한 키가 존재할 경우에만 정상 반환
        if (raritySpriteDict != null && raritySpriteDict.TryGetValue(rarity, out Sprite sprite))
        {
            return sprite;
        }

        // 그 외의 모든 실패 상황에서는 기본 스프라이트를 반환하여 오류 방지
        return defaultSprite;
    }
}