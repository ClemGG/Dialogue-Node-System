using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class StartNode : BaseNode
    {
        private StartData startData = new StartData();
        public StartData StartData { get => startData; set => startData = value; }

        public StartNode() : base("Start", Vector2.zero)
        {

        }

        public StartNode(Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView) : base("Start", position, window, graphView, "StartNodeStyleSheet")
        {

            //La StartNode possède un port d'entrée dans le cas où l'on voudrait relancer le dialogue depuis le début
            //(incluant le nettoyage dans la Start() du DialogueManager)
            NodeBuilder.AddPort(this, "Input", Direction.Input, Port.Capacity.Multi);
            NodeBuilder.AddPort(this, "Start", Direction.Output);


            AddDefaultStartNodeMarker();
            TopButton();


            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
            RefreshPorts();
        }


        #region Fields


        /// <summary>
        /// Ajoute un toggle à la StartNode pour indiquer qu'elle est la node de départ par défaut si à true.
        /// </summary>
        private void AddDefaultStartNodeMarker()
        {
            Box boxContainer = NodeBuilder.NewBox(this, "BoxRow");

            //Quand on clique passe le toggle à true, on prend toutes les StartNodes qui ne sont pas celle-ci et on passe leurs toggles à false
            //de sorte à n'avoir qu'une seule node de départ par défaut
            Action onToggled = () =>
            {
                GraphView.nodes.ToList().Where(node => node is StartNode).Cast<StartNode>().ToList().ForEach(startNode =>
                {
                    if(startNode != this)
                    {
                        startNode.StartData.isDefault.Value = false;
                        startNode.LoadValueIntoField();
                    }
                });
            };
            StartData.Toggle = NodeBuilder.NewToggle(boxContainer, StartData.isDefault, onToggled, "Default", "Toggle");
        }



        /// <summary>
        /// Ajoute des conditions à la node
        /// </summary>
        private void TopButton()
        {
            NodeBuilder.NewTitleButton(this, "Add Condition", () => AddCondition(), "TopBtn");
        }


        /// <summary>
        /// Ajoute un champ contenant une nouvelle condition
        /// </summary>
        /// <param name="stringEvent"></param>
        public void AddCondition(EventData_StringEventCondition stringEvent = null)
        {
            NodeBuilder.AddStringConditionEvent(this, StartData.StringConditions, stringEvent);
        }

        #endregion



        #region On Load

        public override void LoadValueIntoField()
        {
            if(startData.Toggle != null)
            {
                startData.Toggle.SetValueWithoutNotify(startData.isDefault.Value);
            }
        }

        #endregion
    }
}