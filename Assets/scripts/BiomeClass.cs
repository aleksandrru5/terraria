using System.Collections;
using UnityEngine;

[System.Serializable]
public class BiomeClass
{
    public string biomeName;
    public Color biomeCol;

    public TileAtlas tileAtlas;

    [Header("Noise Settings")]
    public Texture2D caveNoiseTexture;

    [Header("Generation Settings")]
    public bool generateCaves = true;
    public int dirLayerHeight = 5;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 4f;

    [Header("Trees")]
    public int treeChance = 10;
    public int minTreeHeight = 3;
    public int maxTreeHeight = 10;

    [Header("Addons")]
    public int tallGrassChance = 10;

    [Header("Ore Settings")]
    public OreClass[] ores;
}
