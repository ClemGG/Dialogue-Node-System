using UnityEngine;

namespace Project.NodeSystem
{

    [System.Serializable]
    public abstract class DialogueEventSO : ScriptableObject
    {
        public virtual void Invoke()
        {
            Debug.Log("Dialogue Event was called");
        }
    }
}
