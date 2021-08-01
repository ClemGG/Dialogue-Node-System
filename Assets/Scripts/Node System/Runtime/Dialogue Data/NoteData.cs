using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Pour savoir quels éléments sont attachés à ce Groupe
/// </summary>
[System.Serializable]
public class NoteData
{
    public string Title;
    public string Content;
    public Vector2 Position;
}
