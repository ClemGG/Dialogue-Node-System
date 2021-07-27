using System.Collections.Generic;
using UnityEngine;

namespace Project.NodeSystem
{

    //Contient l'ensemble des nodes d'un graphe
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue", fileName = "New Dialogue")]
    [System.Serializable]
    public class DialogueContainerSO : ScriptableObject
    {
        public List<LinkData> linkDatas = new List<LinkData>();

        public List<StartData> startDatas = new List<StartData>();
        public List<CharacterData> characterDatas = new List<CharacterData>();
        public List<RepliqueData> repliqueDatas = new List<RepliqueData>();
        public List<BranchData> branchDatas = new List<BranchData>();
        public List<ChoiceData> choiceDatas = new List<ChoiceData>();
        public List<EventData> eventDatas = new List<EventData>();
        public List<EndData> endDatas = new List<EndData>();

        public List<BaseData> AllDatas
        {
            get
            {
                List<BaseData> tmp = new List<BaseData>();
                tmp.AddRange(startDatas);
                tmp.AddRange(characterDatas);
                tmp.AddRange(repliqueDatas);
                tmp.AddRange(branchDatas);
                tmp.AddRange(choiceDatas);
                tmp.AddRange(eventDatas);
                tmp.AddRange(endDatas);

                return tmp;
            }
        }

    }




}