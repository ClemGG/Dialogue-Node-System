using UnityEngine.Events;

namespace Project.NodeSystem
{
    public class DialogueButtonContainer
    {
        public UnityAction OnChoiceClicked { get; set; }
        public string Text { get; set; }
        public bool ConditionCheck { get; set; }
        public ChoiceStateType ChoiceState { get; set; }
    }
}