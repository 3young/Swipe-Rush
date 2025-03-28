using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;
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
    public LevelData[] levelDataArray; // 레벨 데이터 배열
    public Gem[] activeGems; // 활성화된 보석 배열

    public Gem[,] allGems; // 보드의 모든 보석을 저장하는 배열

    public float gemSpeed = 15; // Board 클래스에서 보석 이동 속도 관리 -> 보드의 모든 보석이 동일한 속도로 이동

    public MatchFinder matchFinder; 

    public enum BoardState { moving, waiting }
    public BoardState currentState = BoardState.moving;

    public RoundManager roundManager;
    private LevelData currentLevelData;
    private int currentStoneCount = 0;

    private void Awake()
    {
        if (matchFinder == null)
            matchFinder = Object.FindFirstObjectByType<MatchFinder>();

        string currentSceneName = SceneManager.GetActiveScene().name;

        int levelNumber = 1;
        if (currentSceneName.StartsWith("Level") && currentSceneName.Length > 5)
        {
            int.TryParse(currentSceneName.Substring(5), out levelNumber);
            levelNumber = Mathf.Clamp(levelNumber, 1, levelDataArray.Length);
        }

        if (levelNumber <= levelDataArray.Length && levelDataArray[levelNumber - 1] != null)
        {
            currentLevelData = levelDataArray[levelNumber - 1];
            activeGems = currentLevelData.availableGems;
            width = currentLevelData.width;
            height = currentLevelData.height;
        }
        else
        {
            Debug.LogWarning($"레벨 데이터가 없습니다: Level {levelNumber}, 기본 설정을 사용합니다.");
            activeGems = gems; // 기본 보석 사용
        }
    }

    private void Start()
    {
        Time.timeScale = 1;
        ScoreManager.instance.ResetScore();

        tileSize = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        boardOffset = new Vector2(-width / 2.0f + 0.5f, -height / 2.0f + 0.5f) * tileSize;

        allGems = new Gem[width, height];
        SetupBoard();
        CountStoneBlocks();
    }

    // 보드 설정
    private void SetupBoard()
    {
        // 스톤 카운트 초기화
        currentStoneCount = 0;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x * tileSize, y * tileSize) + boardOffset;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.transform.parent = transform;
                tile.name = $"Tile ({x}, {y})";

                // 스톤 생성 조건
                bool createStone = currentStoneCount < currentLevelData.maxStoneBlocks && 
                                  Random.value < currentLevelData.stoneSpawnChance;
                
                Gem gemToSpawn;
                if (createStone)
                {
                    // 스톤 젬 찾기
                    Gem stoneGem = null;
                    foreach (Gem gem in activeGems)
                    {
                        if (gem.gemType == Gem.GemType.Stone)
                        {
                            stoneGem = gem;
                            break;
                        }
                    }
                    
                    if (stoneGem != null)
                    {
                        gemToSpawn = stoneGem;
                        currentStoneCount++;
                        SpawnGem(new Vector2Int(x, y), gemToSpawn);
                        continue; // 스톤은 매치 체크 없이 바로 생성
                    }
                }
                
                // 일반 젬 생성
                int gemToUse = Random.Range(0, activeGems.Length);
                int iterations = 0;
                
                // 스톤이 아닌 젬만 선택
                while (activeGems[gemToUse].gemType == Gem.GemType.Stone && iterations < 100)
                {
                    gemToUse = Random.Range(0, activeGems.Length);
                    iterations++;
                }
                
                // 매치 체크 반복
                iterations = 0;
                while (MatchesAt(new Vector2Int(x, y), activeGems[gemToUse]) && iterations < 100)
                {
                    gemToUse = Random.Range(0, activeGems.Length);
                    // 스톤 제외
                    while (activeGems[gemToUse].gemType == Gem.GemType.Stone && iterations < 100)
                    {
                        gemToUse = Random.Range(0, activeGems.Length);
                        iterations++;
                    }
                    
                    iterations++;
                    if (iterations >= 100)
                    {
                        Debug.LogError("SetupBoard failed: too many retries, possible infinite loop.");
                    }
                }
                
                SpawnGem(new Vector2Int(x, y), activeGems[gemToUse]);
            }
        }
    }

    // 보석 생성
    private void SpawnGem(Vector2Int gridPosition, Gem gemToSpawn)
    {
        if (gemToSpawn.gemType == GemType.Stone)
        {
            if (currentStoneCount >= currentLevelData.maxStoneBlocks)
            {
                Gem nonStoneGem = null;
                foreach (Gem activeGem in activeGems)
                {
                    if (activeGem.gemType != Gem.GemType.Stone)
                    {
                        nonStoneGem = activeGem;
                        break;
                    }
                }

                if (nonStoneGem != null)
                {
                    gemToSpawn = nonStoneGem;
                }
            }
            else
            {
                currentStoneCount++;
            }
        }

        // 모든 보석 타입(스톤 포함)에 대해 동일한 스폰 위치 사용
        Vector3 spawnPosition = new Vector3(gridPosition.x * tileSize, (gridPosition.y + height) * tileSize, -0.1f) + (Vector3)boardOffset;
        
        // 보석을 월드 좌표에 생성
        Gem gem = Instantiate(gemToSpawn, spawnPosition, Quaternion.identity);
        gem.transform.parent = transform;
        gem.name = $"Gem ({gridPosition.x}, {gridPosition.y})";
        
        // 보석의 그리드 좌표 설정
        allGems[gridPosition.x, gridPosition.y] = gem;
        
        // 보석의 그리드 좌표와 보드 참조 설정
        gem.SetupGem(gridPosition, this);
        
        // 스톤 보석이면 바로 최종 위치로 이동시킴
        if (gem.gemType == GemType.Stone)
        {
            gem.transform.position = GetWorldPosition(gridPosition);
        }
    }

    public bool CheckForPossibleMatches()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem currentGem = allGems[x, y];
                if (currentGem == null) continue;
                
                // 이동 가능한 보석인지 확인
                if (!currentGem.IsMovable)
                    continue;

                Vector2Int[] directions = { Vector2Int.right, Vector2Int.up };

                foreach(Vector2Int direction in directions)
                {
                    int newX = x + direction.x;
                    int newY = y + direction.y;

                    if(IsInBounds(newX, newY))
                    {
                        Gem otherGem = allGems[newX, newY];
                        
                        // 대상 보석도 이동 가능한지 확인
                        if (otherGem == null || !otherGem.IsMovable)
                            continue;
                        
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
        Gem gem1 = allGems[x1, y1];
        Gem gem2 = allGems[x2, y2];

        // 속성 사용하여 이동 가능한지 확인
        if (gem1 == null || gem2 == null || !gem1.IsMovable || !gem2.IsMovable)
        {
            return;
        }

        Gem temp = allGems[x1, y1];
        allGems[x1, y1] = allGems[x2, y2];
        allGems[x2, y2] = temp;

        if (allGems[x1, y1] != null)
        {
            allGems[x1, y1].gridIndex = new Vector2Int(x1, y1);
        }
        if (allGems[x2, y2] != null)
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

    // 매치된 보석 확인 (수정)
    bool MatchesAt(Vector2Int positionToCheck, Gem gemToCheck)
    {
        // 스톤은 매치 체크에서 제외
        if (gemToCheck.gemType == GemType.Stone)
        {
            return false;
        }

        if(positionToCheck.x > 1)
        {
            if (allGems[positionToCheck.x - 1, positionToCheck.y] != null && 
                allGems[positionToCheck.x - 2, positionToCheck.y] != null &&
                allGems[positionToCheck.x - 1, positionToCheck.y].gemType == gemToCheck.gemType &&
                allGems[positionToCheck.x - 2, positionToCheck.y].gemType == gemToCheck.gemType)
            {
                return true;
            }
        }

        if (positionToCheck.y > 1)
        {
            if (allGems[positionToCheck.x, positionToCheck.y - 1] != null && 
                allGems[positionToCheck.x, positionToCheck.y - 2] != null &&
                allGems[positionToCheck.x, positionToCheck.y - 1].gemType == gemToCheck.gemType &&
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
        if (matchFinder.currentMatches.Count == 0)
        {
            return;
        }

        List<Gem> validMatches = new List<Gem>();
        
        // 유효한 매치만 필터링
        foreach (Gem gem in matchFinder.currentMatches)
        {
            if (gem != null && gem.isMatched && gem.IsDestructible)
            {
                validMatches.Add(gem);
            }
        }
        
        // 유효한 매치만 처리
        foreach (Gem gem in validMatches)
        {
            DestroyMatchedGemAt(gem.gridIndex.x, gem.gridIndex.y);
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
    private void DestroyMatchedGemAt(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            Debug.LogWarning($"범위 밖의 좌표: ({x}, {y})");
            return;
        }

        Gem gem = allGems[x, y];

        if (gem == null)
        {
            Debug.LogWarning($"좌표 ({x}, {y})에 젬이 없음");
            return;
        }

        // 추가 검증
        if (gem.gridIndex.x != x || gem.gridIndex.y != y)
        {
            Debug.LogWarning($"젬 좌표 불일치: 젬 내부 ({gem.gridIndex.x}, {gem.gridIndex.y}) vs 배열 좌표 ({x}, {y})");
            return;
        }

        // 파괴 가능한 보석인지 확인
        if (!gem.IsDestructible)
        {
            gem.isMatched = false;
            return;
        }

        if (!gem.isMatched)
        {
            Debug.LogWarning($"매치되지 않은 젬 파괴 시도: ({x}, {y})");
            return;
        }

        gem.isMatched = false;

        ScoreManager.instance.AddScore(gem.scoreValue);

        if (gem.destroyEffect != null)
        {
            GameObject effect = Instantiate(gem.destroyEffect, gem.transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        SFXManager.instance.PlayGemBreak();

        Debug.Log($"젬 파괴: {gem.name} at ({x}, {y})");
        Destroy(gem.gameObject);
        allGems[x, y] = null;
    }

    // 보석 이동 코루틴 함수 수정
    private IEnumerator DecreaseRowCo()
    {
        yield return new WaitForSeconds(0.25f);

        // 모든 보석의 위치 검증
        ValidateGemPositions();

        for(int x = 0; x < width; x++)
        {
            int nullCount = 0; 

            for(int y = 0; y < height; y++)
            {
                // 1. 빈 공간이 있는지 확인
                if (allGems[x, y] == null)
                {
                    nullCount++;
                }
                // 2. 빈 공간 위에 있는 보석 이동 처리
                else if(nullCount > 0)
                {
                    var gemToMove = allGems[x, y];
                    
                    // 로그 추가
                    Debug.Log($"젬 이동: {gemToMove.name} 위치 ({x}, {y}) -> ({x}, {y - nullCount})");
                    
                    // 이동 가능한 보석인지 확인 
                    if (!gemToMove.IsMovable)
                    {
                        nullCount = 0; // 이 위치에서 nullCount 리셋
                        continue;
                    }
                    
                    // 위치 이동 처리
                    if(y - nullCount >= 0) // 유효한 인덱스인지 확인
                    {
                        allGems[x, y - nullCount] = gemToMove; 
                        gemToMove.gridIndex = new Vector2Int(x, y - nullCount);
                        allGems[x, y] = null;
                    }
                    else
                    {
                        Debug.LogError($"잘못된 이동 좌표 (y - nullCount < 0): ({x}, {y - nullCount})");
                    }
                }
            }
        }
        
        // 다시 한 번 위치 검증
        ValidateGemPositions();
        
        StartCoroutine(FillBoardCo());
    }

    public IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(0.2f);
        
        // 빈 타일 확인 및 로깅
        int emptyTiles = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    emptyTiles++;
                }
            }
        }
        Debug.Log($"FillBoardCo 시작: 빈 타일 개수 = {emptyTiles}");
        
        RefillBoard();
        
        // 스톤 보석 위치 강제 고정
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem gem = allGems[x, y];
                if (gem != null && (gem.gemType == Gem.GemType.Stone || gem.isIndestructible))
                {
                    gem.transform.position = GetWorldPosition(new Vector2Int(x, y));
                }
            }
        }
        
        // 빈 타일 다시 확인
        emptyTiles = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    emptyTiles++;
                    Debug.LogWarning($"리필 후에도 비어있는 타일: ({x}, {y})");
                }
            }
        }
        Debug.Log($"리필 후 빈 타일 개수 = {emptyTiles}");

        yield return new WaitForSeconds(0.2f);
        matchFinder.FindAllMatches();

        if(matchFinder.currentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.2f);
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
                yield return new WaitForSeconds(0.2f);
                currentState = BoardState.moving;
            }
        }
    }

    private void RefillBoard()
    {
        int stonesToSpawn = 0; // 이번 리필에서 생성할 스톤 수 제한
        int maxNewStones = 1; // 한 번에 최대 1개의 새 스톤만 허용
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    bool createStone = false;

                    if (currentStoneCount < currentLevelData.maxStoneBlocks && 
                        stonesToSpawn < maxNewStones &&
                        Random.value < currentLevelData.stoneSpawnChance)
                    {
                        createStone = true;
                        stonesToSpawn++;
                    }

                    if (createStone)
                    {
                        Gem stoneGem = null;
                        foreach (Gem gem in activeGems)
                        {
                            if (gem.gemType == Gem.GemType.Stone)
                            {
                                stoneGem = gem;
                                break;
                            }
                        }

                        if (stoneGem != null)
                        {
                            SpawnGem(new Vector2Int(x, y), stoneGem);
                        }
                        else
                        {
                            int gemToUse = Random.Range(0, activeGems.Length);
                            SpawnGem(new Vector2Int(x, y), activeGems[gemToUse]);
                        }
                    }
                    else
                    {
                        int gemToUse = Random.Range(0, activeGems.Length);

                        int attempts = 0;
                        while (activeGems[gemToUse].gemType == Gem.GemType.Stone && attempts < 100)
                        {
                            gemToUse = Random.Range(0, activeGems.Length);
                            attempts++;
                        }
                        SpawnGem(new Vector2Int(x, y), activeGems[gemToUse]);
                    }
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

    private void CountStoneBlocks()
    {
        currentStoneCount = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] != null && allGems[x, y].gemType == Gem.GemType.Stone)
                {
                    currentStoneCount++;
                }
            }
        }
    }

    // 보드에 정의하세요
    public void ValidateGemPositions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem gem = allGems[x, y];
                if (gem != null)
                {
                    if (gem.gridIndex.x != x || gem.gridIndex.y != y)
                    {
                        Debug.LogError($"젬 좌표 불일치: {gem.name}, 배열에서는 ({x}, {y})이지만 젬 내부 좌표는 ({gem.gridIndex.x}, {gem.gridIndex.y})");
                        gem.gridIndex = new Vector2Int(x, y); // 강제 수정
                    }
                }
            }
        }
    }
}
