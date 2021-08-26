using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using static Project.Utilities.ValueTypes.Types;

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

            };

            //Les commandes affich�es dans le sous-menu
            AddNodeEntries(tree);
            //La commande d'ajout de Groupes
            AddGroupEntry(tree);
            //La commande d'ajout de StickyNotes
            AddStickyNoteEntry(tree);

            return tree;
        }


        #region Entries

        /// <summary>
        /// Ajoute tous les types de node en un seul endroit pour ne pas avoir � les rajouter individuellement dans le constructeur
        /// </summary>
        /// <param name="tree"></param>
        private void AddNodeEntries(List<SearchTreeEntry> tree)
        {
            //R�cup�re tous les types de nodes d�rivant de BaseNode

            Type[] allNodeTypes = SubclassesOf<BaseNode>();
            allNodeTypes.ForEach(type =>
            {
                tree.Add(AddNodeEntry(type.Name.Replace("Node", ""), _graphView.CreateNode(type)));
            });

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

        private void AddGroupEntry(List<SearchTreeEntry> tree)
        {
            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent("Group", _empty))
            {
                level = 1,
                userData = new Group()
            };

            tree.Add(entry);
        }

        private void AddStickyNoteEntry(List<SearchTreeEntry> tree)
        {
            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent("Sticky Note", _empty))
            {
                level = 1,
                userData = new StickyNote()
            };
            tree.Add(entry);
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