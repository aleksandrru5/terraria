using System.Collections;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "ToolClass", menuName = "Tool Class")]
public class ToolClass : ScriptableObject
{
    public string namee;
    public Sprite spryte;
    public ItemClass.ToolType toolType;
}
