using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GemFactory 
{
    private readonly float tileSize;
    private readonly Vector2 boardOffset;
    private readonly Transform parent;
    private readonly Board board;

    private readonly Gem[] activeGems;
    private readonly int maxIterations = 100;

    public bool IsStone(Gem gem) => gem != null && gem.gemType == Gem.GemType.Stone;

    public GemFactory(float tileSize, Vector2 boardOffset, Transform parent, Board board, Gem[] activeGems)
    {
        this.tileSize = tileSize;
        this.boardOffset = boardOffset;
        this.parent = parent;
        this.board = board;
        this.activeGems = activeGems;
    }

    public Gem CreateGem(Gem gemPrefab, Vector2Int gridPos)
    {
        Gem gem = GemPoolManager.instance.GetGem(gemPrefab.gemType);
        gem.transform.position = new Vector3(gridPos.x * tileSize, gridPos.y * tileSize, -0.1f) + (Vector3)boardOffset;
        gem.name = $"Gem ({gridPos.x}, {gridPos.y})";
        gem.SetupGem(gridPos, board);
        return gem;
    }

    public Gem GetRandomNonStoneGem()
    {
        int gemToUse = Random.Range(0, activeGems.Length);
        int iterations = 0;

        while (activeGems[gemToUse].gemType == Gem.GemType.Stone && iterations < maxIterations)
        {
            gemToUse = Random.Range(0, activeGems.Length);
            iterations++;
        }

        return activeGems[gemToUse];
    }

    public Gem FindStoneGem()
    {
        return activeGems.FirstOrDefault(g => g.gemType == Gem.GemType.Stone);
    }
}
