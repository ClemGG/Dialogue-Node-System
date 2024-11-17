using System;
using Project.Utilities.Assets;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem.Editor
{
    /// <summary>
    /// Dialogue editor window
    /// </summary>
    public class DialogueEditorWindow : EditorWindow
    {
        #region Constantes

        /// <summary>
        /// TO : Move to Preferences tab
        /// </summary>
        private const string _graphViewStyleSheet = "USS/EditorWindow/EditorWindowStyleSheet";

        #endregion

        #region Propriétés

        /// <summary>
        /// The selected language in the editor window
        /// </summary>
        public LanguageType SelectedLanguage { get; set; } = LanguageType.French;

        #endregion

        #region Variables d'instance

        private DialogueContainerSO _currentDialogueContainer;

        private DialogueGraphView _graphView;

        private DialogueSaveLoad _saveLoad;

        private ToolbarMenu languageDropdown;

        private Label dialogueContainerLabel;

        private bool _isGridVisible = false;

        #endregion

        #region Méthodes Unity

        private void OnEnable()
        {
            CreateGraphView();
            CreateToolbar();

            // When Unity starts, we retrive the last dialogue stored in memory and we open it.
            // If the asset was moved while Unity was closed, AssetFinderUtilities will just return null, so we risk nothing

            if (!_currentDialogueContainer && PlayerPrefs.HasKey("lastDialogue"))
            {
                _currentDialogueContainer = AssetFinderUtilities.FindAssetAtPath<DialogueContainerSO>(PlayerPrefs.GetString("lastDialogue"));
            }

            Load();
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
            PlayerPrefs.SetInt("Editor_makeGridVisible", _isGridVisible ? 1 : 0);

            //Stores the instanceID to open the last editoed dialogue when Unity starts
            if (_currentDialogueContainer)
                PlayerPrefs.SetString("lastDialogue", AssetFinderUtilities.GetAssetPath(_currentDialogueContainer));
        }



        /// <summary>
        /// Open the dialogue in the Editor WIndow by double-clicking on it
        /// <returns></returns>
        [OnOpenAsset(1)]
        public static bool ShowWindow(int instanceID, int line)
        {
            //Stores the instanceID to open the last editoed dialogue when Unity starts
            PlayerPrefs.SetString("lastDialogue", AssetFinderUtilities.GetAssetPath(instanceID));

            //Retrieves the dialogue in the Asset folder by its ID
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
            Button exportButton = new Button(() => CSVCustomTools.SaveDialoguesToCSV()) { text = "Export CSV" };
            Button importButton = new Button(() => CSVCustomTools.LoadDialoguesFromCSV()) { text = "Import CSV" };


            //Hide/Show grid
            Label showGridLabel = new Label("Show Grid");
            showGridLabel.AddToClassList("showGridLabel");

            Toggle showGridToggle = new Toggle();
            _isGridVisible = PlayerPrefs.GetInt("Editor_makeGridVisible", _isGridVisible ? 1 : 0) == 1 ? true : false;
            showGridToggle.value = _isGridVisible;
            showGridToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                showGridToggle.value = _isGridVisible = evt.newValue;
                _graphView.ToggleGrid(_isGridVisible);
                PlayerPrefs.SetInt("Editor_makeGridVisible", _isGridVisible ? 1 : 0);
            });


            showGridLabel.AddToClassList("showGridLabel");

            //We activate the toggle once to synchronize it with its value
            _graphView.ToggleGrid(_isGridVisible);




            //Language dropdown
            languageDropdown = new ToolbarMenu();
            ForEach<LanguageType>(language =>
            {
                languageDropdown.menu.AppendAction(language.ToString(), new Action<DropdownMenuAction>(x => SetLanguage(language)));
            });

            dialogueContainerLabel = new Label();
            dialogueContainerLabel.AddToClassList("dialogueContainerName");

            toolbar.Add(dialogueContainerLabel);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(exportButton);
            toolbar.Add(importButton);
            toolbar.Add(showGridLabel);
            toolbar.Add(showGridToggle);
            toolbar.Add(languageDropdown);

            rootVisualElement.Add(toolbar);
        }

        #endregion

        #region Save & Load

        public void Load()
        {
            if (_currentDialogueContainer)
            {
                SetLanguage(SelectedLanguage);
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
            SelectedLanguage = newLanguage;

            //Translates all nodes in the graph
            _graphView.ReloadLanguage();
        }


        #endregion
    }
}