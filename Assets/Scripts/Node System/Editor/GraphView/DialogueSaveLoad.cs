using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Project.NodeSystem.Editor
{

    public class DialogueSaveLoad
    {
        private List<Edge> edges => graphView.edges.ToList();
        private List<BaseNode> nodes => graphView.nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();

        private DialogueGraphView graphView;

        public DialogueSaveLoad(DialogueGraphView graphView)
        {
            this.graphView = graphView;
        }

        public void Save(DialogueContainerSO dialogueContainerSO)
        {
            SaveEdges(dialogueContainerSO);
            SaveNodes(dialogueContainerSO);

            EditorUtility.SetDirty(dialogueContainerSO);
            AssetDatabase.SaveAssets();
        }

        public void Load(DialogueContainerSO dialogueContainerSO)
        {
            ClearGraph();
            GenerateNodes(dialogueContainerSO);
            ConnectNodes(dialogueContainerSO);
        }




        #region Save

        private void SaveEdges(DialogueContainerSO dialogueContainerSO)
        {
            dialogueContainerSO.linkDatas.Clear();

            Edge[] connectedEdges = edges.Where(edge => edge.input.node != null).ToArray();
            for (int i = 0; i < connectedEdges.Count(); i++)
            {
                BaseNode outputNode = (BaseNode)connectedEdges[i].output.node;
                BaseNode inputNode = connectedEdges[i].input.node as BaseNode;

                dialogueContainerSO.linkDatas.Add(new LinkData
                {
                    baseNodeGuid = outputNode.NodeGuid,
                    basePortName = connectedEdges[i].output.portName,
                    targetNodeGuid = inputNode.NodeGuid,
                    targetPortName = connectedEdges[i].input.portName,
                });
            }
        }

        private void SaveNodes(DialogueContainerSO dialogueContainerSO)
        {
            dialogueContainerSO.eventDatas.Clear();
            dialogueContainerSO.endDatas.Clear();
            dialogueContainerSO.startDatas.Clear();
            dialogueContainerSO.branchDatas.Clear();
            dialogueContainerSO.dialogueDatas.Clear();
            dialogueContainerSO.choiceDatas.Clear();

            nodes.ForEach(node =>
            {
                switch (node)
                {
                    case DialogueNode dialogueNode:
                        dialogueContainerSO.dialogueDatas.Add(SaveNodeData(dialogueNode));
                        break;
                    case StartNode startNode:
                        dialogueContainerSO.startDatas.Add(SaveNodeData(startNode));
                        break;
                    case EndNode endNode:
                        dialogueContainerSO.endDatas.Add(SaveNodeData(endNode));
                        break;
                    case EventNode eventNode:
                        dialogueContainerSO.eventDatas.Add(SaveNodeData(eventNode));
                        break;
                    case BranchNode branchNode:
                        dialogueContainerSO.branchDatas.Add(SaveNodeData(branchNode));
                        break;
                    case ChoiceNode choiceNode:
                        dialogueContainerSO.choiceDatas.Add(SaveNodeData(choiceNode));
                        break;
                    default:
                        break;
                }
            });
        }

        private DialogueData SaveNodeData(DialogueNode node)
        {
            DialogueData dialogueData = new DialogueData
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };

            // Set ID (Instancie les éléments Traduction et Personnage dans le bon ordre)
            for (int i = 0; i < node.DialogueData.baseContainers.Count; i++)
            {
                node.DialogueData.baseContainers[i].ID.value = i;
            }

            foreach (DialogueData_BaseContainer baseContainer in node.DialogueData.baseContainers)
            {
                // Stocke le personnage
                if (baseContainer is DialogueData_CharacterSO)
                {
                    DialogueData_CharacterSO tmp = (baseContainer as DialogueData_CharacterSO);
                    DialogueData_CharacterSO tmpData = new DialogueData_CharacterSO();

                    tmpData.ID.value = tmp.ID.value;
                    tmpData.character.value = tmp.character.value;
                    tmpData.characterName.value = tmp.characterName.value;
                    tmpData.sprite.value = tmp.sprite.value;
                    tmpData.mood.value = tmp.mood.value;
                    tmpData.faceDirection.value = tmp.faceDirection.value;
                    tmpData.sidePlacement.value = tmp.sidePlacement.value;


                    dialogueData.characters.Add(tmpData);

                }

                // Stocke les traductions
                if (baseContainer is DialogueData_Translation)
                {
                    DialogueData_Translation tmp = (baseContainer as DialogueData_Translation);
                    DialogueData_Translation tmpData = new DialogueData_Translation();

                    tmpData.ID = tmp.ID;
                    tmpData.guid = tmp.guid;
                    tmpData.texts = tmp.texts;
                    tmpData.audioClips = tmp.audioClips;

                    dialogueData.translations.Add(tmpData);
                }

            }

            // Port
            foreach (DialogueData_Port port in node.DialogueData.ports)
            {
                DialogueData_Port portData = new DialogueData_Port();

                portData.outputGuid = string.Empty;
                portData.inputGuid = string.Empty;
                portData.portGuid = port.portGuid;

                foreach (Edge edge in edges)
                {
                    if (edge.output.portName == port.portGuid)
                    {
                        portData.outputGuid = (edge.output.node as BaseNode).NodeGuid;
                        portData.inputGuid = (edge.input.node as BaseNode).NodeGuid;
                    }
                }

                dialogueData.ports.Add(portData);
            }

            return dialogueData;
        }

        private StartData SaveNodeData(StartNode node)
        {
            StartData nodeData = new StartData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };

            return nodeData;
        }

        private EndData SaveNodeData(EndNode node)
        {
            EndData nodeData = new EndData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };
            nodeData.endNodeType.value = node.EndData.endNodeType.value;

            return nodeData;
        }

        private EventData SaveNodeData(EventNode node)
        {
            EventData nodeData = new EventData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };

            // Save Dialogue Event
            foreach (ContainerValue<DialogueEventSO> dialogueEvent in node.EventData.scriptableEvents)
            {
                nodeData.scriptableEvents.Add(dialogueEvent);
            }

            // Save String Event
            foreach (EventData_StringModifier stringEvents in node.EventData.stringEvents)
            {
                EventData_StringModifier tmp = new EventData_StringModifier();
                tmp.number.value = stringEvents.number.value;
                tmp.stringEvent.value = stringEvents.stringEvent.value;
                tmp.modifierType.value = stringEvents.modifierType.value;

                nodeData.stringEvents.Add(tmp);
            }

            return nodeData;
        }

        private BranchData SaveNodeData(BranchNode node)
        {
            List<Edge> tmpEdges = edges.Where(x => x.output.node == node).Cast<Edge>().ToList();

            Edge trueOutput = edges.FirstOrDefault(x => x.output.node == node && x.output.portName == "True");
            Edge flaseOutput = edges.FirstOrDefault(x => x.output.node == node && x.output.portName == "False");

            BranchData nodeData = new BranchData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
                trueNodeGuid = (trueOutput != null ? (trueOutput.input.node as BaseNode).NodeGuid : string.Empty),
                falseNodeGuid = (flaseOutput != null ? (flaseOutput.input.node as BaseNode).NodeGuid : string.Empty),
            };

            foreach (EventData_StringCondition stringEvents in node.BranchData.stringConditions)
            {
                EventData_StringCondition tmp = new EventData_StringCondition();
                tmp.number.value = stringEvents.number.value;
                tmp.stringEvent.value = stringEvents.stringEvent.value;
                tmp.conditionType.value = stringEvents.conditionType.value;

                nodeData.stringConditions.Add(tmp);
            }

            return nodeData;
        }

        private ChoiceData SaveNodeData(ChoiceNode node)
        {
            ChoiceData nodeData = new ChoiceData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,

                texts = node.ChoiceData.texts,
                audioClips = node.ChoiceData.audioClips,
            };
            nodeData.choiceStateType.value = node.ChoiceData.choiceStateType.value;

            foreach (EventData_StringCondition stringEvents in node.ChoiceData.stringConditions)
            {
                EventData_StringCondition tmp = new EventData_StringCondition();
                tmp.stringEvent.value = stringEvents.stringEvent.value;
                tmp.number.value = stringEvents.number.value;
                tmp.conditionType.value = stringEvents.conditionType.value;

                nodeData.stringConditions.Add(tmp);
            }

            return nodeData;
        }


        #endregion






        #region Load

        private void ClearGraph()
        {
            edges.ForEach(edge => graphView.RemoveElement(edge));

            foreach (BaseNode node in nodes)
            {
                graphView.RemoveElement(node);
            }
        }

        private void GenerateNodes(DialogueContainerSO dialogueContainer)
        {
            // Start
            foreach (StartData node in dialogueContainer.startDatas)
            {
                StartNode tempNode = graphView.CreateNode<StartNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;

                graphView.AddElement(tempNode);
            }

            // End Node 
            foreach (EndData node in dialogueContainer.endDatas)
            {
                EndNode tempNode = graphView.CreateNode<EndNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;
                tempNode.EndData.endNodeType.value = node.endNodeType.value;

                tempNode.LoadValueIntoField();
                graphView.AddElement(tempNode);
            }

            // Event Node
            foreach (EventData node in dialogueContainer.eventDatas)
            {
                EventNode tempNode = graphView.CreateNode<EventNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;

                foreach (ContainerValue<DialogueEventSO> item in node.scriptableEvents)
                {
                    tempNode.AddScriptableEvent(item);
                }
                foreach (EventData_StringModifier item in node.stringEvents)
                {
                    tempNode.AddStringEvent(item);
                }

                tempNode.LoadValueIntoField();
                graphView.AddElement(tempNode);
            }

            // Breach Node
            foreach (BranchData node in dialogueContainer.branchDatas)
            {
                BranchNode tempNode = graphView.CreateNode<BranchNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;

                foreach (EventData_StringCondition item in node.stringConditions)
                {
                    tempNode.AddCondition(item);
                }

                tempNode.LoadValueIntoField();
                tempNode.ReloadLanguage();
                graphView.AddElement(tempNode);
            }

            // Choice Node
            foreach (ChoiceData node in dialogueContainer.choiceDatas)
            {
                ChoiceNode tempNode = graphView.CreateNode<ChoiceNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;

                tempNode.ChoiceData.choiceStateType.value = node.choiceStateType.value;

                foreach (LanguageGeneric<string> dataText in node.texts)
                {
                    foreach (LanguageGeneric<string> editorText in tempNode.ChoiceData.texts)
                    {
                        if (editorText.language == dataText.language)
                        {
                            editorText.data = dataText.data;
                        }
                    }
                }
                foreach (LanguageGeneric<AudioClip> dataAudioClip in node.audioClips)
                {
                    foreach (LanguageGeneric<AudioClip> editorAudioClip in tempNode.ChoiceData.audioClips)
                    {
                        if (editorAudioClip.language == dataAudioClip.language)
                        {
                            editorAudioClip.data = dataAudioClip.data;
                        }
                    }
                }

                foreach (EventData_StringCondition item in node.stringConditions)
                {
                    tempNode.AddCondition(item);
                }

                tempNode.LoadValueIntoField();
                tempNode.ReloadLanguage();
                graphView.AddElement(tempNode);
            }

            // Dialogue Node
            foreach (DialogueData node in dialogueContainer.dialogueDatas)
            {
                DialogueNode tempNode = graphView.CreateNode<DialogueNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;

                List<DialogueData_BaseContainer> data_BaseContainer = new List<DialogueData_BaseContainer>();

                data_BaseContainer.AddRange(node.characters);
                data_BaseContainer.AddRange(node.translations);

                data_BaseContainer.Sort(delegate (DialogueData_BaseContainer x, DialogueData_BaseContainer y)
                {
                    return x.ID.value.CompareTo(y.ID.value);
                });

                foreach (DialogueData_BaseContainer data in data_BaseContainer)
                {
                    switch (data)
                    {
                        case DialogueData_CharacterSO Character:
                            tempNode.AddCharacter(Character);
                            break;
                        case DialogueData_Translation texts:
                            tempNode.AddTextLine(texts);
                            break;
                        default:
                            break;
                    }
                }

                foreach (DialogueData_Port port in node.ports)
                {
                    tempNode.AddChoicePort(tempNode, port);
                }

                tempNode.LoadValueIntoField();
                tempNode.ReloadLanguage();
                graphView.AddElement(tempNode);
            }
        }

        private void ConnectNodes(DialogueContainerSO dialogueContainer)
        {
            // Make connection for all node.
            for (int i = 0; i < nodes.Count; i++)
            {
                List<LinkData> connections = dialogueContainer.linkDatas.Where(edge => edge.baseNodeGuid == nodes[i].NodeGuid).ToList();

                List<Port> allOutputPorts = nodes[i].outputContainer.Children().Where(x => x is Port).Cast<Port>().ToList();

                for (int j = 0; j < connections.Count; j++)
                {
                    string targetNodeGuid = connections[j].targetNodeGuid;
                    BaseNode targetNode = nodes.First(node => node.NodeGuid == targetNodeGuid);

                    if (targetNode == null)
                        continue;

                    foreach (Port item in allOutputPorts)
                    {
                        if (item.portName == connections[j].basePortName)
                        {
                            LinkNodesTogether(item, (Port)targetNode.inputContainer[0]);
                        }
                    }
                }
            }
        }

        private void LinkNodesTogether(Port outputPort, Port inputPort)
        {
            Edge tempEdge = new Edge()
            {
                output = outputPort,
                input = inputPort
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            graphView.Add(tempEdge);
        }

        #endregion
    }
}