using UnityEngine;


namespace Project.NodeSystem
{

    //Les propriétés doivent être publiques pour être modifiées par l'EventNode
    public class DE_Trigger : MonoBehaviour
    {
        public DialogueEventSO dialogueEvent;

        [SerializeField] private int money = 50;
        [SerializeField] private bool hasSomethingToDo = false;

        public int Money { get => money; set => money = value; }
        public bool HasSomethingToDo { get => hasSomethingToDo; set => hasSomethingToDo = value; }

        private void Awake()
        {
            dialogueEvent.onDialogueEventCalled += Print;
        }

        private void Print()
        {
            print($"L'event de DE_Trigger a bien été appelé. Money : {Money} ; HasSomethingToDo : {HasSomethingToDo}.");
        }

        private void OnDestroy()
        {
            dialogueEvent.onDialogueEventCalled -= Print;
        }
    }
}