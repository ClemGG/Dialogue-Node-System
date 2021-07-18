using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.NodeSystem
{

    //Cette classe nous permet de récupérer les données du DialogueContainerSO
    public class DialogueGetData : MonoBehaviour
    {
        [SerializeField] protected DialogueContainerSO dialogue;


        protected BaseData GetNodeByGuid(string guid)
        {
            return dialogue.AllDatas.Find(node => node.nodeGuid == guid);
        }
        
        
        //Pour savoir quelle node récupérer ensuite si on a plusieurs choix
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
    }
}