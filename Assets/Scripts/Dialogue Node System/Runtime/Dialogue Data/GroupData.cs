using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pour savoir quels �l�ments sont attach�s � ce Groupe
/// </summary>
[System.Serializable]
public class GroupData
{
    public string GroupName;
    public Vector2 Position;
    public List<string> ContainedGuids = new List<string>(); //Les guids des nodes contenus dans ce groupe, stock�es dans DialogueSaveLoad
}
