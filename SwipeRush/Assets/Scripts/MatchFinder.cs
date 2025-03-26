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
        if (x > 0 && x < board.width - 1)
        {
            Gem leftGem = board.allGems[x - 1, y];
            Gem rightGem = board.allGems[x + 1, y];
            if (leftGem != null && rightGem != null)
            {
                if (leftGem.gemType == currentGem.gemType && rightGem.gemType == currentGem.gemType) // 가로로 3개 이상 매치
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

    private void CheckVerticalMatch(int x, int y, Gem currentGem)
    {
        if (y > 0 && y < board.height - 1)
        {
            Gem upGem = board.allGems[x, y + 1];
            Gem downGem = board.allGems[x, y - 1];
            if (upGem != null && downGem != null)
            {
                if (upGem.gemType == currentGem.gemType && downGem.gemType == currentGem.gemType) // 세로로 3개 이상 매치
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