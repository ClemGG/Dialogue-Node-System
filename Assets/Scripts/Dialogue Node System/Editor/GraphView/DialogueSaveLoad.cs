using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace Project.NodeSystem.Editor
{

    public class DialogueSaveLoad
    {
        #region Fields

        private List<Edge> Edges => _graphView.edges.ToList();
        private List<BaseNode> Nodes => _graphView.nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();
        private List<Group> Groups => _graphView.graphElements.ToList().Where(node => node is Group).Cast<Group>().ToList();
        private List<StickyNote> StickyNotes => _graphView.graphElements.ToList().Where(node => node is StickyNote).Cast<StickyNote>().ToList();


        private List<BaseNode> _collectedNodes = new List<BaseNode>();


        private DialogueGraphView _graphView;


        #endregion




        #region Constructor

        public DialogueSaveLoad(DialogueGraphView graphView)
        {
            this._graphView = graphView;
        }

        public void Save(DialogueContainerSO dialogueContainerSO)
        {
            SaveEdges(dialogueContainerSO);
            SaveNodes(dialogueContainerSO);
            SaveGroups(dialogueContainerSO);
            SaveNotes(dialogueContainerSO);

            EditorUtility.SetDirty(dialogueContainerSO);
            AssetDatabase.SaveAssets();
        }

        public void Load(DialogueContainerSO dialogueContainerSO)
        {
            ClearGraph();

            GenerateNodes(dialogueContainerSO);
            ConnectNodes(dialogueContainerSO);
            GroupNodes(dialogueContainerSO);
            AddStickyNotes(dialogueContainerSO);
        }

        #endregion



        #region Save

        private void SaveEdges(DialogueContainerSO dialogueContainerSO)
        {
            dialogueContainerSO.linkDatas.Clear();

            Edge[] connectedEdges = Edges.Where(edge => edge.input.node != null).ToArray();
            for (int i = 0; i < connectedEdges.Count(); i++)
            {
                BaseNode outputNode = (BaseNode)connectedEdges[i].output.node;
                BaseNode inputNode = connectedEdges[i].input.node as BaseNode;

                dialogueContainerSO.linkDatas.Add(new LinkData
                {
                    BaseNodeGuid = outputNode.NodeGuid,
                    BasePortName = connectedEdges[i].output.portName,
                    TargetNodeGuid = inputNode.NodeGuid,
                    TargetPortName = connectedEdges[i].input.portName,
                });
            }
        }

        private void SaveNodes(DialogueContainerSO dialogueContainerSO)
        {
            dialogueContainerSO.StartDatas.Clear();
            dialogueContainerSO.UIDatas.Clear();
            dialogueContainerSO.BackgroundDatas.Clear();
            dialogueContainerSO.BranchDatas.Clear();
            dialogueContainerSO.CharacterDatas.Clear();
            dialogueContainerSO.RepliqueDatas.Clear();
            dialogueContainerSO.ChoiceDatas.Clear();
            dialogueContainerSO.EventDatas.Clear();
            dialogueContainerSO.DelayDatas.Clear();
            dialogueContainerSO.EndDatas.Clear();

            Nodes.ForEach(node =>
            {

                switch (node)
                {
                    case StartNode startNode:
                        dialogueContainerSO.StartDatas.Add(SaveNodeData(startNode));
                        break;
                    case UINode uiNode:
                        dialogueContainerSO.UIDatas.Add(SaveNodeData(uiNode));
                        break;
                    case BackgroundNode backgroundNode:
                        dialogueContainerSO.BackgroundDatas.Add(SaveNodeData(backgroundNode));
                        break;
                    case CharacterNode characterNode:
                        dialogueContainerSO.CharacterDatas.Add(SaveNodeData(characterNode));
                        break;
                    case RepliqueNode repliqueNode:
                        dialogueContainerSO.RepliqueDatas.Add(SaveNodeData(repliqueNode));
                        break;
                    case EndNode endNode:
                        dialogueContainerSO.EndDatas.Add(SaveNodeData(endNode));
                        break;
                    case EventNode eventNode:
                        dialogueContainerSO.EventDatas.Add(SaveNodeData(eventNode));
                        break;
                    case BranchNode branchNode:
                        dialogueContainerSO.BranchDatas.Add(SaveNodeData(branchNode));
                        break;
                    case ChoiceNode choiceNode:
                        dialogueContainerSO.ChoiceDatas.Add(SaveNodeData(choiceNode));
                        break;
                    case DelayNode delayNode:
                        dialogueContainerSO.DelayDatas.Add(SaveNodeData(delayNode));
                        break;
                    default:
                        break;
                }
            });

        }

        private void SaveGroups(DialogueContainerSO dialogueContainerSO)
        {
            dialogueContainerSO.GroupDatas.Clear();

            Groups.ForEach(group =>
            {
                dialogueContainerSO.GroupDatas.Add(SaveGroupData(group));
            });
        }

        private void SaveNotes(DialogueContainerSO dialogueContainerSO)
        {
            dialogueContainerSO.NoteDatas.Clear();

            StickyNotes.ForEach(note =>
            {
                dialogueContainerSO.NoteDatas.Add(SaveNoteData(note));
            });
        }




        private StartData SaveNodeData(StartNode node)
        {
            StartData startData = new StartData()
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };

            startData.isDefault.Value = node.StartData.isDefault.Value;

            foreach (EventData_StringEventCondition stringEvents in node.StartData.StringConditions)
            {
                EventData_StringEventCondition tmp = new EventData_StringEventCondition();
                tmp.Number.Value = stringEvents.Number.Value;
                tmp.StringEvent.Value = stringEvents.StringEvent.Value;
                tmp.ConditionType.Value = stringEvents.ConditionType.Value;

                startData.StringConditions.Add(tmp);
            }

            return startData;
        }

        private UIData SaveNodeData(UINode node)
        {
            UIData uiData = new UIData()
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };

            uiData.show.Value = node.UIData.show.Value;

            return uiData;
        }

        private BackgroundData SaveNodeData(BackgroundNode node)
        {
            BackgroundData characterData = new BackgroundData
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };

            // Stocke le personnage
            BackgroundData_Transition tmpData = new BackgroundData_Transition();
            BackgroundData_Transition transition = node.BackgroundData.Transition.Value;

            tmpData.BackgroundTex.Value = transition.BackgroundTex.Value;
            tmpData.BackgroundName.Value = transition.BackgroundName.Value;
            tmpData.StartSettings.Value = transition.StartSettings.Value;
            tmpData.EndSettings.Value = transition.EndSettings.Value;


            characterData.Transition.Value = tmpData;

            return characterData;
        }

        private CharacterData SaveNodeData(CharacterNode node)
        {
            CharacterData characterData = new CharacterData
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };

            // Set ID (Instantiates the modules by ID)
            for (int i = 0; i < node.CharacterData.Characters.Count; i++)
            {
                node.CharacterData.Characters[i].ID.Value = i;
            }

            foreach (CharacterData_CharacterSO character in node.CharacterData.Characters)
            {
                // Stores the character
                CharacterData_CharacterSO tmpData = new CharacterData_CharacterSO();

                tmpData.ID.Value = character.ID.Value;
                tmpData.Character.Value = character.Character.Value;
                tmpData.CharacterName.Value = character.CharacterName.Value;
                tmpData.CharacterNames.AddRange(character.CharacterNames);
                tmpData.Sprite.Value = character.Sprite.Value;
                tmpData.Mood.Value = character.Mood.Value;
                tmpData.FaceDirection.Value = character.FaceDirection.Value;
                tmpData.SidePlacement.Value = character.SidePlacement.Value;
                tmpData.UseAutoDelay.Value = character.UseAutoDelay.Value;
                tmpData.AutoDelayDuration.Value = character.AutoDelayDuration.Value;


                characterData.Characters.Add(tmpData);
            }

            return characterData;
        }

        private RepliqueData SaveNodeData(RepliqueNode node)
        {
            RepliqueData repliqueData = new RepliqueData
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };

            // Set ID (Instantiates the modules by ID)
            for (int i = 0; i < node.RepliqueData.Repliques.Count; i++)
            {
                node.RepliqueData.Repliques[i].ID.Value = i;
            }

            foreach (RepliqueData_Replique replique in node.RepliqueData.Repliques)
            {
                RepliqueData_Replique tmpData = new RepliqueData_Replique();

                tmpData.ID = replique.ID;
                tmpData.Guid = replique.Guid;
                tmpData.Texts = replique.Texts;
                tmpData.AudioClips = replique.AudioClips;

                tmpData.AppendToText.Value = replique.AppendToText.Value;
                tmpData.CanClickOnContinue.Value = replique.CanClickOnContinue.Value;
                tmpData.OverrideWriteSpeed.Value = replique.OverrideWriteSpeed.Value;
                tmpData.WriteSpeed.Value = replique.WriteSpeed.Value;
                tmpData.UseAutoDelay.Value = replique.UseAutoDelay.Value;
                tmpData.AutoDelayDuration.Value = replique.AutoDelayDuration.Value;

                repliqueData.Repliques.Add(tmpData);
            }

            return repliqueData;
        }

        private EndData SaveNodeData(EndNode node)
        {
            EndData endData = new EndData()
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };
            endData.EndNodeType.Value = node.EndData.EndNodeType.Value;

            return endData;
        }

        private EventData SaveNodeData(EventNode node)
        {
            EventData eventData = new EventData()
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };

            // Set ID (Instantiates the modules by ID)
            for (int i = 0; i < node.EventData.Events.Count; i++)
            {
                node.EventData.Events[i].ID.Value = i;
            }

            foreach (NodeData_BaseContainer item in node.EventData.Events)
            {

                switch (item)
                {
                    // Save Dialogue Event
                    case EventData_ScriptableEvent scriptableEvent:

                        EventData_ScriptableEvent tmpScriptable = new EventData_ScriptableEvent();
                        tmpScriptable.ID.Value = scriptableEvent.ID.Value;
                        tmpScriptable.ScriptableObject.Value = scriptableEvent.ScriptableObject.Value;

                        eventData.ScriptableEvents.Add(tmpScriptable);
                        break;

                    // Save String Event
                    case EventData_StringEventModifier stringEvent:

                        EventData_StringEventModifier tmpString = new EventData_StringEventModifier();
                        tmpString.ID.Value = stringEvent.ID.Value;
                        tmpString.Number.Value = stringEvent.Number.Value;
                        tmpString.StringEvent.Value = stringEvent.StringEvent.Value;
                        tmpString.ModifierType.Value = stringEvent.ModifierType.Value;

                        eventData.StringEvents.Add(tmpString);
                        break;
                }
            }

            return eventData;
        }

        private BranchData SaveNodeData(BranchNode node)
        {
            List<Edge> tmpEdges = Edges.Where(x => x.output.node == node).Cast<Edge>().ToList();

            Edge trueOutput = Edges.FirstOrDefault(x => x.output.node == node && x.output.portName == "True");
            Edge flaseOutput = Edges.FirstOrDefault(x => x.output.node == node && x.output.portName == "False");

            BranchData branchData = new BranchData()
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
                TrueNodeGuid = (trueOutput != null ? (trueOutput.input.node as BaseNode).NodeGuid : string.Empty),
                FalseNodeGuid = (flaseOutput != null ? (flaseOutput.input.node as BaseNode).NodeGuid : string.Empty),
            };

            foreach (EventData_StringEventCondition stringEvents in node.BranchData.StringConditions)
            {
                EventData_StringEventCondition tmp = new EventData_StringEventCondition();
                tmp.Number.Value = stringEvents.Number.Value;
                tmp.StringEvent.Value = stringEvents.StringEvent.Value;
                tmp.ConditionType.Value = stringEvents.ConditionType.Value;

                branchData.StringConditions.Add(tmp);
            }

            return branchData;
        }

        private ChoiceData SaveNodeData(ChoiceNode node)
        {
            ChoiceData choiceData = new ChoiceData()
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };

            // Set ID (Instantiates the modules by ID)
            for (int i = 0; i < node.ChoiceData.Choices.Count; i++)
            {
                node.ChoiceData.Choices[i].ID.Value = i;
            }



            foreach (ChoiceData_Container choice in node.ChoiceData.Choices)
            {


                //Init
                ChoiceData_Container tmpChoice = new ChoiceData_Container();

                tmpChoice.Guid = choice.Guid;
                tmpChoice.ID = choice.ID;
                tmpChoice.Texts = choice.Texts;
                tmpChoice.AudioClips = choice.AudioClips;
                tmpChoice.ChoiceStateType.Value = choice.ChoiceStateType.Value;


                //Conditions
                foreach (ChoiceData_Condition condition in choice.Conditions)
                {
                    ChoiceData_Condition tmpCondition = new ChoiceData_Condition();

                    tmpCondition.Guid.Value = condition.Guid.Value;
                    tmpCondition.DescriptionsIfNotMet = condition.DescriptionsIfNotMet;

                    EventData_StringEventCondition tmpEvent = new EventData_StringEventCondition();

                    tmpEvent.StringEvent.Value = condition.StringCondition.StringEvent.Value;
                    tmpEvent.Number.Value = condition.StringCondition.Number.Value;
                    tmpEvent.ConditionType.Value = condition.StringCondition.ConditionType.Value;

                    tmpCondition.StringCondition = tmpEvent;

                    tmpChoice.Conditions.Add(tmpCondition);
                }



                // Choice Ports
                NodeData_Port portData = new NodeData_Port();

                portData.OutputGuid = string.Empty;
                portData.InputGuid = string.Empty;
                portData.PortGuid = choice.LinkedPort.PortGuid;

                foreach (Edge edge in Edges)
                {
                    if (edge.output.portName == choice.LinkedPort.PortGuid)
                    {
                        portData.OutputGuid = (edge.output.node as BaseNode).NodeGuid;
                        portData.InputGuid = (edge.input.node as BaseNode).NodeGuid;
                    }
                }

                tmpChoice.LinkedPort = portData;


                choiceData.Choices.Add(tmpChoice);
            }


            return choiceData;
        }

        private DelayData SaveNodeData(DelayNode node)
        {
            DelayData endData = new DelayData()
            {
                NodeGuid = node.NodeGuid,
                Position = node.GetPosition().position,
            };
            endData.Delay.Value = node.DelayData.Delay.Value;

            return endData;
        }




        private GroupData SaveGroupData(Group group)
        {
            GroupData groupData = new GroupData
            {
                GroupName = group.title,
                Position = group.GetPosition().position,
            };

            //Get all BaseNodes in Group
            _collectedNodes = group.containedElements.ToList().FindAll(node => node is BaseNode).Cast<BaseNode>().ToList();

            //For each BaseNode, we store its Guid to reattach it to the Group during the loading phase
            for (int i = 0; i < _collectedNodes.Count; i++)
            {
                BaseNode node = _collectedNodes[i] as BaseNode;
                groupData.ContainedGuids.Add(node.NodeGuid);
            }

            return groupData;
        }

        private StickyNoteData SaveNoteData(StickyNote note)
        {
            StickyNoteData noteData = new StickyNoteData
            {
                Title = note.title,
                Content = note.contents,
                Position = note.GetPosition().position,
            };

            return noteData;
        }


        #endregion







        #region Load

        private void ClearGraph()
        {
            Edges.ForEach(edge => _graphView.RemoveElement(edge));

            foreach (BaseNode node in Nodes)
            {
                _graphView.RemoveElement(node);
            }

            Groups.ForEach(group => _graphView.RemoveElement(group));
            StickyNotes.ForEach(note => _graphView.RemoveElement(note));
        }

        private void GenerateNodes(DialogueContainerSO dialogueContainer)
        {
            // Start
            foreach (StartData savedData in dialogueContainer.StartDatas)
            {
                StartNode tmpNode = _graphView.CreateNode<StartNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;
                tmpNode.StartData.isDefault.Value = savedData.isDefault.Value;

                foreach (EventData_StringEventCondition item in savedData.StringConditions)
                {
                    tmpNode.AddCondition(item);
                }

                tmpNode.ReloadFields();
                _graphView.AddElement(tmpNode);
            }

            // UI
            foreach (UIData savedData in dialogueContainer.UIDatas)
            {
                UINode tmpNode = _graphView.CreateNode<UINode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;
                tmpNode.UIData.show.Value = savedData.show.Value;


                tmpNode.ReloadFields();
                _graphView.AddElement(tmpNode);
            }

            // End Node 
            foreach (EndData savedData in dialogueContainer.EndDatas)
            {
                EndNode tmpNode = _graphView.CreateNode<EndNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;
                tmpNode.EndData.EndNodeType.Value = savedData.EndNodeType.Value;

                tmpNode.ReloadFields();
                _graphView.AddElement(tmpNode);
            }

            // Event Node
            foreach (EventData savedData in dialogueContainer.EventDatas)
            {
                EventNode tmpNode = _graphView.CreateNode<EventNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;



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


                tmpNode.ReloadFields();
                _graphView.AddElement(tmpNode);
            }

            // Breach Node
            foreach (BranchData savedData in dialogueContainer.BranchDatas)
            {
                BranchNode tmpNode = _graphView.CreateNode<BranchNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;

                foreach (EventData_StringEventCondition item in savedData.StringConditions)
                {
                    tmpNode.AddCondition(item);
                }

                tmpNode.ReloadFields();
                _graphView.AddElement(tmpNode);
            }

            // Choice Node
            foreach (ChoiceData savedData in dialogueContainer.ChoiceDatas)
            {
                ChoiceNode tmpNode = _graphView.CreateNode<ChoiceNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;



                //As we create a choice by default in the constructor, we destroy it here
                //in order to avoid duplicates
                tmpNode.DeleteBox(tmpNode.ChoiceData.Choices[0].BoxContainer);
                NodeBuilder.DeleteChoicePort(tmpNode, tmpNode.ChoiceData.Choices[0].LinkedPort.Port);
                tmpNode.ChoiceData.Choices.RemoveAt(0);




                List<ChoiceData_Container> newChoices = new List<ChoiceData_Container>();
                newChoices.AddRange(savedData.Choices);


                newChoices.Sort(delegate (ChoiceData_Container x, ChoiceData_Container y)
                {
                    return x.ID.Value.CompareTo(y.ID.Value);
                });

                foreach (ChoiceData_Container choice in newChoices)
                {
                    tmpNode.AddChoice(choice);
                }


                tmpNode.ReloadFields();
                tmpNode.ReloadLanguage();
                
                _graphView.AddElement(tmpNode);
            }

            // Background Node
            foreach (BackgroundData savedData in dialogueContainer.BackgroundDatas)
            {
                BackgroundNode tmpNode = _graphView.CreateNode<BackgroundNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;




                //As we create a background by default in the constructor, we destroy it here
                //in order to avoid duplicates
                tmpNode.DeleteBox(tmpNode.BackgroundData.Transition.Value.BoxContainer);
                tmpNode.BackgroundData.Transition.Value = null;


                tmpNode.AddBackground(savedData.Transition.Value);

                tmpNode.ReloadFields();

                _graphView.AddElement(tmpNode);
            }

            // Character Node
            foreach (CharacterData savedData in dialogueContainer.CharacterDatas)
            {
                CharacterNode tmpNode = _graphView.CreateNode<CharacterNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;




                //As we create a character by default in the constructor, we destroy it here
                //in order to avoid duplicates
                tmpNode.DeleteBox(tmpNode.CharacterData.Characters[0].BoxContainer);
                tmpNode.CharacterData.Characters.RemoveAt(0);




                List<CharacterData_CharacterSO> newCharacters = new List<CharacterData_CharacterSO>();
                newCharacters.AddRange(savedData.Characters);

                newCharacters.Sort(delegate (CharacterData_CharacterSO x, CharacterData_CharacterSO y)
                {
                    return x.ID.Value.CompareTo(y.ID.Value);
                });

                foreach (CharacterData_CharacterSO character in newCharacters)
                {
                    tmpNode.AddCharacter(character);
                }

                tmpNode.ReloadFields();
                tmpNode.ReloadLanguage();

                _graphView.AddElement(tmpNode);
            }

            // Replique Node
            foreach (RepliqueData savedData in dialogueContainer.RepliqueDatas)
            {
                RepliqueNode tmpNode = _graphView.CreateNode<RepliqueNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;




                //As we create a text field by default in the constructor, we destroy it here
                //in order to avoid duplicates
                tmpNode.DeleteBox(tmpNode.RepliqueData.Repliques[0].BoxContainer);
                tmpNode.RepliqueData.Repliques.RemoveAt(0);




                List<RepliqueData_Replique> newRepliques = new List<RepliqueData_Replique>();
                newRepliques.AddRange(savedData.Repliques);

                newRepliques.Sort(delegate (RepliqueData_Replique x, RepliqueData_Replique y)
                {
                    return x.ID.Value.CompareTo(y.ID.Value);
                });

                foreach (RepliqueData_Replique replique in newRepliques)
                {
                    tmpNode.AddReplique(replique);
                }

                tmpNode.ReloadFields();
                tmpNode.ReloadLanguage();
                
                _graphView.AddElement(tmpNode);
            }

            // Delay Node 
            foreach (DelayData savedData in dialogueContainer.DelayDatas)
            {
                DelayNode tmpNode = _graphView.CreateNode<DelayNode>(savedData.Position);
                tmpNode.NodeGuid = savedData.NodeGuid;
                tmpNode.DelayData.Delay.Value = savedData.Delay.Value;

                tmpNode.ReloadFields();
                _graphView.AddElement(tmpNode);
            }
        }

        private void ConnectNodes(DialogueContainerSO dialogueContainer)
        {
            // Make connection for all node.
            for (int i = 0; i < Nodes.Count; i++)
            {
                List<LinkData> connections = dialogueContainer.linkDatas.Where(edge => edge.BaseNodeGuid == Nodes[i].NodeGuid).ToList();

                List<Port> allOutputPorts = Nodes[i].outputContainer.Children().Where(x => x is Port).Cast<Port>().ToList();

                for (int j = 0; j < connections.Count; j++)
                {
                    string targetNodeGuid = connections[j].TargetNodeGuid;
                    BaseNode targetNode = Nodes.First(node => node.NodeGuid == targetNodeGuid);

                    if (targetNode == null)
                        continue;

                    foreach (Port item in allOutputPorts)
                    {
                        if (item.portName == connections[j].BasePortName)
                        {
                            LinkNodesTogether(item, (Port)targetNode.inputContainer[0]);
                        }
                    }
                }
            }
        }

        private void LinkNodesTogether(Port outputPort, Port inputPort)
        {
            Edge tmpEdge = new Edge()
            {
                output = outputPort,
                input = inputPort
            };
            tmpEdge.input.Connect(tmpEdge);
            tmpEdge.output.Connect(tmpEdge);

            _graphView.Add(tmpEdge);
        }

        private void GroupNodes(DialogueContainerSO dialogueContainer)
        {
            foreach (GroupData savedData in dialogueContainer.GroupDatas)
            {
                Group tmpGroup = GraphBuilder.AddGroup(_graphView, savedData.GroupName, savedData.Position);

                tmpGroup.AddElements(Nodes.Where(node => savedData.ContainedGuids.Contains(node.NodeGuid)));

                _graphView.AddElement(tmpGroup);
            }
        }

        private void AddStickyNotes(DialogueContainerSO dialogueContainer)
        {
            foreach (StickyNoteData savedData in dialogueContainer.NoteDatas)
            {
                StickyNote tmpNote = GraphBuilder.AddStickyNote(savedData.Title, savedData.Content, savedData.Position);

                _graphView.AddElement(tmpNote);
            }
        }

        #endregion
    }
}