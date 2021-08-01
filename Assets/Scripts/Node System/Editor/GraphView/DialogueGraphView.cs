using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Project.NodeSystem.Editor 
{

    public class DialogueGraphView : GraphView
    {
        #region Fields

        private NodeSearchWindow _searchWindow;
        private MiniMap _miniMap;
        private const string _graphViewStyleSheet = "USS/GraphView/GraphViewStyleSheet";
        private Vector2 _minimapLastPos;
        private readonly GridBackground _grid;

        public DialogueEditorWindow Window { get; set; }


        #endregion



        public DialogueGraphView(DialogueEditorWindow window)
        {
            Window = window;

            StyleSheet styleSheet = Resources.Load<StyleSheet>(_graphViewStyleSheet);
            styleSheets.Add(styleSheet);

            //Définit les limites du zoom sur le graphe
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            //Des utilitaires d'Unity
            AddManipulators();
            AddCopyPasteCallbacks();
            AddKeyboardAndMouseEvents();



            //La grille (couleurs définies dans la styleSheet)
            _grid = new GridBackground();
            Insert(0, _grid);
            _grid.StretchToParentSize();

            AddSearchWindow();
            CreateMinimap();

        }



        #region Editor Window

        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());      //Pour déplacer les nodes sur la grille
            this.AddManipulator(new SelectionDragger());    //Pour déplacer la sélection
            this.AddManipulator(new RectangleSelector());   //Pour sélectionner dans un Rect
            this.AddManipulator(new FreehandSelector());    //Pour sélectionner une seule node
            this.AddManipulator(GraphBuilder.AddGroupContextualMenu(this));  //Pour ajouter des Groupes
            this.AddManipulator(GraphBuilder.RemoveGroupContextualMenu(this));  //Pour supprimer des Groupes
            this.AddManipulator(GraphBuilder.AddStickyNoteContextualMenu(this));  //Pour ajouter des Notes
        }



        private void AddCopyPasteCallbacks()
        {
            serializeGraphElements = CopySelection;
            canPasteSerializedData = CanPasteSelection;
            unserializeAndPaste = PasteSelection;
        }

        private void AddKeyboardAndMouseEvents()
        {
            RegisterCallback<KeyDownEvent>(KeyDownCallback);
            RegisterCallback<DragPerformEvent>(DragPerformedCallback);
            RegisterCallback<DragUpdatedEvent>(DragUpdatedCallback);
            RegisterCallback<MouseDownEvent>(MouseDownCallback);
            RegisterCallback<MouseUpEvent>(MouseUpCallback);
        }


        /// <summary>
        /// Appelée depuis la fenêtre d'éditeur pour garder la carte en haut à gauche de l'écran
        /// </summary>
        public void UpdateMinimap()
        {
            if (_minimapLastPos != _miniMap.GetPosition().position)
            {
                _miniMap.SetPosition(new Rect(0, 20, 200, 200));
                _minimapLastPos = _miniMap.GetPosition().position;
            }
        }


        private void CreateMinimap()
        {
            _miniMap = new MiniMap { anchored = true };

            //Place la minimap en haut à fauche de l'écran
            _miniMap.SetPosition(new Rect(0, 20, 200, 200));
            Add(_miniMap);

        }



        /// <summary>
        /// Ajoute le menu de recherche des nodes à la fenêtre (Espace ou clic droit pour y accéder).
        /// </summary>
        private void AddSearchWindow()
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Configure(Window, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }


        /// <summary>
        /// Appelée depuis la fenêtre d'editeur pour récupérer toutes ses nodes dépendant de la langue et les traduire
        /// </summary>
        public void ReloadLanguage()
        {
            List<BaseNode> allLanguageNodes = nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();
            foreach (BaseNode languageNode in allLanguageNodes)
            {
                languageNode.ReloadLanguage();
            }
        }



        public void ToggleGrid(bool makeGridVisible)
        {
            string hideUssClass = "Hide";
            if (makeGridVisible)
            {
                _grid.RemoveFromClassList(hideUssClass);
            }
            else
            {
                _grid.AddToClassList(hideUssClass);
            }
        }



        #endregion




        #region Commands


        /// <summary>
        /// Appelle la même fonction que le bouton Load du DialogueEditorWindow, mais ajoute le dialogue à charger en additif
        /// quand il est glissé-déposé dans la fenêtre, et sélectionne les nouveaux éléments chargés.
        /// </summary>
        /// <param name="dialogueContainerSO"></param>
        /// <param name="selectLoadedElements"></param>
        public void LoadAdditive(DialogueContainerSO dialogueContainerSO, bool selectLoadedElements = false)
        {
            
        }




        /// <summary>
        /// Pour pouvoir dire au GraphView quelles nodes peuvent être connectées à quel port
        /// </summary>
        /// <param name="startPort">Le port qu'on veut connecter</param>
        /// <param name="nodeAdapter">Pour connecter des nodes de différent types (On n'en a pas donc on s'en fout)</param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            
            //On regarde si le port demandé est éligible pour la connexion
            ports.ForEach(compatiblePort =>
            {
                Port portView = compatiblePort;

                //D'abord, on dit au port qu'il ne peut pas se connecter avec lui-même, ou à un port de la même node.
                //Ensuite, on lui dit qu'un port d'entrée ne peut se connecter qu'avec un port de sortie, et vice-versa.
                //Les ports ne peuvent aussi se connecter qu'avec un port de la même couleur.
                if(startPort != portView && startPort.node != portView.node && startPort.direction != portView.direction && startPort.portColor == portView.portColor)
                {
                    compatiblePorts.Add(compatiblePort);
                }
            });

            return compatiblePorts; //Renvoie les ports pouvant se connecter au nôtre
        }



        #endregion


        #region Callbacks


        #region Copy Paste

        protected override bool canCopySelection
        {
            get { return selection.Any(e => e is BaseNode || e is Group || e is StickyNote); }
        }

        protected override bool canCutSelection
        {
            get { return selection.Any(e => e is BaseNode || e is Group || e is StickyNote); }
        }



        private string CopySelection(IEnumerable<GraphElement> elements)
        {
            return string.Empty;
        }

        private bool CanPasteSelection(string serializedData)
        {
            return false;
        }

        private void PasteSelection(string operationName, string serializedData)
        {

        }

        #endregion


        #region Keyboard and Mouse Events


        private void KeyDownCallback(KeyDownEvent e)
        {
            if (e.actionKey) 
            {
                switch (e.keyCode)
                {
                    case KeyCode.S:
                        Window.Save();
                        e.StopPropagation();
                        break;
                    case KeyCode.L:
                        Window.Load();
                        e.StopPropagation();
                        break;

                }
            }
            else if (e.shiftKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.G:
                        GraphBuilder.RemoveGroup(this);
                        break;

                }
            }
            else
            {
                switch (e.keyCode)
                {
                    case KeyCode.G:
                        AddElement(GraphBuilder.AddGroup(this, "Group", this.GetLocalMousePosition(e.originalMousePosition)));
                        break;
                    case KeyCode.N:
                        AddElement(GraphBuilder.AddStickyNote("Note", "<i>Write your text here...</i>", this.GetLocalMousePosition(e.originalMousePosition)));
                        break;

                }
            }

        }

        private void MouseUpCallback(MouseUpEvent e)
        {

        }

        private void MouseDownCallback(MouseDownEvent e)
        {

        }

        private void DragPerformedCallback(DragPerformEvent e)
        {
            //var mousePos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, e.localMousePosition);

            DragAndDrop.AcceptDrag();


            // External objects drag and drop
            if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
            {
                //Undo.RegisterCompleteObjectUndo(contentContainer, "Create Node From Object(s)");
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    switch (obj)
                    {
                        case DialogueContainerSO dialogueContainerSO:
                            LoadAdditive(dialogueContainerSO, true);
                            break;

                        default:
                            break;
                    }
                }
            }

            Event.current.Use();

        }

        private void DragUpdatedCallback(DragUpdatedEvent e)
        {
            //var mousePos = (e.currentTarget as VisualElement).ChangeCoordinatesTo(contentViewContainer, e.localMousePosition);


            // External objects drag and drop
            if (DragAndDrop.paths.Length == DragAndDrop.objectReferences.Length)
            {
                //Undo.RegisterCompleteObjectUndo(contentContainer, "Create Node From Object(s)");
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    switch (obj)
                    {
                        case DialogueContainerSO dialogueContainerSO:

                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            break;

                        default:
                            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                            break;
                    }
                }
            }

            Event.current.Use();
        }


        #endregion



        #endregion

    }
}