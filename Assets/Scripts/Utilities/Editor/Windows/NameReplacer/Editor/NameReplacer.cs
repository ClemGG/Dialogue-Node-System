using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using Project.Utilities.ValueTypes;

namespace Project.Utilities.Editor.NameReplacer
{
    public class NameReplacer : EditorWindow
    {
        //Affichées dans l'inspector
        string searchedAssetName;
        string select;
        string replaceBy;
        bool strictSearch = true;
        bool respectUppercases = true;

        //Pour l'update de la sélection avec les boutons AddScript et RemoveScript
        List<Object> retrievedAssets = new List<Object>();
        List<string> allPathsOfAssetsToUpdate = new List<string>();






        [MenuItem("My Tools/Research/Name Replacer")]
        public static void ShowWindow()
        {
            GetWindow<NameReplacer>("Name Replacer");
        }



        private void OnLostFocus()
        {
            hasPressedSearchButton = false;
            hasPressedReplaceButton = false;

            retrievedAssets.Clear();
        }



        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            searchedAssetName = EditorGUILayout.TextField("Search name : ", searchedAssetName);
            strictSearch = EditorGUILayout.ToggleLeft("Strict search", strictSearch);
            respectUppercases = EditorGUILayout.ToggleLeft("Respect uppercases", respectUppercases);

            if (EditorGUI.EndChangeCheck())
            {
                hasPressedReplaceButton = hasPressedSearchButton = false;
            }

            EditorGUILayout.Space();

            select = EditorGUILayout.TextField("Select : ", select);
            replaceBy = EditorGUILayout.TextField("Replace by : ", replaceBy);






            if (!string.IsNullOrEmpty(searchedAssetName))
            {
                if (GUILayout.Button("Make selection"))
                {
                    hasPressedSearchButton = true;
                    hasPressedReplaceButton = false;
                    nbOccurencesReplaced = 0;

                    OnWizardCreate();
                }
            }


            if (Strings.NoneNull(select) && retrievedAssets.Count > 0)
            {

                if (GUILayout.Button("Replace text"))
                {
                    hasPressedSearchButton = false;
                    hasPressedReplaceButton = true;

                    ReplaceText();
                    OnWizardCreate();

                }
            }



            DisplayHelpBoxes();


        }

        private void ReplaceText()
        {

            for (int i = 0; i < retrievedAssets.Count; i++)
            {

                if (retrievedAssets[i].name.Contains(select))
                {
                    //Undo.RecordObject(retrievedAssets[i], "Asset Renamed");   //Apparemment on ne peut pas annuler les changements de nom faits à une asset
                    string newName = retrievedAssets[i].name.Replace(select, replaceBy);
                    AssetDatabase.RenameAsset(allPathsOfAssetsToUpdate[i], newName);
                    retrievedAssets[i].name = newName;
                    nbOccurencesReplaced++;
                    EditorUtility.SetDirty(retrievedAssets[i]);
                }
            }
            AssetDatabase.SaveAssets();
        }



        void OnWizardCreate()
        {



            if (Strings.AnyNull(searchedAssetName))
            {
                return;
            }



            allPathsOfAssetsToUpdate.Clear();
            retrievedAssets.Clear();

            string[] allAssetsPathsInProject = AssetDatabase.FindAssets(strictSearch ? searchedAssetName : string.Empty, new[] { "Assets" });



            for (int i = 0; i < allAssetsPathsInProject.Length; i++)
            {

                //Affiche le chemin de l'asset 
                //Debug.Log(AssetDatabase.GUIDToAssetPath(allAssetsPathsInProject[i]));

                //Affiche le nom de l'asset, que ce soit un dossier ou non
                //Debug.Log(AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(allAssetsPathsInProject[i]), typeof(object)).name);

                string path = AssetDatabase.GUIDToAssetPath(allAssetsPathsInProject[i]);

                UnityEngine.Object currentAsset = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

                string s = searchedAssetName.SetupForComparison(respectUppercases);
                string comparedName = currentAsset.name.SetupForComparison(respectUppercases);


                if (comparedName.Contains(s))
                {
                    retrievedAssets.Add(currentAsset);
                    allPathsOfAssetsToUpdate.Add(path);
                }
                else if (!strictSearch)
                {
                    bool hasAnOccurence = true;
                    int index = 0;

                    while (index < s.Length && hasAnOccurence)
                    {
                        string character = s[index].ToString();

                        if (!comparedName.Contains(character))
                        {
                            hasAnOccurence = false;
                        }
                        else
                        {
                            index++;
                        }
                    }

                    if (hasAnOccurence)
                    {
                        retrievedAssets.Add(currentAsset);
                        allPathsOfAssetsToUpdate.Add(path);
                    }
                }


            }

            //Ces deux lignes affichent le nom de chaque asset, son ID ainsi que son type (et son contenu, s'il s'agit d'un TextAsset)
            //for (int i = 0; i < assetsToUpdate.Count; i++)
            //{
            //    //Debug.Log(retrievedAssets[i].name);
            //    Debug.Log(assetsToUpdate[i]);
            //}

            Selection.objects = retrievedAssets.ToArray();

        }

















        bool hasPressedSearchButton = false, hasPressedReplaceButton = false;
        int nbOccurencesReplaced = 0;

        private void DisplayHelpBoxes()
        {

            bool noObjectsWereFoundWithThatName = Selection.objects.Length == 0;




            if (noObjectsWereFoundWithThatName && !string.IsNullOrEmpty(searchedAssetName) && hasPressedSearchButton)
            {

                EditorGUILayout.HelpBox("Erreur : Aucune correspondance n'a été trouvée avec le nom recherché.",
                                         MessageType.Info);
            }


            if (hasPressedReplaceButton && nbOccurencesReplaced == 0)
            {

                EditorGUILayout.HelpBox("Erreur : Aucune correspondance n'a été trouvée avec le mot à remplacer.",
                                         MessageType.Info);
            }


            if (retrievedAssets.Count > 0 && !noObjectsWereFoundWithThatName)
            {
                StringBuilder sb = new StringBuilder(150);
                sb.AppendFormat("{0}\n", "Assets trouvées : ");

                for (int i = 0; i < retrievedAssets.Count; i++)
                {
                    sb.AppendFormat("{0}\n", " - " + retrievedAssets[i].name);
                }

                EditorGUILayout.HelpBox(sb.ToString(), MessageType.Info);
            }




            if (Strings.AnyNull(searchedAssetName))
            {
                EditorGUILayout.HelpBox("Entrez un nom pour lancer la recherche d'objets.",
                                         MessageType.Info);
            }

            if (Strings.AnyNull(select) && retrievedAssets.Count > 0 && hasPressedSearchButton)
            {
                EditorGUILayout.HelpBox("Entrez les caractères à remplacer.",
                                         MessageType.Info);
            }


        }
    }
}