using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.NodeSystem.Editor
{
    public class CSVLanguageUpdater
    {
        //public void UpdateLanguage()
        //{
        //    //List<DialogueContainerSO> dialogueContainers = CSVHelper.FindAllObjectsFromResources<DialogueContainerSO>();
        //    List<DialogueContainerSO> dialogueContainers = CSVHelper.FindAllDialogueContainers();

        //    foreach (DialogueContainerSO container in dialogueContainers)
        //    {
        //        foreach (DialogueData data in container.dialogueDatas)
        //        {
        //            data.texts = UpdateLanguageGeneric(data.texts);
        //            data.audioClips = UpdateLanguageGeneric(data.audioClips);

        //            foreach (ChoicePort ports in data.dialogueNodePorts)
        //            {
        //                ports.choicesTexts = UpdateLanguageGeneric(ports.choicesTexts);
        //            }
        //        }
        //    }
        //}


        //private List<LanguageGeneric<T>> UpdateLanguageGeneric<T>(List<LanguageGeneric<T>> languageGenerics)
        //{
        //    List<LanguageGeneric<T>> tmp = new List<LanguageGeneric<T>>();

        //    //On cr�e une entr�e dans la liste pour chaque langue
        //    foreach (LanguageType languageType in (LanguageType[])Enum.GetValues(typeof(LanguageType)))
        //    {
        //        tmp.Add(new LanguageGeneric<T>
        //        {
        //            language = languageType
        //        });
        //    }

        //    //Si la liste en param�tre comporte des donn�es pour la langue en question, on ajoute ces donn�es � la liste
        //    foreach (LanguageGeneric<T> languageGeneric in languageGenerics)
        //    {
        //        if (tmp.Find(generic => generic.language == languageGeneric.language) != null)
        //        {
        //            tmp.Find(generic => generic.language == languageGeneric.language).data = languageGeneric.data;
        //        }
        //    }

        //    return tmp;
        //}
    }
}