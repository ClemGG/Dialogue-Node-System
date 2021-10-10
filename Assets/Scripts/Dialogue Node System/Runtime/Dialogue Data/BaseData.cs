using UnityEngine;
using UnityEngine.Serialization;

namespace Project.NodeSystem
{
    //Indicates which datas must be stored depending on the node's type.
    [System.Serializable]
    public class BaseData
    {
        public string NodeGuid;
        public Vector2 Position;
    }
}