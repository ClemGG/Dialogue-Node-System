using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.NodeSystem.Editor
{
    public static class CSVHelper
    {

        /// <summary>
        /// Runtime version of finding assets in Resources.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> FindAllObjectsFromResources<T>()
        {
            List<T> tmp = new List<T>();
            string path = $"{Application.dataPath}/Resources";
            string[] directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

            //Pour chaque dossier dans Resources, chercher l'asset à cet emplacement
            foreach (string directory in directories)
            {
                string dirPath = directory.Substring(path.Length + 1);  //Recupère les chemins des fichiers relatif au chemin du dossier
                T[] result = Resources.LoadAll(dirPath, typeof(T)).Cast<T>().ToArray();

                foreach (T obj in result)
                { 
                    //Evite les duplicatas
                    if (!tmp.Contains(obj))
                        tmp.Add(obj);
                }
            }

            return tmp;
        }


        /// <summary>
        /// Version dans l'éditeur, permet de trouver les dialogues n'importe où dans le dossier Assets
        /// </summary>
        /// <returns></returns>
        public static List<DialogueContainerSO> FindAllDialogueContainers()
        {
            string[] guids = AssetDatabase.FindAssets("t:DialogueContainerSO");

            DialogueContainerSO[] dialoguesFound = new DialogueContainerSO[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                dialoguesFound[i] = AssetDatabase.LoadAssetAtPath<DialogueContainerSO>(path);
            }

            return dialoguesFound.ToList();
        }
    }
}