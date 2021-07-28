using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pour savoir quels �l�ments sont attach�s � ce Groupe
/// </summary>
[System.Serializable]
public class GroupData
{
    public string groupName;
    public Vector2 position;
    public List<string> containedGuids = new List<string>(); //Les guids des nodes contenus dans ce groupe, stock�es dans DialogueSaveLoad
}
