using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project.Utilities.Assets
{

    public class AssetFinderUtilities : MonoBehaviour
    {
        /// <summary>
        /// Runtime version of finding assets in Resources.
        /// </summary>
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


        public static T FindInResourcesByName<T>(string assetName) where T : Object
        {
            List<T> assets = FindAllObjectsFromResources<T>();
            return assets.Find(asset => asset.name == assetName);
        }

        public static T FindInResourcesAtPath<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }



        public static T FindInResourcesByID<T>(int instanceID) where T : Object
        {
            return Resources.InstanceIDToObject(instanceID) as T;
        }



#if UNITY_EDITOR


        /// <summary>
        /// Version dans l'éditeur, permet de trouver les dialogues n'importe où dans le dossier Assets
        /// </summary>
        /// <returns></returns>
        public static List<T> FindAllAssetsOfType<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:DialogueContainerSO");

            T[] assetsFound = new T[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                assetsFound[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return assetsFound.ToList();
        }



        public static T FindAssetAtPath<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }



        public static string GetAssetPath(Object asset)
        {
            return AssetDatabase.GetAssetPath(asset);
        }


        public static string GetAssetPath(int instanceID)
        {
            return AssetDatabase.GetAssetPath(instanceID);
        }
#endif
    }
}