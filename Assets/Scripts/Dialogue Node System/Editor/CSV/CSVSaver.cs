using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

using static Project.Utilities.ValueTypes.Enums;
using Project.Utilities.Assets;

namespace Project.NodeSystem.Editor
{
    public class CSVSaver
    {
        private readonly string _csvDirName = "Resources/Dialogue System/CSV Files";
        private readonly string _csvFileName = "CSV_Dialogue.csv";
        private readonly string _csvSeparator = ",";
        private readonly List<string> _csvHeaders = new List<string>(); //(French, English, Spanish, etc)
        private readonly string _node_ID = "Node Guid ID";
        private readonly string _text_ID = "Text Guid ID";
        private readonly string _dialogueName = "Dialogue Name";


        public void Save()
        {
            List<DialogueContainerSO> dialogueContainers = AssetFinderUtilities.FindAllAssetsOfType<DialogueContainerSO>();

            CreateFile();

            foreach (DialogueContainerSO dialogueContainer in dialogueContainers)
            {
               
                //Dialogue lines
                foreach (RepliqueData nodeData in dialogueContainer.RepliqueDatas)
                {
                    foreach (RepliqueData_Replique textData in nodeData.Repliques)
                    {
                        List<string> texts = new List<string>();

                        texts.Add(dialogueContainer.name);
                        texts.Add(nodeData.NodeGuid);
                        texts.Add(textData.Guid.Value);

                        ForEach<LanguageType>(languageType =>
                        {
                            string tmp = textData.Texts.Find(language => language.Language == languageType).Data.Replace("\"", "\"\"");
                            texts.Add($"\"{tmp}\"");
                        });

                        AppendToFile(texts);
                    }
                }


                //Choices
                foreach (ChoiceData nodeData in dialogueContainer.ChoiceDatas)
                {
                    foreach (ChoiceData_Container choice in nodeData.Choices)
                    {
                        List<string> texts = new List<string>();

                        texts.Add(dialogueContainer.name);
                        texts.Add(nodeData.NodeGuid);
                        texts.Add(choice.Guid.Value);

                        ForEach<LanguageType>(languageType =>
                        {
                            string tmp = choice.Texts.Find(language => language.Language == languageType).Data.Replace("\"", "\"\"");
                            texts.Add($"\"{tmp}\"");
                        });

                        AppendToFile(texts);

                    }

                }


                //Descriptions
                foreach (ChoiceData nodeData in dialogueContainer.ChoiceDatas)
                {
                    foreach (ChoiceData_Container choice in nodeData.Choices)
                    {
                        foreach (ChoiceData_Condition condition in choice.Conditions)
                        {
                            List<string> texts = new List<string>();

                            texts.Add(dialogueContainer.name);
                            texts.Add(nodeData.NodeGuid);
                            texts.Add(condition.Guid.Value);

                            ForEach<LanguageType>(languageType =>
                            {
                                string tmp = condition.DescriptionsIfNotMet.Find(language => language.Language == languageType).Data.Replace("\"", "\"\"");
                                texts.Add($"\"{tmp}\"");
                            });


                            AppendToFile(texts);
                        }


                    }

                }

            }

            AssetDatabase.Refresh();
            Selection.activeObject = Resources.Load($"{_csvDirName}/{_csvFileName}".Replace("Resources/", string.Empty).Replace(".csv", string.Empty));
        }




        #region File


        private void MakeHeader()
        {
            _csvHeaders.Clear();
            _csvHeaders.Add(_dialogueName);
            _csvHeaders.Add(_node_ID);
            _csvHeaders.Add(_text_ID);

            ForEach<LanguageType>(languageType =>
            {
                _csvHeaders.Add(languageType.ToString());
            });
        }

        private void CreateFile()
        {
            VerifyDirectory();
            MakeHeader();

            using (StreamWriter sw = File.CreateText(GetFilePath()))
            {
                string finalString = "";

                foreach (string header in _csvHeaders)
                {
                    if (finalString != "")
                    {
                        finalString += _csvSeparator;    //New column
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
                        finalString += _csvSeparator;    //New column
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
            return $"{Application.dataPath}/{_csvDirName}";
        }

        private string GetFilePath()
        {
            return $"{GetDirectoryPath()}/{_csvFileName}";
        }

        #endregion
    }
}