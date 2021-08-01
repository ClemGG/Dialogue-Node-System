using UnityEngine;


namespace Project.NodeSystem
{

    //Les propriétés doivent être publiques pour être modifiées par l'EventNode
    public class DE_Trigger : MonoBehaviour
    {
        public DialogueEventSO dialogueEvent;

        [ReadOnly, SerializeField] private int _money = 50;
        [ReadOnly, SerializeField] private bool _hasSomethingToDo = false;

        public int Money { get => _money; set => _money = value; }
        public bool HasSomethingToDo { get => _hasSomethingToDo; set => _hasSomethingToDo = value; }

        private void Awake()
        {
            dialogueEvent.OnDialogueEventCalled += Print;
        }

        private void Print()
        {
            //print($"L'event de DE_Trigger a bien été appelé. Money : {Money} ; HasSomethingToDo : {HasSomethingToDo}.");
        }

        private void OnDestroy()
        {
            dialogueEvent.OnDialogueEventCalled -= Print;
        }
    }
}