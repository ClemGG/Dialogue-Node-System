using System;
using UnityEngine;


namespace Project.NodeSystem
{

    //Les propriétés doivent être publiques pour être modifiées par l'EventNode
    public class DE_EventCaller : MonoBehaviour
    {
        [SerializeField] private DialogueEventSO _dialogueEvent;
        [SerializeField] private DialogueEventSO _githubEvent;
        [SerializeField] private DialogueEventSO _youtubeEvent;

        [ReadOnly, SerializeField] private bool _orLikeThat = false;
        [ReadOnly, SerializeField] private bool _choiceBlocked = true;

        public bool OrLikeThat { get => _orLikeThat; set => _orLikeThat = value; }
        public bool ChoiceBlocked { get => _orLikeThat; set => _orLikeThat = value; }

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