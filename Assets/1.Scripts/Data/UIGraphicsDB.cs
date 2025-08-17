// UIGraphicsDB.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "UIGraphicsDB", menuName = "GameData/UIGraphics Database")]
public class UIGraphicsDB : ScriptableObject
{
    // --- 싱글톤(Singleton) 설정 ---
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

    // --- 데이터 구조 ---
    [System.Serializable]
    public struct RaritySpriteEntry
    {
        public CardRarity rarity;
        public Sprite sprite;
    }

    public List<RaritySpriteEntry> raritySprites;

    // --- 내부 데이터 및 함수 ---
    private Dictionary<CardRarity, Sprite> raritySpriteDict;

    private void OnEnable()
    {
        // 리스트를 딕셔너리로 변환하여 검색 속도를 높입니다.
        raritySpriteDict = raritySprites.ToDictionary(x => x.rarity, x => x.sprite);
    }

    public Sprite GetRaritySprite(CardRarity rarity)
    {
        if (raritySpriteDict != null && raritySpriteDict.TryGetValue(rarity, out Sprite sprite))
        {
            return sprite;
        }
        return null; // 해당하는 스프라이트가 없을 경우
    }
}