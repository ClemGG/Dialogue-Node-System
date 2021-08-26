using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Project.Utilities.ValueTypes.Enums;
using Project.Utilities.Assets;

namespace Project.NodeSystem.Editor
{

    public class DialogueEditorWindow : EditorWindow
    {
        #region Fields

        private DialogueContainerSO _currentDialogueContainer;
        private DialogueGraphView _graphView;
        private DialogueSaveLoad _saveLoad;
        private const string _graphViewStyleSheet = "USS/EditorWindow/EditorWindowStyleSheet";
        private bool _makeGridVisible = false;
        private int _nbMaxChoices = 4;
        private LanguageType selectedLanguage = LanguageType.French;


        private ToolbarMenu languageDropdown;   //Permet de choisir la langue dans la barre d'outils de la fenêtre
        private Label dialogueContainerLabel;   //Affiche le nom du Dialogue en cours d'édition

        /// <summary>
        /// La langue sélectionnée dans l'éditeur.
        /// </summary>
        public LanguageType SelectedLanguage { get => selectedLanguage; set => selectedLanguage = value; }
        public int NbMaxChoices { get => _nbMaxChoices; set => _nbMaxChoices = value; }




        #endregion



        #region Mono

        private void OnEnable()
        {
            CreateGraphView();
            CreateToolbar();




            // Au lancement de l'éditeur, si la fenêtre est ouverte mais qu'elle est vide, 
            // on récupère le dialogue enregistré lors de la précédente session.
            //Si l'asset a été déplacée alors que Unity était fermé, AssetFinderUtilities renverra juste null, donc on risque rien.
            if (!_currentDialogueContainer && PlayerPrefs.HasKey("lastDialogue"))
            {
                _currentDialogueContainer = AssetFinderUtilities.FindAssetAtPath<DialogueContainerSO>(PlayerPrefs.GetString("lastDialogue"));
            }

            Load();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
            PlayerPrefs.SetInt("Editor_makeGridVisible", _makeGridVisible ? 1 : 0);
            PlayerPrefs.SetInt("Editor_NbMaxChoices", NbMaxChoices);

            //Sauvegarde l'instanceID pour rouvrir le dernier dialogue édité au lancement de l'éditeur
            if(_currentDialogueContainer)
                PlayerPrefs.SetString("lastDialogue", AssetFinderUtilities.GetAssetPath(_currentDialogueContainer));
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
            //Sauvegarde l'instanceID pour rouvrir le dernier dialogue édité au lancement de l'éditeur
            PlayerPrefs.SetString("lastDialogue", AssetFinderUtilities.GetAssetPath(instanceID));

            //Récupère le dialogue dans les Assets via son ID
            UnityEngine.Object item = EditorUtility.InstanceIDToObject(instanceID);

            if (item is DialogueContainerSO)
            {
                DialogueEditorWindow window = GetWindow<DialogueEditorWindow>();
                window.titleContent = new GUIContent("Dialogue Editor");
                window._currentDialogueContainer = item as DialogueContainerSO;
                window.minSize = new Vector2(500, 250);
                window.Load();

                return true;
            }

            return false;
        }


        private void OnGUI()
        {
            _graphView.UpdateMinimap();
        }


        #endregion



        #region GraphView

        private void CreateGraphView()
        {
            _graphView = new DialogueGraphView(this);
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);

            _saveLoad = new DialogueSaveLoad(_graphView);
        }

        private void CreateToolbar()
        {
            StyleSheet styleSheet = Resources.Load<StyleSheet>(_graphViewStyleSheet);
            rootVisualElement.styleSheets.Add(styleSheet);

            Toolbar toolbar = new Toolbar();

            Button saveButton = new Button(() => Save()) { text = "Save" };
            Button loadButton = new Button(() => Load()) { text = "Load" };


            //Pour afficher/cacher la grille
            Label showGridLabel = new Label("Show Grid");
            showGridLabel.AddToClassList("showGridLabel");

            Toggle showGridToggle = new Toggle();
            _makeGridVisible = PlayerPrefs.GetInt("Editor_makeGridVisible", _makeGridVisible ? 1 : 0) == 1 ? true : false;
            showGridToggle.value = _makeGridVisible;
            showGridToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                showGridToggle.value = _makeGridVisible = evt.newValue;
                _graphView.ToggleGrid(_makeGridVisible);
                PlayerPrefs.SetInt("Editor_makeGridVisible", _makeGridVisible ? 1 : 0);
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
            _graphView.ToggleGrid(_makeGridVisible);




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
            if (_currentDialogueContainer)
            {
                SetLanguage(selectedLanguage);
                dialogueContainerLabel.text = _currentDialogueContainer.name;

                _saveLoad.Load(_currentDialogueContainer);
            }

        }


        public void Save()
        {
            if (_currentDialogueContainer)
                _saveLoad.Save(_currentDialogueContainer);
        }


        private void SetLanguage(LanguageType newLanguage)
        {
            languageDropdown.text = $"Language : {newLanguage.ToString()}";
            selectedLanguage = newLanguage;

            //Le graphView récupère toutes ses DialogueNodes et les traduit
            _graphView.ReloadLanguage();
        }


        #endregion
    }
}