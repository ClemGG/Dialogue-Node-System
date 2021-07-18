using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Project.NodeSystem
{
    //Pour créer des ports à choix
    [System.Serializable]
    public class ChoicePort
    {
        public string portGuid;
        public string inputGuid;
        public string outputGuid;
        public TextField textField;
        public List<LanguageGeneric<string>> choicesTexts = new List<LanguageGeneric<string>>();
    }

}