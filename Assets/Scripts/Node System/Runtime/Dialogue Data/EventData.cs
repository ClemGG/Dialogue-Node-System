using System.Collections.Generic;
using UnityEngine;

namespace Project.NodeSystem
{


    [System.Serializable]
    public class EventData : BaseData
    {
        //Contenir les StringEvents et les ScriptableEvents dans une seule liste nous permet de les trier par ID
        //et de les jouer dans un certain ordre
        public List<NodeData_BaseContainer> events = new List<NodeData_BaseContainer>();
        public List<EventData_StringEventModifier> stringEvents = new List<EventData_StringEventModifier>();
        public List<EventData_ScriptableEvent> scriptableEvents = new List<EventData_ScriptableEvent>();

        readonly List<NodeData_BaseContainer> sortedEvents = new List<NodeData_BaseContainer>();

        //Les events triés pour les MonoBehaviours
        public List<NodeData_BaseContainer> SortedEvents
        {
            get
            {
                sortedEvents.Clear();
                sortedEvents.AddRange(stringEvents);
                sortedEvents.AddRange(scriptableEvents);

                sortedEvents.Sort(delegate (NodeData_BaseContainer x, NodeData_BaseContainer y)
                {
                    return x.ID.value.CompareTo(y.ID.value);
                });

                return sortedEvents;
            }
        }


    }


}
