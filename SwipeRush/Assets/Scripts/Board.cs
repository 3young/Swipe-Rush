using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Gem;
using static UnityEditor.PlayerSettings;

public class Board : MonoBehaviour
{
    [HideInInspector] public float tileSize;
    [HideInInspector] public Vector2 boardOffset;

    public int width;
    public int height;
    public GameObject tilePrefab;

    public Gem[] gems; // 보석 종류 배열
    public Gem[,] allGems; // 보드의 모든 보석을 저장하는 배열

    public float gemSpeed = 0; // Board 클래스에서 보석 이동 속도 관리 -> 보드의 모든 보석이 동일한 속도로 이동

    public MatchFinder matchFinder; 

    public enum BoardState { moving, waiting }
    public BoardState currentState = BoardState.moving;

    private void Awake()
    {
        if(matchFinder == null)
           matchFinder = Object.FindFirstObjectByType<MatchFinder>(); 
    }

    void Start()
    {
        tileSize = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        boardOffset = new Vector2(-width / 2.0f + 0.5f, -height / 2.0f + 0.5f) * tileSize;

        allGems = new Gem[width, height];
        SetupBoard();
    }

    // 보드 설정
    private void SetupBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x * tileSize, y * tileSize) + boardOffset;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = $"Tile ({x}, {y})";

                int gemToUse = Random.Range(0, gems.Length); // 보석 종류 랜덤 선택
                int iterations = 0; // 반복 횟수

                while (MatchesAt(new Vector2Int(x, y), gems[gemToUse]) && iterations < 100)
                {
                    gemToUse = Random.Range(0, gems.Length);
                    iterations++;

                    if (iterations > 0 && iterations < 100)
                    {
                        Debug.LogWarning($"Retrying gem selection at ({x}, {y}), attempts: {iterations}");
                    }
                    else if (iterations >= 100)
                    {
                        Debug.LogError("SetupBoard failed: too many retries, possible infinite loop.");
                    }
                }
                
                SpawnGem(new Vector2Int(x, y), gems[gemToUse]);
            }
        }
    }

    // 보석 생성
    private void SpawnGem(Vector2Int gridPosition, Gem gemToSpawn)
    {
        Vector3 spawnPosition = new Vector3(gridPosition.x * tileSize, (gridPosition.y + height) * tileSize, -0.1f) + (Vector3)boardOffset;
        // 보석을 월드 좌표에 생성
        Gem gem = Instantiate(gemToSpawn, spawnPosition, Quaternion.identity);
        gem.transform.parent = transform;
        gem.name = $"Gem ({gridPosition.x}, {gridPosition.y})";
        // 보석의 그리드 좌표 설정
        allGems[gridPosition.x, gridPosition.y] = gem;
        // 보석의 그리드 좌표와 보드 참조 설정
        gem.SetupGem(gridPosition, this);
    }

   
    public bool CheckForPossibleMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem currentGem = allGems[x, y];
                if (currentGem == null) continue;

                Vector2Int[] directions = { Vector2Int.right, Vector2Int.up };

                foreach(Vector2Int direction in directions)
                {
                    int newX = x + direction.x;
                    int newY = y + direction.y;

                    if(IsInBounds(newX, newY))
                    {
                        SwapGems(x, y, newX, newY);
                        matchFinder.FindAllMatches();

                        if (matchFinder.currentMatches.Count > 0)
                        {
                            SwapGems(x, y, newX, newY);
                            matchFinder.currentMatches.Clear();
                            return true;
                        }
                        SwapGems(x, y, newX, newY);
                        matchFinder.currentMatches.Clear();
                    }
                }
            }
        }
        return false;
    }

    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void SwapGems(int x1, int y1, int x2, int y2)
    {
        Gem temp = allGems[x1, y1];
        allGems[x1, y1] = allGems[x2, y2];
        allGems[x2, y2] = temp;

        if (allGems[x1, y1] != null)
        {
            allGems[x1, y1].gridIndex = new Vector2Int(x1, y1);
        }
        if(allGems[x2, y2] != null)
        {
            allGems[x2, y2].gridIndex = new Vector2Int(x2, y2);
        }
    }

    public IEnumerator ShuffleBoardCo()
    {
        currentState = BoardState.waiting;
        yield return new WaitForSeconds(1.5f);

        List<Gem> gemList = new List<Gem>();
        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null)
                {
                    gemList.Add(allGems[x, y]);
                    allGems[x, y] = null;
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                yield return new WaitForSeconds(0.01f);

                int gemIndex = Random.Range(0, gemList.Count);
                Gem gem = gemList[gemIndex];
                gem.SetupGem(new Vector2Int(x, y), this);
                allGems[x, y] = gem;
                gemList.RemoveAt(gemIndex);

                gem.transform.position = GetWorldPosition(new Vector2Int(x, y));
            }
        }
        currentState = BoardState.moving;
    }

    private Vector3 GetWorldPosition(Vector2Int gridIndex)
    {
        return new Vector3(gridIndex.x * tileSize, gridIndex.y * tileSize, -0.1f) + (Vector3)boardOffset;
    }

    // 매치된 보석 확인
    bool MatchesAt(Vector2Int positionToCheck, Gem gemToCheck)
    {
        if(positionToCheck.x > 1)
        {
            if (allGems[positionToCheck.x - 1, positionToCheck.y].gemType == gemToCheck.gemType &&
                allGems[positionToCheck.x - 2, positionToCheck.y].gemType == gemToCheck.gemType)
            {
                return true;
            }
        }

        if (positionToCheck.y > 1)
        {
            if (allGems[positionToCheck.x, positionToCheck.y - 1].gemType == gemToCheck.gemType &&
                allGems[positionToCheck.x, positionToCheck.y - 2].gemType == gemToCheck.gemType)
            {
                return true;
            }
        }

        return false;
    }

    // 매치된 보석 제거
    public void DestroyMatches()
    {
        foreach (var gem in matchFinder.currentMatches)
        {
            if (gem != null)
            {
                DestroyMatchedGemAt(gem.gridIndex);
            }
        }

        matchFinder.currentMatches.Clear();
        ResetAllMatchFlags();
        StartCoroutine(DecreaseRowCo());
    }

    private void ResetAllMatchFlags()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null)
                {
                    allGems[x, y].isMatched = false;
                }
            }
        }
    }

    // 매치된 보석 제거
    private void DestroyMatchedGemAt(Vector2Int gridIndex)
    {
        var gem = allGems[gridIndex.x, gridIndex.y];

        if (gem != null && gem.isMatched)
        {
            Destroy(gem.gameObject);
            allGems[gridIndex.x, gridIndex.y] = null;
        }
    }

    // 보석 이동 코루틴 함수
    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(0.25f);

        for(int x = 0; x < width; x++)
        {
            int nullCount = 0; 

            for(int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    nullCount++;
                }
                else if(nullCount > 0)
                {
                    var gemToMove = allGems[x, y];
                    allGems[x, y - nullCount] = gemToMove; 
                    gemToMove.gridIndex = new Vector2Int(x, y - nullCount); // 보석의 그리드 좌표 업데이트
                    allGems[x, y] = null; // 이동한 보석의 이전 위치를 null로 설정
                }
            }
        }
        StartCoroutine(FillBoardCo());
    }

    public IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(0.3f);
        RefillBoard();

        yield return new WaitForSeconds(0.3f);
        matchFinder.FindAllMatches();

        if(matchFinder.currentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.3f);
            DestroyMatches();
        }
        else
        {
            if (!CheckForPossibleMatches())
            {
                yield return StartCoroutine(ShuffleBoardCo());
                yield return StartCoroutine(FillBoardCo());
            }
            else
            {
                yield return new WaitForSeconds(0.3f);
                currentState = BoardState.moving;
            }
        }
    }

    private void RefillBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    int gemToUse = Random.Range(0, gems.Length);
                    SpawnGem(new Vector2Int(x, y), gems[gemToUse]);
                }
            }
        }

        CleanUpUnregisteredGems();
    }

    // 잉여 보석 제거
    private void CleanUpUnregisteredGems()
    {
        // 모든 Gem 오브젝트를 포함하는 리스트
        List<Gem> foundGems = new List<Gem>();
        foundGems.AddRange(Object.FindObjectsByType<Gem>(FindObjectsSortMode.None)); 

        // 보드 배열 안에 있는 보석들을 리스트에서 제거
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (foundGems.Contains(allGems[x, y]))
                {
                    foundGems.Remove(allGems[x, y]);
                }
            }
        }

        // 떠도는 보석들 제거
        foreach (Gem gem in foundGems)
        {
            Destroy(gem.gameObject);
        }
    }
}
