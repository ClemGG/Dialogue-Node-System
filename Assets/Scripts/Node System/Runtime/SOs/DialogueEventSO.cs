using UnityEngine;
using UnityEngine.Events;

namespace Project.NodeSystem
{
    /// <summary>
    /// La classe parente de tous les DialogueEvents activés lors de la lecture du dialogue.
    /// </summary>
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Event", fileName = "New Dialogue Event")]
    [System.Serializable]
    public class DialogueEventSO : ScriptableObject
    {
        public UnityAction onDialogueEventCalled;

        public void Invoke()
        {
            if(onDialogueEventCalled != null)
            {
                onDialogueEventCalled.Invoke();
            }
            else
            {
                Debug.LogError("DialogueEventSO was called but nothing was subscribed to it.");
            }
        }
    }
}
