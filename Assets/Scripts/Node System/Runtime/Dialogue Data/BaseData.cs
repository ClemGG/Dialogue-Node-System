using UnityEngine;

namespace Project.NodeSystem
{
    //Indique quelles données de la node doivent être sauvegardées en fonction
    //de son type.
    [System.Serializable]
    public class BaseData
    {
        public string nodeGuid;
        public Vector2 position;

    }
}