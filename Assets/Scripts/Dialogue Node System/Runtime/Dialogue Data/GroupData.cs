using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores which elements are grouped under this VisualElement
/// </summary>
[System.Serializable]
public class GroupData
{
    public string GroupName;
    public Vector2 Position;
    public List<string> ContainedGuids = new List<string>(); //The node Guids contained in this group, stored in DialogueSaveLoad
}
