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

        //Pour d�coller les suggestions du bord de la fen�tre
        private Texture2D empty;


        public void Configure(DialogueEditorWindow window, DialogueGraphView graphView)
        {
            this.window = window;
            this.graphView = graphView;

            //Pour d�coller les suggestions du bord de la fen�tre
            empty = new Texture2D(1, 1);
            empty.SetPixel(0, 0, Color.clear);
            empty.Apply();
        }


        //Cr�e un menu de recherche o� l'on peut choisir le type de node � ajouter au graphe
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            List<SearchTreeEntry> tree = new List<SearchTreeEntry>
            {
                //Affiche "Dialogue" en menu ppal, et "Dialogue Nodes" en sous-menu
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),

                //Les commandes affich�es dans le sous-menu
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


        //Une fois l'action s�lectionn�e, on place la node correspondante � la position de la souris
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            //Position de la souris sur l'�cran
            Vector2 mousePos = window.rootVisualElement.ChangeCoordinatesTo
                (
                    window.rootVisualElement.parent, context.screenMousePosition - window.position.position
                );

            //Convertit mousePos pour l'adapter au Rect de la fen�tre
            Vector2 graphMousePos = graphView.contentViewContainer.WorldToLocal(mousePos);

            return CheckForNodeType(searchTreeEntry, graphMousePos);
        }


        //Cr�e la node correspondant au type de la commande entr�e
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