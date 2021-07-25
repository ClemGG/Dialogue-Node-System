using UnityEngine;


namespace Project.NodeSystem
{

    //Les variables doivent �tre publiques pour �tre modifi�es par l'EventNode
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
            print("L'event de test a bien �t� appel�.");
        }

        private void OnDestroy()
        {
            dialogueEvent.onDialogueEventCalled -= Print;
        }
    }
}