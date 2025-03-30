using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 보석을 생성하는 팩토리 클래스
/// 다양한 타입의 보석을 생성하고 관리
/// </summary>
public class GemFactory 
{
    private readonly float tileSize;       // 타일 크기
    private readonly Vector2 boardOffset;  // 보드 오프셋
    private readonly Transform parent;     // 부모 Transform
    private readonly Board board;          // 보드 참조

    private readonly Gem[] activeGems;     // 활성화된 보석 배열
    private readonly int maxIterations = 100; // 최대 시도 횟수

    /// <summary>
    /// 주어진 보석이 스톤 타입인지 확인
    /// </summary>
    /// <param name="gem">확인할 보석</param>
    /// <returns>스톤 타입이면 true, 아니면 false</returns>
    public bool IsStone(Gem gem) => gem != null && gem.gemType == Gem.GemType.Stone;

    /// <summary>
    /// GemFactory를 초기화
    /// </summary>
    /// <param name="tileSize">타일 크기</param>
    /// <param name="boardOffset">보드 오프셋</param>
    /// <param name="parent">부모 Transform</param>
    /// <param name="board">보드 참조</param>
    /// <param name="activeGems">활성화된 보석 배열</param>
    public GemFactory(float tileSize, Vector2 boardOffset, Transform parent, Board board, Gem[] activeGems)
    {
        this.tileSize = tileSize;
        this.boardOffset = boardOffset;
        this.parent = parent;
        this.board = board;
        this.activeGems = activeGems;
    }

    /// <summary>
    /// 지정된 위치에 보석을 생성
    /// </summary>
    /// <param name="gemPrefab">생성할 보석 프리팹</param>
    /// <param name="gridPos">그리드 위치</param>
    /// <returns>생성된 보석</returns>
    public Gem CreateGem(Gem gemPrefab, Vector2Int gridPos)
    {
        Gem gem = GemPoolManager.instance.GetGem(gemPrefab.gemType);
        gem.transform.position = new Vector3(gridPos.x * tileSize, gridPos.y * tileSize, -0.1f) + (Vector3)boardOffset;
        gem.name = $"Gem ({gridPos.x}, {gridPos.y})";
        gem.SetupGem(gridPos, board);
        return gem;
    }

    /// <summary>
    /// 스톤 타입이 아닌 랜덤 보석을 반환
    /// </summary>
    /// <returns>랜덤 보석 (스톤 타입 제외)</returns>
    public Gem GetRandomNonStoneGem()
    {
        int gemToUse = Random.Range(0, activeGems.Length);
        int iterations = 0;

        // 스톤 타입이 아닌 보석 선택
        while (activeGems[gemToUse].gemType == Gem.GemType.Stone && iterations < maxIterations)
        {
            gemToUse = Random.Range(0, activeGems.Length);
            iterations++;
        }

        return activeGems[gemToUse];
    }

    /// <summary>
    /// 활성화된 보석 중에서 스톤 타입 보석을 탐색
    /// </summary>
    /// <returns>스톤 타입 보석 또는 없으면 null</returns>
    public Gem FindStoneGem()
    {
        return activeGems.FirstOrDefault(g => g.gemType == Gem.GemType.Stone);
    }
}
