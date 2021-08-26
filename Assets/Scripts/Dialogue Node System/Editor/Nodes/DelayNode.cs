using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{
    public class DelayNode : BaseNode
    {
        #region Fields

        public DelayData DelayData { get; set; } = new DelayData();

        #endregion


        #region Constructors

        public DelayNode() : base("Delay", Vector2.zero)
        {

        }

        public DelayNode(Vector2 position, DialogueEditorWindow window , DialogueGraphView graphView) : base("Delay", position, window, graphView, "DelayNodeStyleSheet")
        {

            //La StartNode possède un port d'entrée dans le cas où l'on voudrait relancer le dialogue depuis le début
            //(incluant le nettoyage dans la Start() du DialogueManager)
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Start", Direction.Output);


            AddDelayFloatField();


            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }


        #endregion


        #region UI Fields

        private void AddDelayFloatField()
        {
            Box boxContainer = NodeBuilder.NewBox(this, "BoxRow");
            DelayData.FloatField = NodeBuilder.NewFloatField("Delay", boxContainer, DelayData.Delay, new Vector2(0f, int.MaxValue), "FloatField");
        }

        #endregion



        #region On Load

        public override void ReloadFields()
        {
            if (DelayData.FloatField != null)
            {
                DelayData.FloatField.SetValueWithoutNotify(DelayData.Delay.Value);
            }
        }

        #endregion
    }
}