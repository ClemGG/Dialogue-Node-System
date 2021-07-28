using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public static class GraphBuilder
    {


        #region Manipulators

        /// <summary>
        /// Ajoute un onglet "Add Group" dans le menu contextuel (clic droit)
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
        /// Ajoute un onglet "Remove Group" dans le menu contextuel (clic droit)
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
        /// Ajoute un onglet "Add Sticky Node" dans le menu contextuel (clic droit)
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
        /// Crée un Group dans la GraphView
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

            //Si on a sélectionné des nodes au moment de créer un groupe, on les ajoute à ce groupe
            if(graphView.selection.Count > 0)
            {
                AddSelectionToGroup(group, graphView.selection);
            }

            //Change la couleur du groupe dans la Minimap
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
        /// Retire les contenus des Groupes sélectionnés et supprime ces derniers de la GraphView
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
        /// Crée une StickyNote dans la GraphView
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

            //Change la couleur du groupe dans la Minimap
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

        /// <summary>
        /// Quand on charge un nouveau dialogue en additif, on change tous ses guids pour éviter les conflits de connexion
        /// </summary>
        /// <param name="loadedElements"></param>
        public static void ChangerAllNodesAndConnectionsGuids(DialogueContainerSO dialogueContainerSO, List<GraphElement> loadedElements)
        {

        }



        #endregion





        #region Utilities

        public static Vector2 GetLocalMousePosition(this DialogueGraphView graphView, Vector2 localMousePosition)
        {
            //Convertit localMousePosition pour l'adapter au Rect de la fenêtre
            Vector2 graphMousePos = graphView.contentViewContainer.WorldToLocal(localMousePosition);

            return graphMousePos;
        }

        #endregion
    }
}