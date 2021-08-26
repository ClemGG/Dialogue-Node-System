using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;

namespace Project.Utilities.Editor.ObjectSetter
{
    public class ObjectSetter : EditorWindow
    {
        //Affichées dans l'inspector
        string searchName;
        string searchTag;
        bool alsoSearchByTag = false;
        bool searchOnlyByTag = false;
        bool alsoSearchInactiveObjects = false;
        bool strictSearch = true;
        bool alsoSearchByScript = false;
        bool searchOnlyByScript = false;
        bool addScriptToObjectsIfMissing = false;
        MonoScript scriptToSet;


        List<GameObject> objectsWithoutAssignedScript = new List<GameObject>(); //Pour la recherche de scripts


        //Pour l'update de la sélection avec les boutons AddScript et RemoveScript
        bool shouldUpdateSelection = false;
        bool shouldRemoveSelection = false;
        List<GameObject> objectsToUpdate = new List<GameObject>();
        List<GameObject> objectsToClear = new List<GameObject>();
        Type T = null;




        [MenuItem("My Tools/Research/Object Setter")]
        public static void ShowWindow()
        {
            GetWindow<ObjectSetter>("Object Setter");
        }








        private void OnGUI()
        {

            EditorGUI.BeginChangeCheck();
            searchName = EditorGUILayout.TextField("Search name : ", searchName);
            searchTag = EditorGUILayout.TextField("Search tag : ", searchTag);


            EditorGUILayout.Space();

            alsoSearchByTag = EditorGUILayout.BeginToggleGroup("Also search by tag", alsoSearchByTag);
            if (alsoSearchByTag) searchOnlyByTag = EditorGUILayout.ToggleLeft("Search only by tag", searchOnlyByTag);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();


            alsoSearchByScript = EditorGUILayout.BeginToggleGroup("Also search by script", alsoSearchByScript);
            if (alsoSearchByScript) searchOnlyByScript = EditorGUILayout.ToggleLeft("Search only by script", searchOnlyByScript);
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.Space();

            strictSearch = EditorGUILayout.ToggleLeft("Strict search", strictSearch);
            alsoSearchInactiveObjects = EditorGUILayout.ToggleLeft("Also search inactive objects", alsoSearchInactiveObjects);
            addScriptToObjectsIfMissing = EditorGUILayout.ToggleLeft("Add script to objects if missing", addScriptToObjectsIfMissing);

            scriptToSet = EditorGUILayout.ObjectField(scriptToSet, typeof(MonoScript), false) as MonoScript;





            if (!string.IsNullOrEmpty(searchName) ||
                !string.IsNullOrEmpty(searchTag) && alsoSearchByTag ||
                scriptToSet != null && alsoSearchByScript && scriptToSet.GetClass().IsSubclassOf(typeof(MonoBehaviour)))
            {

                if (GUILayout.Button("Make selection"))
                {
                    OnWizardCreate();
                }
            }

            UpdateSelection();



            if (shouldUpdateSelection && scriptToSet != null && objectsToUpdate.Count > 0)
            {

                if (GUILayout.Button("Add script"))
                {
                    for (int i = 0; i < objectsToUpdate.Count; i++)
                    {
                        Type T = scriptToSet.GetClass();
                        //objectsToUpdate[i].AddComponent(T);
                        Undo.AddComponent(objectsToUpdate[i], T);
                        //On rafraîchit la sélection pour pouvoir à nouveau appeler DisplayScriptProperties()
                        UnityEngine.Object[] o = Selection.objects;
                        Selection.objects = GameObject.FindGameObjectsWithTag("Untagged");
                        Selection.objects = o;
                    }

                    objectsToUpdate.Clear();
                    shouldUpdateSelection = false;

                }
            }

            if (shouldRemoveSelection && scriptToSet != null && objectsToClear.Count > 0)
            {
                if (GUILayout.Button("Remove script"))
                {

                    for (int i = 0; i < objectsToClear.Count; i++)
                    {
                        T = scriptToSet.GetClass();
                        GameObject go = objectsToClear[i] as GameObject;
                        //DestroyImmediate(go.GetComponent(T));
                        Undo.DestroyObjectImmediate(go.GetComponent(T));
                        //On rafraîchit la sélection pour pouvoir à nouveau appeler DisplayScriptProperties()
                        UnityEngine.Object[] o = Selection.objects;
                        Selection.objects = GameObject.FindGameObjectsWithTag("Untagged");
                        Selection.objects = o;
                    }

                    objectsToClear.Clear();
                    shouldRemoveSelection = false;
                }
            }


            DisplayHelpBoxes();


        }






        private void UpdateSelection()
        {

            objectsToUpdate.Clear();
            objectsToClear.Clear();

            if (scriptToSet == null)
                return;


            if (Selection.objects.Length > 0)
            {
                T = scriptToSet.GetClass();
                MonoBehaviour scriptOnThisObject = null;

                for (int i = 0; i < Selection.objects.Length; i++)
                {
                    GameObject go = Selection.objects[i] as GameObject;

                    if (T != null)
                        scriptOnThisObject = (MonoBehaviour)go.GetComponent(T);

                    if (scriptOnThisObject == null)
                    {
                        objectsToUpdate.Add(go);
                    }
                    else
                    {
                        objectsToClear.Add(go);
                    }
                }


                if (objectsToUpdate.Count == 0)
                    shouldUpdateSelection = false;
                else
                    shouldUpdateSelection = true;


                if (objectsToClear.Count == 0)
                    shouldRemoveSelection = false;
                else
                    shouldRemoveSelection = true;



            }


        }









