using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.NodeSystem
{

    /// <summary>
    /// Cette classe nous permet de r�cup�rer les donn�es du DialogueContainerSO.
    /// </summary>
    public abstract class DialogueGetData : MonoBehaviour
    {
        [SerializeField] protected DialogueContainerSO dialogue;

        #region Dialogue

        protected BaseData GetNodeByGuid(string guid)
        {
            return dialogue.AllDatas.Find(node => node.nodeGuid == guid);
        }
        
        
        //Pour savoir quelle node r�cup�rer ensuite si on a plusieurs choix
        protected BaseData GetNodeByNodePort(ChoicePort nodePort)
        {
            return dialogue.AllDatas.Find(node => node.nodeGuid == nodePort.inputGuid);
        }

        //Si la node n'a qu'une seule sortie, on cherche directement la node suivante
        protected BaseData GetNextNode(BaseData curNode)
        {
            LinkData nodeLinkData = dialogue.linkDatas.Find(edge => edge.baseNodeGuid == curNode.nodeGuid);
            return GetNodeByGuid(nodeLinkData.targetNodeGuid);
        }



        /// <summary>
        /// R�cup�re l'action � ex�cuter en fonction du type de node atteint.
        /// </summary>
        /// <param name="inputData">La node � convertir.</param>
        protected void CheckNodeType(BaseData inputData)
        {
            switch (inputData)
            {
                case StartData nodeData:
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
                case EndData nodeData:
                    RunNode(nodeData);
                    break;
                default:
                    break;
            }
        }



        protected abstract void StartDialogue();
        protected abstract void RunNode(StartData nodeData);
        protected abstract void RunNode(CharacterData nodeData);
        protected abstract void RunNode(RepliqueData nodeData);
        protected abstract void RunNode(BranchData nodeData);
        protected abstract void RunNode(ChoiceData nodeData);
        protected abstract void RunNode(EventData nodeData);
        protected abstract void RunNode(EndData nodeData);





        #endregion
    }
}