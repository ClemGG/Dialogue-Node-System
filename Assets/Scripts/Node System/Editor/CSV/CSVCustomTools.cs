using UnityEditor;
using UnityEngine;
using Project.Utilities.Editor;


namespace Project.NodeSystem.Editor
{
    public class CSVCustomTools
    {
        static CSVSaver s_saver;
        static CSVLoader s_loader;
        static DialogueEditorWindow s_window;
        static CSVSaver Saver => s_saver ??= new CSVSaver();
        static CSVLoader Loader => s_loader ??= new CSVLoader();
        static DialogueEditorWindow Window => s_window ??= EditorUtilities.FindEditorWindow<DialogueEditorWindow>("Dialogue Editor", 0);


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

            //Si on a un DialogueEditor ouvert, on rafraîchit le GraphView
            if (Window) 
            {
                Window.Load();
            }

            Debug.Log("Dialogues loaded from CSV.");
        }
    }
}