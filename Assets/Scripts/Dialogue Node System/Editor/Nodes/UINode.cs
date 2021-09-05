using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class UINode : BaseNode
    {
        #region Fields

        public UIData UIData { get; set; } = new UIData();

        #endregion


        #region Constructors

        public UINode() : base("UI", Vector2.zero)
        {

        }

        public UINode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("UI", position, window, graphView, "UINodeStyleSheet")
        {
            //La StartNode possède un port d'entrée dans le cas où l'on voudrait relancer le dialogue depuis le début
            //(incluant le nettoyage dans la Start() du DialogueManager)
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Output", Direction.Output);

            AddFields();


            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }

        #endregion


        #region Fields

        private void AddFields()
        {
            Box boxContainer = NodeBuilder.NewBox(this, "BoxRow");
            UIData.Toggle = NodeBuilder.NewToggle("Show Dialogue UI", boxContainer, UIData.show, "ToggleLabel");
        }


        #endregion



        #region On Load

        public override void ReloadFields()
        {
            if(UIData.Toggle != null)
            {
                UIData.Toggle.SetValueWithoutNotify(UIData.show.Value);
            }
        }

        #endregion
    }
}