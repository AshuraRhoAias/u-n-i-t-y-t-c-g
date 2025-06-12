// Assets/WaifuSummoner/Scripts/Developer Data Manager/Editor/Tabs/CollectionTab.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class CollectionTab
{
    // — UI state —
    private Vector2 scrollDecks, scrollCards, scrollDetails;
    private ReorderableList reorderableCardsList;

    // — Selection state —
    private string selectedDeck;
    private CollectionData selectedData;
    private List<string> cardPathList = new List<string>();
    private string selectedCardPath;
    private WaifuData selectedCard;

    // — Preview offsets (solo para waifus) —
    private readonly Vector2 lvlOffset = new Vector2(0, -4);
    private readonly Vector2 atkNumOff = new Vector2(-8, 36);
    private readonly Vector2 atkLabOff = new Vector2(-8, 38);
    private readonly Vector2 ambNumOff = new Vector2(50, 36);
    private readonly Vector2 ambLabOff = new Vector2(50, 38);

    // — Styling & drawer map —
    private Font carlitoFont;
    private GUIStyle style;
    private Dictionary<EffectType, (IEffectDrawer Drawer, string PropertyName)> _drawerMap;

    // — Deck list on disk —
    private string[] deckPaths = new string[0];

    public void Initialize()
    {
        RefreshDeckList();
        carlitoFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/WaifuSummoner/Fonts/Carlito-Bold.ttf");
        if (carlitoFont == null) Debug.LogError("No se encontró Assets/WaifuSummoner/Fonts/Carlito-Bold.ttf");
        style = new GUIStyle { font = carlitoFont, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        BuildDrawerMap();
    }

    void BuildDrawerMap()
    {
        _drawerMap = new Dictionary<EffectType, (IEffectDrawer, string)>();
        var asm = Assembly.GetAssembly(typeof(IEffectDrawer));
        foreach (var t in asm.GetTypes()
            .Where(x => typeof(IEffectDrawer).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
        {
            var attr = t.GetCustomAttribute<EffectDrawerAttribute>();
            if (attr != null)
                _drawerMap[attr.EffectType] = ((IEffectDrawer)System.Activator.CreateInstance(t), attr.PropertyName);
        }
    }

    public void DrawTab()
    {
        EditorGUILayout.BeginHorizontal();
        DrawDeckColumn();
        DrawCardColumn();
        DrawDetailsColumn();
        DrawPreviewColumn();
        EditorGUILayout.EndHorizontal();
    }

    void DrawDeckColumn()
    {
        GUILayout.BeginVertical(GUILayout.Width(200));
        GUILayout.Label("Decks / Sets", EditorStyles.boldLabel);
        if (GUILayout.Button("+ Add Set")) CreateNewDeck();
        scrollDecks = EditorGUILayout.BeginScrollView(scrollDecks);
        foreach (var path in deckPaths)
            if (GUILayout.Button(Path.GetFileName(path)))
                SelectDeck(path);
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void SelectDeck(string path)
    {
        selectedDeck = path;
        RefreshCardList();
        var dataPath = $"{selectedDeck}/CollectionData.asset";
        selectedData = AssetDatabase.LoadAssetAtPath<CollectionData>(dataPath);
        if (selectedData == null)
        {
            selectedData = ScriptableObject.CreateInstance<CollectionData>();
            selectedData.displayName = Path.GetFileName(selectedDeck);
            selectedData.identifier = selectedData.displayName.Length >= 3
                ? selectedData.displayName.Substring(0, 3).ToUpper()
                : selectedData.displayName.ToUpper();
            AssetDatabase.CreateAsset(selectedData, dataPath);
            AssetDatabase.SaveAssets();
        }
    }

    void DrawCardColumn()
    {
        GUILayout.BeginVertical(GUILayout.Width(250));
        if (selectedData != null)
        {
            GUILayout.Label($"Cards in {selectedData.displayName}", EditorStyles.boldLabel);
            if (GUILayout.Button("+ Add Card")) CreateNewCard();
            if (reorderableCardsList == null) SetupReorderableList();
            scrollCards = EditorGUILayout.BeginScrollView(scrollCards);
            reorderableCardsList.DoLayoutList();
            EditorGUILayout.EndScrollView();

            GUILayout.Space(8);
            EditorGUILayout.LabelField("Deck Info", EditorStyles.boldLabel);
            selectedData.format = (FormatType)EditorGUILayout.EnumPopup("Format", selectedData.format);

            EditorGUI.BeginChangeCheck();
            selectedData.displayName = EditorGUILayout.DelayedTextField("Name", selectedData.displayName);
            if (EditorGUI.EndChangeCheck())
            {
                var parent = Path.GetDirectoryName(selectedDeck);
                AssetDatabase.RenameAsset(selectedDeck, selectedData.displayName);
                AssetDatabase.SaveAssets();
                AssetDatabase.MoveAsset(
                    $"{parent}/CollectionData.asset",
                    $"{parent}/{selectedData.displayName}/CollectionData.asset"
                );
                AssetDatabase.SaveAssets();
                selectedDeck = $"{parent}/{selectedData.displayName}".Replace("\\", "/");
                RefreshDeckList();
                RefreshCardList();
            }

            EditorGUI.BeginChangeCheck();
            selectedData.identifier = EditorGUILayout.DelayedTextField("ID", selectedData.identifier);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(selectedData);
                AssetDatabase.SaveAssets();
                RenameAllCardsInDeck();
                RefreshCardList();
            }

            if (GUILayout.Button("Delete Card Group") &&
                EditorUtility.DisplayDialog("Confirm Delete", $"Delete '{selectedData.displayName}'?", "Yes", "No"))
            {
                AssetDatabase.DeleteAsset(selectedDeck);
                AssetDatabase.SaveAssets();
                selectedDeck = null;
                selectedData = null;
                RefreshDeckList();
                RefreshCardList();
            }
        }
        GUILayout.EndVertical();
    }

    void SetupReorderableList()
    {
        cardPathList = Directory.Exists(selectedDeck)
            ? Directory.GetFiles(selectedDeck, "*.asset").Where(f => !f.EndsWith("CollectionData.asset")).ToList()
            : new List<string>();

        reorderableCardsList = new ReorderableList(cardPathList, typeof(string), true, false, false, false);
        reorderableCardsList.drawHeaderCallback = _ => { };
        reorderableCardsList.drawElementCallback = (rect, i, _, _) =>
        {
            var path = cardPathList[i];
            var name = Path.GetFileNameWithoutExtension(path);
            if (GUI.Button(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), name))
            {
                selectedCardPath = path;
                LoadCard();
            }
        };
        reorderableCardsList.onReorderCallback = _ =>
        {
            RenameAllCardsInDeck();
            RefreshCardList();
        };
    }

    void RefreshCardList()
    {
        cardPathList = !string.IsNullOrEmpty(selectedDeck)
            ? Directory.GetFiles(selectedDeck, "*.asset").Where(f => !f.EndsWith("CollectionData.asset")).ToList()
            : new List<string>();
        reorderableCardsList = null;
    }

    void RenameAllCardsInDeck()
    {
        for (int i = 0; i < cardPathList.Count; i++)
        {
            var oldPath = cardPathList[i];
            var card = AssetDatabase.LoadAssetAtPath<WaifuData>(oldPath);
            if (card == null) continue;
            var idx = (i + 1).ToString("000");
            var safeName = card.waifuName.Replace('/', '_').Replace('\\', '_');
            var newName = $"{selectedData.identifier}-{idx} {safeName}.asset";
            if (Path.GetFileName(oldPath) != newName)
                AssetDatabase.RenameAsset(oldPath, newName);
        }
        AssetDatabase.SaveAssets();
    }

    void DrawDetailsColumn()
    {
        GUILayout.BeginVertical(GUILayout.Width(350));
        GUILayout.Label("Card Details", EditorStyles.boldLabel);
        scrollDetails = EditorGUILayout.BeginScrollView(scrollDetails);

        if (selectedCard != null)
        {
            var so = new SerializedObject(selectedCard);
            so.Update();

            // — Card Type sin 'Any', default Waifu —
            var cardTypeProp = so.FindProperty("cardType");
            var current = (CardType)cardTypeProp.enumValueIndex;
            var entries = System.Enum.GetValues(typeof(CardType))
                             .Cast<CardType>()
                             .Where(ct => ct != CardType.Any)
                             .ToArray();
            int sel = System.Array.IndexOf(entries, current);
            if (sel < 0) sel = System.Array.IndexOf(entries, CardType.Waifu);
            sel = EditorGUILayout.Popup("Card Type", sel, entries.Select(e => e.ToString()).ToArray());
            cardTypeProp.enumValueIndex = (int)entries[sel];

            // — Si es WAIFU, dibujar todos estos campos —
            if ((CardType)cardTypeProp.enumValueIndex == CardType.Waifu)
            {
                EditorGUILayout.PropertyField(so.FindProperty("rarity"), new GUIContent("Rarity"));
                EditorGUILayout.PropertyField(so.FindProperty("waifuName"), new GUIContent("Waifu Name"));
                EditorGUILayout.PropertyField(so.FindProperty("reign"), new GUIContent("Reign"));
                EditorGUILayout.PropertyField(so.FindProperty("role"));
                EditorGUILayout.PropertyField(so.FindProperty("element"));
                EditorGUILayout.PropertyField(so.FindProperty("level"));
                EditorGUILayout.PropertyField(so.FindProperty("summonType"), new GUIContent("Summon Type"));
                DrawStatField(so, "attack", "Attack");
                DrawStatField(so, "ambush", "Ambush");
            }
            // — Si es otro tipo (Enchantment, Mood, Accessory), solo el nombre y rarity_nombre —
            else
            {
                EditorGUILayout.PropertyField(so.FindProperty("rarity"), new GUIContent("Rarity"));
                EditorGUILayout.PropertyField(so.FindProperty("waifuName"), new GUIContent("Name"));
                // nada más
            }

            // — Efectos (todos los tipos) —
            EditorGUILayout.LabelField("Effect Groups", EditorStyles.boldLabel);
            var groupsProp = so.FindProperty("effectGroups");
            int removeGroup = -1;
            for (int gi = 0; gi < groupsProp.arraySize; gi++)
            {
                var groupProp = groupsProp.GetArrayElementAtIndex(gi);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Effect Group {gi + 1}", EditorStyles.miniBoldLabel);

                // Triggers
                var trigProp = groupProp.FindPropertyRelative("triggers");
                int remT = -1;
                for (int ti = 0; ti < trigProp.arraySize; ti++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(
                        trigProp.GetArrayElementAtIndex(ti),
                        new GUIContent($"Trigger {ti + 1}")
                    );
                    if (GUILayout.Button("–", GUILayout.Width(20))) remT = ti;
                    EditorGUILayout.EndHorizontal();
                }
                if (remT >= 0) trigProp.DeleteArrayElementAtIndex(remT);
                if (GUILayout.Button("+ Add Trigger"))
                    trigProp.InsertArrayElementAtIndex(trigProp.arraySize);

                // Times Per Turn
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Times Per Turn");
                EditorGUILayout.PropertyField(
                    groupProp.FindPropertyRelative("timesPerTurn"),
                    GUIContent.none
                );
                EditorGUILayout.EndHorizontal();
                if ((TimesPerTurn)groupProp.FindPropertyRelative("timesPerTurn").enumValueIndex
                    == TimesPerTurn.Custom)
                {
                    var customProp = groupProp.FindPropertyRelative("customTimes");
                    customProp.intValue =
                        EditorGUILayout.IntField("  Custom Count", Mathf.Max(1, customProp.intValue));
                }

                // Description
                EditorGUILayout.PropertyField(
                    groupProp.FindPropertyRelative("effectDescription"),
                    new GUIContent("Description")
                );

                // Sub-effects
                var effList = groupProp.FindPropertyRelative("effects");
                int removeEffectIdx = -1;
                for (int ei = 0; ei < effList.arraySize; ei++)
                {
                    var eProp = effList.GetArrayElementAtIndex(ei);
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.PropertyField(
                        eProp.FindPropertyRelative("effectType"),
                        new GUIContent("Effect Type")
                    );
                    var eType = (EffectType)eProp
                        .FindPropertyRelative("effectType")
                        .enumValueIndex;
                    if (_drawerMap.TryGetValue(eType, out var entry))
                        entry.Drawer.Draw(eProp.FindPropertyRelative(entry.PropertyName));
                    if (GUILayout.Button("Remove Effect"))
                        removeEffectIdx = ei;
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(4);
                }
                if (removeEffectIdx >= 0)
                    effList.DeleteArrayElementAtIndex(removeEffectIdx);
                if (GUILayout.Button("+ Add Effect"))
                    effList.InsertArrayElementAtIndex(effList.arraySize);

                // Remove Group
                if (gi > 0 && GUILayout.Button("Remove Group"))
                    removeGroup = gi;

                EditorGUILayout.EndVertical();
                GUILayout.Space(4);
            }
            if (removeGroup >= 0)
                groupsProp.DeleteArrayElementAtIndex(removeGroup);

            if (GUILayout.Button("+ Add Independent Effect"))
                groupsProp.InsertArrayElementAtIndex(groupsProp.arraySize);

            // — Artwork + delete —
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(so.FindProperty("artwork"), new GUIContent("Full Card Image"));
            if (GUILayout.Button("Browse…", GUILayout.Width(70)))
                SelectArtwork();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(8);
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), GUILayout.Height(24)) &&
                EditorUtility.DisplayDialog("Confirm Delete", $"Delete '{selectedCard.waifuName}'?", "Yes", "No"))
            {
                AssetDatabase.DeleteAsset(selectedCardPath);
                selectedCard = null;
                selectedCardPath = null;
                RefreshCardList();
                GUIUtility.ExitGUI();
            }

            so.ApplyModifiedProperties();
            RenameAssetToMatchName();
        }
        else
        {
            GUILayout.Label("Select a card.");
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    void DrawPreviewColumn()
    {
        const float previewW = 320f;
        GUILayout.BeginVertical(GUILayout.Width(previewW));
        GUILayout.Label("Preview", EditorStyles.boldLabel);

        if (selectedCard == null || selectedCard.artwork == null)
        {
            GUILayout.Label("No Preview");
        }
        else
        {
            var tex = selectedCard.artwork.texture;
            float aspect = (float)tex.width / tex.height;
            Rect rect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.Width(previewW));
            GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit, true);

            // Solo dibujamos level/attack/ambush si es Waifu
            if (selectedCard.cardType == CardType.Waifu)
            {
                int numFS = Mathf.RoundToInt(rect.height * 0.06f);
                int lblFS = Mathf.RoundToInt(rect.height * 0.03f);
                Color32 fill = new Color32(0xFC, 0xF4, 0xB6, 255);

                void DrawText(string txt, Rect r, int fs)
                {
                    style.fontSize = fs;
                    style.normal.textColor = new Color32(0x84, 0x27, 0x48, 255);
                    for (int dx = -2; dx <= 2; dx++)
                        for (int dy = -2; dy <= 2; dy++)
                            GUI.Label(new Rect(r.x + dx, r.y + dy, r.width, r.height), txt, style);
                    style.normal.textColor = fill;
                    GUI.Label(r, txt, style);
                }

                DrawText(selectedCard.level.ToString(),
                    new Rect(new Vector2(rect.x + rect.width * 0.06f, rect.y + rect.height * 0.01f) + lvlOffset,
                             new Vector2(rect.width * 0.12f, rect.height * 0.12f)), numFS);
                DrawText(selectedCard.attack.ToString(),
                    new Rect(new Vector2(rect.x + rect.width * 0.06f, rect.y + rect.height * 0.82f) + atkNumOff,
                             new Vector2(rect.width * 0.12f, rect.height * 0.12f)), numFS);
                DrawText("Atk",
                    new Rect(new Vector2(rect.x + rect.width * 0.06f, rect.y + rect.height * 0.82f) + atkLabOff,
                             new Vector2(rect.width * 0.12f, rect.height * 0.12f * 0.4f)), lblFS);
                DrawText(selectedCard.ambush.ToString(),
                    new Rect(new Vector2(rect.x + rect.width * 0.80f - rect.width * 0.12f, rect.y + rect.height * 0.82f) + ambNumOff,
                             new Vector2(rect.width * 0.12f, rect.height * 0.12f)), numFS);
                DrawText("Amb",
                    new Rect(new Vector2(rect.x + rect.width * 0.80f - rect.width * 0.12f, rect.y + rect.height * 0.82f) + ambLabOff,
                             new Vector2(rect.width * 0.12f, rect.height * 0.12f * 0.4f)), lblFS);
            }
            // Para Enchantment/Mood/Accessory no dibujamos nada más
        }

        GUILayout.EndVertical();
    }

    void DrawStatField(SerializedObject so, string propName, string label)
    {
        var prop = so.FindProperty(propName);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        prop.intValue = Mathf.RoundToInt(EditorGUILayout.Slider(prop.intValue, 0, 30));
        prop.intValue = EditorGUILayout.IntField(prop.intValue, GUILayout.Width(50));
        prop.intValue = Mathf.Clamp(prop.intValue, 0, 999);
        EditorGUILayout.EndHorizontal();
    }

    void RenameAssetToMatchName()
    {
        var oldName = Path.GetFileNameWithoutExtension(selectedCardPath);
        var prefix = oldName.Split(' ')[0];
        var safe = selectedCard.waifuName.Replace('/', '_').Replace('\\', '_');
        var newName = $"{prefix} {safe}.asset";
        if (Path.GetFileName(selectedCardPath) != newName)
        {
            AssetDatabase.RenameAsset(selectedCardPath, newName);
            AssetDatabase.SaveAssets();
            selectedCardPath = Path.Combine(Path.GetDirectoryName(selectedCardPath), newName).Replace("\\", "/");
            RefreshCardList();
        }
    }

    void RefreshDeckList()
    {
        const string cardsRoot = "Assets/WaifuSummoner/Cards";
        const string artRoot = cardsRoot + "/Artwork";
        if (!Directory.Exists(cardsRoot)) Directory.CreateDirectory(cardsRoot);
        if (!Directory.Exists(artRoot)) Directory.CreateDirectory(artRoot);
        deckPaths = Directory.GetDirectories(cardsRoot)
            .Where(d => Path.GetFileName(d) != "Artwork")
            .ToArray();
    }

    void LoadCard()
    {
        selectedCard = AssetDatabase.LoadAssetAtPath<WaifuData>(selectedCardPath);
    }

    void CreateNewDeck()
    {
        var cardsRoot = "Assets/WaifuSummoner/Cards";
        var newDeckPath = AssetDatabase.GenerateUniqueAssetPath(cardsRoot + "/NewDeck");
        Directory.CreateDirectory(newDeckPath);
        AssetDatabase.Refresh();
        selectedDeck = newDeckPath;
        selectedData = ScriptableObject.CreateInstance<CollectionData>();
        selectedData.displayName = Path.GetFileName(newDeckPath);
        selectedData.identifier = selectedData.displayName.Substring(0, Mathf.Min(3, selectedData.displayName.Length)).ToUpper();
        AssetDatabase.CreateAsset(selectedData, $"{newDeckPath}/CollectionData.asset");
        AssetDatabase.SaveAssets();
        RefreshDeckList();
        RefreshCardList();
    }

    void CreateNewCard()
    {
        var card = ScriptableObject.CreateInstance<WaifuData>();
        card.waifuName = "New Waifu";
        int idx = cardPathList.Count + 1;
        string num = idx.ToString("000");
        string fn = $"{selectedData.identifier}-{num} {card.waifuName}.asset";
        string ap = AssetDatabase.GenerateUniqueAssetPath($"{selectedDeck}/{fn}");
        AssetDatabase.CreateAsset(card, ap);
        AssetDatabase.SaveAssets();
        RefreshCardList();
    }

    void SelectArtwork()
    {
        if (selectedCard == null) return;
        var src = EditorUtility.OpenFilePanel("Select Full Card Image", "", "png,jpg,jpeg");
        if (string.IsNullOrEmpty(src)) return;
        var cardsRoot = "Assets/WaifuSummoner/Cards";
        var deckName = Path.GetFileName(selectedDeck);
        var destDir = $"{cardsRoot}/Artwork/{deckName}";
        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
        var fn = Path.GetFileName(src);
        var dest = Path.Combine(destDir, fn);
        File.Copy(src, dest, true);
        AssetDatabase.Refresh();
        if (AssetImporter.GetAtPath(dest) is TextureImporter imp)
        {
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.SaveAndReimport();
        }
        selectedCard.artwork = AssetDatabase.LoadAssetAtPath<Sprite>(dest);
        EditorUtility.SetDirty(selectedCard);
        AssetDatabase.SaveAssets();
    }
}
#endif
