using UnityEngine;

namespace Project.NodeSystem
{
    [CreateAssetMenu(menuName = "Dialogue/New Dialogue Event/On Dialogue Event Called", fileName = "On Dialogue Event Called")]
    public class DE_TriggerSO : DialogueEventSO
    {
        public override void Invoke()
        {
            GameEvents.CalledDialogueRuntimeEvent();
            base.Invoke();
        }
    }
}