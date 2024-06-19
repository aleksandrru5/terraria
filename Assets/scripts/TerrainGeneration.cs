using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Light")]
    public Texture2D worldTilesMap;
    public Material lightShader;
    public float groundLightThreshold = 0.7f;
    public float airLightThreshold = 0.85f;
    public float lightRadius = 7;
    List<Vector2Int> unlitBloks= new List<Vector2Int>();    

    public PlayerController player;
    public CamController cam;

    public GameObject tileDrop;

    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed;

    public BiomeClass[] biomes;

    [Header("Biomes")]
    public float biomeFrequency;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Generation Settings")]
    public int chunkSize = 16;
    public bool generateCaves = true;
    public int worldSize = 100;
    public int heightAddition = 25;

    [Header("Noise Settings")]
    public Texture2D caveNoiseTexture;
    public float terrainFreq = 0.05f;
    public float caveFreq = 0.05f;

    [Header("Ore Settings")]
    public OreClass[] ores;

    private GameObject[] worldChunks;
    
    //private List<GameObject> worldTileObjects = new List<GameObject>();

    private GameObject[,] world_ForegroundObjekts;
    private GameObject[,] world_BackgrondObjekts;

    private TileClass[,] world_ForegroundTiles;
    private TileClass[,] world_BackgrondTiles;

    private BiomeClass curBiome;
    private Color[] biomeCols;

    public void OnValidate()
    {
        DrawTextures();
    }

    private void Start()
    {
        world_ForegroundTiles = new TileClass[worldSize, worldSize];
        world_BackgrondTiles = new TileClass[worldSize, worldSize];

        world_ForegroundObjekts = new GameObject[worldSize, worldSize];
        world_BackgrondObjekts = new GameObject[worldSize, worldSize];

        worldTilesMap = new Texture2D(worldSize, worldSize);
        //worldTilesMap.filterMode = FilterMode.Point; //Пиксельное освещение или гладкое
        lightShader.SetTexture("_ShadowTex", worldTilesMap);

        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                worldTilesMap.SetPixel(x, y, Color.white);
            }
        }
        worldTilesMap.Apply();

        seed = Random.Range(-10000, 10000);

        for (int i = 0; i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }

        biomeCols = new Color[biomes.Length];

        for (int i = 0; i < biomeCols.Length; i++)
        {
            biomeCols[i] = biomes[i].biomeCol;
        }

        DrawBiomeMap();
        DrawCavesAndOres();
        CreateChunks();
        GenerateTerrain();

        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
            {
                if (worldTilesMap.GetPixel(x, y) == Color.white)
                {
                    LightBlock(x, y, 1, 0);
                }
            }
        }
        worldTilesMap.Apply();


        cam.Spawn(new Vector3(player.spawnPos.x, player.spawnPos.y, -10));
        cam.worldSize = worldSize;
        player.Spawn();
    }

    private void Update()
    {
        RefreshChanks();
    }

    public void RefreshChanks()
    {
        for (int i = 0; i < worldChunks.Length; i++)
        {
            if (Vector2.Distance(new Vector2((i * chunkSize) + (chunkSize / 2), 0), new Vector2(player.transform.position.x, 0)) > Camera.main.orthographicSize * 6f) 
            {
                worldChunks[i].SetActive(false);
            }
            else
            {
                worldChunks[i].SetActive(true);
            }
        }
    }

    public void DrawBiomeMap()
    {
        float b;
        Color col;
        biomeMap = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                b = Mathf.PerlinNoise((x + seed) * biomeFrequency, (x + seed) * biomeFrequency);
                col = biomeGradient.Evaluate(b);
                biomeMap.SetPixel(x, y, col);
            }
        }
        biomeMap.Apply();
    }

    public void DrawCavesAndOres()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        float v;
        float o;

        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for (int y = 0; y < caveNoiseTexture.height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                if (v > curBiome.surfaceValue)
                {
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    caveNoiseTexture.SetPixel(x, y, Color.black);
                }

                for (int i = 0; i < ores.Length; i++)
                {
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].frequency, (y + seed) * curBiome.ores[i].frequency);
                        if (o > curBiome.ores[i].size)
                        {
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);
                        }

                        ores[i].spreadTexture.Apply();
                    }
                }
            }
        }
        caveNoiseTexture.Apply();
    }

    public void DrawTextures()
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
                GenerateNoiseTextures(biomes[i].ores[o].frequency, biomes[i].ores[o].size, biomes[i].ores[o].spreadTexture);
            }
        }
    }

    private void GenerateNoiseTextures(float frequency, float limit, Texture2D noiseTexture)
    {
        float v;

        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);

                if (v > limit)
                {
                    noiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    noiseTexture.SetPixel(x, y, Color.black);
                }
            }
        }

        noiseTexture.Apply();
    }

    public void CreateChunks()
    {
        int numChunks = worldSize / chunkSize;
        worldChunks = new GameObject[numChunks];

        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        if(System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y)) >= 0)
        {
            return biomes[System.Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y))];
        }

        return curBiome;
    }

    public void GenerateTerrain()
    {
        TileClass tileClass;
        for (int x = 0; x < worldSize; x++)
        {
            float height;

            for (int y = 0; y < worldSize - 1; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAddition;

                if (x == worldSize / 2)
                {
                    player.spawnPos = new Vector2(x, height + 3);
                }

                if (y >= height)
                {
                    break;
                }

                if (y < height - curBiome.dirLayerHeight)
                {
                    tileClass = curBiome.tileAtlas.stone;

                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5 && height - y > ores[0].maxSpawnHeight)
                    {
                        tileClass = tileAtlas.coal;
                    }
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5 && height - y > ores[1].maxSpawnHeight)
                    {
                        tileClass = tileAtlas.iron;
                    }
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5 && height - y > ores[2].maxSpawnHeight)
                    {
                        tileClass = tileAtlas.gold;
                    }
                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5 && height - y > ores[3].maxSpawnHeight)
                    {
                        tileClass = tileAtlas.diamond;
                    }
                }
                else if (y < height - 1)
                {
                    tileClass = curBiome.tileAtlas.dirt;
                }
                else
                {
                    tileClass = curBiome.tileAtlas.glass;
                }

                if (generateCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileClass, x, y, true);
                    }
                    else if(tileClass.wallVariant != null)
                    {
                        PlaceTile(tileClass.wallVariant, x, y, true);
                    }
                }
                else
                {
                    PlaceTile(tileClass, x, y, true);
                }

                if (y >= height - 1)
                {
                    int t = Random.Range(0, curBiome.treeChance);

                    if (t == 1)
                    {
                        if (GetTileFromWorld(x, y))
                        {
                            if(curBiome.biomeName == "Desert")
                            {
                                GenerateCactus(curBiome.tileAtlas, Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);
                            }
                            else
                            {
                                GenerateTree(Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);
                            }
                        }
                    }
                    else
                    {
                        int i = Random.Range(0, curBiome.tallGrassChance);

                        if (i == 1)
                        {
                            if(GetTileFromWorld(x, y))
                            {
                                if (curBiome.tileAtlas.tallGrass != null)
                                {
                                    PlaceTile(curBiome.tileAtlas.tallGrass, x, y + 1, true);
                                }
                            }
                        }
                    }
                }
            }
        }
        worldTilesMap.Apply();
    }

    public void GenerateCactus(TileAtlas atlas, int treeHeight, int x, int y)
    {
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(atlas.log, x, y + i, true);
        }
    }

    public void GenerateTree(int treeHeight, int x, int y)
    {
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log, x, y + i, true);
        }

        PlaceTile(tileAtlas.leaf, x, y + treeHeight, true);
        PlaceTile(tileAtlas.leaf, x, y + treeHeight + 1, true);
        PlaceTile(tileAtlas.leaf, x, y + treeHeight + 2, true);

        PlaceTile(tileAtlas.leaf, x - 1, y + treeHeight, true);
        PlaceTile(tileAtlas.leaf, x - 1, y + treeHeight + 1, true);

        PlaceTile(tileAtlas.leaf, x + 1, y + treeHeight, true);
        PlaceTile(tileAtlas.leaf, x + 1, y + treeHeight + 1, true);
    }

    public void RemoveTile(int x, int y)
    {
        if (GetTileFromWorld(x, y) && x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            TileClass tile = GetTileFromWorld(x, y);
            RemoveTileFromWorld(x, y);
            if (tile.wallVariant != null)
            {
                if (tile.naturallyPlaced)
                {
                    PlaceTile(tile.wallVariant, x, y, true);
                }
            }

            if (tile.tileDrop)
            {
                GameObject newtileDrop = Instantiate(tileDrop, new Vector2(x + 0.5f, y + 0.5f), Quaternion.identity);
                newtileDrop.GetComponent<SpriteRenderer>().sprite = tile.tileDrop.tileSprites[0];
                ItemClass tileDropItem = new ItemClass(tile.tileDrop);
                newtileDrop.GetComponent<TileDropControiler>().item = tileDropItem;
            }

            if (!GetTileFromWorld(x, y))
            {
                worldTilesMap.SetPixel(x, y, Color.white);
                LightBlock(x, y, 1, 0);
                worldTilesMap.Apply();
            }

            Destroy(GetObjectFromWorld(x, y));
            RemoveObjectFromWorld(x, y);
        }
    }

    public void CheckTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    {
        if (x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            if (tile.inBackground)
            {

                if (GetTileFromWorld(x + 1, y) || GetTileFromWorld(x - 1, y) || GetTileFromWorld(x, y + 1) || GetTileFromWorld(x, y - 1))
                {
                    if (!GetTileFromWorld(x, y))
                    {
                        RemoveLidhtSourse(x, y);
                        PlaceTile(tile, x, y, isNaturallyPlaced);
                    }
                    else
                    {
                        if (!GetTileFromWorld(x, y).inBackground)
                        {
                            RemoveLidhtSourse(x, y);
                            PlaceTile(tile, x, y, isNaturallyPlaced);
                        }
                    }
                }
            }
            else
            {
                if (GetTileFromWorld(x + 1, y) || GetTileFromWorld(x - 1, y) || GetTileFromWorld(x, y + 1) || GetTileFromWorld(x, y - 1))
                {
                    if (!GetTileFromWorld(x, y))
                    {
                            RemoveLidhtSourse(x, y);
                            PlaceTile(tile, x, y, isNaturallyPlaced);
                    }
                    else
                    {
                        if (GetTileFromWorld(x, y).inBackground)
                        {
                            RemoveLidhtSourse(x, y);
                            PlaceTile(tile, x, y, isNaturallyPlaced);
                        }
                    }
                }
            }
        }
    }

    public void PlaceTile(TileClass tile, int x, int y, bool isNaturallyPlaced)
    {
        if (x >= 0 && x <= worldSize && y >= 0 && y <= worldSize)
        {
            GameObject newTile = new GameObject();

            int chunkCoord = Mathf.RoundToInt(Mathf.Round(x / chunkSize) * chunkSize);
            chunkCoord /= chunkSize;

            newTile.transform.parent = worldChunks[chunkCoord].transform;

            newTile.AddComponent<SpriteRenderer>();

            int spriteIndex = Random.Range(0, tile.tileSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = tile.tileSprites[spriteIndex];


            worldTilesMap.SetPixel(x, y, Color.black);

            if (tile.inBackground)
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -10;

                if (tile.name.ToLower().Contains("wall"))
                {
                    newTile.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
                }
                else
                {
                    worldTilesMap.SetPixel(x, y, Color.white);
                }
            }
            else
            {
                newTile.GetComponent<SpriteRenderer>().sortingOrder = -5;
                newTile.AddComponent<BoxCollider2D>();
                newTile.GetComponent<BoxCollider2D>().size = Vector2.one;
                newTile.tag = "ground";
            }

            newTile.name = tile.tileSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            TileClass newTileClass = TileClass.CreateInstance(tile, isNaturallyPlaced);

            AddObjectToWorld(x, y, newTile, newTileClass);
            AddTileToWorld(x, y, newTileClass);
        }
    }

    public void AddTileToWorld(int x, int y, TileClass tile)
    {
        if (tile.inBackground)
        {
            world_BackgrondTiles[x, y] = tile;
        }
        else
        {
            world_ForegroundTiles[x, y] = tile;
        }
    }

    public void AddObjectToWorld(int x, int y, GameObject tileOblekt, TileClass tile)
    {
        if (tile.inBackground)
        {
            world_BackgrondObjekts[x, y] = tileOblekt;
        }
        else
        {
            world_ForegroundObjekts[x, y] = tileOblekt;
        }
    }

    public void RemoveTileFromWorld(int x, int y)
    {
        if (world_ForegroundTiles[x, y] != null)
        {
            world_ForegroundTiles[x, y] = null;
        }
        else if (world_BackgrondTiles[x, y] != null)
        {
            world_BackgrondTiles[x, y] = null;
        }
    }

    public void RemoveObjectFromWorld(int x, int y)
    {
        if (world_ForegroundObjekts[x, y] != null)
        {
            world_ForegroundObjekts[x, y] = null;
        }
        else if (world_BackgrondObjekts[x, y] != null)
        {
            world_BackgrondObjekts[x, y] = null;
        }
    }

    GameObject GetObjectFromWorld(int x, int y)
    {
        if (world_ForegroundObjekts[x, y] != null)
        {
            return world_ForegroundObjekts[x, y];
        }
        else if (world_BackgrondObjekts[x, y] != null)
        {
            return world_BackgrondObjekts[x, y];
        }

        return null;
    }

    TileClass GetTileFromWorld(int x, int y)
    {
        if (world_ForegroundTiles[x, y] != null)
        {
            return world_ForegroundTiles[x, y];
        }
        else if (world_BackgrondTiles[x, y] != null)
        {
            return world_BackgrondTiles[x, y];
        }

        return null;
    }

    public void LightBlock(int x, int y, float intensivity, int interation)
    {
        if (interation < lightRadius)
        {
            worldTilesMap.SetPixel(x, y, Color.white * intensivity);


            float thresh = groundLightThreshold;

            if (x >= 0 && x < worldSize && y >= 0 && y < worldSize)
            {
                if (world_ForegroundTiles[x, y])
                {
                    thresh = groundLightThreshold;
                }
                else
                {
                    thresh = airLightThreshold;
                }
            }

            for (int nx = x - 1; nx < x + 2; nx++)
            {
                for (int ny = y - 1; ny < y + 2; ny++)
                {
                    if (nx != x || ny != y)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(nx, ny));
                        float targetIntensity = Mathf.Pow(thresh, dist) * intensivity;

                        if (worldTilesMap.GetPixel(nx, ny).r < targetIntensity)
                        {
                            LightBlock(nx, ny, targetIntensity, interation + 1);
                        }
                    }
                }
            }
            worldTilesMap.Apply();
        }
    }
    public void RemoveLidhtSourse(int x, int y)
    {
        unlitBloks.Clear();
        UnLightBlock(x, y, x, y);

        List<Vector2Int> toRelight = new List<Vector2Int>();

        foreach (Vector2Int block in unlitBloks)
        {
            for (int nx = block.x - 1; nx < block.x + 2; nx++)
            {
                for (int ny = block.y - 1; ny < block.y + 2; ny++)
                {
                    if (worldTilesMap.GetPixel(nx, ny) != null)
                    {
                        if (worldTilesMap.GetPixel(nx, ny).r > worldTilesMap.GetPixel(block.x, block.y).r)
                        {
                            if (!toRelight.Contains(new Vector2Int(nx, ny)))
                            {
                                toRelight.Add(new Vector2Int(nx, ny));
                            }
                        }
                    }
                }
            }
        }
        foreach (Vector2Int source in toRelight)
        {
            LightBlock(source.x ,source.y, worldTilesMap.GetPixel(source.x, source.y).r, 0);
        }

        worldTilesMap.Apply();
    }

    public void UnLightBlock(int x, int y, int ix, int iy)
    {
        if (Mathf.Abs(x - ix) >= lightRadius || Mathf.Abs(y - iy) >= lightRadius || unlitBloks.Contains(new Vector2Int(x, y)))
        {
            return;
        }
        for (int nx = x - 1; nx < x + 2; nx++)
        {
            for (int ny = y - 1; ny < y + 2; ny++)
            {
                if(nx != x || ny != y)
                {
                    if (worldTilesMap.GetPixel(nx, ny) != null)
                    {
                        if (worldTilesMap.GetPixel(nx, ny).r < worldTilesMap.GetPixel(x, y).r)
                        {
                            UnLightBlock(nx, ny, ix, iy);
                        }
                    }
                }
            }
        }

        worldTilesMap.SetPixel(x, y, Color.black);
        unlitBloks.Add(new Vector2Int(x, y));
    }
}