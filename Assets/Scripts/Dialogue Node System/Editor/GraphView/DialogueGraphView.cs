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

            //Zoom
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            //Utilities
            AddManipulators();
            AddCopyPasteCallbacks();    //Not yet implemented
            AddKeyboardAndMouseEvents();



            //The grid
            _grid = new GridBackground();
            Insert(0, _grid);
            _grid.StretchToParentSize();

            AddSearchWindow();
            CreateMinimap();

        }



        #region Editor Window

        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());      //Moves nodes on the grid
            this.AddManipulator(new SelectionDragger());    //Moves the selection
            this.AddManipulator(new RectangleSelector());   //To select items in a Rect
            this.AddManipulator(new FreehandSelector());    //To select in a free "Rect"
            this.AddManipulator(GraphBuilder.AddGroupContextualMenu(this));         //Add Groups
            this.AddManipulator(GraphBuilder.RemoveGroupContextualMenu(this));      //Remove Groups
            this.AddManipulator(GraphBuilder.AddStickyNoteContextualMenu(this));    //Add Notes
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
        /// Keeps the minimap in the top left corner of the window
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

            // Places the minimap in the top left corner of the window
            _miniMap.SetPosition(new Rect(0, 20, 200, 200));

            Add(_miniMap);

        }



        /// <summary>
        /// Add Node Search Window
        /// </summary>
        private void AddSearchWindow()
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Configure(Window, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }


        /// <summary>
        /// Translate all nodes in the graph when the language type is changed
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



        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            
            //We check if the port is eligible for connection
            ports.ForEach(compatiblePort =>
            {
                Port portView = compatiblePort;

                //The port can't connect with itself or the same node
                //An input port can only connect with an output port, and vice versa.
                //The ports must share the same color.
                if(startPort != portView && startPort.node != portView.node && startPort.direction != portView.direction && startPort.portColor == portView.portColor)
                {
                    compatiblePorts.Add(compatiblePort);
                }
            });

            return compatiblePorts;
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