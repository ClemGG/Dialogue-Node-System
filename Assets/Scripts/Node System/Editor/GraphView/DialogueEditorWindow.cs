using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem.Editor
{

    public class DialogueEditorWindow : EditorWindow
    {
        #region Fields

        private DialogueContainerSO currentDialogueContainer;
        private DialogueGraphView graphView;
        private DialogueSaveLoad saveLoad;
        private string graphViewStyleSheet = "USS/EditorWindow/EditorWindowStyleSheet";


        private ToolbarMenu languageDropdown;   //Permet de choisir la langue dans la barre d'outils de la fenêtre
        private Label dialogueContainerLabel;   //Affiche le nom du Dialogue en cours d'édition

        /// <summary>
        /// La langue sélectionnée dans l'éditeur.
        /// </summary>
        public LanguageType SelectedLanguage { get => selectedLanguage; set => selectedLanguage = value; }
        private LanguageType selectedLanguage = LanguageType.French;



        #endregion



        #region Mono

        private void OnEnable()
        {

            CreateGraphView();
            CreateToolbar();
            Load();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(graphView);
        }


        
        /// <summary>
        /// Ouvre un DialogueContainer dans l'éditeur quand on double clique sur son asset.
        /// Plus le callbackOrder de OnOpenAsset est grand, plus cette fonction sera appelée en dernier.
        /// (Dans le cas où l'on aurait d'autres OnOpenAssets dans ce script).
        /// </summary>
        /// <param name="instanceID">L'ID du dialogue, permet au script de le retrouver dans les Assets et l'ouvrir.</param>
        /// <param name="line">Pas important.</param>
        /// <returns></returns>
        [OnOpenAsset(1)]
        public static bool ShowWindow(int instanceID, int line)
        {
            //Récupère le dialogue dans les Assets via son ID
            UnityEngine.Object item = EditorUtility.InstanceIDToObject(instanceID);

            if (item is DialogueContainerSO)
            {
                DialogueEditorWindow window = GetWindow<DialogueEditorWindow>();
                window.titleContent = new GUIContent("Dialogue Editor");
                window.currentDialogueContainer = item as DialogueContainerSO;
                window.minSize = new Vector2(500, 250);
                window.Load();

                return true;
            }

            return false;
        }


        private void OnGUI()
        {
            graphView.UpdateMinimap();
        }


        #endregion



        #region GraphView

        private void CreateGraphView()
        {
            graphView = new DialogueGraphView(this);
            graphView.StretchToParentSize();
            rootVisualElement.Add(graphView);

            saveLoad = new DialogueSaveLoad(graphView);
        }

        private void CreateToolbar()
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>(graphViewStyleSheet);
            rootVisualElement.styleSheets.Add(styleSheet);

            Toolbar toolbar = new Toolbar();

            Button saveButton = new Button(() => Save()) { text = "Save" };
            Button loadButton = new Button(() => Load()) { text = "Load" };

            //Pour chaque langue, créer une option dans le dropdown pour changer la langue
            languageDropdown = new ToolbarMenu();
            ForEach<LanguageType>(language =>
            {
                languageDropdown.menu.AppendAction(language.ToString(), new Action<DropdownMenuAction>(x => SetLanguage(language)));
            });

            dialogueContainerLabel = new Label();
            dialogueContainerLabel.AddToClassList("dialogueContainerName");

            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(languageDropdown);
            toolbar.Add(dialogueContainerLabel);

            rootVisualElement.Add(toolbar);
        }

        #endregion




        #region Save & Load

        private void Load()
        {
            if (currentDialogueContainer)
            {
                SetLanguage(selectedLanguage);
                dialogueContainerLabel.text = currentDialogueContainer.name;

                saveLoad.Load(currentDialogueContainer);
            }

        }


        private void Save()
        {
            if (currentDialogueContainer)
                saveLoad.Save(currentDialogueContainer);
        }


        private void SetLanguage(LanguageType newLanguage)
        {
            languageDropdown.text = $"Language : {newLanguage.ToString()}";
            selectedLanguage = newLanguage;

            //Le graphView récupère toutes ses DialogueNodes et les traduit
            graphView.ReloadLanguage();
        }


        #endregion
    }
}