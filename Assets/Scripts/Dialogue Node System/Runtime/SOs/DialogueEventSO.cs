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
