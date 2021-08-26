using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace Project.NodeSystem
{
    [System.Serializable]
    public class StartData : BaseData
    {

#if UNITY_EDITOR
        public Toggle Toggle { get; set; }
#endif

        //Si à true, cette node sera la node de départ par défaut
        public ContainerValue<bool> isDefault = new ContainerValue<bool>();

        //Si ses conditions sont réunies, cette StartNode pourra être choisie au lancement du dialogue
        public List<EventData_StringEventCondition> StringConditions = new List<EventData_StringEventCondition>();



    }

}