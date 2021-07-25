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
    //Contient toutes les méthodes de création de UIElements pour les nodes (EnumField, ObjectField, Button, etc.)
    //Ca nous permet d'éviter d'encombrer la BaseNode de toutes ces fonctions.
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
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
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

        public static Box NewBox(BaseNode node, params string[] USSx)
        {
            // Make Container Box
            Box boxContainer = new Box();
            boxContainer.AddStyle(USSx);

            node.mainContainer.Add(boxContainer);

            return boxContainer;
        }

        public static Box NewBox(VisualElement container, params string[] USSx)
        {
            // Make Container Box
            Box boxContainer = new Box();
            boxContainer.AddStyle(USSx);

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







        /// <summary>
        /// Add String Modifier Event to UI element.
        /// </summary>
        /// <param name="stringEventModifier">The List<EventData_StringModifier> that EventData_StringModifier should be added to.</param>
        /// <param name="stringEvent">EventData_StringModifier that should be use.</param>
        public static void AddStringModifierEvent(BaseNode node, List<EventData_StringModifier> stringEventModifier, EventData_StringModifier stringEvent = null)
        {
            EventData_StringModifier tmpStringEventModifier = new EventData_StringModifier();

            // If we paramida value is not null we load in values.
            if (stringEvent != null)
            {
                tmpStringEventModifier.stringEvent.value = stringEvent.stringEvent.value;
                tmpStringEventModifier.number.value = stringEvent.number.value;
                tmpStringEventModifier.modifierType.value = stringEvent.modifierType.value;
            }

            stringEventModifier.Add(tmpStringEventModifier);

            // Container of all object.
            Box boxContainer = new Box();
            Box boxfloatField = new Box();
            boxContainer.AddStyle("StringEventBox");
            boxfloatField.AddStyle("StringEventBoxfloatField");

            // Text.
            TextField textField = NewTextField(tmpStringEventModifier.stringEvent, "String Event", "stringEvent");

            // ID number.
            FloatField floatField = NewFloatField(tmpStringEventModifier.number, "StringEventInt");

            // TODO: Delete maby?
            // Check for StringEventType and add the proper one.
            //EnumField enumField = null;

            // String Event Modifier
            Action tmp = () => ShowHide_StringEventModifierType(tmpStringEventModifier.modifierType.value, boxfloatField);
            // EnumField String Event Modifier
            EnumField enumField = NewStringEventModifierTypeField(tmpStringEventModifier.modifierType, tmp, "StringEventEnum");
            // Run the show and hide.
            ShowHide_StringEventModifierType(tmpStringEventModifier.modifierType.value, boxfloatField);

            // Remove button.
            Action onClicked = () =>
            {
                stringEventModifier.Remove(tmpStringEventModifier);
                node.DeleteBox(boxContainer);
            };
            Button btn = NewButton("X", onClicked, "removeBtn");


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
        /// Add String Condition Event to UI element.
        /// </summary>
        /// <param name="stringEventConditions">The List<EventData_StringComparer> that EventData_StringComparer should be added to.</param>
        /// <param name="stringEvent">EventData_StringComparer that should be use.</param>
        public static ChoiceData_Condition AddStringConditionEvent(BaseNode node, VisualElement element, ChoiceData_Container choice, EventData_StringCondition stringEvent = null)
        {
            ChoiceData_Condition condition = new ChoiceData_Condition();
            choice.conditions.Add(condition);

            EventData_StringCondition tmpStringEventCondition = new EventData_StringCondition();

            // If we paramida value is not null we load in values.
            if (stringEvent != null)
            {
                tmpStringEventCondition.stringEvent.value = stringEvent.stringEvent.value;
                tmpStringEventCondition.number.value = stringEvent.number.value;
                tmpStringEventCondition.conditionType.value = stringEvent.conditionType.value;
            }
            condition.stringCondition = tmpStringEventCondition;
            condition.guid.value = Guid.NewGuid().ToString();




            // Container of all object.
            Box boxContainer = NewBox(element, "StringEventBox");
            Box boxfloatField = new Box();
            boxfloatField.AddStyle("StringEventBoxfloatField");



            // Text.
            TextField textField = NewTextField(tmpStringEventCondition.stringEvent, "String Event", "stringEvent");

            // ID number.
            FloatField floatField = NewFloatField(tmpStringEventCondition.number, "StringEventInt");

            // String Event Condition
            Action tmp = () => ShowHide_StringEventConditionType(tmpStringEventCondition.conditionType.value, boxfloatField);
            // EnumField String Event Condition
            EnumField enumField = NewStringEventConditionTypeField(tmpStringEventCondition.conditionType, tmp, "StringEventEnum");
            // Run the show and hide.
            ShowHide_StringEventConditionType(tmpStringEventCondition.conditionType.value, boxfloatField);

            // Remove button.
            Action onClicked = () =>
            {
                choice.conditions.Remove(condition);
                node.DeleteBox(choice.choiceContainer, element);
            };
            Button btn = NewButton("X", onClicked, "removeBtn");





            // Add it to the box
            boxContainer.Add(textField);
            boxContainer.Add(enumField);
            boxfloatField.Add(floatField);
            boxContainer.Add(boxfloatField);
            boxContainer.Add(btn);




            // Text
            TextField descTextField = NewTextLanguagesField(node, element, condition.descriptionsIfNotMet, "Description if not met", "TextBox");
            condition.DescTextField = descTextField;


            node.RefreshExpandedState();
            return condition;
        }


        /// <summary>
        /// Add String Condition Event to UI element.
        /// </summary>
        /// <param name="stringEventConditions">The List<EventData_StringComparer> that EventData_StringComparer should be added to.</param>
        /// <param name="stringEvent">EventData_StringComparer that should be use.</param>
        public static void AddStringConditionEvent(BaseNode node, List<EventData_StringCondition> stringEventConditions, EventData_StringCondition stringEvent = null)
        {
            EventData_StringCondition tmpStringEventCondition = new EventData_StringCondition();

            // If we paramida value is not null we load in values.
            if (stringEvent != null)
            {
                tmpStringEventCondition.stringEvent.value = stringEvent.stringEvent.value;
                tmpStringEventCondition.number.value = stringEvent.number.value;
                tmpStringEventCondition.conditionType.value = stringEvent.conditionType.value;
            }
            stringEventConditions.Add(tmpStringEventCondition);



            // Container of all object.
            Box boxContainer = NewBox(node, "StringEventBox");
            Box boxfloatField = new Box();
            boxfloatField.AddStyle("StringEventBoxfloatField");





            // Text.
            TextField textField = NewTextField(tmpStringEventCondition.stringEvent, "String Event", "stringEvent");

            // ID number.
            FloatField floatField = NewFloatField(tmpStringEventCondition.number, "StringEventInt");

            // String Event Condition
            Action tmp = () => ShowHide_StringEventConditionType(tmpStringEventCondition.conditionType.value, boxfloatField);
            // EnumField String Event Condition
            EnumField enumField = NewStringEventConditionTypeField(tmpStringEventCondition.conditionType, tmp, "StringEventEnum");
            // Run the show and hide.
            ShowHide_StringEventConditionType(tmpStringEventCondition.conditionType.value, boxfloatField);

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









        /// <summary>
        /// Ajoute un ScriptableEvent à la Node, ce qui permet de lier des DialogueEventSO aux DE_Trigger de la scène.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="eventData"></param>
        /// <param name="scriptableEvent"></param>
        public static void AddScriptableEvent(BaseNode node, EventData eventData, ContainerValue<DialogueEventSO> scriptableEvent = null)
        {
            //Pour sauvegarder l'event
            ContainerValue<DialogueEventSO> tmp = new ContainerValue<DialogueEventSO>();
            if (scriptableEvent != null)
            {
                tmp.value = scriptableEvent.value;
            }
            eventData.scriptableEvents.Add(tmp);


            //Container
            Box boxContainer = new Box();
            boxContainer.AddStyle("EventBox");



            //Scriptable Object Event
            ObjectField eventField = NewDialogueEventField(tmp, "EventObject");



            //Remove button
            Action onClicked = () =>
            {
                node.DeleteBox(boxContainer);
                eventData.scriptableEvents.Remove(tmp);
            };
            Button removeBtn = NewButton("X", onClicked, "removeBtn");


            boxContainer.Add(eventField);
            boxContainer.Add(removeBtn);

            node.mainContainer.Add(boxContainer);
            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            node.RefreshExpandedState();
        }






        // value's --------------------------------------------------------------------------

        #region Values

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
                inputValue.value = value.newValue;
            });
            integerField.SetValueWithoutNotify(inputValue.value);

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
                inputValue.value = value.newValue;
            });
            floatField.SetValueWithoutNotify(inputValue.value);

            // Set uss class for stylesheet.
            floatField.AddStyle(USSx);

            return floatField;
        }

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
                inputValue.value = value.newValue;
            });
            textField.SetValueWithoutNotify(inputValue.value);

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
        /// Get a new ObjectField with a Sprite as the Object.
        /// </summary>
        /// <param name="inputCharacter">Container_Sprite that need to be set in to the ObjectField</param>
        /// <returns></returns>
        public static ObjectField NewCharacterField(DialogueData_CharacterSO container, ContainerValue<DialogueCharacterSO> inputCharacter, params string[] USSx)
        {
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(DialogueCharacterSO),
                allowSceneObjects = false,
                value = inputCharacter.value,
            };

            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                if (value.newValue != null)
                {
                    inputCharacter.value = value.newValue as DialogueCharacterSO;
                    container.characterName.value = inputCharacter.value.characterName;

                    if (container.mood.enumField == null)
                        container.mood.value = CharacterMood.Idle;

                    //Afficher l'image du nouveau perso quand on change de DialogueCharacterSO
                    container.sprite.value = inputCharacter.value.GetFaceFromMood(container.mood.value);
                    container.spriteField.image = inputCharacter.value != null ? container.sprite.value.texture : null;
                }
                else
                {
                    inputCharacter.value = null;
                    container.characterName.value = "";
                    container.sprite.value = null;
                    container.spriteField.image = null;
                }

                container.mood.enumField.SetValueWithoutNotify(container.mood.value);
                container.nameField.SetValueWithoutNotify(container.characterName.value);
            });
            container.spriteField.image = inputCharacter.value != null ? container.sprite.value.texture : null;

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
                value = inputSprite.value,
            };

            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                inputSprite.value = value.newValue as Sprite;

                imagePreview.image = inputSprite.value != null ? inputSprite.value.texture : null;
            });
            imagePreview.image = inputSprite.value != null ? inputSprite.value.texture : null;

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);

            return objectField;
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
                value = inputDialogueEventSO.value,
            };

            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                inputDialogueEventSO.value = value.newValue as DialogueEventSO;
            });
            objectField.SetValueWithoutNotify(inputDialogueEventSO.value);

            // Set uss class for stylesheet.
            objectField.AddStyle(USSx);

            return objectField;
        }


        #endregion



        // Enum's --------------------------------------------------------------------------

        #region Enums

        /// <summary>
        /// Get a new EnumField where the emum is ChoiceStateType.
        /// </summary>
        /// <param name="enumType">Container_ChoiceStateType that need to be set in to the EnumField</param>
        /// <returns></returns>
        public static EnumField NewChoiceStateTypeField(ContainerEnum<ChoiceStateType> enumType, params string[] USSx)
        {
            EnumField enumField = new EnumField()
            {
                value = enumType.value
            };
            enumField.Init(enumType.value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                enumType.value = (ChoiceStateType)value.newValue;
            });
            enumField.SetValueWithoutNotify(enumType.value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            enumType.enumField = enumField;
            return enumField;
        }

        /// <summary>
        /// Get a new EnumField where the emum is EndNodeType.
        /// </summary>
        /// <param name="enumType">Container_EndNodeType that need to be set in to the EnumField</param>
        /// <returns></returns>
        public static EnumField NewEndNodeTypeField(ContainerEnum<EndNodeType> enumType, params string[] USSx)
        {
            EnumField enumField = new EnumField()
            {
                value = enumType.value
            };
            enumField.Init(enumType.value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                enumType.value = (EndNodeType)value.newValue;
            });
            enumField.SetValueWithoutNotify(enumType.value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            enumType.enumField = enumField;
            return enumField;
        }


        public static EnumField NewCharacterMoodField(DialogueData_CharacterSO container, ContainerEnum<CharacterMood> enumType, params string[] USSx)
        {
            EnumField enumField = new EnumField()
            {
                value = enumType.value
            };
            enumField.Init(enumType.value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                enumType.value = (CharacterMood)value.newValue;

                if (container.character.value != null)
                {
                    //Quand on change d'humeur, on affiche le sprite correspondant
                    container.sprite.value = container.character.value.GetFaceFromMood(enumType.value);
                    container.spriteField.image = container.sprite.value.texture;
                }
            });
            enumField.SetValueWithoutNotify(enumType.value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            enumType.enumField = enumField;
            return enumField;
        }

        public static EnumField NewDialogueSideField(ContainerEnum<DialogueSide> enumType, params string[] USSx)
        {
            EnumField enumField = new EnumField()
            {
                value = enumType.value
            };
            enumField.Init(enumType.value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                enumType.value = (DialogueSide)value.newValue;
            });
            enumField.SetValueWithoutNotify(enumType.value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            enumType.enumField = enumField;
            return enumField;
        }



        /// <summary>
        /// Get a new EnumField where the emum is modifierType.
        /// </summary>
        /// <param name="enumType">ContainerEnum<modifierType> that need to be set in to the EnumField</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static EnumField NewStringEventModifierTypeField(ContainerEnum<StringEventModifierType> enumType, Action action, params string[] USSx)
        {
            EnumField enumField = new EnumField()
            {
                value = enumType.value,
            };
            enumField.Init(enumType.value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                enumType.value = (StringEventModifierType)value.newValue;
                action?.Invoke();
            });
            enumField.SetValueWithoutNotify(enumType.value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            enumType.enumField = enumField;
            return enumField;
        }

        /// <summary>
        /// Get a new EnumField where the emum is StringEventComparerType.
        /// </summary>
        /// <param name="enumType">ContainerEnum<StringEventComparerType>  that need to be set in to the EnumField</param>
        /// <param name="action">A Action that is use to hide/show depending on if a FloatField is needed</param>
        /// <returns></returns>
        public static EnumField NewStringEventConditionTypeField(ContainerEnum<StringEventConditionType> enumType, Action action, params string[] USSx)
        {
            EnumField enumField = new EnumField()
            {
                value = enumType.value,
            };
            enumField.Init(enumType.value);

            // When we change the variable from graph view.
            enumField.RegisterValueChangedCallback((value) =>
            {
                enumType.value = (StringEventConditionType)value.newValue;
                action?.Invoke();
            });
            enumField.SetValueWithoutNotify(enumType.value);

            // Set uss class for stylesheet.
            enumField.AddStyle(USSx);

            enumType.enumField = enumField;
            return enumField;
        }


        #endregion

        // Custom-made's --------------------------------------------------------------------------

        #region Custom


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
                    language = languageType,
                    data = ""
                });
            });


            // Make TextField.
            TextField textField = new TextField("");

            // Add it to the reaload current language list.
            node.textHolders.Add(new LanguageGenericHolder_Text(texts, textField, placeholderText));

            // When we change the variable from graph view.
            textField.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.language == node.Window.SelectedLanguage).data = value.newValue;
            });
            textField.SetValueWithoutNotify(texts.Find(text => text.language == node.Window.SelectedLanguage).data);

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
                    language = languageType,
                    data = null
                });
            });

            // Make ObjectField.
            ObjectField objectField = new ObjectField()
            {
                objectType = typeof(AudioClip),
                allowSceneObjects = false,
                value = audioClips.Find(audioClip => audioClip.language == node.Window.SelectedLanguage).data,
            };

            // Add it to the reaload current language list.
            node.audioClipHolders.Add(new LanguageGenericHolder_AudioClip(audioClips, objectField));


            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                audioClips.Find(audioClip => audioClip.language == node.Window.SelectedLanguage).data = value.newValue as AudioClip;
            });
            objectField.SetValueWithoutNotify(audioClips.Find(audioClip => audioClip.language == node.Window.SelectedLanguage).data);

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
