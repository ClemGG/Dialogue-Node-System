using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Project.NodeSystem.Editor 
{

    public class DialogueGraphView : GraphView
    {
        private DialogueEditorWindow window;
        private NodeSearchWindow searchWindow;
        private MiniMap miniMap;
        private string graphViewStyleSheet = "USS/GraphView/GraphViewStyleSheet";
        private Vector2 minimapLastPos;
        private GridBackground grid;




        public DialogueGraphView(DialogueEditorWindow window)
        {
            this.window = window;

            StyleSheet styleSheet = Resources.Load<StyleSheet>(graphViewStyleSheet);
            styleSheets.Add(styleSheet);

            //D�finit les limites du zoom sur le graphe
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);


            //Des utilitaires d'Unity
            this.AddManipulator(new ContentDragger());      //Pour d�placer les nodes sur la grille
            this.AddManipulator(new SelectionDragger());    //Pour d�placer la s�lection
            this.AddManipulator(new RectangleSelector());   //Pour s�lectionner dans un Rect
            this.AddManipulator(new FreehandSelector());    //Pour s�lectionner une seule node

            //La grille (couleurs d�finies dans la styleSheet)
            grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddSearchWindow();
            CreateMinimap();
        }




        #region Editor Window

        /// <summary>
        /// Appel�e depuis la fen�tre d'�diteur pour garder la carte en haut � gauche de l'�cran
        /// </summary>
        public void UpdateMinimap()
        {
            if (minimapLastPos != miniMap.GetPosition().position)
            {
                miniMap.SetPosition(new Rect(0, 20, 200, 200));
                minimapLastPos = miniMap.GetPosition().position;
            }
        }


        private void CreateMinimap()
        {
            miniMap = new MiniMap { anchored = true };

            //Place la minimap en haut � fauche de l'�cran
            miniMap.SetPosition(new Rect(0, 20, 200, 200));
            Add(miniMap);

        }



        /// <summary>
        /// Ajoute le menu de recherche des nodes � la fen�tre (Espace ou clic droit pour y acc�der).
        /// </summary>
        private void AddSearchWindow()
        {
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            searchWindow.Configure(window, this);
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
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
                grid.RemoveFromClassList(hideUssClass);
            }
            else
            {
                grid.AddToClassList(hideUssClass);
            }
        }



        #endregion


        #region Commands



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
            ports.ForEach(port =>
            {
                Port portView = port;

                //D'abord, on dit au port qu'il ne peut pas se connecter avec lui-m�me, ou � un port de la m�me node.
                //Ensuite, on lui dit qu'un port d'entr�e ne peut se connecter qu'avec un port de sortie, et vice-versa.
                //Les ports ne peuvent aussi se connecter qu'avec un port de la m�me couleur.
                if(startPort != portView && startPort.node != portView.node && startPort.direction != portView.direction && startPort.portColor == portView.portColor)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts; //Renvoie les ports pouvant se connecter au n�tre
        }

        #endregion


        #region Create Nodes

        public T CreateNode<T>(Vector2 pos)
        {
            return (T)Activator.CreateInstance(typeof(T), pos, window, this);
        }

        public BaseNode CreateNode(Type t, Vector2 pos)
        {
            return (BaseNode)Activator.CreateInstance(t, pos, window, this);
        }

        
        #endregion
    }
}