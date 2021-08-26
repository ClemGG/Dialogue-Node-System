using UnityEngine;
using UnityEngine.Serialization;

namespace Project.NodeSystem
{
    //Indique quelles données de la node doivent être sauvegardées en fonction
    //de son type.
    [System.Serializable]
    public class BaseData
    {
        public string NodeGuid;
        public Vector2 Position;
    }
}