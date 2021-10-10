using System.Collections.Generic;
using UnityEngine;

namespace Project.NodeSystem
{

    //Contains all nodes in a graph
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue", fileName = "New Dialogue")]
    [System.Serializable]
    public class DialogueContainerSO : ScriptableObject
    {
        public List<LinkData> linkDatas = new List<LinkData>();

        public List<StartData> StartDatas = new List<StartData>();
        public List<UIData> UIDatas = new List<UIData>();
        public List<BackgroundData> BackgroundDatas = new List<BackgroundData>();
        public List<CharacterData> CharacterDatas = new List<CharacterData>();
        public List<RepliqueData> RepliqueDatas = new List<RepliqueData>();
        public List<BranchData> BranchDatas = new List<BranchData>();
        public List<ChoiceData> ChoiceDatas = new List<ChoiceData>();
        public List<DelayData> DelayDatas = new List<DelayData>();
        public List<EventData> EventDatas = new List<EventData>();
        public List<EndData> EndDatas = new List<EndData>();


#if UNITY_EDITOR

        //Stores groupes and the Guids they contain
        public List<GroupData> GroupDatas = new List<GroupData>();

        //Stores Sticky Notes
        public List<StickyNoteData> NoteDatas = new List<StickyNoteData>();

#endif

        //Used in DialogueGetData pour reach for the next node by Guid
        public List<BaseData> AllDatas
        {
            get
            {
                List<BaseData> tmp = new List<BaseData>();
                tmp.AddRange(StartDatas);
                tmp.AddRange(UIDatas);
                tmp.AddRange(BackgroundDatas);
                tmp.AddRange(CharacterDatas);
                tmp.AddRange(RepliqueDatas);
                tmp.AddRange(BranchDatas);
                tmp.AddRange(ChoiceDatas);
                tmp.AddRange(EventDatas);
                tmp.AddRange(DelayDatas);
                tmp.AddRange(EndDatas);

                return tmp;
            }
        }

    }




}