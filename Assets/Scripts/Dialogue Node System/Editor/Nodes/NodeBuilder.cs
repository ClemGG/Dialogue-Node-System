using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem.Editor
{
    /// <summary>
    /// Contient toutes les méthodes de création de UIElements pour les nodes (EnumField, ObjectField, Button, etc.)
    /// Ca nous permet d'éviter d'encombrer la BaseNode de toutes ces fonctions.
    /// </summary>
    public static class NodeBuilder
    {



        #region Node


        /// <summary>
        /// Ajoute un port au conteneur correspondant à sa Direction
        /// </summary>
        /// <param name="name">Le nom du port affiché sur la node.</param>
        /// <param name="portDirection">Indique s'il s'agit d'un port d'entrée ou de sortie.</param>
        /// <param name="capacity">Indique s'il n'accepte qu'une seule ou plusieurs connexions.</param>
        public static Port AddPort(BaseNode node, string name, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = GetPortInstance(node, portDirection, capacity);
            port.portName = name;


            //A voir si on peut changer les ports d'emplacement pour afficher les nodes verticalement
            if (portDirection == Direction.Output)
                node.outputContainer.Add(port);
            else
                node.inputContainer.Add(port);

            return port;
        }

        public static Port GetPortInstance(BaseNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port newPort = node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
            return newPort;
        }

        /// <summary>
        /// Récupère la liste des ports de sortie d'une node et supprime le port sélectionné.
        /// </summary>
        /// <param name="node">La node à mettre à jour.</param>
        /// <param name="portsList">La liste de ports (passé directement pour ne pas être limité par l'héritage des Data_Containers)</param>
        /// <param name="port">Le port à supprimer</param>
        public static void DeleteChoicePort(BaseNode node, Port port)
        {
            IEnumerable<Edge> portEdge = node.GraphView.edges.ToList().Where(edge => edge.output == port);

            if (portEdge.Any())
            {
                Edge edge = portEdge.First();
                edge.input.Disconnect(edge);
                edge.output.Disconnect(edge);
                node.GraphView.RemoveElement(edge);
            }

            node.outputContainer.Remove(port);

            // Refresh
            node.RefreshPorts();
            node.RefreshExpandedState();
        }




        #endregion




        #region Get New Fields


        #region Node

        public static Box NewBox(BaseNode node, params string[] USSx)
        {
            // Make Container Box
            Box boxContainer = new Box();
            boxContainer.AddStyle(USSx);
            boxContainer.AddStyle("Box");

            node.mainContainer.Add(boxContainer);

            return boxContainer;
        }

        public static Box NewBox(VisualElement container, params string[] USSx)
        {
            // Make Container Box
            Box boxContainer = new Box();
            boxContainer.AddStyle(USSx);
            boxContainer.AddStyle("Box");

            container.Add(boxContainer);

            return boxContainer;
        }


        /// <summary>
        /// Get a new Label
        /// </summary>
        /// <param name="labelName">Text in the label</param>
        /// <returns></returns>
        public static Label NewLabel(string labelName, params string[] USSx)
        {
            Label label = new Label(labelName);

            // Set uss class for stylesheet.
            label.AddStyle(USSx);

            return label;
        }


        /// <summary>
        /// Get a new Label
        /// </summary>
        /// <param name="labelName">Text in the label</param>
        /// <returns></returns>
        public static Label NewLabel(VisualElement container, string labelName, params string[] USSx)
        {
            Label label = new Label(labelName);

            // Set uss class for stylesheet.
            label.AddStyle(USSx);

            container.Add(label);

            return label;
        }

        /// <summary>
        /// Get a new Button
        /// </summary>
        /// <param name="btnText">Text in the button</param>
        /// <returns></returns>
        public static Button NewButton(string btnText, Action onClicked, params string[] USSx)
        {
            Button btn = new Button(onClicked)
            {
                text = btnText,
            };

            // Set uss class for stylesheet.
            btn.AddStyle(USSx);

            return btn;
        }

        /// <summary>
        /// Get a new Button
        /// </summary>
        /// <param name="btnText">Text in the button</param>
        /// <returns></returns>
        public static Button NewButton(VisualElement container, string btnText, Action onClicked, params string[] USSx)
        {
            Button btn = new Button(onClicked)
            {
                text = btnText,
            };

            // Set uss class for stylesheet.
            btn.AddStyle(USSx);
            container.Add(btn);

            return btn;
        }


        /// <summary>
        /// Get a new Button
        /// </summary>
        /// <param name="btnText">Text in the button</param>
        /// <returns></returns>
        public static Button NewTitleButton(BaseNode node, string btnText, Action onClicked, params string[] USSx)
        {
            Button btn = new Button(onClicked)
            {
                text = btnText,
            };

            // Set uss class for stylesheet.
            btn.AddStyle(USSx);
            node.titleButtonContainer.Add(btn);

            return btn;
        }



        /// <summary>
        /// Crée une barre d'outils dans le coin supérieur droit de la node.
        /// </summary>
        /// <param name="node">La node à laquelle ajouter la barre</param>
        /// <param name="title">Le nom de la barre</param>
        /// <param name="USSx">Le style USS à lui ajotuer</param>
        /// <returns></returns>
        public static ToolbarMenu NewToolbar(BaseNode node, string title, params string[] USSx)
        {
            ToolbarMenu toolbar = new ToolbarMenu();
            toolbar.text = title;
            toolbar.AddStyle(USSx);

            node.titleContainer.Add(toolbar);
            return toolbar;
        }

        /// <summary>
        /// Ajoute des actions à la barre d'outils passée en paramètre
        /// </summary>
        /// <param name="toolbar">La toolbar à laquelle attacher les actions</param>
        /// <param name="menuActions">Les actions et leur titre</param>
        public static void AddMenuActions(this ToolbarMenu toolbar, params (string menuActionName, Action<DropdownMenuAction> menuAction)[] menuActions)
        {
            for (int i = 0; i < menuActions.Length; i++)
            {
                toolbar.menu.AppendAction(menuActions[i].menuActionName, menuActions[i].menuAction);
            }
        }




        #endregion


        #region Event Node


        /// <summary>
        /// Add String Modifier Event to UI element.
        /// </summary>
        /// <param name="stringEventModifier">The List<EventData_StringModifier> that EventData_StringModifier should be added to.</param>
        /// <param name="stringEvent">EventData_StringModifier that should be use.</param>
        public static void AddStringModifierEvent(BaseNode node, EventData eventData, EventData_StringEventModifier stringEvent = null)
        {
            EventData_StringEventModifier tmp = new EventData_StringEventModifier();

            // If we paramida value is not null we load in values.
            if (stringEvent != null)
            {
                tmp.StringEvent.Value = stringEvent.StringEvent.Value;
                tmp.Number.Value = stringEvent.Number.Value;
                tmp.ModifierType.Value = stringEvent.ModifierType.Value;
            }
            eventData.Events.Add(tmp);





            // Container of all object.
            Box boxContainer = new Box();
            Box boxfloatField = new Box();
            boxContainer.AddStyle("StringEventBox");
            boxfloatField.AddStyle("StringEventBoxfloatField");

            // Text.
            TextField textField = NewTextField(tmp.StringEvent, "String Event", "stringEvent");

            // ID number.
            FloatField floatField = NewFloatField(tmp.Number, "StringEventInt");


            // String Event Modifier
            Action action = () => ShowHide_StringEventModifierType(tmp.ModifierType.Value, boxfloatField);
            // EnumField String Event Modifier
            EnumField enumField = NewEnumField(tmp.ModifierType, action, "StringEventEnum");
            // Run the show and hide.
            ShowHide_StringEventModifierType(tmp.ModifierType.Value, boxfloatField);

            // Remove button.
            Action onClicked = () =>
            {
                eventData.Events.Remove(tmp); 
                node.DeleteBox(boxContainer);
            };
            Button btn = NewButton("X", onClicked, "removeBtn");


            //Ajoute les boutons pour déplacer le container
            tmp.BtnsBox = AddMoveButtons(node, tmp, boxContainer);
            tmp.EventBox = boxContainer;

            // Add it to the box
            boxContainer.Add(textField);
            boxContainer.Add(enumField);
            boxfloatField.Add(floatField);
            boxContainer.Add(boxfloatField);
            boxContainer.Add(btn);

            node.mainContainer.Add(boxContainer);
            node.RefreshExpandedState();
        }


        /// <summary>
        /// Ajoute un ScriptableEvent à la Node, ce qui permet de lier des DialogueEventSO aux DE_EventCaller de la scène.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="eventData"></param>
        /// <param name="scriptableEvent"></param>
        public static void AddScriptableEvent(BaseNode node, EventData eventData, EventData_ScriptableEvent scriptableEvent = null)
        {
            //Pour sauvegarder l'event
            EventData_ScriptableEvent tmp = new EventData_ScriptableEvent();
            if (scriptableEvent != null)
            {
                tmp.ScriptableObject.Value = scriptableEvent.ScriptableObject.Value;
            }
            eventData.Events.Add(tmp);


            //Container
            Box boxContainer = new Box();
            boxContainer.AddStyle("EventBox");



            //Scriptable Object Event
            ObjectField eventField = NewDialogueEventField(tmp.ScriptableObject, "ObjectField");



            //Remove button
            Action onClicked = () =>
            {
                eventData.Events.Remove(tmp);
                node.DeleteBox(boxContainer);
            };
            Button removeBtn = NewButton("X", onClicked, "removeBtn");


            //Ajoute les boutons pour déplacer le container
            tmp.BtnsBox = AddMoveButtons(node, tmp, boxContainer);
            tmp.EventBox = boxContainer;

            boxContainer.Add(eventField);
            boxContainer.Add(removeBtn);

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            node.mainContainer.Add(boxContainer);
            node.RefreshExpandedState();
        }



        #endregion


        #region Choice Node


        /// <summary>
        /// Add String Condition Event to UI element.
        /// </summary>
        /// <param name="stringEventConditions">The List<EventData_StringComparer> that EventData_StringComparer should be added to.</param>
        /// <param name="stringEvent">EventData_StringComparer that should be use.</param>
        public static ChoiceData_Condition AddStringConditionEvent(BaseNode node, VisualElement element, ChoiceData_Container choice, EventData_StringEventCondition stringEvent = null)
        {
            ChoiceData_Condition condition = new ChoiceData_Condition();
            choice.Conditions.Add(condition);

            EventData_StringEventCondition tmpStringEventCondition = new EventData_StringEventCondition();

            // If we paramida value is not null we load in values.
            if (stringEvent != null)
            {
                tmpStringEventCondition.StringEvent.Value = stringEvent.StringEvent.Value;
                tmpStringEventCondition.Number.Value = stringEvent.Number.Value;
                tmpStringEventCondition.ConditionType.Value = stringEvent.ConditionType.Value;
            }
            condition.StringCondition = tmpStringEventCondition;
            condition.Guid.Value = Guid.NewGuid().ToString();




            // Container of all object.
            Box boxContainer = NewBox(element, "StringEventBox");
            Box boxfloatField = new Box();
            boxfloatField.AddStyle("StringEventBoxfloatField");



            // Text.
            TextField textField = NewTextField(tmpStringEventCondition.StringEvent, "String Event", "stringEvent");

            // ID number.
            FloatField floatField = NewFloatField(tmpStringEventCondition.Number, "StringEventInt");

            // String Event Condition
            Action tmp = () => ShowHide_StringEventConditionType(tmpStringEventCondition.ConditionType.Value, boxfloatField);
            // EnumField String Event Condition
            EnumField enumField = NewEnumField(tmpStringEventCondition.ConditionType, tmp, "StringEventEnum");
            // Run the show and hide.
            ShowHide_StringEventConditionType(tmpStringEventCondition.ConditionType.Value, boxfloatField);

            // Remove button.
            Action onClicked = () =>
            {
                choice.Conditions.Remove(condition);
                node.DeleteBox(choice.BoxContainer, element);
            };
            Button btn = NewButton("X", onClicked, "removeBtn");





            // Add it to the box
            boxContainer.Add(textField);
            boxContainer.Add(enumField);
            boxfloatField.Add(floatField);
            boxContainer.Add(boxfloatField);
            boxContainer.Add(btn);




            // Text
            TextField descTextField = NewTextLanguagesField(node, element, condition.DescriptionsIfNotMet, "Description if not met", "TextBox");
            condition.DescTextField = descTextField;


            node.RefreshExpandedState();
            return condition;
        }


        #endregion


        #region Branch Node

        /// <summary>
        /// Add String Condition Event to UI element.
        /// </summary>
        /// <param name="stringEventConditions">The List<EventData_StringComparer> that EventData_StringComparer should be added to.</param>
        /// <param name="stringEvent">EventData_StringComparer that should be use.</param>
        public static void AddStringConditionEvent(BaseNode node, List<EventData_StringEventCondition> stringEventConditions, EventData_StringEventCondition stringEvent = null)
        {
            EventData_StringEventCondition tmpStringEventCondition = new EventData_StringEventCondition();

            // If we paramida value is not null we load in values.
            if (stringEvent != null)
            {
                tmpStringEventCondition.StringEvent.Value = stringEvent.StringEvent.Value;
                tmpStringEventCondition.Number.Value = stringEvent.Number.Value;
                tmpStringEventCondition.ConditionType.Value = stringEvent.ConditionType.Value;
            }
            stringEventConditions.Add(tmpStringEventCondition);



            // Container of all object.
            Box boxContainer = NewBox(node, "StringEventBox");
            Box boxfloatField = new Box();
            boxfloatField.AddStyle("StringEventBoxfloatField");





            // Text.
            TextField textField = NewTextField(tmpStringEventCondition.StringEvent, "String Event", "stringEvent");

            // ID number.
            FloatField floatField = NewFloatField(tmpStringEventCondition.Number, "StringEventInt");

            // String Event Condition
            Action tmp = () => ShowHide_StringEventConditionType(tmpStringEventCondition.ConditionType.Value, boxfloatField);
            // EnumField String Event Condition
            EnumField enumField = NewEnumField(tmpStringEventCondition.ConditionType, tmp, "StringEventEnum");
            // Run the show and hide.
            ShowHide_StringEventConditionType(tmpStringEventCondition.ConditionType.Value, boxfloatField);

            // Remove button.
            Action onClicked = () =>
            {
                stringEventConditions.Remove(tmpStringEventCondition);
                node.DeleteBox(boxContainer);
            };
            Button btn = NewButton("X", onClicked, "removeBtn");





            // Add it to the box
            boxContainer.Add(textField);
            boxContainer.Add(enumField);
            boxfloatField.Add(floatField);
            boxContainer.Add(boxfloatField);
            boxContainer.Add(btn);



            node.RefreshExpandedState();
        }

        #endregion


        #region Replique Node


        /// <summary>
        /// Add String Condition Event to UI element.
        /// </summary>
        /// <param name="stringEventConditions">The List<EventData_StringComparer> that EventData_StringComparer should be added to.</param>
        /// <param name="stringEvent">EventData_StringComparer that should be use.</param>
        public static void AddToggleFloatField(string label, BaseNode node, VisualElement container, ContainerValue<bool> boolValue, ContainerValue<float> floatValue)
        {
            Box overrideContainer = NewBox(container, "BoxRow");
            FloatField floatField = NewFloatField(floatValue, new Vector2(0, int.MaxValue), "FloatField");

            Action onToggled = () =>  { ShowHide(boolValue.Value, floatField); };

            NewToggle(label, overrideContainer, boolValue, onToggled, "Toggle");
            overrideContainer.Add(floatField);
            ShowHide(boolValue.Value, floatField);

            container.Add(overrideContainer);

            node.RefreshExpandedState();
        }

        #endregion


        #region Character Node





        /// <summary>
        /// Get a new ObjectField with a Sprite as the Object.
        /// </summary>
        /// <param name="inputCharacter">Container_Sprite that need to be set in to the ObjectField</param>
        /// <returns></returns>
        public static ObjectField NewCharacterField(BaseNode node, CharacterData_CharacterSO newCharacter, ContainerValue<DialogueCharacterSO> inputCharacter, params string[] USSx)
        {
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(DialogueCharacterSO),
                allowSceneObjects = false,
                value = inputCharacter.Value,
            };

            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                if (value.newValue != null)
                {
                    inputCharacter.Value = value.newValue as DialogueCharacterSO;
                    newCharacter.CharacterName.Value = inputCharacter.Value.CharacterNames[(int)node.Window.SelectedLanguage];

                    newCharacter.CharacterNames.Clear();
                    newCharacter.CharacterNames.AddRange(inputCharacter.Value.CharacterNames);

                    if (newCharacter.Mood.EnumField == null)
                        newCharacter.Mood.Value = CharacterMood.Idle;

                    //Afficher l'image du nouveau perso quand on change de DialogueCharacterSO
                    newCharacter.Sprite.Value = inputCharacter.Value.GetFaceFromMood(newCharacter.Mood.Value);
                    newCharacter.SpriteField.image = inputCharacter.Value != null ? newCharacter.Sprite.Value.texture : null;


                }
                else
                {
                    inputCharacter.Value = null;
                    newCharacter.CharacterName.Value = "";
                    newCharacter.CharacterNames.Clear();
                    newCharacter.Sprite.Value = null;
                    newCharacter.SpriteField.image = null;
                }

                newCharacter.Mood.EnumField.SetValueWithoutNotify(newCharacter.Mood.Value);
                newCharacter.NameField.SetValueWithoutNotify(newCharacter.CharacterName.Value);
            });
            newCharacter.SpriteField.image = inputCharacter.Value != null ? newCharacter.Sprite.Value.texture : null;

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);

            return objectField;
        }


        /// <summary>
        /// Get a new ObjectField with a Sprite as the Object.
        /// </summary>
        /// <param name="inputSprite">Container_Sprite that need to be set in to the ObjectField</param>
        /// <param name="imagePreview">Image that need to be set as preview image</param>
        /// <returns></returns>
        public static ObjectField NewSpriteField(ContainerValue<Sprite> inputSprite, Image imagePreview, params string[] USSx)
        {
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(Sprite),
                allowSceneObjects = false,
                value = inputSprite.Value,
            };

            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                inputSprite.Value = value.newValue as Sprite;

                imagePreview.image = inputSprite.Value != null ? inputSprite.Value.texture : null;
            });
            imagePreview.image = inputSprite.Value != null ? inputSprite.Value.texture : null;

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);

            return objectField;
        }


        /// <summary>
        /// Get a new ObjectField with a Sprite as the Object.
        /// </summary>
        /// <param name="inputSprite">Container_Sprite that need to be set in to the ObjectField</param>
        /// <param name="imagePreview">Image that need to be set as preview image</param>
        /// <returns></returns>
        public static ObjectField NewSpriteField(VisualElement container, ContainerValue<Sprite> inputSprite, Image imagePreview, params string[] USSx)
        {
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(Sprite),
                allowSceneObjects = false,
                value = inputSprite.Value,
            };

            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                inputSprite.Value = value.newValue as Sprite;

                imagePreview.image = inputSprite.Value != null ? inputSprite.Value.texture : null;
            });
            imagePreview.image = inputSprite.Value != null ? inputSprite.Value.texture : null;

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);

            container.Add(objectField);

            return objectField;
        }


        #endregion


        public static Box AddMoveButtons(BaseNode node, NodeData_BaseContainer idContainer, VisualElement container)
        {
            // Buttons
            Box buttonsBox = NewBox(container, "BtnBox");

            // Move up button.
            Action onClicked = () =>
            {
                node.MoveBox(idContainer, true);
            };
            Button moveUpBtn = NewButton(buttonsBox, "", onClicked, "MoveUpBtn");

            // Move down button.
            onClicked = () =>
            {
                node.MoveBox(idContainer, false);
            };
            Button moveDownBtn = NewButton(buttonsBox, "", onClicked, "MoveDownBtn");


            return buttonsBox;
        }








        // value's --------------------------------------------------------------------------

        #region Values


        #region Toggle

        /// <summary>
        /// Get a new Toggle.
        /// </summary>
        /// <param name="inputValue">Container_Bool that need to be set in to the Toggle</param>
        /// <returns></returns>
        public static Toggle NewToggle(VisualElement container, ContainerValue<bool> inputValue, params string[] USSx)
        {
            Toggle toggle = new Toggle()
            {
                value = inputValue.Value
            };

            // When we change the variable from graph view.
            toggle.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
            });
            toggle.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            toggle.AddStyle(USSx);
            container.Add(toggle);

            return toggle;
        }


        /// <summary>
        /// Get a new Toggle.
        /// </summary>
        /// <param name="inputValue">Container_Bool that need to be set in to the Toggle</param>
        /// <returns></returns>
        public static Toggle NewToggle(string label, VisualElement container, ContainerValue<bool> inputValue, params string[] USSx)
        {
            Toggle toggle = new Toggle()
            {
                value = inputValue.Value,
                label = label
            };

            // When we change the variable from graph view.
            toggle.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
            });
            toggle.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            toggle.AddStyle(USSx);

            //Add elements to container
            //NewLabel(container, label, "ToggleLabel");
            container.Add(toggle);

            return toggle;
        }


        /// <summary>
        /// Get a new Toggle.
        /// </summary>
        /// <param name="inputValue">Container_Bool that need to be set in to the Toggle</param>
        /// <returns></returns>
        public static Toggle NewToggle(VisualElement container, ContainerValue<bool> inputValue, Action onToggled, params string[] USSx)
        {
            Toggle toggle = new Toggle()
            {
                value = inputValue.Value
            };

            // When we change the variable from graph view.
            toggle.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
                onToggled?.Invoke();
            });
            toggle.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            toggle.AddStyle(USSx);
            container.Add(toggle);

            return toggle;
        }


        /// <summary>
        /// Get a new Toggle.
        /// </summary>
        /// <param name="inputValue">Container_Bool that need to be set in to the Toggle</param>
        /// <returns></returns>
        public static Toggle NewToggle(string label, VisualElement container, ContainerValue<bool> inputValue, Action onToggled, params string[] USSx)
        {
            Toggle toggle = new Toggle()
            {
                value = inputValue.Value,
                label = label
            };

            // When we change the variable from graph view.
            toggle.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
                onToggled?.Invoke();
            });
            toggle.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            toggle.AddStyle(USSx);

            //Add elements to container
            //NewLabel(container, label, "ToggleLabel");
            container.Add(toggle);

            return toggle;
        }

        #endregion



        #region FloatField

        /// <summary>
        /// Get a new IntegerField.
        /// </summary>
        /// <param name="inputValue">Container_Int that need to be set in to the IntegerField</param>
        /// <returns></returns>
        public static IntegerField NewIntField(ContainerValue<int> inputValue, params string[] USSx)
        {
            IntegerField integerField = new IntegerField();

            // When we change the variable from graph view.
            integerField.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
            });
            integerField.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            integerField.AddStyle(USSx);

            return integerField;
        }

        /// <summary>
        /// Get a new FloatField.
        /// </summary>
        /// <param name="inputValue">Container_Float that need to be set in to the FloatField</param>
        /// <returns></returns>
        public static FloatField NewFloatField(ContainerValue<float> inputValue, params string[] USSx)
        {
            FloatField floatField = new FloatField();

            // When we change the variable from graph view.
            floatField.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
            });
            floatField.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            floatField.AddStyle(USSx);

            return floatField;
        }


        /// <summary>
        /// Get a new FloatField.
        /// </summary>
        /// <param name="inputValue">Container_Float that need to be set in to the FloatField</param>
        /// <returns></returns>
        public static FloatField NewFloatField(ContainerValue<float> inputValue, Vector2 clampValues, params string[] USSx)
        {
            FloatField floatField = new FloatField();

            // When we change the variable from graph view.
            floatField.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = Mathf.Clamp(value.newValue, clampValues.x, clampValues.y);
            });
            floatField.SetValueWithoutNotify(Mathf.Clamp(inputValue.Value, clampValues.x, clampValues.y));

            // Set uss class for stylesheet.
            floatField.AddStyle(USSx);

            return floatField;
        }


        /// <summary>
        /// Get a new FloatField.
        /// </summary>
        /// <param name="inputValue">Container_Float that need to be set in to the FloatField</param>
        /// <returns></returns>
        public static FloatField NewFloatField(VisualElement container, ContainerValue<float> inputValue, params string[] USSx)
        {
            FloatField floatField = new FloatField();

            // When we change the variable from graph view.
            floatField.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
            });
            floatField.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            floatField.AddStyle(USSx);
            container.Add(floatField);

            return floatField;
        }


        /// <summary>
        /// Get a new FloatField.
        /// </summary>
        /// <param name="inputValue">Container_Float that need to be set in to the FloatField</param>
        /// <returns></returns>
        public static FloatField NewFloatField(VisualElement container, ContainerValue<float> inputValue, Vector2 clampValues, params string[] USSx)
        {
            FloatField floatField = new FloatField();

            // When we change the variable from graph view.
            floatField.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = Mathf.Clamp(value.newValue, clampValues.x, clampValues.y);
            });
            floatField.SetValueWithoutNotify(Mathf.Clamp(inputValue.Value, clampValues.x, clampValues.y));

            // Set uss class for stylesheet.
            floatField.AddStyle(USSx);
            container.Add(floatField);

            return floatField;
        }




        /// <summary>
        /// Get a new FloatField.
        /// </summary>
        /// <param name="inputValue">Container_Float that need to be set in to the FloatField</param>
        /// <returns></returns>
        public static FloatField NewFloatField(string label, VisualElement container, ContainerValue<float> inputValue, params string[] USSx)
        {
            FloatField floatField = new FloatField();

            // When we change the variable from graph view.
            floatField.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
            });
            floatField.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            floatField.AddStyle(USSx);

            //Add elements to container
            if (!string.IsNullOrEmpty(label))
            {
                Box b = NewBox(container, "BoxRow");
                NewLabel(b, label, "FloatLabel");
                b.Add(floatField);
                container.Add(b);
            }
            else
            {
                container.Add(floatField);
            }

            return floatField;
        }


        /// <summary>
        /// Get a new FloatField.
        /// </summary>
        /// <param name="inputValue">Container_Float that need to be set in to the FloatField</param>
        /// <returns></returns>
        public static FloatField NewFloatField(string label, VisualElement container, ContainerValue<float> inputValue, Vector2 clampValues, params string[] USSx)
        {
            FloatField floatField = new FloatField();

            // When we change the variable from graph view.
            floatField.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = Mathf.Clamp(value.newValue, clampValues.x, clampValues.y);
            });
            floatField.SetValueWithoutNotify(Mathf.Clamp(inputValue.Value, clampValues.x, clampValues.y));

            // Set uss class for stylesheet.
            floatField.AddStyle(USSx);

            //Add elements to container
            if (!string.IsNullOrEmpty(label))
            {
                Box b = NewBox(container, "BoxRow");
                NewLabel(b, label, "FloatLabel");
                b.Add(floatField);
                container.Add(b);
            }
            else
            {
                container.Add(floatField);
            }


            return floatField;
        }


        #endregion


        #region Object Fields


        /// <summary>
        /// Get a new TextField.
        /// </summary>
        /// <param name="inputValue">Container_String that need to be set in to the TextField</param>
        /// <param name="placeholderText"></param>
        /// <returns></returns>
        public static TextField NewTextField(ContainerValue<string> inputValue, string placeholderText, params string[] USSx)
        {
            TextField textField = new TextField();

            // When we change the variable from graph view.
            textField.RegisterValueChangedCallback(value =>
            {
                inputValue.Value = value.newValue;
            });
            textField.SetValueWithoutNotify(inputValue.Value);

            // Set uss class for stylesheet.
            textField.AddStyle(USSx);

            // Set Place Holder
            SetPlaceholderText(textField, placeholderText);

            return textField;
        }






        /// <summary>
        /// Get a new Image.
        /// </summary>
        /// <returns></returns>
        public static Image NewImage(params string[] USSx)
        {
            Image imagePreview = new Image();

            // Set uss class for stylesheet.
            imagePreview.AddStyle(USSx);

            return imagePreview;
        }


        /// <summary>
        /// Get a new Image.
        /// </summary>
        /// <returns></returns>
        public static Image NewImage(VisualElement container = null, params string[] USSx)
        {
            Image imagePreview = new Image();

            // Set uss class for stylesheet.
            imagePreview.AddStyle(USSx);
            if (container != null) container.Add(imagePreview);

            return imagePreview;
        }







        /// <summary>
        /// Get a new ObjectField with a Container_DialogueEventSO as the Object.
        /// </summary>
        /// <param name="inputDialogueEventSO">Container_DialogueEventSO that need to be set in to the ObjectField</param>
        /// <returns></returns>
        public static ObjectField NewDialogueEventField(ContainerValue<DialogueEventSO> inputDialogueEventSO, params string[] USSx)
        {
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(DialogueEventSO),
                allowSceneObjects = false,
                value = inputDialogueEventSO.Value,
            };

            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                inputDialogueEventSO.Value = value.newValue as DialogueEventSO;
            });
            objectField.SetValueWithoutNotify(inputDialogueEventSO.Value);

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);

            return objectField;
        }



        public static ObjectField NewObjectField<T>(string label, VisualElement container, ContainerValue<T> inputObject, params string[] USSx) where T : UnityEngine.Object
        {
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(T),
                allowSceneObjects = false,
                value = inputObject.Value,
            };



            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                inputObject.Value = value.newValue as T;
            });
            objectField.SetValueWithoutNotify(inputObject.Value);

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);


            if (!string.IsNullOrEmpty(label))
            {
                Box b = NewBox(container, "BoxRow");
                NewLabel(b, label, "ObjectLabel");
                b.Add(objectField);
                container.Add(b);
            }
            else
            {
                container.Add(objectField);
            }


            return objectField;
        }

        public static ObjectField NewObjectField<T>(string label, VisualElement container, ContainerValue<T> inputObject, Action<UnityEngine.Object> onValueChanged, params string[] USSx) where T : UnityEngine.Object
        {
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(T),
                allowSceneObjects = false,
                value = inputObject.Value,
            };



            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                inputObject.Value = value.newValue as T;
                onValueChanged?.Invoke(value.newValue);
            });
            objectField.SetValueWithoutNotify(inputObject.Value);

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);


            if (!string.IsNullOrEmpty(label))
            {
                Box b = NewBox(container, "BoxRow");
                NewLabel(b, label, "ObjectLabel");
                b.Add(objectField);
                container.Add(b);
            }
            else
            {
                container.Add(objectField);
            }


            return objectField;
        }

        #endregion


        #region ColorFields


        public static ColorField NewColorField(string label, VisualElement container, ContainerValue<Color> inputColor, params string[] USSx)
        {
            ColorField colorField = new ColorField()
            {
                value = inputColor.Value
            };



            // When we change the variable from graph view.
            colorField.RegisterValueChangedCallback(value =>
            {
                inputColor.Value = value.newValue;
            });
            colorField.SetValueWithoutNotify(inputColor.Value);

            // Set uss class for stylesheet.
            colorField.AddStyle(USSx);


            if (!string.IsNullOrEmpty(label))
            {
                Box b = NewBox(container, "BoxRow");
                NewLabel(b, label, "ObjectLabel");
                b.Add(colorField);
                container.Add(b);
            }
            else
            {
                container.Add(colorField);
            }


            return colorField;
        }

        #endregion


        #endregion



        // Enum's --------------------------------------------------------------------------

        #region Enums

        /// <summary>
        /// Generic enum constructor.
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="container">The object to add this field to</param>
        /// <param name="inputEnumValue">The value we want to modify</param>
        /// <param name="USSx">USS Style</param>
        /// <returns></returns>
        public static EnumField NewEnumField<T>(ContainerEnum<T> inputEnumValue, params string[] USSx) where T : System.Enum
        {
            EnumField enumField = new EnumField()
            {
                value = inputEnumValue.Value
            };
            enumField.Init(inputEnumValue.Value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                inputEnumValue.Value = (T)value.newValue;
            });
            enumField.SetValueWithoutNotify(inputEnumValue.Value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            inputEnumValue.EnumField = enumField;
            return enumField;
        }

        /// <summary>
        /// Generic enum constructor.
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="container">The object to add this field to</param>
        /// <param name="inputEnumValue">The value we want to modify</param>
        /// <param name="USSx">USS Style</param>
        /// <returns></returns>
        public static EnumField NewEnumField<T>(ContainerEnum<T> inputEnumValue, Action onChanged = null, params string[] USSx) where T : System.Enum
        {
            EnumField enumField = new EnumField()
            {
                value = inputEnumValue.Value
            };
            enumField.Init(inputEnumValue.Value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                inputEnumValue.Value = (T)value.newValue;
                onChanged?.Invoke();
            });
            enumField.SetValueWithoutNotify(inputEnumValue.Value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            inputEnumValue.EnumField = enumField;
            return enumField;
        }

        /// <summary>
        /// Generic enum constructor.
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="container">The object to add this field to</param>
        /// <param name="inputEnumValue">The value we want to modify</param>
        /// <param name="USSx">USS Style</param>
        /// <returns></returns>
        public static EnumField NewEnumField<T>(string label, VisualElement container, ContainerEnum<T> inputEnumValue, params string[] USSx) where T : System.Enum
        {
            EnumField enumField = new EnumField()
            {
                value = inputEnumValue.Value
            };
            enumField.Init(inputEnumValue.Value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                inputEnumValue.Value = (T)value.newValue;
            });
            enumField.SetValueWithoutNotify(inputEnumValue.Value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            inputEnumValue.EnumField = enumField;

            if (!string.IsNullOrEmpty(label))
            {
                Box b = NewBox(container, "BoxRow");
                NewLabel(b, label, "EnumLabel");
                b.Add(enumField);
                container.Add(b);
            }
            else
            {
                container.Add(enumField);
            }

            return enumField;
        }

        /// <summary>
        /// Generic enum constructor.
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="container">The object to add this field to</param>
        /// <param name="inputEnumValue">The value we want to modify</param>
        /// <param name="USSx">USS Style</param>
        /// <returns></returns>
        public static EnumField NewEnumField<T>(string label, VisualElement container, ContainerEnum<T> inputEnumValue, Action onChanged = null, params string[] USSx) where T : System.Enum
        {
            EnumField enumField = new EnumField()
            {
                value = inputEnumValue.Value
            };
            enumField.Init(inputEnumValue.Value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                inputEnumValue.Value = (T)value.newValue;
                onChanged?.Invoke();
            });
            enumField.SetValueWithoutNotify(inputEnumValue.Value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            inputEnumValue.EnumField = enumField;
            container.Add(enumField);

            if (!string.IsNullOrEmpty(label))
            {
                Box b = NewBox(container, "BoxRow");
                NewLabel(b, label, "EnumLabel");
                b.Add(enumField);
                container.Add(b);
            }
            else
            {
                container.Add(enumField);
            }

            return enumField;
        }


        #endregion

        // Translated Fields --------------------------------------------------------------------------

        #region Translated Fields


        /// <summary>
        /// Get a new TextField that use a List<LanguageGeneric<string>> text.
        /// </summary>
        /// <param name="texts">List of LanguageGeneric<string> Text</param>
        /// <param name="placeholderText">The text that will be displayed if the text field is empty</param>
        /// <returns></returns>
        public static TextField NewTextLanguagesField(BaseNode node, VisualElement container, List<LanguageGeneric<string>> texts, string placeholderText = "", params string[] USSx)
        {
            // Add languages
            ForEach<LanguageType>(languageType =>
            {
                texts.Add(new LanguageGeneric<string>
                {
                    Language = languageType,
                    Data = ""
                });
            });


            // Make TextField.
            TextField textField = new TextField("");

            // Add it to the reaload current language list.
            node.TextHolders.Add(new LanguageGenericHolder_Text(texts, textField, placeholderText));

            // When we change the variable from graph view.
            textField.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.Language == node.Window.SelectedLanguage).Data = value.newValue;
            });
            textField.SetValueWithoutNotify(texts.Find(text => text.Language == node.Window.SelectedLanguage).Data);

            // Text field is set to be multiline.
            textField.multiline = true;

            // Set uss class for stylesheet.
            textField.AddStyle(USSx);

            if(container != null) container.Add(textField);

            return textField;
        }


        /// <summary>
        /// Get a new ObjectField that use List<LanguageGeneric<AudioClip>>.
        /// </summary>
        /// <param name="audioClips"></param>
        /// <returns></returns>
        public static ObjectField NewAudioClipLanguagesField(BaseNode node, VisualElement container, List<LanguageGeneric<AudioClip>> audioClips, params string[] USSx)
        {
            // Add languages.
            // Add languages
            ForEach<LanguageType>(languageType =>
            {
                audioClips.Add(new LanguageGeneric<AudioClip>
                {
                    Language = languageType,
                    Data = null
                });
            });

            // Make ObjectField.
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = audioClips.Find(audioClip => audioClip.Language == node.Window.SelectedLanguage).Data,
            };

            // Add it to the reaload current language list.
            node.AudioClipHolders.Add(new LanguageGenericHolder_AudioClip(audioClips, objectField));


            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                audioClips.Find(audioClip => audioClip.Language == node.Window.SelectedLanguage).Data = value.newValue as AudioClip;
            });
            objectField.SetValueWithoutNotify(audioClips.Find(audioClip => audioClip.Language == node.Window.SelectedLanguage).Data);

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);

            if (container != null) container.Add(objectField);

            return objectField;
        }

        #endregion


        #endregion



        #region Style





        /// <summary>
        /// Set a placeholder text on a TextField.
        /// </summary>
        /// <param name="textField">TextField that need a placeholder</param>
        /// <param name="placeholder">The text that will be displayed if the text field is empty</param>
        public static void SetPlaceholderText(TextField textField, string placeholder)
        {
            string placeholderClass = TextField.ussClassName + "__placeholder";

            CheckForText();
            onFocusOut();
            textField.RegisterCallback<FocusInEvent>(evt => onFocusIn());
            textField.RegisterCallback<FocusOutEvent>(evt => onFocusOut());

            //Quand on clique sur le TextField, on efface le placeholder si le champ est vide
            void onFocusIn()
            {
                if (textField.ClassListContains(placeholderClass))
                {
                    textField.value = string.Empty;
                    textField.RemoveFromClassList(placeholderClass);
                }
            }

            //Quand on quitte le TextField, s'il est toujours vide, on ajoute le placeholder
            void onFocusOut()
            {
                if (string.IsNullOrEmpty(textField.text))
                {
                    textField.SetValueWithoutNotify(placeholder);
                    textField.AddToClassList(placeholderClass);
                }
            }

            void CheckForText()
            {
                if (!string.IsNullOrEmpty(textField.text))
                {
                    textField.RemoveFromClassList(placeholderClass);
                }
            }
        }



        /// <summary>
        /// Ajoute du style aux VisualElements
        /// </summary>
        /// <param name="element">Le VisualElement auquel appliquer le style.</param>
        /// <param name="USSx">Les classes USS à appliquer au VisualElement (liées aux StyleSheets de la node)</param>
        public static void AddStyle(this VisualElement element, params string[] USSx)
        {
            for (int i = 0; i < USSx.Length; i++)
            {
                if (!string.IsNullOrEmpty(USSx[i]))
                    element.AddToClassList(USSx[i]);
            }
        }



        /// <summary>
        /// hid and show the UI element
        /// </summary>
        /// <param name="value">modifierType</param>
        /// <param name="boxContainer">The Box that will be hidden or shown</param>
        private static void ShowHide_StringEventModifierType(StringEventModifierType value, Box boxContainer)
        {
            if (value == StringEventModifierType.SetTrue || value == StringEventModifierType.SetFalse)
            {
                ShowHide(false, boxContainer);
            }
            else
            {
                ShowHide(true, boxContainer);
            }
        }

        /// <summary>
        /// hid and show the UI element
        /// </summary>
        /// <param name="value">comparerType</param>
        /// <param name="boxContainer">The Box that will be hidden or shown</param>
        private static void ShowHide_StringEventConditionType(StringEventConditionType value, Box boxContainer)
        {
            if (value == StringEventConditionType.True || value == StringEventConditionType.False)
            {
                ShowHide(false, boxContainer);
            }
            else
            {
                ShowHide(true, boxContainer);
            }
        }




        /// <summary>
        /// Add or remove the USS Hide tag.
        /// </summary>
        /// <param name="show">true = show - flase = hide</param>
        /// <param name="element">which visual element to add the desired USS tag to</param>
        public static void ShowHide(bool show, VisualElement element)
        {
            string hideUssClass = "Hide";
            if (show == true)
            {
                element.RemoveFromClassList(hideUssClass);
            }
            else
            {
                element.AddToClassList(hideUssClass);
            }
        }


        /// <summary>
        /// Change la couleur du groupe dans la Minimap
        /// </summary>
        /// <param name="element"></param>
        /// <param name="hexColor"></param>
        public static void ChangeColorInMinimap(this GraphElement element, string hexColor)
        {
            if (ColorUtility.TryParseHtmlString(hexColor, out Color newCol))
            {
                element.elementTypeColor = newCol;
            }
        }


        /// <summary>
        /// Change la couleur du groupe dans la Minimap
        /// </summary>
        /// <param name="element"></param>
        /// <param name="hexColor"></param>
        public static void ChangeColorInMinimap(this GraphElement element, Color color)
        {
            element.elementTypeColor = color;
        }


        #endregion




        #region Language Generic Holders



        //Utilisée pour stocker les champs et leurs valeurs
        public class LanguageGenericHolder_Text
        {
            public LanguageGenericHolder_Text(List<LanguageGeneric<string>> inputText, TextField textField, string placeholderText = "placeholderText")
            {
                this.inputText = inputText;
                this.textField = textField;
                this.placeholderText = placeholderText;
            }
            public List<LanguageGeneric<string>> inputText;
            public TextField textField;
            public string placeholderText;
        }

        public class LanguageGenericHolder_AudioClip
        {
            public LanguageGenericHolder_AudioClip(List<LanguageGeneric<AudioClip>> inputAudioClip, ObjectField objectField)
            {
                this.inputAudioClip = inputAudioClip;
                this.objectField = objectField;
            }
            public List<LanguageGeneric<AudioClip>> inputAudioClip;
            public ObjectField objectField;
        }
    }

    #endregion




}
