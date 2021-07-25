using UnityEngine;


namespace Project.NodeSystem
{

    //Les variables doivent être publiques pour être modifiées par l'EventNode
    public class DE_Trigger : MonoBehaviour
    {
        public DialogueEventSO dialogueEvent;

        public int money = 50;
        public bool hasSomethingToDo = false;



        private void Awake()
        {
            dialogueEvent.onDialogueEventCalled += Print;
        }

        private void Print()
        {
            print("L'event de test a bien été appelé.");
        }

        private void OnDestroy()
        {
            dialogueEvent.onDialogueEventCalled -= Print;
        }
    }
}