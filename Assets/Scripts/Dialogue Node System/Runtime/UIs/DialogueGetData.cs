using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.NodeSystem
{

    /// <summary>
    /// Cette classe nous permet de récupérer les données du DialogueContainerSO.
    /// </summary>
    public abstract class DialogueGetData : MonoBehaviour
    {
        [SerializeField] protected DialogueContainerSO dialogue;

        #region Dialogue

        protected BaseData GetNodeByGuid(string guid)
        {
            return dialogue.AllDatas.Find(node => node.NodeGuid == guid);
        }
        
        
        //Pour savoir quelle node récupérer ensuite si on a plusieurs choix
        protected BaseData GetNodeByNodePort(ChoicePort nodePort)
        {
            return dialogue.AllDatas.Find(node => node.NodeGuid == nodePort.InputGuid);
        }

        //Si la node n'a qu'une seule sortie, on cherche directement la node suivante
        protected BaseData GetNextNode(BaseData curNode)
        {
            LinkData nodeLinkData = dialogue.linkDatas.Find(edge => edge.BaseNodeGuid == curNode.NodeGuid);
            return GetNodeByGuid(nodeLinkData.TargetNodeGuid);
        }



        /// <summary>
        /// Récupère l'action à exécuter en fonction du type de node atteint.
        /// </summary>
        /// <param name="inputData">La node à convertir.</param>
        protected void CheckNodeType(BaseData inputData)
        {
            switch (inputData)
            {
                case StartData nodeData:
                    RunNode(nodeData);
                    break;
                case UIData nodeData:
                    RunNode(nodeData);
                    break;
                case BackgroundData nodeData:
                    RunNode(nodeData);
                    break;
                case CharacterData nodeData:
                    RunNode(nodeData);
                    break;
                case RepliqueData nodeData:
                    RunNode(nodeData);
                    break;
                case BranchData nodeData:
                    RunNode(nodeData);
                    break;
                case ChoiceData nodeData:
                    RunNode(nodeData);
                    break;
                case EventData nodeData:
                    RunNode(nodeData);
                    break;
                case DelayData nodeData:
                    RunNode(nodeData);
                    break;
                case EndData nodeData:
                    RunNode(nodeData);
                    break;
                default:
                    break;
            }
        }


        protected abstract void StartDialogue();
        protected abstract void EndDialogue();
        protected virtual void RunNode(StartData nodeData) { }
        protected virtual void RunNode(UIData nodeData) { }
        protected virtual void RunNode(BackgroundData nodeData) { }
        protected virtual void RunNode(CharacterData nodeData) { }
        protected virtual void RunNode(RepliqueData nodeData) { }
        protected virtual void RunNode(BranchData nodeData) { }
        protected virtual void RunNode(ChoiceData nodeData) { }
        protected virtual void RunNode(EventData nodeData) { }
        protected virtual void RunNode(DelayData nodeData) { }
        protected virtual void RunNode(EndData nodeData) { }





        #endregion
    }
}