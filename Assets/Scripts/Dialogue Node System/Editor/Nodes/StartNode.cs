using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.NodeSystem.Editor
{

    public class StartNode : BaseNode
    {
        #region Fields

        public StartData StartData { get; set; } = new StartData();

        #endregion


        #region Constructors

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

        #endregion


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
                        startNode.ReloadFields();
                    }
                });
            };
            StartData.Toggle = NodeBuilder.NewToggle("Default", boxContainer, StartData.isDefault, onToggled);
        }



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
        /// <param name="stringEvent"></param>
        public void AddCondition(EventData_StringEventCondition stringEvent = null)
        {
            NodeBuilder.AddStringConditionEvent(this, StartData.StringConditions, stringEvent);
        }

        #endregion



        #region On Load

        public override void ReloadFields()
        {
            if(StartData.Toggle != null)
            {
                StartData.Toggle.SetValueWithoutNotify(StartData.isDefault.Value);
            }
        }

        #endregion
    }
}