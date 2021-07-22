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
        private bool makeGridVisible = false;
        private int nbMaxChoices = 4;
        private LanguageType selectedLanguage = LanguageType.French;


        private ToolbarMenu languageDropdown;   //Permet de choisir la langue dans la barre d'outils de la fen�tre
        private Label dialogueContainerLabel;   //Affiche le nom du Dialogue en cours d'�dition

        /// <summary>
        /// La langue s�lectionn�e dans l'�diteur.
        /// </summary>
        public LanguageType SelectedLanguage { get => selectedLanguage; set => selectedLanguage = value; }
        public int NbMaxChoices { get => nbMaxChoices; set => nbMaxChoices = value; }




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
            PlayerPrefs.SetInt("Editor_makeGridVisible", makeGridVisible ? 1 : 0);
            PlayerPrefs.SetInt("Editor_NbMaxChoices", NbMaxChoices);
        }



        /// <summary>
        /// Ouvre un DialogueContainer dans l'�diteur quand on double clique sur son asset.
        /// Plus le callbackOrder de OnOpenAsset est grand, plus cette fonction sera appel�e en dernier.
        /// (Dans le cas o� l'on aurait d'autres OnOpenAssets dans ce script).
        /// </summary>
        /// <param name="instanceID">L'ID du dialogue, permet au script de le retrouver dans les Assets et l'ouvrir.</param>
        /// <param name="line">Pas important.</param>
        /// <returns></returns>
        [OnOpenAsset(1)]
        public static bool ShowWindow(int instanceID, int line)
        {
            //R�cup�re le dialogue dans les Assets via son ID
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


            //Pour afficher/cacher la grille
            Label showGridLabel = new Label("Show Grid");
            showGridLabel.AddToClassList("showGridLabel");

            Toggle showGridToggle = new Toggle();
            makeGridVisible = PlayerPrefs.GetInt("Editor_makeGridVisible", makeGridVisible ? 1 : 0) == 1 ? true : false;
            showGridToggle.value = makeGridVisible;
            showGridToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                showGridToggle.value = makeGridVisible = evt.newValue;
                graphView.ToggleGrid(makeGridVisible);
                PlayerPrefs.SetInt("Editor_makeGridVisible", makeGridVisible ? 1 : 0);
            });


            Label nbChoicesLabel = new Label("Nb Max Choices");
            showGridLabel.AddToClassList("showGridLabel");

            //Pour changer le nombre de choix permis pour la ChoiceNode
            IntegerField nbChoicedAllowed = new IntegerField();
            NbMaxChoices = PlayerPrefs.GetInt("Editor_NbMaxChoices", NbMaxChoices);
            nbChoicedAllowed.value = NbMaxChoices;
            nbChoicedAllowed.RegisterValueChangedCallback(value =>
            {
                nbChoicedAllowed.value = value.newValue;
                NbMaxChoices = nbChoicedAllowed.value;
                PlayerPrefs.SetInt("Editor_NbMaxChoices", NbMaxChoices);
            });



            //On l'active une fois pour synchroniser la grille avec le toggle
            graphView.ToggleGrid(makeGridVisible);




            //Pour chaque langue, cr�er une option dans le dropdown pour changer la langue
            languageDropdown = new ToolbarMenu();
            ForEach<LanguageType>(language =>
            {
                languageDropdown.menu.AppendAction(language.ToString(), new Action<DropdownMenuAction>(x => SetLanguage(language)));
            });

            dialogueContainerLabel = new Label();
            dialogueContainerLabel.AddToClassList("dialogueContainerName");

            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(showGridLabel);
            toolbar.Add(showGridToggle);
            toolbar.Add(nbChoicesLabel);
            toolbar.Add(nbChoicedAllowed);
            toolbar.Add(languageDropdown);
            toolbar.Add(dialogueContainerLabel);

            rootVisualElement.Add(toolbar);
        }

        #endregion




        #region Save & Load

        public void Load()
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

            //Le graphView r�cup�re toutes ses DialogueNodes et les traduit
            graphView.ReloadLanguage();
        }


        #endregion
    }
}