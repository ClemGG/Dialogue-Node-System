using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using static Project.Utilities.ValueTypes.Enums;

namespace Project.NodeSystem.Editor
{

    public class BaseNode : Node
    {
        #region Fields

        private string nodeGuid;
        protected DialogueGraphView graphView;
        protected DialogueEditorWindow window;
        protected Vector2 defaultNodeSize = new Vector2(200, 250);

        private List<LanguageGenericHolder<string>> textHolders = new List<LanguageGenericHolder<string>>();
        private List<LanguageGenericHolder<AudioClip>> audioClipHolders = new List<LanguageGenericHolder<AudioClip>>();

        public string NodeGuid { get => nodeGuid; set => nodeGuid = value; }

        #endregion




        #region Methods

        //Toutes les classes filles possèdent un constructeur vide pour pouvoir les créer depuis la barre de recherche
        //directement depuis le graphe
        public BaseNode(string name, Vector2 position, DialogueEditorWindow window, DialogueGraphView graphView)
        {
            this.window = window;
            this.graphView = graphView;

            StyleSheet styleSheet = Resources.Load<StyleSheet>("USS/Nodes/NodeStyleSheet");
            styleSheets.Add(styleSheet);


            title = name;
            SetPosition(new Rect(position, defaultNodeSize));
            NodeGuid = Guid.NewGuid().ToString();

        }


        /// <summary>
        /// Ajoute un port au conteneur correspondant à sa Direction
        /// </summary>
        /// <param name="name">Le nom du port affiché sur la node.</param>
        /// <param name="portDirection">Indique s'il s'agit d'un port d'entrée ou de sortie.</param>
        /// <param name="capacity">Indique s'il n'accepte qu'une seule ou plusieurs connexions.</param>
        public Port AddPort(string name, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            Port port = GetPortInstance(portDirection, capacity);
            port.portName = name;


            //A voir si on peut changer les ports d'emplacement pour afficher les nodes verticalement
            if (portDirection == Direction.Output)
                outputContainer.Add(port);
            else
                inputContainer.Add(port);

            return port;
        }

        public Port GetPortInstance(Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }


        //Quand on charge les données de la node, on l'assigne à ses champs si elle en a
        public virtual void LoadValueIntoField() { }

        //Idem que LoadValueIntoField() mais recharge les champs dépendant de la langue
        public virtual void ReloadLanguage()
        {
            foreach (LanguageGenericHolder<string> textHolder in textHolders)
            {
                textHolder.field.RegisterValueChangedCallback(value =>
                {
                    textHolder.inputObjects.Find(text => text.language == window.SelectedLanguage).data = value.newValue;
                });

                textHolder.field.SetValueWithoutNotify(textHolder.inputObjects.Find(text => text.language == window.SelectedLanguage).data);
                SetPlaceholderText(textHolder.field as TextField, textHolder.placeHolderValue as string);
            }

            foreach (LanguageGenericHolder<AudioClip> audioClipHolder in audioClipHolders)
            {
                audioClipHolder.field.RegisterValueChangedCallback(value =>
                {
                    audioClipHolder.inputObjects.Find(clip => clip.language == window.SelectedLanguage).data = value.newValue;
                });

                if(audioClipHolder.field != null)
                    audioClipHolder.field.SetValueWithoutNotify(audioClipHolder.inputObjects.Find(clip => clip.language == window.SelectedLanguage).data);
            }

        }




        /// <summary>
        /// Add String Modifier Event to UI element.
        /// </summary>
        /// <param name="stringEventModifier">The List<EventData_StringModifier> that EventData_StringModifier should be added to.</param>
        /// <param name="stringEvent">EventData_StringModifier that should be use.</param>
        protected void AddStringModifierEventBuild(List<EventData_StringModifier> stringEventModifier, EventData_StringModifier stringEvent = null)
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
            boxContainer.AddToClassList("StringEventBox");
            boxfloatField.AddToClassList("StringEventBoxfloatField");

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
                DeleteBox(boxContainer);
            };
            Button btn = NewButton("X", onClicked, "removeBtn");
            

