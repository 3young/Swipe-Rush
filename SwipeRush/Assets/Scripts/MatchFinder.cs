using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MatchFinder : MonoBehaviour
{
    private Board board;
    public List<Gem> currentMatches = new List<Gem>(); // 현재 매치된 보석 리스트

    private void Awake()
    {
        board = Object.FindFirstObjectByType<Board>(); // Board 클래스 참조
    }

    public void FindAllMatches()
    {
        currentMatches.Clear(); // 매치된 보석 리스트 초기화
        
        // 모든 젬의 매치 상태 초기화
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

    private void CheckHorizontalMatch(int x, int y, Gem currentGem)
    {
        // 매치 가능한 보석인지 확인
        if (currentGem == null || !currentGem.IsMatchable)
        {
            return;
        }

        // 추가 검증 - 좌표가 일치하는지 확인
        if (currentGem.gridIndex.x != x || currentGem.gridIndex.y != y)
        {
            Debug.LogWarning($"젬 좌표 불일치 (수평 매치 검사): 예상 ({x}, {y}), 실제 ({currentGem.gridIndex.x}, {currentGem.gridIndex.y})");
            return;
        }

        if (x > 0 && x < board.width - 1)
        {
            Gem leftGem = board.allGems[x - 1, y];
            Gem rightGem = board.allGems[x + 1, y];
            
            if (leftGem != null && rightGem != null)
            {
                // 추가 검증 - 좌표가 일치하는지 확인
                if (leftGem.gridIndex.x != x-1 || leftGem.gridIndex.y != y ||
                    rightGem.gridIndex.x != x+1 || rightGem.gridIndex.y != y)
                {
                    Debug.LogWarning($"이웃 젬 좌표 불일치 (수평 매치): 위치 ({x}, {y})");
                    return;
                }
                
                // 속성 사용하여 매치 가능한지 확인
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

    private void CheckVerticalMatch(int x, int y, Gem currentGem)
    {
        // 매치 가능한 보석인지 확인
        if (currentGem == null || !currentGem.IsMatchable)
        {
            return;
        }

        // 추가 검증 - 좌표가 일치하는지 확인
        if (currentGem.gridIndex.x != x || currentGem.gridIndex.y != y)
        {
            Debug.LogWarning($"젬 좌표 불일치 (세로 매치 검사): 예상 ({x}, {y}), 실제 ({currentGem.gridIndex.x}, {currentGem.gridIndex.y})");
            return;
        }

        if (y > 0 && y < board.height - 1)
        {
            Gem upGem = board.allGems[x, y + 1];
            Gem downGem = board.allGems[x, y - 1];
            
            if (upGem != null && downGem != null)
            {
                // 추가 검증 - 좌표가 일치하는지 확인
                if (upGem.gridIndex.x != x || upGem.gridIndex.y != y + 1 ||
                    downGem.gridIndex.x != x || downGem.gridIndex.y != y - 1)
                {
                    Debug.LogWarning($"이웃 젬 좌표 불일치 (세로 매치): 위치 ({x}, {y})");
                    return;
                }
                
                // 속성 사용하여 매치 가능한지 확인
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