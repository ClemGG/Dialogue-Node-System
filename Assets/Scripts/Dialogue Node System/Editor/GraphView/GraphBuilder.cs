using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public static class GraphBuilder
    {


        #region Manipulators

        /// <summary>
        /// Adds a "Add Group" option in the contextual menu (right click)
        /// </summary>
        /// <returns></returns>
        public static IManipulator AddGroupContextualMenu(DialogueGraphView graphView)
        {
            ContextualMenuManipulator cmm = new ContextualMenuManipulator
                (
                    menuEvent => menuEvent.menu.AppendAction("Add Group _g", actionEvent => graphView.AddElement(AddGroup(graphView, "Group", graphView.GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );


            return cmm;
        }


        /// <summary>
        /// Adds a "Remove Group" option in the contextual menu (right click)
        /// </summary>
        /// <returns></returns>
        public static IManipulator RemoveGroupContextualMenu(DialogueGraphView graphView)
        {
            ContextualMenuManipulator cmm = new ContextualMenuManipulator
                (
                    menuEvent => menuEvent.menu.AppendAction("Remove Group #_g", actionEvent => RemoveGroup(graphView))
                );


            return cmm;
        }




        /// <summary>
        /// Adds a "Add Sticky" option in the contextual menu (right click)
        /// </summary>
        /// <returns></returns>
        public static IManipulator AddStickyNoteContextualMenu(DialogueGraphView graphView)
        {
            ContextualMenuManipulator cmm = new ContextualMenuManipulator
                (
                    menuEvent => menuEvent.menu.AppendAction("Add Sticky Note _n", actionEvent => graphView.AddElement(AddStickyNote("Note", "<i>Write your text here...</i>", graphView.GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
                );


            return cmm;
        }



        #endregion




        #region Commands

        /// <summary>
        /// Creates a Group in the GraphView
        /// </summary>
        /// <param name="title"></param>
        /// <param name="localMousePosition"></param>
        /// <returns></returns>
        

        public static Group AddGroup(DialogueGraphView graphView, string title, Vector2 localMousePosition)
        {
            Group group = new Group
            {
                title = title
            };

            group.SetPosition(new Rect(localMousePosition, Vector2.zero));

            //If some nodes were already selected, add them to this Group
            if(graphView.selection.Count > 0)
            {
                AddSelectionToGroup(group, graphView.selection);
            }

            group.ChangeColorInMinimap("#33AACC");
            group.AddStyle("Group");

            return group;
        }

        public static void AddSelectionToGroup(Group group, List<ISelectable> selection)
        {
            List<GraphElement> elements = new List<GraphElement>();
            elements.AddRange(selection.Where(element => element is BaseNode).Cast<BaseNode>().ToList());

            group.AddElements(elements);

        }





        /// <summary>
        /// Remove the content of a Group and deletes this Group from the GraphView
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static void RemoveGroup(DialogueGraphView graphView)
        {
            List<Group> selectedGroups = graphView.selection.Where(element => element is Group).Cast<Group>().ToList();

            for (int i = 0; i < selectedGroups.Count; i++)
            {
                List<VisualElement> children = selectedGroups[i].Children().ToList();

                for (int j = 0; j < children.Count; j++)
                {
                    selectedGroups[i].Remove(children[j]);
                }

                graphView.RemoveElement(selectedGroups[i]);
            }
        }



        /// <summary>
        /// Creates a StickyNote in the GraphView
        /// </summary>
        /// <param name="title"></param>
        /// <param name="localMousePosition"></param>
        /// <returns></returns>
        public static StickyNote AddStickyNote(string title, string text, Vector2 localMousePosition)
        {
            StickyNote note = new StickyNote
            {
                title = title,
                theme = StickyNoteTheme.Black,
                contents = text,
            };

            note.SetPosition(new Rect(localMousePosition, new Vector2(200, 200)));


            note.ChangeColorInMinimap("#CCAA33");
            note.AddStyle("Note");

            return note;
        }
        
        
        
        
        /// <summary>
        /// Creates a StickyNote in the GraphView from an existing saved StickyNote
        /// </summary>
        /// <returns></returns>
        /// 
        public static StickyNote AddStickyNote(StickyNoteData data)
        {
            StickyNote note = new StickyNote
            {
                title = data.Title,
                theme = StickyNoteTheme.Black,
                contents = data.Content,
            };

            note.SetPosition(new Rect(data.Position, data.Size));


            note.ChangeColorInMinimap("#CCAA33");
            note.AddStyle("Note");

            return note;
        }





        #endregion






        #region Create Nodes


        public static T CreateNode<T>(this DialogueGraphView graphView, Vector2 pos) where T : BaseNode
        {
            return (T)Activator.CreateInstance(typeof(T), pos, graphView.Window, graphView);
        }

        public static BaseNode CreateNode(this DialogueGraphView graphView, Type t, Vector2 pos)
        {
            return (BaseNode)Activator.CreateInstance(t, pos, graphView.Window, graphView);
        }

        public static BaseNode CreateNode(this DialogueGraphView graphView, Type t)
        {
            return (BaseNode)Activator.CreateInstance(t);
        }


        #endregion





        #region Callbacks

        ///// <summary>
        ///// Quand on charge un nouveau dialogue en additif, on change tous ses guids pour éviter les conflits de connexion
        ///// </summary>
        ///// <param name="loadedElements"></param>
        //public static void ChangerAllNodesAndConnectionsGuids(DialogueContainerSO dialogueContainerSO, List<GraphElement> loadedElements)
        //{

        //}



        #endregion





        #region Utilities

        public static Vector2 GetLocalMousePosition(this DialogueGraphView graphView, Vector2 localMousePosition)
        {
            //Converts localMousePosition to adapt it to the window's rect
            Vector2 graphMousePos = graphView.contentViewContainer.WorldToLocal(localMousePosition);

            return graphMousePos;
        }

        #endregion
    }
}