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
            dialogueContainerSO.startDatas.Clear();
            dialogueContainerSO.branchDatas.Clear();
            dialogueContainerSO.characterDatas.Clear();
            dialogueContainerSO.repliqueDatas.Clear();
            dialogueContainerSO.choiceDatas.Clear();
            dialogueContainerSO.eventDatas.Clear();
            dialogueContainerSO.endDatas.Clear();

            nodes.ForEach(node =>
            {
                switch (node)
                {
                    case CharacterNode characterNode:
                        dialogueContainerSO.characterDatas.Add(SaveNodeData(characterNode));
                        break;
                    case RepliqueNode repliqueNode:
                        dialogueContainerSO.repliqueDatas.Add(SaveNodeData(repliqueNode));
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





        private CharacterData SaveNodeData(CharacterNode node)
        {
            CharacterData characterData = new CharacterData
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };

            // Set ID (Instancie les éléments Traduction et Personnage dans le bon ordre)
            for (int i = 0; i < node.CharacterData.characters.Count; i++)
            {
                node.CharacterData.characters[i].ID.value = i;
            }

            foreach (CharacterData_CharacterSO character in node.CharacterData.characters)
            {
                // Stocke le personnage
                CharacterData_CharacterSO tmpData = new CharacterData_CharacterSO();

                tmpData.ID.value = character.ID.value;
                tmpData.character.value = character.character.value;
                tmpData.characterName.value = character.characterName.value;
                tmpData.characterNames.AddRange(character.characterNames);
                tmpData.sprite.value = character.sprite.value;
                tmpData.mood.value = character.mood.value;
                tmpData.faceDirection.value = character.faceDirection.value;
                tmpData.sidePlacement.value = character.sidePlacement.value;


                characterData.characters.Add(tmpData);
            }

            return characterData;
        }

        private RepliqueData SaveNodeData(RepliqueNode node)
        {
            RepliqueData repliqueData = new RepliqueData
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };

            // Set ID (Instancie les éléments Traduction et Personnage dans le bon ordre)
            for (int i = 0; i < node.RepliqueData.repliques.Count; i++)
            {
                node.RepliqueData.repliques[i].ID.value = i;
            }

            foreach (RepliqueData_Replique replique in node.RepliqueData.repliques)
            {
                RepliqueData_Replique tmpData = new RepliqueData_Replique();

                tmpData.ID = replique.ID;
                tmpData.guid = replique.guid;
                tmpData.texts = replique.texts;
                tmpData.audioClips = replique.audioClips;

                repliqueData.repliques.Add(tmpData);
            }

            return repliqueData;
        }

        private StartData SaveNodeData(StartNode node)
        {
            StartData startData = new StartData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };

            return startData;
        }

        private EndData SaveNodeData(EndNode node)
        {
            EndData endData = new EndData()
            {
                nodeGuid = node.NodeGuid,
                position = node.GetPosition().position,
            };
            //nodeData.endNodeType.value = node.EndData.endNodeType.value;

            return endData;
        }

        private EventData SaveNodeData(EventNode node)
        {
            EventData eventData = new EventData()
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

                        eventData.scriptableEvents.Add(tmpScriptable);
                        break;

                    // Save String Event
                    case EventData_StringEventModifier stringEvent:

                        EventData_StringEventModifier tmpString = new EventData_StringEventModifier();
                        tmpString.ID.value = stringEvent.ID.value;
                        tmpString.number.value = stringEvent.number.value;
                        tmpString.stringEvent.value = stringEvent.stringEvent.value;
                        tmpString.modifierType.value = stringEvent.modifierType.value;

                        eventData.stringEvents.Add(tmpString);
                        break;
                }
            }

            return eventData;
        }

        private BranchData SaveNodeData(BranchNode node)
        {
            List<Edge> tmpEdges = edges.Where(x => x.output.node == node).Cast<Edge>().ToList();

            Edge trueOutput = edges.FirstOrDefault(x => x.output.node == node && x.output.portName == "True");
            Edge flaseOutput = edges.FirstOrDefault(x => x.output.node == node && x.output.portName == "False");

            BranchData branchData = new BranchData()
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

                branchData.stringConditions.Add(tmp);
            }

            return branchData;
        }

        private ChoiceData SaveNodeData(ChoiceNode node)
        {
            ChoiceData choiceData = new ChoiceData()
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


                choiceData.choices.Add(tmpChoice);
            }


            return choiceData;
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
            foreach (StartData savedData in dialogueContainer.startDatas)
            {
                StartNode tmpNode = graphView.CreateNode<StartNode>(savedData.position);
                tmpNode.NodeGuid = savedData.nodeGuid;

                graphView.AddElement(tmpNode);
            }

            // End Node 
            foreach (EndData savedData in dialogueContainer.endDatas)
            {
                EndNode tmpNode = graphView.CreateNode<EndNode>(savedData.position);
                tmpNode.NodeGuid = savedData.nodeGuid;
                //tmpNode.EndData.endNodeType.value = node.endNodeType.value;

                //tmpNode.LoadValueIntoField();
                graphView.AddElement(tmpNode);
            }

            // Event Node
            foreach (EventData savedData in dialogueContainer.eventDatas)
            {
                EventNode tmpNode = graphView.CreateNode<EventNode>(savedData.position);
                tmpNode.NodeGuid = savedData.nodeGuid;



                //Ils sont déjà concaténés et triés dans SortedEvents
                List<NodeData_BaseContainer> newEvents = savedData.SortedEvents;

                foreach (NodeData_BaseContainer item in newEvents)
                {
                    switch (item)
                    {
                        // Load Dialogue Event
                        case EventData_ScriptableEvent scriptableEvent:
                            tmpNode.AddScriptableEvent(scriptableEvent);
                            break;

                        // Load String Event
                        case EventData_StringEventModifier stringEvent:
                            tmpNode.AddStringEvent(stringEvent);
                            break;

                        default:
                            break;
                    }
                }


                tmpNode.LoadValueIntoField();
                graphView.AddElement(tmpNode);
            }

            // Breach Node
            foreach (BranchData savedData in dialogueContainer.branchDatas)
            {
                BranchNode tmpNode = graphView.CreateNode<BranchNode>(savedData.position);
                tmpNode.NodeGuid = savedData.nodeGuid;

                foreach (EventData_StringEventCondition item in savedData.stringConditions)
                {
                    tmpNode.AddCondition(item);
                }

                tmpNode.LoadValueIntoField();
                tmpNode.ReloadLanguage();
                graphView.AddElement(tmpNode);
            }

            // Choice Node
            foreach (ChoiceData savedData in dialogueContainer.choiceDatas)
            {
                ChoiceNode tmpNode = graphView.CreateNode<ChoiceNode>(savedData.position);
                tmpNode.NodeGuid = savedData.nodeGuid;



                //Comme on crée un choix par défaut dans la node, on le supprime au chargement pour ne pas le recréer
                //en plus de tous les autres
                tmpNode.DeleteBox(tmpNode.ChoiceData.choices[0].boxContainer);
                NodeBuilder.DeleteChoicePort(tmpNode, tmpNode.ChoiceData.choices[0].linkedPort.port);
                tmpNode.ChoiceData.choices.RemoveAt(0);




                List<ChoiceData_Container> newChoices = new List<ChoiceData_Container>();
                newChoices.AddRange(savedData.choices);


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

            // Character Node
            foreach (CharacterData savedData in dialogueContainer.characterDatas)
            {
                CharacterNode tmpNode = graphView.CreateNode<CharacterNode>(savedData.position);
                tmpNode.NodeGuid = savedData.nodeGuid;



                //Comme on crée une réplique par défaut dans la node, on la supprime au chargement pour ne pas la recréer
                //en plus de toutes les autres
                tmpNode.DeleteBox(tmpNode.CharacterData.characters[0].boxContainer);
                tmpNode.CharacterData.characters.RemoveAt(0);




                List<CharacterData_CharacterSO> newCharacters = new List<CharacterData_CharacterSO>();
                newCharacters.AddRange(savedData.characters);

                newCharacters.Sort(delegate (CharacterData_CharacterSO x, CharacterData_CharacterSO y)
                {
                    return x.ID.value.CompareTo(y.ID.value);
                });

                foreach (CharacterData_CharacterSO character in newCharacters)
                {
                    tmpNode.AddCharacter(character);
                }

                tmpNode.LoadValueIntoField();
                tmpNode.ReloadLanguage();
                graphView.AddElement(tmpNode);
            }

            // Replique Node
            foreach (RepliqueData savedData in dialogueContainer.repliqueDatas)
            {
                RepliqueNode tmpNode = graphView.CreateNode<RepliqueNode>(savedData.position);
                tmpNode.NodeGuid = savedData.nodeGuid;



                //Comme on crée une réplique par défaut dans la node, on la supprime au chargement pour ne pas la recréer
                //en plus de toutes les autres
                tmpNode.DeleteBox(tmpNode.RepliqueData.repliques[0].boxContainer);
                tmpNode.RepliqueData.repliques.RemoveAt(0);




                List<RepliqueData_Replique> newRepliques = new List<RepliqueData_Replique>();
                newRepliques.AddRange(savedData.repliques);

                newRepliques.Sort(delegate (RepliqueData_Replique x, RepliqueData_Replique y)
                {
                    return x.ID.value.CompareTo(y.ID.value);
                });

                foreach (RepliqueData_Replique replique in newRepliques)
                {
                    tmpNode.AddReplique(replique);
                }

                tmpNode.LoadValueIntoField();
                tmpNode.ReloadLanguage();
                graphView.AddElement(tmpNode);
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