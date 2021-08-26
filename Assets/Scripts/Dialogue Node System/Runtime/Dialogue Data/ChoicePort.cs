using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Serialization;

namespace Project.NodeSystem
{
    //Pour créer des ports à choix
    [System.Serializable]
    public class ChoicePort
    {
        public string PortGuid;
        public string InputGuid;
        public string OutputGuid;
        public TextField TextField;
        public List<LanguageGeneric<string>> ChoicesTexts = new List<LanguageGeneric<string>>();
    }

}