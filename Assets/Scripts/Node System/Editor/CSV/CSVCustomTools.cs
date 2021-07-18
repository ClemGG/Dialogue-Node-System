using UnityEditor;
using UnityEngine;

namespace Project.NodeSystem.Editor
{
    public class CSVCustomTools
    {
        static CSVSaver saver;
        static CSVLoader loader;
        static CSVLanguageUpdater updater;
        static CSVSaver Saver => saver ??= new CSVSaver();
        static CSVLoader Loader => loader ??= new CSVLoader();
        static CSVLanguageUpdater Updater => updater ??= new CSVLanguageUpdater();


        [MenuItem("DalogueSystem/CSV/Save Dialogues To CSV")]
        public static void SaveDialoguesToCSV()
        {
            Saver.Save();
            Debug.Log("Dialogues saved to CSV.");
        }


        [MenuItem("DalogueSystem/CSV/Load Dialogues From CSV")]
        public static void LoadDialoguesFromCSV()
        {
            Loader.Load();
            Debug.Log("Dialogues loaded from CSV.");
        }


        ////Utilisé dans le cas où l'on rajoute ou enlève des langues
        //[MenuItem("DalogueSystem/CSV/Update Dialogue Language")]
        //public static void UpdateDialogueLanguage()
        //{
        //    Updater.UpdateLanguage();
        //    Debug.Log("Dialogues updated for all containers.");
        //}
    }
}