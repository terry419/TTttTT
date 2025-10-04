using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

[CustomEditor(typeof(NewCardDataSO))]
public class CardDataSOEditor : Editor
{
    private SerializedProperty cardIDProp;
    private SerializedProperty cardNameProp;
    private SerializedProperty cardIllustrationProp;
    private SerializedProperty typeProp;
    private SerializedProperty rarityProp;
    private SerializedProperty effectDescriptionProp;

    private void OnEnable()
    {
        // 'basicInfo' 하위의 프로퍼티들을 찾습니다.
        cardIDProp = serializedObject.FindProperty("basicInfo.cardID");
        cardNameProp = serializedObject.FindProperty("basicInfo.cardName");
        cardIllustrationProp = serializedObject.FindProperty("basicInfo.cardIllustration");
        typeProp = serializedObject.FindProperty("basicInfo.type");
        rarityProp = serializedObject.FindProperty("basicInfo.rarity");
        effectDescriptionProp = serializedObject.FindProperty("basicInfo.effectDescription");
    }

    // [추가된 함수 1] 문자열을 CardType 열거형으로 변환
    private CardType ParseCardType(string typeString)
    {
        switch (typeString)
        {
            case "물리":
            case "Physical":
                return CardType.Physical;
            case "마법":
            case "Magical":
                return CardType.Magical;
            default:
                Debug.LogWarning($"알 수 없는 카드 타입: '{typeString}'. 기본값(Physical)으로 설정합니다.");
                return CardType.Physical;
        }
    }

    // [추가된 함수 2] 문자열을 CardRarity 열거형으로 변환
    private CardRarity ParseCardRarity(string rarityString)
    {
        switch (rarityString)
        {
            case "일반":
            case "Common":
                return CardRarity.Common;
            case "희귀":
            case "Rare":
                return CardRarity.Rare;
            case "영웅":
            case "Epic":
                return CardRarity.Epic;
            case "전설":
            case "Legendary":
                return CardRarity.Legendary;
            case "신화":
            case "Mythic":
                return CardRarity.Mythic;
            default:
                Debug.LogWarning($"알 수 없는 카드 등급: '{rarityString}'. 기본값(Common)으로 설정합니다.");
                return CardRarity.Common;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // basicInfo 필드를 수동으로 그립니다.
        SerializedProperty basicInfoProperty = serializedObject.FindProperty("basicInfo");
        if (basicInfoProperty != null)
        {
            EditorGUILayout.PropertyField(basicInfoProperty.FindPropertyRelative("cardID"), new GUIContent("Card ID"));

            // cardName (LocalizedString) 필드를 m_StringReference를 통해 그립니다.
            SerializedProperty cardNameProp = basicInfoProperty.FindPropertyRelative("cardName");
            EditorGUILayout.PropertyField(cardNameProp.FindPropertyRelative("m_StringReference"), new GUIContent("Card Name"));

            EditorGUILayout.PropertyField(basicInfoProperty.FindPropertyRelative("cardIllustration"), new GUIContent("Card Illustration"));

            // type (CardType enum) 필드를 EnumPopup으로 그립니다.
            SerializedProperty typeProp = basicInfoProperty.FindPropertyRelative("type");
            typeProp.enumValueIndex = (int)(CardType)EditorGUILayout.EnumPopup(new GUIContent("Card Type"), (CardType)typeProp.enumValueIndex);

            // rarity (CardRarity enum) 필드를 EnumPopup으로 그립니다.
            SerializedProperty rarityProp = basicInfoProperty.FindPropertyRelative("rarity");
            rarityProp.enumValueIndex = (int)(CardRarity)EditorGUILayout.EnumPopup(new GUIContent("Card Rarity"), (CardRarity)rarityProp.enumValueIndex);

            // effectDescription (LocalizedString) 필드를 m_StringReference를 통해 그립니다.
            SerializedProperty effectDescriptionProp = basicInfoProperty.FindPropertyRelative("effectDescription");
            EditorGUILayout.PropertyField(effectDescriptionProp.FindPropertyRelative("m_StringReference"), new GUIContent("Effect Description"));
        }

        EditorGUILayout.Space(20); // 버튼과 구분을 위해 공간을 추가

        if (GUILayout.Button("Load All Card Data from JSONs in Folder"))
        {
            string dirPath = EditorUtility.OpenFolderPanel("Select Folder with Card JSONs", "", "");
            if (!string.IsNullOrEmpty(dirPath))
            {
                var jsonFiles = Directory.GetFiles(dirPath, "*.json").ToList();
                foreach (var filePath in jsonFiles)
                {
                    string json = File.ReadAllText(filePath);
                    CardDataJson jsonData = JsonUtility.FromJson<CardDataJson>(json);

                    string soPath = $"Assets/Resources_moved/CardData/{jsonData.cardID}.asset";
                    NewCardDataSO so = AssetDatabase.LoadAssetAtPath<NewCardDataSO>(soPath);
                    if (so == null)
                    {
                        so = ScriptableObject.CreateInstance<NewCardDataSO>();
                        AssetDatabase.CreateAsset(so, soPath);
                    }

                    SerializedObject soToUpdate = new SerializedObject(so);
                    soToUpdate.Update();

                    soToUpdate.FindProperty("basicInfo.cardID").stringValue = jsonData.cardID;
                    soToUpdate.FindProperty("basicInfo.cardName").stringValue = jsonData.cardName;
                    soToUpdate.FindProperty("basicInfo.effectDescription").stringValue = jsonData.effectDescription;

                    // [핵심 수정] 변환 함수를 사용하여 올바른 Enum 값을 할당합니다.
                    soToUpdate.FindProperty("basicInfo.type").enumValueIndex = (int)ParseCardType(jsonData.type);
                    soToUpdate.FindProperty("basicInfo.rarity").enumValueIndex = (int)ParseCardRarity(jsonData.rarity);

                    soToUpdate.ApplyModifiedProperties();
                    EditorUtility.SetDirty(so); // 변경사항을 저장하도록 표시
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Finished loading data from {jsonFiles.Count} JSON files.");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}