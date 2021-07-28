using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pour savoir quels éléments sont attachés à ce Groupe
/// </summary>
[System.Serializable]
public class GroupData
{
    public string groupName;
    public Vector2 position;
    public List<string> containedGuids = new List<string>(); //Les guids des nodes contenus dans ce groupe, stockées dans DialogueSaveLoad
}
