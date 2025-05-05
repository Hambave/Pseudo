using UnityEngine;

public class LevelConfig
{
    public int Width;
    public int Height;
    public int EnemyCount;
    public int WallCount;
    public int FoodCount;

    public LevelConfig(int levelNumber)
    {
        Width = Mathf.Clamp(10 + levelNumber / 2, 10, 30);
        Height = Mathf.Clamp(10 + levelNumber / 2, 10, 30);

        EnemyCount = Mathf.Clamp(2 + levelNumber, 1, 50);

        WallCount = Mathf.Clamp(10 + levelNumber * 2, 6, 60);

        FoodCount = Mathf.Max(10 - levelNumber / 2, 1);
    }
}