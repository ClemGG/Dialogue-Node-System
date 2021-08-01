

namespace Project.NodeSystem
{
    /// <summary>
    /// La langue de l'éditeur ou de la donnée enregistrée
    /// </summary>
    public enum LanguageType
    {
        French = 0,
        English = 1,
        Spanish = 2,
    }


    /// <summary>
    /// Indique l'action à exécuter quand le EndNode est atteinte
    /// </summary>
    public enum EndNodeType
    {
        End = 0,   //FInit le dialogue
        Repeat = 1, //Rejoue la node
        GoBack = 2, //Retourne à la node précédente
        ReturnToStart = 3,   //Retourne au début du dialogue
    }


    /// <summary>
    /// Indique l'humeur du personnage en train de parler (Récupère le sprite correspondant à l'humeur donnée)
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
    ///  Utilisé pour :
    ///  - Faire regarder le sprite du perso à gauche ou à droite
    ///  - Placer le sprite à gauche ou à droite du canvas de dialogue
    /// </summary>
    public enum DialogueSide
    {
        Left = 0,
        Right = 1,
    }


    /// <summary>
    /// Pour griser ou cacher le choix si ses conditions ne sont pas remplies
    /// </summary>
    public enum ChoiceStateType
    {
        Hide = 0,
        GreyOut = 1,
    }





    /// <summary>
    /// Pour ajouter les comparateurs aux conditions des string events
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
    /// Pour réaliser des opérations aux string events
    /// </summary>
    public enum StringEventModifierType
    {
        SetTrue = 0,
        SetFalse = 1,
        Add = 2,
        Substract = 3,
    }
}