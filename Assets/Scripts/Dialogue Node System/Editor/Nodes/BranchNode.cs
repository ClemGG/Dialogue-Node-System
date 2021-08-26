using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class BranchNode : BaseNode
    {
        #region Fields

        public BranchData BranchData { get; set; } = new BranchData();

        #endregion



        #region Constructors

        //Constructeur vide pour la NodeSearchWindow
        public BranchNode() : base("Branch", Vector2.zero)
        {

        }

        public BranchNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Branch", position, window, graphView, "BranchNodeStyleSheet")
        {

            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "True", Direction.Output);
            NodeBuilder.AddPort(this, "False", Direction.Output);

            TopButton();

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }

        #endregion


        #region Methods



        /// <summary>
        /// Crée un bouton en haut de la node pour ajouter des conditions
        /// </summary>
        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "Add Condition", () => AddCondition(), "TopBtn");
        }

        /// <summary>
        /// Ajoute un champ contenant une nouvelle condition
        /// </summary>
        /// <param name="stringEvent">L'event à charger (si null, le script en crée un nouveau)</param>
        public void AddCondition(EventData_StringEventCondition stringEvent = null)
        {
            NodeBuilder.AddStringConditionEvent(this, BranchData.StringConditions, stringEvent);
        }


        #endregion

    }
}