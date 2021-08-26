using UnityEditor;
using UnityEngine;

using static Project.Utilities.ValueTypes.Conversions;
using Project.Utilities.ValueTypes;

namespace Project.Utilities.Editor.PlayerPrefs
{
    public class PlayerPrefsEditor : EditorWindow
    {
        public int keyValueI;
        public float keyValueF;
        public bool keyValueB;
        public string keyValueS;


        public enum KeyType { Int, Float, Bool, String };
        public KeyType keyType;
        public string keyTag;

        bool exists = false, created = false, deleted = false, assigned = false;



        [MenuItem("My Tools/PlayerPrefs/Delete All")]
        public static void DeleteAll()
        {
            UnityEngine.PlayerPrefs.DeleteAll();
        }




        [MenuItem("My Tools/PlayerPrefs/Settings")]
        public static void ShowWindow()
        {
            GetWindow<PlayerPrefsEditor>("PlayerPrefs Settings");
        }


        private void OnGUI()
        {

            DisplayVariables();
            DisplayHelpBoxes();
        }

        private void DisplayVariables()
        {

            EditorGUI.BeginChangeCheck();
            keyTag = EditorGUILayout.TextField("Key tag : ", keyTag);
            keyType = (KeyType)EditorGUILayout.EnumPopup(keyType);

            if (!Strings.IsNull(keyTag))
            {

                switch (keyType)
                {
                    case KeyType.Int:
                        keyValueI = EditorGUILayout.IntField(keyValueI);
                        break;

                    case KeyType.Float:
                        keyValueF = EditorGUILayout.FloatField(keyValueF);
                        break;

                    case KeyType.Bool:
                        keyValueB = EditorGUILayout.Toggle(keyValueB);
                        break;

                    case KeyType.String:
                        keyValueS = EditorGUILayout.TextField(keyValueS);
                        break;


                }
            }


            if (EditorGUI.EndChangeCheck())
            {
                created = deleted = exists = assigned = false;
            }

            if (!Strings.IsNull(keyTag))
            {

                if (!UnityEngine.PlayerPrefs.HasKey(keyTag))
                {
                    exists = false;
                    assigned = false;

                    if (GUILayout.Button("Create key entry"))
                    {
                        created = true;
                        deleted = false;
                        SaveToPlayerPrefs();
                    }
                }
                else
                {
                    exists = true;

                    if (GUILayout.Button("Assign new value for this key"))
                    {
                        assigned = true;
                        created = false;
                        SaveToPlayerPrefs();
                    }

                    if (GUILayout.Button("Delete key entry"))
                    {
                        deleted = true;
                        UnityEngine.PlayerPrefs.DeleteKey(keyTag);
                    }
                }
            }


        }

        private void SaveToPlayerPrefs()
        {
            switch (keyType)
            {
                case KeyType.Int:
                    UnityEngine.PlayerPrefs.SetInt(keyTag, keyValueI);
                    break;

                case KeyType.Float:
                    UnityEngine.PlayerPrefs.SetFloat(keyTag, keyValueF);
                    break;

                case KeyType.Bool:
                    UnityEngine.PlayerPrefs.SetInt(keyTag, keyValueB.ToInt(false));
                    break;

                case KeyType.String:
                    UnityEngine.PlayerPrefs.SetString(keyTag, keyValueS);
                    break;

            }
        }








        private void DisplayHelpBoxes()
        {
            if (assigned)
            {
                var val = "0";

                switch (keyType)
                {
                    case KeyType.Int:
                        val = keyValueI.ToString();
                        break;

                    case KeyType.Bool:
                        val = keyValueB.ToString();
                        break;

                    case KeyType.String:
                        val = keyValueS;
                        break;

                }


                EditorGUILayout.HelpBox($"La nouvelle valeur \"{val}\" a été assignée avec succès à la clé \"{keyTag}\".",
                                        MessageType.Info);
            }
            else
            {
                if (created)
                {
                    EditorGUILayout.HelpBox($"La clé \"{keyTag}\" a été créée avec succès et contient la valeur \"{GetValueFromPlayerPrefs(keyTag).ToString()}\".",
                                                MessageType.Info);
                }
            }


            if (exists)
                EditorGUILayout.HelpBox($"La clé \"{keyTag}\" " + (UnityEngine.PlayerPrefs.HasKey(keyTag) ? "existe" : "n'existe pas") + " dans les PlayerPrefs " +
                                         $"et contient la valeur \"{GetValueFromPlayerPrefs(keyTag).ToString()}\".",
                                            MessageType.Info);

            if (deleted)
                EditorGUILayout.HelpBox($"La clé \"{keyTag}\" a été supprimée avec succès.",
                                            MessageType.Info);

        }

        private object GetValueFromPlayerPrefs(string keyTag)
        {

            switch (keyType)
            {
                case KeyType.Int:
                    return UnityEngine.PlayerPrefs.GetInt(keyTag);

                case KeyType.Bool:
                    return UnityEngine.PlayerPrefs.GetInt(keyTag).ToBool();

                case KeyType.String:
                    return UnityEngine.PlayerPrefs.GetString(keyTag);

                default:
                    return default;

            }


        }
    }
}