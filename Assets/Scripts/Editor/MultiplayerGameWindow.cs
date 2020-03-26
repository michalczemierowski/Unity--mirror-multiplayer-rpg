/*
 * Michał Czemierowski
 * https://github.com/michalczemierowski
*/

using UnityEngine;
using UnityEditor;
using MULTIPLAYER_GAME.Inventory.Items;
using MULTIPLAYER_GAME.Systems;

public class MultiplayerGameWindow : EditorWindow
{
    private ObjectDatabase objectDatabase;
    private Texture2D logo;

    private string[] _options = new string[3] { "Item", "Equipment", "Weapon" };
    private string itemName;
    private int selectedOption;

    private short itemID, eqID, weaponID;

    private string PREFS_ITEMS = "ITEM_IDS";
    private string PREFS_EQUIPMENT = "EQUIPMENT_IDS";
    private string PREFS_WEAPONS = "WEAPON_IDS";

    private Vector2 scrollPosition;

    [MenuItem("Tools/MULTIPLAYER GAME/Game Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<MultiplayerGameWindow>("Multiplayer game");
    }

    public void Refresh()
    {
        objectDatabase = FindObjectOfType<ObjectDatabase>();
    }

    private void Awake()
    {
        logo = Resources.Load<Texture2D>("Editor/logo");

        PlayerPrefs.DeleteKey(PREFS_ITEMS);
        PlayerPrefs.DeleteKey(PREFS_EQUIPMENT);
        PlayerPrefs.DeleteKey(PREFS_WEAPONS);

        // ITEMS
        if (!PlayerPrefs.HasKey(PREFS_ITEMS))
        {
            PlayerPrefs.SetInt(PREFS_ITEMS, 0);
        }
        // EQUIPMENT
        if (!PlayerPrefs.HasKey(PREFS_EQUIPMENT))
        {
            PlayerPrefs.SetInt(PREFS_EQUIPMENT, 10000);
        }
        // WEAPONS
        if (!PlayerPrefs.HasKey(PREFS_WEAPONS))
        {
            PlayerPrefs.SetInt(PREFS_WEAPONS, 20000);
        }
    }

    private void OnEnable()
    {
        objectDatabase = FindObjectOfType<ObjectDatabase>();

        itemID = (short)PlayerPrefs.GetInt(PREFS_ITEMS);
        eqID = (short)PlayerPrefs.GetInt(PREFS_EQUIPMENT);
        weaponID = (short)PlayerPrefs.GetInt(PREFS_WEAPONS);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt(PREFS_ITEMS, itemID);
        PlayerPrefs.SetInt(PREFS_EQUIPMENT, eqID);
        PlayerPrefs.SetInt(PREFS_WEAPONS, weaponID);
    }

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(
            scrollPosition);

        GUILayout.Label(logo, EditorStyles.centeredGreyMiniLabel);

        if (objectDatabase)
        {
            EditorGUILayout.LabelField("Create Item", EditorStyles.centeredGreyMiniLabel);

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();

            GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);
            selectedOption = EditorGUILayout.Popup(selectedOption, _options);
            GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);

            GUILayout.EndHorizontal();

            GUILayout.Space(20);

            itemName = EditorGUILayout.TextField("Item name:", itemName);
            switch (selectedOption)
            {
                case 0:
                    itemID = (short)EditorGUILayout.IntField("Item ID:", itemID);
                    break;
                case 1:
                    eqID = (short)EditorGUILayout.IntField("Equipment ID:", eqID);
                    break;
                case 2:
                    weaponID = (short)EditorGUILayout.IntField("Weapon ID:", weaponID);
                    break;
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Create item", GUILayout.Height(30)) && itemName != string.Empty)
            {
                switch (selectedOption)
                {
                    case 0:
                        Item item = ScriptableObject.CreateInstance<Item>();
                        item.ID = itemID++;

                        AssetDatabase.CreateAsset(item, "Assets/Resources/Items/" + itemName + ".asset");
                        AssetDatabase.SaveAssets();

                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = item;

                        //objectDatabase._allItems.Add(item);
                        break;
                    case 1:
                        Equipment equipment = ScriptableObject.CreateInstance<Equipment>();
                        equipment.ID = eqID++;

                        AssetDatabase.CreateAsset(equipment, "Assets/Resources/Items/Equipment/" + itemName + ".asset");
                        AssetDatabase.SaveAssets();

                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = equipment;

                        //objectDatabase._allItems.Add(equipment);
                        break;
                    case 2:
                        Weapon weapon = ScriptableObject.CreateInstance<Weapon>();
                        weapon.ID = weaponID++;

                        AssetDatabase.CreateAsset(weapon, "Assets/Resources/Items/Weapons/" + itemName + ".asset");
                        AssetDatabase.SaveAssets();

                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = weapon;

                        //objectDatabase._allItems.Add(weapon);
                        //objectDatabase._allWeapons.Add(weapon);
                        break;
                }
            }
        }
        else
        {
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();

            GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label("There is no active Object Database in current scene", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label("--------", EditorStyles.centeredGreyMiniLabel);

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("Search for object database", GUILayout.Height(30)))
                Refresh();
        }
        GUILayout.EndScrollView();
    }
}
