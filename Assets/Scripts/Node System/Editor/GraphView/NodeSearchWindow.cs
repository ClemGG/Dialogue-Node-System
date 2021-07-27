using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueEditorWindow window;
        private DialogueGraphView graphView;

        //Pour décoller les suggestions du bord de la fenêtre
        private Texture2D empty;


        public void Configure(DialogueEditorWindow window, DialogueGraphView graphView)
        {
            this.window = window;
            this.graphView = graphView;

            //Pour décoller les suggestions du bord de la fenêtre
            empty = new Texture2D(1, 1);
            empty.SetPixel(0, 0, Color.clear);
            empty.Apply();
        }


        //Crée un menu de recherche où l'on peut choisir le type de node à ajouter au graphe
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>
            {
                //Affiche "Dialogue" en menu ppal, et "Dialogue Nodes" en sous-menu
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),

                //Les commandes affichées dans le sous-menu
                AddNodeSearch("Start Node", new StartNode()),
                AddNodeSearch("Character Node", new CharacterNode()),
                AddNodeSearch("Replique Node", new RepliqueNode()),
                AddNodeSearch("Branch Node", new BranchNode()),
                AddNodeSearch("Choice Node", new ChoiceNode()),
                AddNodeSearch("Event Node", new EventNode()),
                AddNodeSearch("End Node", new EndNode()),
            };

            return tree;
        }

        private SearchTreeEntry AddNodeSearch(string name, BaseNode baseNode)
        {
            SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(name, empty))
            {
                level = 2,
                userData = baseNode
            };

            return tmp;
        }


        //Une fois l'action sélectionnée, on place la node correspondante à la position de la souris
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            //Position de la souris sur l'écran
            Vector2 mousePos = window.rootVisualElement.ChangeCoordinatesTo
                (
                    window.rootVisualElement.parent, context.screenMousePosition - window.position.position
                );

            //Convertit mousePos pour l'adapter au Rect de la fenêtre
            Vector2 graphMousePos = graphView.contentViewContainer.WorldToLocal(mousePos);

            return CheckForNodeType(searchTreeEntry, graphMousePos);
        }


        //Crée la node correspondant au type de la commande entrée
        private bool CheckForNodeType(SearchTreeEntry searchTreeEntry, Vector2 pos)
        {
            if (searchTreeEntry.userData is BaseNode)
            {
                graphView.AddElement(graphView.CreateNode(searchTreeEntry.userData.GetType(), pos));
                return true;
            }

            return false;
        }
    }
}