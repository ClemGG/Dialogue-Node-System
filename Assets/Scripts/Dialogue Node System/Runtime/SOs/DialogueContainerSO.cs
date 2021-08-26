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

        public List<StartData> StartDatas = new List<StartData>();
        public List<BackgroundData> BackgroundDatas = new List<BackgroundData>();
        public List<CharacterData> CharacterDatas = new List<CharacterData>();
        public List<RepliqueData> RepliqueDatas = new List<RepliqueData>();
        public List<BranchData> BranchDatas = new List<BranchData>();
        public List<ChoiceData> ChoiceDatas = new List<ChoiceData>();
        public List<DelayData> DelayDatas = new List<DelayData>();
        public List<EventData> EventDatas = new List<EventData>();
        public List<EndData> EndDatas = new List<EndData>();


#if UNITY_EDITOR

        //Garde en m�moire les Groups et les nodes qu'ils contiennent
        public List<GroupData> GroupDatas = new List<GroupData>();

        //Garde en m�moire les Notes
        public List<StickyNoteData> NoteDatas = new List<StickyNoteData>();

#endif

        //Utilis� dans DialogueGetData pour r�cup�rer la node suivante d'un Guid
        public List<BaseData> AllDatas
        {
            get
            {
                List<BaseData> tmp = new List<BaseData>();
                tmp.AddRange(StartDatas);
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