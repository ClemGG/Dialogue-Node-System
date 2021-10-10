using System;
using UnityEngine;


namespace Project.NodeSystem
{
    /* This script is the base script used by the Event Node to communicate with the rest of the game.
     * To implement your own events and properties, you can rewrite this script, but you must implement
     * a child class of DE_EventCaller to use it in a dialogue.
     */


    public class DE_EventCaller : MonoBehaviour
    {
        [SerializeField] private DialogueEventSO _dialogueEvent;
        [SerializeField] private DialogueEventSO _githubEvent;
        [SerializeField] private DialogueEventSO _youtubeEvent;

        [ReadOnly, SerializeField] private bool _orLikeThat = false;
        [ReadOnly, SerializeField] private bool _choiceBlocked = true;

        //The properties must be public to be modified by the Event Node.
        public bool OrLikeThat { get => _orLikeThat; set => _orLikeThat = value; }
        public bool ChoiceBlocked { get => _choiceBlocked; set => _choiceBlocked = value; }

        private void Awake()
        {
            _dialogueEvent.OnDialogueEventCalled += Print;
            _githubEvent.OnDialogueEventCalled += GoToGithub;
            _youtubeEvent.OnDialogueEventCalled += GoToYoutube;
        }

        private void GoToGithub()
        {
            Application.OpenURL("https://github.com/ClemGG/Dialogue-Node-System");
        }

        private void GoToYoutube()
        {
            Application.OpenURL("https://www.youtube.com/channel/UCOsUV9D5o0v5RiIK2124xcA");
        }

        private void Print()
        {
            print($"L'event de DE_EventCaller a bien été appelé.");
        }

        private void OnDestroy()
        {
            _dialogueEvent.OnDialogueEventCalled -= Print;
        }
    }
}