            // Add it to the box
            boxContainer.Add(textField);
            boxContainer.Add(enumField);
            boxfloatField.Add(floatField);
            boxContainer.Add(boxfloatField);
            boxContainer.Add(btn);

            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }

        /// <summary>
        /// Add String Condition Event to UI element.
        /// </summary>
        /// <param name="stringEventCondition">The List<EventData_StringComparer> that EventData_StringComparer should be added to.</param>
        /// <param name="stringEvent">EventData_StringComparer that should be use.</param>
        protected void AddStringConditionEventBuild(List<EventData_StringCondition> stringEventCondition, EventData_StringCondition stringEvent = null)
        {
            EventData_StringCondition tmpStringEventCondition = new EventData_StringCondition();

            // If we paramida value is not null we load in values.
            if (stringEvent != null)
            {
                tmpStringEventCondition.stringEvent.value = stringEvent.stringEvent.value;
                tmpStringEventCondition.number.value = stringEvent.number.value;
                tmpStringEventCondition.conditionType.value = stringEvent.conditionType.value;
            }

            stringEventCondition.Add(tmpStringEventCondition);

            // Container of all object.
            Box boxContainer = new Box();
            Box boxfloatField = new Box();
            boxContainer.AddToClassList("StringEventBox");
            boxfloatField.AddToClassList("StringEventBoxfloatField");

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
                stringEventCondition.Remove(tmpStringEventCondition);
                DeleteBox(boxContainer);
            };
            Button btn = NewButton("X", onClicked, "removeBtn");
            

            // Add it to the box
            boxContainer.Add(textField);
            boxContainer.Add(enumField);
            boxfloatField.Add(floatField);
            boxContainer.Add(boxfloatField);
            boxContainer.Add(btn);

            mainContainer.Add(boxContainer);
            RefreshExpandedState();
        }



        protected void AddScriptableEventBuild(EventData eventData, ContainerValue<DialogueEventSO> scriptableEvent = null)
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
            boxContainer.AddToClassList("EventBox");



            //Scriptable Object Event
            ObjectField eventField = NewDialogueEventField(tmp, "EventObject");



            //Remove button
            Action onClicked = () =>
            {
                DeleteBox(boxContainer);
                eventData.scriptableEvents.Remove(tmp);
            };
            Button removeBtn = NewButton("X", onClicked, "removeBtn");


            boxContainer.Add(eventField);
            boxContainer.Add(removeBtn);

            mainContainer.Add(boxContainer);

            //On appelle ces fonctions pour mettre à jour le visuel de la Node
            RefreshExpandedState();
        }


        /// <summary>
        /// hid and show the UI element
        /// </summary>
        /// <param name="value">modifierType</param>
        /// <param name="boxContainer">The Box that will be hidden or shown</param>
        private void ShowHide_StringEventModifierType(StringEventModifierType value, Box boxContainer)
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
        private void ShowHide_StringEventConditionType(StringEventConditionType value, Box boxContainer)
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
        /// <param name="boxContainer">which container box to add the desired USS tag to</param>
        protected void ShowHide(bool show, Box boxContainer)
        {
            string hideUssClass = "Hide";
            if (show == true)
            {
                boxContainer.RemoveFromClassList(hideUssClass);
            }
            else
            {
                boxContainer.AddToClassList(hideUssClass);
            }
        }

        /// <summary>
        /// Remove box container.
        /// </summary>
        /// <param name="boxContainer">desired box to delete and remove</param>
        protected virtual void DeleteBox(Box boxContainer)
        {
            mainContainer.Remove(boxContainer);
            RefreshExpandedState();
        }



        #endregion




        #region Get New Fields

        /// <summary>
        /// Get a new Label
        /// </summary>
        /// <param name="labelName">Text in the label</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected Label NewLabel(string labelName, string USS01 = "", string USS02 = "")
        {
            Label label_texts = new Label(labelName);

            // Set uss class for stylesheet.
            label_texts.AddToClassList(USS01);
            label_texts.AddToClassList(USS02);

            return label_texts;
        }

        /// <summary>
        /// Get a new Button
        /// </summary>
        /// <param name="btnText">Text in the button</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected Button NewButton(string btnText, Action onClicked, string USS01 = "", string USS02 = "")
        {
            Button btn = new Button(onClicked)
            {
                text = btnText,
            };

            // Set uss class for stylesheet.
            btn.AddToClassList(USS01);
            btn.AddToClassList(USS02);

            return btn;
        }

        // value's --------------------------------------------------------------------------

        #region Values

        /// <summary>
        /// Get a new IntegerField.
        /// </summary>
        /// <param name="inputValue">Container_Int that need to be set in to the IntegerField</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected IntegerField NewIntField(ContainerValue<int> inputValue, string USS01 = "", string USS02 = "")
        {
            IntegerField integerField = new IntegerField();

            // When we change the variable from graph view.
            integerField.RegisterValueChangedCallback(value =>
            {
                inputValue.value = value.newValue;
            });
            integerField.SetValueWithoutNotify(inputValue.value);

            // Set uss class for stylesheet.
            integerField.AddToClassList(USS01);
            integerField.AddToClassList(USS02);

            return integerField;
        }

        /// <summary>
        /// Get a new FloatField.
        /// </summary>
        /// <param name="inputValue">Container_Float that need to be set in to the FloatField</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected FloatField NewFloatField(ContainerValue<float> inputValue, string USS01 = "", string USS02 = "")
        {
            FloatField floatField = new FloatField();

            // When we change the variable from graph view.
            floatField.RegisterValueChangedCallback(value =>
            {
                inputValue.value = value.newValue;
            });
            floatField.SetValueWithoutNotify(inputValue.value);

            // Set uss class for stylesheet.
            floatField.AddToClassList(USS01);
            floatField.AddToClassList(USS02);

            return floatField;
        }

        /// <summary>
        /// Get a new TextField.
        /// </summary>
        /// <param name="inputValue">Container_String that need to be set in to the TextField</param>
        /// <param name="placeholderText"></param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected TextField NewTextField(ContainerValue<string> inputValue, string placeholderText, string USS01 = "", string USS02 = "")
        {
            TextField textField = new TextField();

            // When we change the variable from graph view.
            textField.RegisterValueChangedCallback(value =>
            {
                inputValue.value = value.newValue;
            });
            textField.SetValueWithoutNotify(inputValue.value);

            // Set uss class for stylesheet.
            textField.AddToClassList(USS01);
            textField.AddToClassList(USS02);

            // Set Place Holder
            SetPlaceholderText(textField, placeholderText);

            return textField;
        }



        #region Set PlaceHolder Text


        /// <summary>
        /// Set a placeholder text on a TextField.
        /// </summary>
        /// <param name="textField">TextField that need a placeholder</param>
        /// <param name="placeholder">The text that will be displayed if the text field is empty</param>
        protected void SetPlaceholderText(TextField textField, string placeholder)
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

        #endregion



        /// <summary>
        /// Get a new Image.
        /// </summary>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected Image NewImage(string USS01 = "", string USS02 = "")
        {
            Image imagePreview = new Image();

            // Set uss class for stylesheet.
            imagePreview.AddToClassList(USS01);
            imagePreview.AddToClassList(USS02);

            return imagePreview;
        }


        /// <summary>
        /// Get a new ObjectField with a Sprite as the Object.
        /// </summary>
        /// <param name="inputCharacter">Container_Sprite that need to be set in to the ObjectField</param>
        /// <param name="imagePreview">Image that need to be set as preview image</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected ObjectField NewCharacterField(DialogueData_CharacterSO container, ContainerValue<DialogueCharacterSO> inputCharacter, string USS01 = "", string USS02 = "")
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
            objectField.AddToClassList(USS01);
            objectField.AddToClassList(USS02);

            return objectField;
        }


        /// <summary>
        /// Get a new ObjectField with a Sprite as the Object.
        /// </summary>
        /// <param name="inputSprite">Container_Sprite that need to be set in to the ObjectField</param>
        /// <param name="imagePreview">Image that need to be set as preview image</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected ObjectField NewSpriteField(ContainerValue<Sprite> inputSprite, Image imagePreview, string USS01 = "", string USS02 = "")
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
            objectField.AddToClassList(USS01);
            objectField.AddToClassList(USS02);

            return objectField;
        }

        /// <summary>
        /// Get a new ObjectField with a Container_DialogueEventSO as the Object.
        /// </summary>
        /// <param name="inputDialogueEventSO">Container_DialogueEventSO that need to be set in to the ObjectField</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected ObjectField NewDialogueEventField(ContainerValue<DialogueEventSO> inputDialogueEventSO, string USS01 = "", string USS02 = "")
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
            objectField.AddToClassList(USS01);
            objectField.AddToClassList(USS02);

            return objectField;
        }


        #endregion



        // Enum's --------------------------------------------------------------------------

        #region Enums

        /// <summary>
        /// Get a new EnumField where the emum is ChoiceStateType.
        /// </summary>
        /// <param name="enumType">Container_ChoiceStateType that need to be set in to the EnumField</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected EnumField NewChoiceStateTypeField(ContainerEnum<ChoiceStateType> enumType, string USS01 = "", string USS02 = "")
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
            enumField.AddToClassList(USS01);
            enumField.AddToClassList(USS02);

            enumType.enumField = enumField;
            return enumField;
        }

        /// <summary>
        /// Get a new EnumField where the emum is EndNodeType.
        /// </summary>
        /// <param name="enumType">Container_EndNodeType that need to be set in to the EnumField</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected EnumField NewEndNodeTypeField(ContainerEnum<EndNodeType> enumType, string USS01 = "", string USS02 = "")
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
            enumField.AddToClassList(USS01);
            enumField.AddToClassList(USS02);

            enumType.enumField = enumField;
            return enumField;
        }


        protected EnumField NewCharacterMoodField(DialogueData_CharacterSO container, ContainerEnum<CharacterMood> enumType, string USS01 = "", string USS02 = "")
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
            enumField.AddToClassList(USS01);
            enumField.AddToClassList(USS02);

            enumType.enumField = enumField;
            return enumField;
        }

        protected EnumField NewDialogueSideField(ContainerEnum<DialogueSide> enumType, string USS01 = "", string USS02 = "")
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
            enumField.AddToClassList(USS01);
            enumField.AddToClassList(USS02);

            enumType.enumField = enumField;
            return enumField;
        }



        /// <summary>
        /// Get a new EnumField where the emum is modifierType.
        /// </summary>
        /// <param name="enumType">ContainerEnum<modifierType> that need to be set in to the EnumField</param>
        /// <param name="action"></param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected EnumField NewStringEventModifierTypeField(ContainerEnum<StringEventModifierType> enumType, Action action, string USS01 = "", string USS02 = "")
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
            enumField.AddToClassList(USS01);
            enumField.AddToClassList(USS02);

            enumType.enumField = enumField;
            return enumField;
        }

        /// <summary>
        /// Get a new EnumField where the emum is StringEventComparerType.
        /// </summary>
        /// <param name="enumType">ContainerEnum<StringEventComparerType>  that need to be set in to the EnumField</param>
        /// <param name="action">A Action that is use to hide/show depending on if a FloatField is needed</param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected EnumField NewStringEventConditionTypeField(ContainerEnum<StringEventConditionType> enumType, Action action, string USS01 = "", string USS02 = "")
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
            enumField.AddToClassList(USS01);
            enumField.AddToClassList(USS02);

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
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected TextField NewTextLanguagesField(List<LanguageGeneric<string>> texts, string placeholderText = "", string USS01 = "", string USS02 = "")
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
            textHolders.Add(new LanguageGenericHolder<string>(texts, textField, placeholderText));

            // When we change the variable from graph view.
            textField.RegisterValueChangedCallback(value =>
            {
                texts.Find(text => text.language == window.SelectedLanguage).data = value.newValue;
            });
            textField.SetValueWithoutNotify(texts.Find(text => text.language == window.SelectedLanguage).data);

            // Text field is set to be multiline.
            textField.multiline = true;

            // Set uss class for stylesheet.
            textField.AddToClassList(USS01);
            textField.AddToClassList(USS02);

            return textField;
        }


        /// <summary>
        /// Get a new ObjectField that use List<LanguageGeneric<AudioClip>>.
        /// </summary>
        /// <param name="audioClips"></param>
        /// <param name="USS01">USS class add to the UI element</param>
        /// <param name="USS02">USS class add to the UI element</param>
        /// <returns></returns>
        protected ObjectField NewAudioClipLanguagesField(List<LanguageGeneric<AudioClip>> audioClips, string USS01 = "", string USS02 = "")
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
                value = audioClips.Find(audioClip => audioClip.language == window.SelectedLanguage).data,
            };

            // Add it to the reaload current language list.
            audioClipHolders.Add(new LanguageGenericHolder<AudioClip>(audioClips, objectField));

            // When we change the variable from graph view.
            objectField.RegisterValueChangedCallback(value =>
            {
                audioClips.Find(audioClip => audioClip.language == window.SelectedLanguage).data = value.newValue as AudioClip;
            });
            objectField.SetValueWithoutNotify(audioClips.Find(audioClip => audioClip.language == window.SelectedLanguage).data);

            // Set uss class for stylesheet.
            objectField.AddToClassList(USS01);
            objectField.AddToClassList(USS02);

            return objectField;
        }

        #endregion


        #endregion




        #region Language Generic Holders


        //Utilisée pour stocker les champs et leurs valeurs
        public class LanguageGenericHolder<T>
        {
            public List<LanguageGeneric<T>> inputObjects;
            public BaseField<T> field;
            public object placeHolderValue = null;

            public LanguageGenericHolder(List<LanguageGeneric<T>> inputObjects, ObjectField field)
            {
                this.inputObjects = inputObjects;
                this.field = field as BaseField<T>;
            }

            public LanguageGenericHolder(List<LanguageGeneric<T>> inputObjects, BaseField<T> field, object placeHolderValue = null)
            {
                this.inputObjects = inputObjects;
                this.field = field;
                this.placeHolderValue = placeHolderValue;
            }

        }

        #endregion



    }
}