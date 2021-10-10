

namespace Project.NodeSystem
{
    /// <summary>
    /// The editor window's language, or the language type of the data stored in the node
    /// </summary>
    public enum LanguageType
    {
        French = 0,
        English = 1,
        Spanish = 2,
    }


    /// <summary>
    /// Indicates what to do when the EndNode is reached
    /// </summary>
    public enum EndNodeType
    {
        End = 0,   //Ends the dialogue
        Repeat = 1, //Replays the previous node (Obsolete)
        GoBack = 2, //Go back to the previous node (Obsolete)
        ReturnToStart = 3,   //restarts the dialogue
    }


    /// <summary>
    /// The mood of the character currently talking
    /// </summary>
    public enum CharacterMood
    {
        Idle = 0,
        Happy = 1,
        Angry = 2,
        Sad = 3,
        Doubtful = 4,
        Determined = 5,
        Thinking = 6,
        Confident = 7,
        Regretful = 8,
        Shocked = 9,
        Empathetic = 10,
    }



    /// <summary>
    /// Used to:
    ///  - Rotate the sprite left and right
    ///  - Place the sprite on the left/right side of the screen
    /// </summary>
    public enum DialogueSide
    {
        Left = 0,
        Right = 1,
    }


    /// <summary>
    /// To hide or grey out the choices if their conditions are not met
    /// </summary>
    public enum ChoiceStateType
    {
        Hide = 0,
        GreyOut = 1,
    }





    /// <summary>
    /// To add comparators to the stringEvents' conditions
    /// </summary>
    public enum StringEventConditionType
    {
        True = 0,
        False = 1,
        Equals = 2,
        Bigger = 3,
        Smaller = 4,
        EqualsOrBigger = 5,
        EqualsOrSmaller = 6,
    }

    /// <summary>
    /// To execute operations on the stringEvents
    /// </summary>
    public enum StringEventModifierType
    {
        SetTrue = 0,
        SetFalse = 1,
        Add = 2,
        Substract = 3,
    }

}