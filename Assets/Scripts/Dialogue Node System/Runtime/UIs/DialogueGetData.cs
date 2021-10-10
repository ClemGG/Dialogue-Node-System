using UnityEngine;

namespace Project.NodeSystem
{

    /// <summary>
    /// Allows us to get all datas contained in a DialogueContainerSO.
    /// </summary>
    public abstract class DialogueGetData : MonoBehaviour
    {
        protected DialogueContainerSO _dialogue;

        #region Dialogue

        protected BaseData GetNodeByGuid(string guid)
        {
            return _dialogue.AllDatas.Find(data => data.NodeGuid == guid);
        }


        //(Obsolete) To know which node link follow if we have multiple choices
        //protected BaseData GetNodeByNodePort(ChoicePort nodePort)
        //{
        //    return _dialogue.AllDatas.Find(data => data.NodeGuid == nodePort.InputGuid);
        //}


        /// <summary>
        /// Once a nodeData has been analyzed, this method goes to the next one.
        /// </summary>
        /// <returns></returns>
        protected BaseData GetNextNode(BaseData curNode)
        {
            LinkData nodeLinkData = _dialogue.linkDatas.Find(edge => edge.BaseNodeGuid == curNode.NodeGuid);
            return GetNodeByGuid(nodeLinkData.TargetNodeGuid);
        }



        /// <summary>
        /// Executes different actions depending on the type of the reached node.
        /// </summary>
        /// <param name="inputData">The node to convert.</param>
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