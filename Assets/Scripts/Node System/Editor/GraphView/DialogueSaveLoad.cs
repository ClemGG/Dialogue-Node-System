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

            foreach (NodeData_BaseContainer baseContainer in node.DialogueData.baseContainers)
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
                if (baseContainer is DialogueData_Repliques)
                {
                    DialogueData_Repliques tmp = (baseContainer as DialogueData_Repliques);
                    DialogueData_Repliques tmpData = new DialogueData_Repliques();

                    tmpData.ID = tmp.ID;
                    tmpData.guid = tmp.guid;
                    tmpData.texts = tmp.texts;
                    tmpData.audioClips = tmp.audioClips;

                    dialogueData.repliques.Add(tmpData);
                }

            }

            // Port
            foreach (NodeData_Port port in node.DialogueData.ports)
            {
                NodeData_Port portData = new NodeData_Port();

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
            //nodeData.endNodeType.value = node.EndData.endNodeType.value;

            return nodeData;
        }

        private EventData SaveNodeData(EventNode node)
        {
            EventData nodeData = new EventData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };

            // Set ID (Instancie les éléments Traduction et Personnage dans le bon ordre)
            for (int i = 0; i < node.EventData.events.Count; i++)
            {
                node.EventData.events[i].ID.value = i;
            }

            foreach (NodeData_BaseContainer item in node.EventData.events)
            {

                switch (item)
                {
                    // Save Dialogue Event
                    case EventData_ScriptableEvent scriptableEvent:

                        EventData_ScriptableEvent tmpScriptable = new EventData_ScriptableEvent();
                        tmpScriptable.ID.value = scriptableEvent.ID.value;
                        tmpScriptable.scriptableObject.value = scriptableEvent.scriptableObject.value;

                        nodeData.scriptableEvents.Add(tmpScriptable);
                        break;

                    // Save String Event
                    case EventData_StringEventModifier stringEvent:

                        EventData_StringEventModifier tmpString = new EventData_StringEventModifier();
                        tmpString.ID.value = stringEvent.ID.value;
                        tmpString.number.value = stringEvent.number.value;
                        tmpString.stringEvent.value = stringEvent.stringEvent.value;
                        tmpString.modifierType.value = stringEvent.modifierType.value;

                        nodeData.stringEvents.Add(tmpString);
                        break;
                }
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

            foreach (EventData_StringEventCondition stringEvents in node.BranchData.stringConditions)
            {
                EventData_StringEventCondition tmp = new EventData_StringEventCondition();
                tmp.number.value = stringEvents.number.value;
                tmp.stringEvent.value = stringEvents.stringEvent.value;
                tmp.conditionType.value = stringEvents.conditionType.value;

                nodeData.stringConditions.Add(tmp);
            }

            return nodeData;
        }

        private ChoiceData SaveNodeData(ChoiceNode node)
        {
            ChoiceData newChoiceData = new ChoiceData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };

            // Set ID (Instancie les éléments Traduction et Personnage dans le bon ordre)
            for (int i = 0; i < node.ChoiceData.choices.Count; i++)
            {
                node.ChoiceData.choices[i].ID.value = i;
            }



            foreach (ChoiceData_Container choice in node.ChoiceData.choices)
            {


                //Init
                ChoiceData_Container tmpChoice = new ChoiceData_Container();

                tmpChoice.guid = choice.guid;
                tmpChoice.ID = choice.ID;
                tmpChoice.texts = choice.texts;
                tmpChoice.audioClips = choice.audioClips;
                tmpChoice.choiceStateType.value = choice.choiceStateType.value;


                //Conditions
                foreach (ChoiceData_Condition condition in choice.conditions)
                {
                    ChoiceData_Condition tmpCondition = new ChoiceData_Condition();

                    tmpCondition.guid.value = condition.guid.value;
                    tmpCondition.descriptionsIfNotMet = condition.descriptionsIfNotMet;

                    EventData_StringEventCondition tmpEvent = new EventData_StringEventCondition();

                    tmpEvent.stringEvent.value = condition.stringCondition.stringEvent.value;
                    tmpEvent.number.value = condition.stringCondition.number.value;
                    tmpEvent.conditionType.value = condition.stringCondition.conditionType.value;

                    tmpCondition.stringCondition = tmpEvent;

                    tmpChoice.conditions.Add(tmpCondition);
                }



                // Choice Ports
                NodeData_Port portData = new NodeData_Port();

                portData.outputGuid = string.Empty;
                portData.inputGuid = string.Empty;
                portData.portGuid = choice.linkedPort.portGuid;

                foreach (Edge edge in edges)
                {
                    if (edge.output.portName == choice.linkedPort.portGuid)
                    {
                        portData.outputGuid = (edge.output.node as BaseNode).NodeGuid;
                        portData.inputGuid = (edge.input.node as BaseNode).NodeGuid;
                    }
                }

                tmpChoice.linkedPort = portData;


                newChoiceData.choices.Add(tmpChoice);
            }


            return newChoiceData;
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
                //tempNode.EndData.endNodeType.value = node.endNodeType.value;

                //tempNode.LoadValueIntoField();
                graphView.AddElement(tempNode);
            }

            // Event Node
            foreach (EventData node in dialogueContainer.eventDatas)
            {
                EventNode tempNode = graphView.CreateNode<EventNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;



                //Ils sont déjà concaténés et triés dans SortedEvents
                List<NodeData_BaseContainer> newEvents = node.SortedEvents;

                foreach (NodeData_BaseContainer item in newEvents)
                {
                    switch (item)
                    {
                        // Load Dialogue Event
                        case EventData_ScriptableEvent scriptableEvent:
                            tempNode.AddScriptableEvent(scriptableEvent);
                            break;

                        // Load String Event
                        case EventData_StringEventModifier stringEvent:
                            tempNode.AddStringEvent(stringEvent);
                            break;

                        default:
                            break;
                    }
                }


                tempNode.LoadValueIntoField();
                graphView.AddElement(tempNode);
            }

            // Breach Node
            foreach (BranchData node in dialogueContainer.branchDatas)
            {
                BranchNode tempNode = graphView.CreateNode<BranchNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;

                foreach (EventData_StringEventCondition item in node.stringConditions)
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
                ChoiceNode tmpNode = graphView.CreateNode<ChoiceNode>(node.position);
                tmpNode.NodeGuid = node.nodeGuid;



                //Comme on crée un choix par défaut dans la node, on le supprime au chargement pour ne pas le recréer
                //en plus de tous les autres
                tmpNode.DeleteBox(tmpNode.ChoiceData.choices[0].choiceContainer);
                NodeBuilder.DeleteChoicePort(tmpNode, tmpNode.ChoiceData.choices[0].linkedPort.port);
                tmpNode.ChoiceData.choices.RemoveAt(0);




                List<ChoiceData_Container> newChoices = new List<ChoiceData_Container>();
                newChoices.AddRange(node.choices);


                newChoices.Sort(delegate (ChoiceData_Container x, ChoiceData_Container y)
                {
                    return x.ID.value.CompareTo(y.ID.value);
                });

                foreach (ChoiceData_Container choice in newChoices)
                {
                    tmpNode.AddChoice(choice);
                }


                tmpNode.LoadValueIntoField();
                tmpNode.ReloadLanguage();
                graphView.AddElement(tmpNode);
            }

            // Dialogue Node
            foreach (DialogueData node in dialogueContainer.dialogueDatas)
            {
                DialogueNode tempNode = graphView.CreateNode<DialogueNode>(node.position);
                tempNode.NodeGuid = node.nodeGuid;

                List<NodeData_BaseContainer> data_BaseContainer = new List<NodeData_BaseContainer>();

                data_BaseContainer.AddRange(node.characters);
                data_BaseContainer.AddRange(node.repliques);

                data_BaseContainer.Sort(delegate (NodeData_BaseContainer x, NodeData_BaseContainer y)
                {
                    return x.ID.value.CompareTo(y.ID.value);
                });

                foreach (NodeData_BaseContainer data in data_BaseContainer)
                {
                    switch (data)
                    {
                        case DialogueData_CharacterSO character:
                            tempNode.AddCharacter(character);
                            break;
                        case DialogueData_Repliques replique:
                            tempNode.AddTextLine(replique);
                            break;
                        default:
                            break;
                    }
                }

                foreach (NodeData_Port port in node.ports)
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