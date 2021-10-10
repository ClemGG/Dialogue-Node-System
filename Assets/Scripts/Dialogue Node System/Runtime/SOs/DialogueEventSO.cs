using UnityEngine;
using UnityEngine.Events;

namespace Project.NodeSystem
{
    /// <summary>
    /// The root class DialogueEvents activated during a dialogue.
    /// Inherit from this class to implement more complex callbacks.
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Event", fileName = "New Dialogue Event")]
    [System.Serializable]
    public class DialogueEventSO : ScriptableObject
    {
        public UnityAction OnDialogueEventCalled;

        public void Invoke()
        {
            try
            {
                OnDialogueEventCalled.Invoke();
            }
            catch
            {
                Debug.LogError($"DialogueEventSO was called on \"{name}\" but nothing was subscribed to it.");
            }
        }
    }
}
