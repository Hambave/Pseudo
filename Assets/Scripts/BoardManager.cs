using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class BoardManager : MonoBehaviour
{
    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;

    public int Width;
    public int Height;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;
    public FoodObject[] FoodPrefab;
    public WallObject[] WallPrefab;
    public Enemy[] EnemyPrefab;
    public ExitCellObject ExitCellPrefab;
    public AmmoPickup[] AmmoPrefab;
    public FoodObject[] FoodLootPrefabs;
    public AmmoPickup[] AmmoLootPrefabs;

    public void Init(LevelConfig config)
    {
        Width = config.Width;
        Height = config.Height;

        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (x == 0 || y == 0 || x == Width - 1 || y == Height - 1)
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        m_EmptyCellsList.Remove(new Vector2Int(1, 1));
        Vector2Int exitCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), exitCoord);
        m_EmptyCellsList.Remove(exitCoord);

        GenerateWall(config.WallCount, Width);
        GenerateFood(config.FoodCount);
        GenerateEnemy(config.EnemyCount);
        GenerateAmmo();
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (cellIndex.x < 0 || cellIndex.x >= Width
            || cellIndex.y < 0 || cellIndex.y >= Height)
        {
            return null;
        }

        return m_BoardData[cellIndex.x, cellIndex.y];
    }

    void GenerateAmmo()
    {
        int ammoCount = Random.Range(1, 4);
        for (int i = 0; i < ammoCount && m_EmptyCellsList.Count > 0; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            AmmoPickup newAmmo = Instantiate(AmmoPrefab[Random.Range(0, AmmoPrefab.Length)]);
            AddObject(newAmmo, coord);
        }
    }

    void GenerateFood(int foodCount)
    {
        for (int i = 0; i < foodCount && m_EmptyCellsList.Count > 0; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            FoodObject newFood = Instantiate(FoodPrefab[Random.Range(0, FoodPrefab.Length)]);
            AddObject(newFood, coord);
        }
    }

    void GenerateWall(int wallCount, int roomWidth)
    {
        int attempts = 0;
        int maxAttempts = wallCount * 5;

        while (wallCount > 0 && m_EmptyCellsList.Count > 0 && attempts < maxAttempts)
        {
            int segmentLength = Random.Range(2, roomWidth - 3);
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int startCoord = m_EmptyCellsList[randomIndex];

            Vector2Int[] directions = new Vector2Int[]
            {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right
            };
            Vector2Int dir = directions[Random.Range(0, directions.Length)];

            List<Vector2Int> segment = new List<Vector2Int>();
            bool canPlace = true;

            for (int j = 0; j < segmentLength; ++j)
            {
                Vector2Int coord = startCoord + dir * j;
                if (!m_EmptyCellsList.Contains(coord))
                {
                    canPlace = false;
                    break;
                }
                segment.Add(coord);
            }

            if (canPlace)
            {
                foreach (var coord in segment)
                {
                    WallObject newWall = Instantiate(WallPrefab[Random.Range(0, WallPrefab.Length)]);
                    AddObject(newWall, coord);
                    m_EmptyCellsList.Remove(coord);
                    --wallCount;
                    if (wallCount <= 0) break;
                }
            }

            ++attempts;
        }
    }


    void GenerateEnemy(int enemyCount)
    {
        for (int i = 0; i < enemyCount && m_EmptyCellsList.Count > 0; ++i)
        {
            int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
            Vector2Int coord = m_EmptyCellsList[randomIndex];
            m_EmptyCellsList.RemoveAt(randomIndex);

            Enemy newEnemy = Instantiate(EnemyPrefab[Random.Range(0, EnemyPrefab.Length)]);
            AddObject(newEnemy, coord);
        }
    }

    public void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void Clean()
    {
        if (m_BoardData == null)
            return;


        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];

                if (cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }

                SetCellTile(new Vector2Int(x, y), null);
            }
        }
    }
}
