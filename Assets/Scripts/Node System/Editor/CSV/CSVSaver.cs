using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem.Editor
{
    public class CSVSaver
    {
        private string csvDirName = "Resources/Dialogue System/CSV Files";
        private string csvFileName = "CSV_Dialogue.csv";
        private string csvSeparator = ",";
        private List<string> csvHeaders = new List<string>(); //Le nom des langues (French, English, Spanish, etc)
        private string node_ID = "Node Guid ID";
        private string text_ID = "Text Guid ID";
        private string dialogueName = "Dialogue Name";


        public void Save()
        {
            List<DialogueContainerSO> dialogueContainers = CSVHelper.FindAllDialogueContainers();

            CreateFile();

            foreach (DialogueContainerSO dialogueContainer in dialogueContainers)
            {
                {
                    //foreach (DialogueData nodeData in dialogueContainer.dialogueDatas)
                    //{
                    //    foreach (DialogueData_Repliques textData in nodeData.repliques)
                    //    {
                    //        List<string> texts = new List<string>();

                    //        texts.Add(dialogueContainer.name);
                    //        texts.Add(nodeData.nodeGuid);
                    //        texts.Add(textData.guid.value);

                    //        ForEach<LanguageType>(languageType =>
                    //        {
                    //            string tmp = textData.texts.Find(language => language.language == languageType).data.Replace("\"", "\"\"");
                    //            texts.Add($"\"{tmp}\"");
                    //        });

                    //        AppendToFile(texts);
                    //    }
                    //}
                }

                //Répliques des dialogues
                foreach (RepliqueData nodeData in dialogueContainer.repliqueDatas)
                {
                    foreach (RepliqueData_Replique textData in nodeData.repliques)
                    {
                        List<string> texts = new List<string>();

                        texts.Add(dialogueContainer.name);
                        texts.Add(nodeData.nodeGuid);
                        texts.Add(textData.guid.value);

                        ForEach<LanguageType>(languageType =>
                        {
                            string tmp = textData.texts.Find(language => language.language == languageType).data.Replace("\"", "\"\"");
                            texts.Add($"\"{tmp}\"");
                        });

                        AppendToFile(texts);
                    }
                }


                //Répliques des choix
                foreach (ChoiceData nodeData in dialogueContainer.choiceDatas)
                {
                    foreach (ChoiceData_Container choice in nodeData.choices)
                    {
                        List<string> texts = new List<string>();

                        texts.Add(dialogueContainer.name);
                        texts.Add(nodeData.nodeGuid);
                        texts.Add(choice.guid.value);

                        ForEach<LanguageType>(languageType =>
                        {
                            string tmp = choice.texts.Find(language => language.language == languageType).data.Replace("\"", "\"\"");
                            texts.Add($"\"{tmp}\"");
                        });

                        AppendToFile(texts);

                    }

                }


                //Descriptions des choix
                foreach (ChoiceData nodeData in dialogueContainer.choiceDatas)
                {
                    foreach (ChoiceData_Container choice in nodeData.choices)
                    {
                        foreach (ChoiceData_Condition condition in choice.conditions)
                        {
                            List<string> texts = new List<string>();

                            texts.Add(dialogueContainer.name);
                            texts.Add(nodeData.nodeGuid);
                            texts.Add(condition.guid.value);

                            ForEach<LanguageType>(languageType =>
                            {
                                string tmp = condition.descriptionsIfNotMet.Find(language => language.language == languageType).data.Replace("\"", "\"\"");
                                texts.Add($"\"{tmp}\"");
                            });


                            AppendToFile(texts);
                        }


                    }

                }

            }

            AssetDatabase.Refresh();
            Selection.activeObject = Resources.Load($"{csvDirName}/{csvFileName}".Replace("Resources/", string.Empty).Replace(".csv", string.Empty));
        }




        #region File


        private void MakeHeader()
        {
            csvHeaders.Clear();
            csvHeaders.Add(dialogueName);
            csvHeaders.Add(node_ID);
            csvHeaders.Add(text_ID);

            ForEach<LanguageType>(languageType =>
            {
                csvHeaders.Add(languageType.ToString());
            });
        }

        private void CreateFile()
        {
            VerifyDirectory();
            MakeHeader();

            using (StreamWriter sw = File.CreateText(GetFilePath()))
            {
                string finalString = "";

                foreach (string header in csvHeaders)
                {
                    if (finalString != "")
                    {
                        finalString += csvSeparator;    //On indique une nouvelle colonne
                    }
                    finalString += header;
                }

                sw.WriteLine(finalString);
            }
        }

        private void AppendToFile(List<string> strings)
        {
            using (StreamWriter sw = File.AppendText(GetFilePath()))
            {
                string finalString = "";

                foreach (string text in strings)
                {
                    if (finalString != "")
                    {
                        finalString += csvSeparator;    //On indique une nouvelle colonne
                    }

                    finalString += text;
                }

                sw.WriteLine(finalString);
            }
        }

        #endregion


        #region Path


        private void VerifyDirectory()
        {
            string directory = GetDirectoryPath();

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private string GetDirectoryPath()
        {
            return $"{Application.dataPath}/{csvDirName}";
        }

        private string GetFilePath()
        {
            return $"{GetDirectoryPath()}/{csvFileName}";
        }

        #endregion
    }
}