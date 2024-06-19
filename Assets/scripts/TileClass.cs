using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "newtileclass", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
    public string tileName;

    public TileClass wallVariant;
    public Sprite[] tileSprites;

    public bool inBackground = false;
    public TileClass tileDrop;
    public bool naturallyPlaced = true;

    public bool isStackable = true;

    public static TileClass CreateInstance (TileClass tile, bool isNaturallyPlaced)
    {
        var thisTile = ScriptableObject.CreateInstance<TileClass>();
        thisTile.Init(tile, isNaturallyPlaced);
        return thisTile;
    }

    public void Init (TileClass tile, bool isNaturallyPlaced)
    {
        tileName = tile.tileName;
        wallVariant = tile.wallVariant;
        tileSprites = tile.tileSprites;
        inBackground = tile.inBackground;
        tileDrop = tile.tileDrop;
        naturallyPlaced = isNaturallyPlaced;
    }
}
