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

            //D�finit les limites du zoom sur le graphe
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            //Des utilitaires d'Unity
            AddManipulators();
            AddCopyPasteCallbacks();
            AddKeyboardAndMouseEvents();



            //La grille (couleurs d�finies dans la styleSheet)
            _grid = new GridBackground();
            Insert(0, _grid);
            _grid.StretchToParentSize();

            AddSearchWindow();
            CreateMinimap();

        }



        #region Editor Window

        private void AddManipulators()
        {
            this.AddManipulator(new ContentDragger());      //Pour d�placer les nodes sur la grille
            this.AddManipulator(new SelectionDragger());    //Pour d�placer la s�lection
            this.AddManipulator(new RectangleSelector());   //Pour s�lectionner dans un Rect
            this.AddManipulator(new FreehandSelector());    //Pour s�lectionner une seule node
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
        /// Appel�e depuis la fen�tre d'�diteur pour garder la carte en haut � gauche de l'�cran
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

            //Place la minimap en haut � fauche de l'�cran
            _miniMap.SetPosition(new Rect(0, 20, 200, 200));
            Add(_miniMap);

        }



        /// <summary>
        /// Ajoute le menu de recherche des nodes � la fen�tre (Espace ou clic droit pour y acc�der).
        /// </summary>
        private void AddSearchWindow()
        {
            _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            _searchWindow.Configure(Window, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }


        /// <summary>
        /// Appel�e depuis la fen�tre d'editeur pour r�cup�rer toutes ses nodes d�pendant de la langue et les traduire
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
        /// Appelle la m�me fonction que le bouton Load du DialogueEditorWindow, mais ajoute le dialogue � charger en additif
        /// quand il est gliss�-d�pos� dans la fen�tre, et s�lectionne les nouveaux �l�ments charg�s.
        /// </summary>
        /// <param name="dialogueContainerSO"></param>
        /// <param name="selectLoadedElements"></param>
        public void LoadAdditive(DialogueContainerSO dialogueContainerSO, bool selectLoadedElements = false)
        {
            
        }




        /// <summary>
        /// Pour pouvoir dire au GraphView quelles nodes peuvent �tre connect�es � quel port
        /// </summary>
        /// <param name="startPort">Le port qu'on veut connecter</param>
        /// <param name="nodeAdapter">Pour connecter des nodes de diff�rent types (On n'en a pas donc on s'en fout)</param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            
            //On regarde si le port demand� est �ligible pour la connexion
            ports.ForEach(compatiblePort =>
            {
                Port portView = compatiblePort;

                //D'abord, on dit au port qu'il ne peut pas se connecter avec lui-m�me, ou � un port de la m�me node.
                //Ensuite, on lui dit qu'un port d'entr�e ne peut se connecter qu'avec un port de sortie, et vice-versa.
                //Les ports ne peuvent aussi se connecter qu'avec un port de la m�me couleur.
                if(startPort != portView && startPort.node != portView.node && startPort.direction != portView.direction && startPort.portColor == portView.portColor)
                {
                    compatiblePorts.Add(compatiblePort);
                }
            });

            return compatiblePorts; //Renvoie les ports pouvant se connecter au n�tre
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