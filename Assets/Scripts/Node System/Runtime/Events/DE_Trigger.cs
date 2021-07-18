using UnityEngine;


namespace Project.NodeSystem
{

    //Ce script s'abonne au GameEvents pour pouvoir appeler sa méthode
    //quand le dialogue s'arrête sur une EventNode.
    public class DE_Trigger : MonoBehaviour
    {
        public int money = 50;
        public bool hasSomethingToDo = false;

        private void Awake()
        {
            GameEvents.OnDialogueEventCalled += Print;
        }

        private void Print()
        {
            print("L'event de test a bien été appelé.");
        }

        private void OnDestroy()
        {
            GameEvents.OnDialogueEventCalled -= Print;
        }
    }
}