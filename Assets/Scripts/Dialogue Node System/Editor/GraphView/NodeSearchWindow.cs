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

        //To move the suggestions away from the border of the window
        private Texture2D _empty;


        public void Configure(DialogueEditorWindow window, DialogueGraphView graphView)
        {
            this._window = window;
            this._graphView = graphView;

            //To move the suggestions away from the border of the window
            _empty = new Texture2D(1, 1);
            _empty.SetPixel(0, 0, Color.clear);
            _empty.Apply();
        }


        //Creates the Search menu where you can search for nodes by type
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {



            List<SearchTreeEntry> tree = new List<SearchTreeEntry>
            {
                //Displays "Dialogue" in the main menu, and "Dialogue Nodes" as a sub-menu
                new SearchTreeGroupEntry(new GUIContent("Dialogue"), 0),
                new SearchTreeGroupEntry(new GUIContent("Dialogue Nodes"), 1),

            };

            //The nodes displayed in the submenu
            AddNodeEntries(tree);
            //Add Group Command
            AddGroupEntry(tree);
            //Add StikyNote Command
            AddStickyNoteEntry(tree);

            return tree;
        }


        #region Entries

        /// <summary>
        /// Adds all node types in the same place to not have to add them manually in the constructor
        /// </summary>
        /// <param name="tree"></param>
        private void AddNodeEntries(List<SearchTreeEntry> tree)
        {
            //Grabs all node types deriving from BaseNode

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

        //Once the command selected, place the node at the mouse's position
        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            Vector2 mousePos = _window.rootVisualElement.ChangeCoordinatesTo
                (
                    _window.rootVisualElement.parent, context.screenMousePosition - _window.position.position
                );

            Vector2 graphMousePos = _graphView.contentViewContainer.WorldToLocal(mousePos);

            return CheckForNodeType(searchTreeEntry, graphMousePos);
        }


        //Creates the node and adds it to the GraphView
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