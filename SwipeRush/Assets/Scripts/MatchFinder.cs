using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 매치-3 패턴을 찾아 관리하는 클래스
/// </summary>
public class MatchFinder : MonoBehaviour
{
    private Board board;            // 보드 참조
    public List<Gem> currentMatches = new List<Gem>(); // 현재 매치된 보석 리스트

    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    private void Awake()
    {
        board = Object.FindFirstObjectByType<Board>(); // Board 클래스 참조
    }

    /// <summary>
    /// 보드에서 모든 매치 패턴을 탐색
    /// </summary>
    public void FindAllMatches()
    {
        currentMatches.Clear(); // 매치된 보석 리스트 초기화
        
        // 모든 보석의 매치 상태 초기화
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Gem gem = board.allGems[x, y];
                if (gem != null)
                {
                    gem.isMatched = false;
                }
            }
        }
        
        // 실제 매치 검사
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Gem currentGem = board.allGems[x, y]; // 현재 보석
                if (currentGem != null)
                {
                    CheckHorizontalMatch(x, y, currentGem);
                    CheckVerticalMatch(x, y, currentGem);
                }
            }
        }

        currentMatches = currentMatches.Distinct().ToList(); // 중복 제거
    }

    /// <summary>
    /// 수평 방향의 매치를 확인
    /// </summary>
    /// <param name="x">보석의 X 좌표</param>
    /// <param name="y">보석의 Y 좌표</param>
    /// <param name="currentGem">검사할 보석</param>
    private void CheckHorizontalMatch(int x, int y, Gem currentGem)
    {
        // 매치 가능한 보석인지 확인
        if (currentGem == null || !currentGem.IsMatchable)
        {
            return;
        }

        // 좌표가 일치하는지 확인
        if (currentGem.gridIndex.x != x || currentGem.gridIndex.y != y)
        {
            return;
        }

        if (x > 0 && x < board.width - 1)
        {
            Gem leftGem = board.allGems[x - 1, y];
            Gem rightGem = board.allGems[x + 1, y];
            
            if (leftGem != null && rightGem != null)
            {
                // 이웃 보석 좌표 확인
                if (leftGem.gridIndex.x != x-1 || leftGem.gridIndex.y != y ||
                    rightGem.gridIndex.x != x+1 || rightGem.gridIndex.y != y)
                {
                    return;
                }
                
                // 매치 가능한지 확인
                if (leftGem.IsMatchable && rightGem.IsMatchable)
                {
                    if (leftGem.gemType == currentGem.gemType && rightGem.gemType == currentGem.gemType)
                    {
                        currentGem.isMatched = true;
                        leftGem.isMatched = true;
                        rightGem.isMatched = true;

                        currentMatches.Add(currentGem);
                        currentMatches.Add(leftGem);
                        currentMatches.Add(rightGem);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 수직 방향의 매치를 확인
    /// </summary>
    /// <param name="x">보석의 X 좌표</param>
    /// <param name="y">보석의 Y 좌표</param>
    /// <param name="currentGem">검사할 보석</param>
    private void CheckVerticalMatch(int x, int y, Gem currentGem)
    {
        // 매치 가능한 보석인지 확인
        if (currentGem == null || !currentGem.IsMatchable)
        {
            return;
        }

        // 좌표가 일치하는지 확인
        if (currentGem.gridIndex.x != x || currentGem.gridIndex.y != y)
        {
            return;
        }

        if (y > 0 && y < board.height - 1)
        {
            Gem upGem = board.allGems[x, y + 1];
            Gem downGem = board.allGems[x, y - 1];
            
            if (upGem != null && downGem != null)
            {
                // 이웃 보석 좌표 확인
                if (upGem.gridIndex.x != x || upGem.gridIndex.y != y + 1 ||
                    downGem.gridIndex.x != x || downGem.gridIndex.y != y - 1)
                {
                    return;
                }
                
                // 매치 가능한지 확인
                if (upGem.IsMatchable && downGem.IsMatchable)
                {
                    if (upGem.gemType == currentGem.gemType && downGem.gemType == currentGem.gemType)
                    {
                        currentGem.isMatched = true;
                        upGem.isMatched = true;
                        downGem.isMatched = true;

                        currentMatches.Add(currentGem);
                        currentMatches.Add(upGem);
                        currentMatches.Add(downGem);
                    }
                }
            }
        }
    }
}