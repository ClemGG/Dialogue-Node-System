using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueEditorWindow _window;
        private DialogueGraphView _graphView;

        //Pour d�coller les suggestions du bord de la fen�tre
        private Texture2D _empty;


        public void Configure(DialogueEditorWindow window, DialogueGraphView graphView)
        {
            this._window = window;
            this._graphView = graphView;

            //Pour d�coller les suggestions du bord de la fen�tre
            _empty = new Texture2D(1, 1);
            _empty.SetPixel(0, 0, Color.clear);
            _empty.Apply();
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
                //AddNodeSearch("Start Node", new StartNode()),
                //AddNodeSearch("Character Node", new CharacterNode()),
                //AddNodeSearch("Replique Node", new RepliqueNode()),
                //AddNodeSearch("Branch Node", new BranchNode()),
                //AddNodeSearch("Choice Node", new ChoiceNode()),
                //AddNodeSearch("Event Node", new EventNode()),
                //AddNodeSearch("End Node", new EndNode()),

                
                
            };

            //Les commandes affich�es dans le sous-menu
            AddEntries<BaseNode>(tree);

            //La commande d'ajout de Groupes
            tree.Add(AddGroupEntry());
            //La commande d'ajout de StickyNotes
            tree.Add(AddStickyNoteEntry());

            return tree;
        }


        #region Entries

        /// <summary>
        /// Ajoute tous les types de node en un seul endroit pour ne pas avoir � les rajouter individuellement dans le constructeur
        /// </summary>
        /// <param name="tree"></param>
        private void AddEntries<T>(List<SearchTreeEntry> tree) where T : BaseNode
        {
            //R�cup�re tous les types de nodes d�rivant de BaseNode
            List<Type> subclassTypes = Assembly
               .GetAssembly(typeof(T))
               .GetTypes()
               .Where(t => t.IsSubclassOf(typeof(T))).ToList();

            subclassTypes.Sort(delegate (Type x, Type y)
            {
                return x.Name.CompareTo(y.Name);
            });

            //Ajoute une commande � la liste pour chaque type de node d�rivant de BaseNode
            for (int i = 0; i < subclassTypes.Count; i++)
            {
                tree.Add(AddNodeEntry(subclassTypes[i].Name.Replace("Node", " Node"), _graphView.CreateNode(subclassTypes[i])));
            }
        }

        private SearchTreeEntry AddNodeEntry(string name, BaseNode baseNode)
        {
            SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(name, _empty))
            {
                level = 2,
                userData = baseNode
            };

            return tmp;
        }

        private SearchTreeEntry AddGroupEntry()
        {
            return new SearchTreeEntry(new GUIContent("Group", _empty))
            {
                level = 1,
                userData = new Group()
            };
        }

        private SearchTreeEntry AddStickyNoteEntry()
        {
            return new SearchTreeEntry(new GUIContent("Sticky Note", _empty))
            {
                level = 1,
                userData = new StickyNote()
            };
        }


        #endregion


        #region On Entry Selected

        //Une fois l'action s�lectionn�e, on place la node correspondante � la position de la souris
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            //Position de la souris sur l'�cran
            Vector2 mousePos = _window.rootVisualElement.ChangeCoordinatesTo
                (
                    _window.rootVisualElement.parent, context.screenMousePosition - _window.position.position
                );

            //Convertit mousePos pour l'adapter au Rect de la fen�tre
            Vector2 graphMousePos = _graphView.contentViewContainer.WorldToLocal(mousePos);

            return CheckForNodeType(searchTreeEntry, graphMousePos);
        }


        //Cr�e la node correspondant au type de la commande entr�e
        private bool CheckForNodeType(SearchTreeEntry searchTreeEntry, Vector2 pos)
        {
            switch (searchTreeEntry.userData)
            {
                case BaseNode node:
                    _graphView.AddElement(_graphView.CreateNode(node.GetType(), pos));
                    return true;

                case Group _:
                    _graphView.AddElement(GraphBuilder.AddGroup(_graphView, "Group", pos));
                    return true;

                case StickyNote _:
                    _graphView.AddElement(GraphBuilder.AddStickyNote("Note", "<i>Write your text here...</i>", pos));
                    return true;
            }

            return false;
        }


        #endregion
    }
}