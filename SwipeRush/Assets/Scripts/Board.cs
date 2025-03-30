using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Board : MonoBehaviour
{
    [HideInInspector] public float tileSize;
    [HideInInspector] public Vector2 boardOffset;

    public int width;
    public int height;
    public GameObject tilePrefab;

    public Gem[] gems;
    public LevelData[] levelDataArray; 
    public Gem[] activeGems; 

    public Gem[,] allGems; 
    public float gemSpeed = 15;

    public MatchFinder matchFinder; 
    public RoundManager roundManager;

    private GemFactory gemFactory;
    private LevelData currentLevelData;
    private int currentStoneCount = 0;

    public enum BoardState { moving, waiting }
    public BoardState currentState = BoardState.moving;

    private const int MAX_ITERATIONS = 100;

    private void Awake()
    {
        if (matchFinder == null)
            matchFinder = Object.FindFirstObjectByType<MatchFinder>();

        string sceneName = SceneManager.GetActiveScene().name;
        int levelNum = 1;

        if (sceneName.StartsWith("Level") && sceneName.Length > 5)
        {
            int.TryParse(sceneName.Substring(5), out levelNum);
            levelNum = Mathf.Clamp(levelNum, 1, levelDataArray.Length);
        }

        if (levelNum <= levelDataArray.Length && levelDataArray[levelNum - 1] != null)
        {
            currentLevelData = levelDataArray[levelNum - 1];
            activeGems = currentLevelData.availableGems;
            width = currentLevelData.width;
            height = currentLevelData.height;
        }
        else
        {
            Debug.LogWarning($"레벨 데이터가 없음: Level {levelNum}, 기본 설정을 사용");
            activeGems = gems; 
        }
    }

    private void Start()
    {
        Time.timeScale = 1;
        ScoreManager.instance.ResetScore();

        tileSize = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        boardOffset = new Vector2(-width / 2.0f + 0.5f, -height / 2.0f + 0.5f) * tileSize;

        allGems = new Gem[width, height];
        gemFactory = new GemFactory(tileSize, boardOffset, transform, this, activeGems);

        SetupBoard();
        CountStoneBlocks();
    }

    private void SetupBoard()
    {
        currentStoneCount = 0;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x * tileSize, y * tileSize) + boardOffset;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile ({x}, {y})";

                bool createStone = currentStoneCount < currentLevelData.maxStoneBlocks && 
                                  Random.value < currentLevelData.stoneSpawnChance;
                
                Gem gemToSpawn;

                if (createStone)
                {
                    Gem stone = gemFactory.FindStoneGem();
                    if (stone != null)
                    {
                        gemToSpawn = stone;
                        currentStoneCount++;
                        SpawnGem(new Vector2Int(x, y), gemToSpawn);
                        continue; 
                    }
                }
                
                gemToSpawn = gemFactory.GetRandomNonStoneGem();
                int iterations = 0;

                while (IsMatchAt(new Vector2Int(x, y), gemToSpawn) && iterations < MAX_ITERATIONS)
                {
                    gemToSpawn = gemFactory.GetRandomNonStoneGem();
                    iterations++;
                }
                
                SpawnGem(new Vector2Int(x, y), gemToSpawn);
            }
        }
    }

    private void SpawnGem(Vector2Int gridPos, Gem gemPrefab)
    {
        if (gemFactory.IsStone(gemPrefab) && currentStoneCount < currentLevelData.maxStoneBlocks)
        {
            currentStoneCount++;
        }
        else if (gemFactory.IsStone(gemPrefab))
        {
            gemPrefab = gemFactory.GetRandomNonStoneGem();
        }

        Gem gem = GemPoolManager.instance.GetGem(gemPrefab.gemType);
        gem.transform.position = GetWorldPosition(gridPos);
        gem.SetupGem(gridPos, this);

        allGems[gridPos.x, gridPos.y] = gem;

        if (gemFactory.IsStone(gem))
        {
            gem.transform.position = GetWorldPosition(gridPos);
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
    private bool IsMatchAt(Vector2Int position, Gem gem)
    {
        // 스톤은 매치 체크 제외
        if (!gem.IsMatchable) return false;
        return CheckHorizontalMatchAt(position, gem) || CheckVerticalMatchAt(position, gem);
    }

    private bool CheckHorizontalMatchAt(Vector2Int position, Gem gem)
    {
        if (position.x <= 1) return false;
        Gem leftGem1 = allGems[position.x - 1, position.y];
        Gem leftGem2 = allGems[position.x - 2, position.y];
        
        return leftGem1 != null && leftGem2 != null &&
               leftGem1.gemType == gem.gemType &&
               leftGem2.gemType == gem.gemType;
    }

    private bool CheckVerticalMatchAt(Vector2Int position, Gem gem)
    {
        if (position.y <= 1) return false;
        Gem downGem1 = allGems[position.x, position.y - 1];
        Gem downGem2 = allGems[position.x, position.y - 2];
        
        return downGem1 != null && downGem2 != null &&
               downGem1.gemType == gem.gemType &&
               downGem2.gemType == gem.gemType;
    }

    public void DestroyMatchedGems()
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
        StartCoroutine(CollapseEmptySpacesCo());
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
            //Debug.LogWarning($"범위 밖의 좌표: ({x}, {y})");
            return;
        }

        Gem gem = allGems[x, y];

        if (gem == null)
        {
            //Debug.LogWarning($"좌표 ({x}, {y})에 젬이 없음");
            return;
        }

        if (gem.gridIndex.x != x || gem.gridIndex.y != y)
        {
            //Debug.LogWarning($"젬 좌표 불일치: 젬 내부 ({gem.gridIndex.x}, {gem.gridIndex.y}) vs 배열 좌표 ({x}, {y})");
            return;
        }

        if (!gem.IsDestructible)
        {
            //gem.isMatched = false;
            return;
        }

        if (!gem.isMatched)
        {
            //Debug.LogWarning($"매치되지 않은 젬 파괴 시도: ({x}, {y})");
            return;
        }

        gem.isMatched = false;

        ScoreManager.instance.AddScore(gem.scoreValue);

        if (gem.destroyEffect != null)
        {
            GameObject effect = GemPoolManager.instance.GetEffect(gem.destroyEffect);
            effect.transform.position = gem.transform.position;
            effect.SetActive(true);
            StartCoroutine(ReturnEffectAfterDelay(effect,1f));
        }

        SFXManager.instance.PlayGemBreak();

        //Debug.Log($"젬 파괴: {gem.name} at ({x}, {y})");

        allGems[x, y] = null;
        GemPoolManager.instance.ReturnGem(gem);
    }

    private IEnumerator ReturnEffectAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        GemPoolManager.instance.ReturnEffect(obj);
    }

    // 보석 이동 코루틴 함수 수정
    private IEnumerator CollapseEmptySpacesCo()
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
                    continue;
                }
                
                // 이동 불가능한 보석 (스톤 등)은 nullCount 리셋
                if (!allGems[x, y].IsMovable)
                {
                    nullCount = 0;
                    continue;
                }
                
                // 2. 빈 공간 위에 있는 보석 이동 처리
                if(nullCount > 0)
                {
                    var gemToMove = allGems[x, y];
                    
                    // 로그 추가
                    // Debug.Log($"젬 이동: {gemToMove.name} 위치 ({x}, {y}) -> ({x}, {y - nullCount})");
                    
                    // 이동 위치에 이미 다른 보석이 있는지 확인 (이상 상황 방지)
                    if (allGems[x, y - nullCount] != null)
                    {
                        //Debug.LogError($"이미 보석이 있는 위치로 이동 시도: ({x}, {y - nullCount})에 {allGems[x, y - nullCount].name} 존재");
                        nullCount = 0; // 충돌 발생 시 nullCount 리셋
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
                        //Debug.LogError($"잘못된 이동 좌표 (y - nullCount < 0): ({x}, {y - nullCount})");
                    }
                }
            }
        }
        
        // 다시 한 번 위치 검증 및 비어있는 타일 확인
        ValidateGemPositions();
        LogEmptyTiles("CollapseEmptySpacesCo 종료 후");
        
        StartCoroutine(FillBoardCo());
    }

    private IEnumerable<Vector2Int> GetEmptyTilePositions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }
    }

    private void LogEmptyTiles(string context)
    {
        var emptyTiles = GetEmptyTilePositions().ToList(); 
        foreach (var position in emptyTiles)
        {
            Debug.LogWarning($"{context}: 비어있는 타일 ({position.x}, {position.y})");
        }
        Debug.Log($"{context}: 비어있는 타일 개수 = {emptyTiles.Count}");
    }


    public IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(0.2f);
        
        LogEmptyTiles("FillBoardCo 시작");

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
        
        LogEmptyTiles("리필 후");

        yield return new WaitForSeconds(0.2f);
        matchFinder.FindAllMatches();

        if(matchFinder.currentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.2f);
            DestroyMatchedGems();
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
        int stonesToSpawn = 0;
        int maxNewStones = 1; 
        
        LogEmptyTiles("RefillBoard 시작");

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

                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector2Int spawnFrom = new Vector2Int(x, y + 5);

                    if (createStone)
                    {
                        Gem stoneGem = gemFactory.FindStoneGem();

                        if (stoneGem != null)
                        {
                            Gem gem = GemPoolManager.instance.GetGem(stoneGem.gemType);
                            gem.transform.position = GetWorldPosition(spawnFrom);
                            gem.SetupGem(gridPos, this);
                            allGems[x, y] = gem;
                            currentStoneCount++;
                        }
                        else
                        {
                            int gemToUse = Random.Range(0, activeGems.Length);
                            Gem gem = GemPoolManager.instance.GetGem(activeGems[gemToUse].gemType);
                            gem.transform.position = GetWorldPosition(spawnFrom);
                            gem.SetupGem(gridPos, this);
                            allGems[x, y] = gem;
                        }
                    }
                    else
                    {
                        int gemToUse = Random.Range(0, activeGems.Length);
                        int attempts = 0;

                        while (activeGems[gemToUse].gemType == Gem.GemType.Stone && attempts < MAX_ITERATIONS)
                        {
                            gemToUse = Random.Range(0, activeGems.Length);
                            attempts++;
                        }

                        Gem gem = GemPoolManager.instance.GetGem(activeGems[gemToUse].gemType);
                        gem.transform.position = GetWorldPosition(spawnFrom);
                        gem.SetupGem(gridPos, this);
                        allGems[x, y] = gem;
                    }
                    
                    if (allGems[x, y] == null)
                    {
                        Debug.LogError($"RefillBoard 실패: ({x}, {y}) 위치에 보석이 생성되지 않음");
                    }
                }
            }
        }

        LogEmptyTiles("RefillBoard 완료");
        RemoveOrphanedGems();
    }

    // 잉여 보석 제거
    private void RemoveOrphanedGems()
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

    public void ValidateGemPositions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem gem = allGems[x, y];
                if (gem == null) continue;

                Vector2Int expected = new Vector2Int(x, y);
                if (gem.gridIndex != expected)
                {
                    Debug.LogWarning($"보석 위치 불일치 수정: {gem.name} => {expected}");
                    gem.gridIndex = expected;
                }
            }
        }
    }

}
