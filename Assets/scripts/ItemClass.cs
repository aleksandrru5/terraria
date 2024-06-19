using System.Collections;
using UnityEngine;

[System.Serializable]
public class ItemClass
{
    public enum ItemType
    {
        block,
        tool
    };

    public enum ToolType
    {
        axe,
        pickaxe,
        hammer
    };

    public ItemType itemType;
    public ToolType toolType;

    public string itemName;
    public Sprite spryte;
    public bool isStackable;

    public TileClass tile;
    public ToolClass tool;

    public ItemClass(TileClass _tile)
    {
        itemName = _tile.tileName;
        spryte = _tile.tileDrop.tileSprites[0];
        isStackable = _tile.isStackable;
        itemType = ItemType.block;
        tile = _tile;
    }

    public ItemClass(ToolClass _tool)
    {
        itemName = _tool.namee;
        spryte = _tool.spryte;
        isStackable = false;
        toolType = _tool.toolType;
        tool = _tool;
    }
}