        void OnWizardCreate()
        {
            objectsWithoutAssignedScript.Clear();



            if (string.IsNullOrEmpty(searchName) && !searchOnlyByTag && !searchOnlyByScript)
            {
                return;
            }




            List<GameObject> objectsInScene = new List<GameObject>();
            GameObject[] gameObjectsInScene = null;

            if (alsoSearchInactiveObjects)
            {
                gameObjectsInScene = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
            }
            else
            {
                gameObjectsInScene = FindObjectsOfType(typeof(GameObject)) as GameObject[];
            }




            for (int i = 0; i < gameObjectsInScene.Length; i++)
            {





                if (searchOnlyByScript)
                {
                    Type T = null;
                    MonoBehaviour scriptOnThisObject = null;



                    if (scriptToSet != null)
                    {
                        T = scriptToSet.GetClass();
                        scriptOnThisObject = (MonoBehaviour)gameObjectsInScene[i].GetComponent(T);
                    }

                    if (scriptOnThisObject != null)
                    {
                        CheckRequirements(ref gameObjectsInScene[i], ref objectsInScene);
                    }
                }





                if (alsoSearchByTag && gameObjectsInScene[i].tag != searchTag)
                {
                    continue;
                }


                string s = searchName.ToLower().Trim().Replace(" ", null);
                string comparedName = gameObjectsInScene[i].name.ToLower().Trim().Replace(" ", null);


                bool myObjectContainsTheSearchedName = comparedName.Contains(s);


                if (myObjectContainsTheSearchedName || searchOnlyByTag)
                {
                    CheckRequirements(ref gameObjectsInScene[i], ref objectsInScene);
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

                        CheckRequirements(ref gameObjectsInScene[i], ref objectsInScene);


                    }

                }



            }



            Selection.objects = objectsInScene.ToArray();

        }












        public void CheckRequirements(ref GameObject selectedGameObject, ref List<GameObject> objectsInScene)
        {


            Type T = null;
            MonoBehaviour scriptOnThisObject = null;



            if (scriptToSet != null)
            {
                T = scriptToSet.GetClass();
                scriptOnThisObject = (MonoBehaviour)selectedGameObject.GetComponent(T);
            }

            if (T != null)
            {
                if (addScriptToObjectsIfMissing)
                {
                    selectedGameObject.AddComponent(T);
                    objectsInScene.Add(selectedGameObject);
                }
            }



            if (alsoSearchByScript)
            {

                if (scriptOnThisObject != null)
                {
                    objectsInScene.Add(selectedGameObject);
                }
                else
                {
                    objectsWithoutAssignedScript.Add(selectedGameObject);
                }
            }
            else
            {
                objectsInScene.Add(selectedGameObject);
            }
        }










        private void DisplayHelpBoxes()
        {

            bool noObjectsWereFoundWithThatName = Selection.objects.Length == 0 && !alsoSearchByScript && !addScriptToObjectsIfMissing;
            bool hasAssignedScript = scriptToSet != null;


            if (objectsToUpdate.Count > 0 /* && (shouldUpdateSelection || shouldRemoveSelection)*/)
            {

                StringBuilder sb = new StringBuilder(150);
                sb.AppendFormat("{0}\n", "Objets sur lesquels le script est manquant : ");

                for (int i = 0; i < objectsToUpdate.Count; i++)
                {
                    sb.AppendFormat("{0}\n", " - " + objectsToUpdate[i].name);
                }

                EditorGUILayout.HelpBox(sb.ToString(), MessageType.Info);
            }




            if (noObjectsWereFoundWithThatName && (!string.IsNullOrEmpty(searchName) || alsoSearchByScript && searchOnlyByScript && scriptToSet != null))
            {

                EditorGUILayout.HelpBox("Erreur : Aucune correspondance n'a été trouvée.",
                                         MessageType.Info);
            }


            if (!hasAssignedScript && (alsoSearchByScript || addScriptToObjectsIfMissing))
            {

                EditorGUILayout.HelpBox("Erreur : Vous tentez de rechercher ou d'assigner un script alors que vous n'en référencez aucun.",
                                         MessageType.Error);
            }

            if (hasAssignedScript && !scriptToSet.GetClass().IsSubclassOf(typeof(MonoBehaviour)) && alsoSearchByScript)
            {
                EditorGUILayout.HelpBox("Erreur : Vous tentez de rechercher ou d'assigner un script qui n'est pas un MonoBehaviour.",
                                         MessageType.Error);
            }


            if (objectsWithoutAssignedScript.Count > 0 && !alsoSearchByScript && !searchOnlyByScript)
            {


                StringBuilder sb = new StringBuilder(150);
                sb.AppendFormat("{0}\n {1}\n", "Warning : Un ou plusieurs objets que vous recherchez ne possèdent pas le script référencé et n'ont pas été sélectionnés.", "Ces objets sont : ");

                for (int i = 0; i < objectsWithoutAssignedScript.Count; i++)
                {
                    sb.AppendFormat("{0}\n", "-" + objectsWithoutAssignedScript[i].name);
                }

                EditorGUILayout.HelpBox(sb.ToString(), MessageType.Warning);

            }


            if (string.IsNullOrEmpty(searchName) && !alsoSearchByTag && !alsoSearchByScript)
            {
                EditorGUILayout.HelpBox("Entrez un nom pour lancer la recherche d'objets.",
                                         MessageType.Info);
            }


        }
    }
}