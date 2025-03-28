using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "SwipeRush/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName;
    public int width = 9;
    public int height = 5;
    public Gem[] availableGems;

    [Header("Stone Block Settings")]
    public int maxStoneBlocks = 5;
    public float stoneSpawnChance = 0.05f;
}
