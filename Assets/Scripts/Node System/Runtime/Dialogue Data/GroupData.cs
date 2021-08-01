using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pour savoir quels éléments sont attachés à ce Groupe
/// </summary>
[System.Serializable]
public class GroupData
{
    public string GroupName;
    public Vector2 Position;
    public List<string> ContainedGuids = new List<string>(); //Les guids des nodes contenus dans ce groupe, stockées dans DialogueSaveLoad
}
