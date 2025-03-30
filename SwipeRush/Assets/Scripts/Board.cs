using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 매치-3 게임의 핵심 클래스. 게임 보드와 보석의 생성, 이동, 매치 관리를 담당
/// </summary>
public class Board : MonoBehaviour
{
    [HideInInspector] public float tileSize;         // 타일 하나의 크기
    [HideInInspector] public Vector2 boardOffset;    // 보드의 중앙 정렬을 위한 오프셋

    public int width;                // 보드의 가로 크기
    public int height;               // 보드의 세로 크기
    public GameObject tilePrefab;    // 타일 프리팹

    public Gem[] gems;               // 기본 보석 프리팹 배열
    public LevelData[] levelDataArray;  // 레벨별 설정 데이터
    public Gem[] activeGems;         // 현재 레벨에서 사용 가능한 보석 배열

    public Gem[,] allGems;           // 보드에 배치된 모든 보석 배열
    public float gemSpeed = 15;      // 보석 이동 속도

    public MatchFinder matchFinder;  // 매치 검색 컴포넌트
    public RoundManager roundManager; // 라운드 관리 컴포넌트

    private GemFactory gemFactory;   // 보석 생성 팩토리
    private LevelData currentLevelData; // 현재 레벨 데이터
    private int currentStoneCount = 0; // 현재 스톤 블록 개수

    public enum BoardState { moving, waiting } // 보드 상태 열거형
    public BoardState currentState = BoardState.moving; // 현재 보드 상태

    // 최대 반복 시도 횟수 (무한 루프 방지용)
    private const int MAX_ITERATIONS = 100;

    /// <summary>
    /// 게임 시작 시 레벨 데이터를 로드하고 초기화
    /// </summary>
    private void Awake()
    {
        // MatchFinder 참조 설정
        if (matchFinder == null)
            matchFinder = Object.FindFirstObjectByType<MatchFinder>();

        // 현재 씬 이름에서 레벨 번호 추출
        string sceneName = SceneManager.GetActiveScene().name;
        int levelNum = 1;

        if (sceneName.StartsWith("Level") && sceneName.Length > 5)
        {
            int.TryParse(sceneName.Substring(5), out levelNum);
            levelNum = Mathf.Clamp(levelNum, 1, levelDataArray.Length);
        }

        // 레벨 데이터 로드
        if (levelNum <= levelDataArray.Length && levelDataArray[levelNum - 1] != null)
        {
            currentLevelData = levelDataArray[levelNum - 1];
            activeGems = currentLevelData.availableGems;
            width = currentLevelData.width;
            height = currentLevelData.height;
        }
        else
        {
            activeGems = gems; 
        }
    }

    /// <summary>
    /// 보드를 초기화하고 게임 시작
    /// </summary>
    private void Start()
    {
        Time.timeScale = 1;
        ScoreManager.instance.ResetScore();

        // 타일 크기 및 보드 오프셋 계산
        tileSize = tilePrefab.GetComponent<SpriteRenderer>().bounds.size.x;
        boardOffset = new Vector2(-width / 2.0f + 0.5f, -height / 2.0f + 0.5f) * tileSize;

        // 보석 배열 초기화 및 보드 설정
        allGems = new Gem[width, height];
        gemFactory = new GemFactory(tileSize, boardOffset, transform, this, activeGems);

        SetupBoard();
        CountStoneBlocks();
    }

    /// <summary>
    /// 초기 보드를 생성하고 보석 배치
    /// </summary>
    private void SetupBoard()
    {
        currentStoneCount = 0;
        
        // 모든 타일 위치에 보석 생성
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 타일 생성
                Vector2 position = new Vector2(x * tileSize, y * tileSize) + boardOffset;
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile ({x}, {y})";

                // 스톤 블록 생성 확률 계산
                bool createStone = currentStoneCount < currentLevelData.maxStoneBlocks && 
                                  Random.value < currentLevelData.stoneSpawnChance;
                
                Gem gemToSpawn;

                // 스톤 블록 생성 시도
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
                
                // 일반 보석 생성 (초기 매치 방지)
                gemToSpawn = gemFactory.GetRandomNonStoneGem();
                int iterations = 0;

                // 초기 매치를 방지하기 위해 매치가 없는 보석만 선택
                while (IsMatchAt(new Vector2Int(x, y), gemToSpawn) && iterations < MAX_ITERATIONS)
                {
                    gemToSpawn = gemFactory.GetRandomNonStoneGem();
                    iterations++;
                }
                
                SpawnGem(new Vector2Int(x, y), gemToSpawn);
            }
        }
    }

    /// <summary>
    /// 지정된 위치에 보석 생성
    /// </summary>
    /// <param name="gridPos">보석을 생성할 그리드 위치</param>
    /// <param name="gemPrefab">생성할 보석 프리팹</param>
    private void SpawnGem(Vector2Int gridPos, Gem gemPrefab)
    {
        // 스톤 블록 개수 관리
        if (gemFactory.IsStone(gemPrefab) && currentStoneCount < currentLevelData.maxStoneBlocks)
        {
            currentStoneCount++;
        }
        else if (gemFactory.IsStone(gemPrefab))
        {
            gemPrefab = gemFactory.GetRandomNonStoneGem();
        }

        // 오브젝트 풀에서 보석 가져오기
        Gem gem = GemPoolManager.instance.GetGem(gemPrefab.gemType);
        gem.transform.position = GetWorldPosition(gridPos);
        gem.SetupGem(gridPos, this);

        allGems[gridPos.x, gridPos.y] = gem;

        // 스톤 블록 위치 고정
        if (gemFactory.IsStone(gem))
        {
            gem.transform.position = GetWorldPosition(gridPos);
        }
    }

    /// <summary>
    /// 현재 보드에 가능한 매치가 있는지 확인
    /// </summary>
    /// <returns>가능한 매치가 있으면 true, 없으면 false</returns>
    public bool CheckForPossibleMatches()
    {
        // 모든 그리드 위치를 순회하며 가능한 매치 확인
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem currentGem = allGems[x, y];
                if (currentGem == null) continue;
                
                // 이동 가능한 보석인지 확인
                if (!currentGem.IsMovable)
                    continue;

                // 오른쪽과 위쪽 방향으로만 확인 (중복 확인 방지)
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
                        
                        // 두 보석을 교환하고 매치 확인
                        SwapGems(x, y, newX, newY);
                        matchFinder.FindAllMatches();

                        // 매치가 있으면 원상복구하고 true 반환
                        if (matchFinder.currentMatches.Count > 0)
                        {
                            SwapGems(x, y, newX, newY);
                            matchFinder.currentMatches.Clear();
                            return true;
                        }
                        
                        // 매치가 없으면 원상복구
                        SwapGems(x, y, newX, newY);
                        matchFinder.currentMatches.Clear();
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 주어진 좌표가 보드 범위 내에 있는지 확인
    /// </summary>
    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    /// <summary>
    /// 두 보석의 위치를 교환
    /// </summary>
    private void SwapGems(int x1, int y1, int x2, int y2)
    {
        Gem gem1 = allGems[x1, y1];
        Gem gem2 = allGems[x2, y2];

        // 이동 가능한 보석인지 확인
        if (gem1 == null || gem2 == null || !gem1.IsMovable || !gem2.IsMovable)
        {
            return;
        }

        // 보석 교환
        Gem temp = allGems[x1, y1];
        allGems[x1, y1] = allGems[x2, y2];
        allGems[x2, y2] = temp;

        // 보석의 그리드 인덱스 업데이트
        if (allGems[x1, y1] != null)
        {
            allGems[x1, y1].gridIndex = new Vector2Int(x1, y1);
        }
        if (allGems[x2, y2] != null)
        {
            allGems[x2, y2].gridIndex = new Vector2Int(x2, y2);
        }
    }

    /// <summary>
    /// 보드의 보석을 랜덤하게 섞음
    /// </summary>
    public IEnumerator ShuffleBoardCo()
    {
        currentState = BoardState.waiting;
        yield return new WaitForSeconds(1.5f);

        // 모든 보석을 리스트에 수집
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

        // 보석을 랜덤하게 재배치
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

    /// <summary>
    /// 그리드 인덱스를 월드 좌표로 변환
    /// </summary>
    private Vector3 GetWorldPosition(Vector2Int gridIndex)
    {
        return new Vector3(gridIndex.x * tileSize, gridIndex.y * tileSize, -0.1f) + (Vector3)boardOffset;
    }

    /// <summary>
    /// 지정된 위치에 보석을 놓았을 때 매치가 생기는지 확인
    /// </summary>
    private bool IsMatchAt(Vector2Int position, Gem gem)
    {
        // 매치 불가능한 보석(스톤 등)은 매치 체크 제외
        if (!gem.IsMatchable) return false;
        
        // 수평 또는 수직 매치 확인
        return CheckHorizontalMatchAt(position, gem) || CheckVerticalMatchAt(position, gem);
    }

    /// <summary>
    /// 수평 방향의 매치 확인
    /// </summary>
    private bool CheckHorizontalMatchAt(Vector2Int position, Gem gem)
    {
        // 왼쪽 2개 보석과 매치 확인
        if (position.x <= 1) return false;
        Gem leftGem1 = allGems[position.x - 1, position.y];
        Gem leftGem2 = allGems[position.x - 2, position.y];
        
        return leftGem1 != null && leftGem2 != null &&
               leftGem1.gemType == gem.gemType &&
               leftGem2.gemType == gem.gemType;
    }

    /// <summary>
    /// 수직 방향의 매치 확인
    /// </summary>
    private bool CheckVerticalMatchAt(Vector2Int position, Gem gem)
    {
        // 아래쪽 2개 보석과 매치 확인
        if (position.y <= 1) return false;
        Gem downGem1 = allGems[position.x, position.y - 1];
        Gem downGem2 = allGems[position.x, position.y - 2];
        
        return downGem1 != null && downGem2 != null &&
               downGem1.gemType == gem.gemType &&
               downGem2.gemType == gem.gemType;
    }

    /// <summary>
    /// 매치된 보석 파괴
    /// </summary>
    public void DestroyMatchedGems()
    {
        if (matchFinder.currentMatches.Count == 0)
        {
            return;
        }

        // 유효한 매치만 필터링
        List<Gem> validMatches = new List<Gem>();
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

        // 매치 리스트 초기화 및 다음 단계로 진행
        matchFinder.currentMatches.Clear();
        ResetAllMatchFlags();
        StartCoroutine(CollapseEmptySpacesCo());
    }

    /// <summary>
    /// 모든 보석의 매치 플래그 초기화
    /// </summary>
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

    /// <summary>
    /// 지정된 위치의 매치된 보석 파괴
    /// </summary>
    private void DestroyMatchedGemAt(int x, int y)
    {
        // 좌표 유효성 검사
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return;
        }

        Gem gem = allGems[x, y];

        // 보석 존재 여부 확인
        if (gem == null)
        {
            return;
        }

        // 그리드 위치 일치 확인
        if (gem.gridIndex.x != x || gem.gridIndex.y != y)
        {
            return;
        }

        // 파괴 가능 여부 확인
        if (!gem.IsDestructible)
        {
            return;
        }

        // 매치 여부 확인
        if (!gem.isMatched)
        {
            return;
        }

        // 매치 플래그 초기화
        gem.isMatched = false;

        // 점수 추가
        ScoreManager.instance.AddScore(gem.scoreValue);

        // 파괴 이펙트 생성
        if (gem.destroyEffect != null)
        {
            GameObject effect = GemPoolManager.instance.GetEffect(gem.destroyEffect);
            effect.transform.position = gem.transform.position;
            effect.SetActive(true);
            StartCoroutine(ReturnEffectAfterDelay(effect,1f));
        }

        // 효과음 재생
        SFXManager.instance.PlayGemBreak();

        // 보석 제거 및 풀에 반환
        allGems[x, y] = null;
        GemPoolManager.instance.ReturnGem(gem);
    }

    /// <summary>
    /// 지정 시간 후 이펙트를 풀에 반환
    /// </summary>
    private IEnumerator ReturnEffectAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        GemPoolManager.instance.ReturnEffect(obj);
    }

    /// <summary>
    /// 빈 공간을 위에 있는 보석으로 채움
    /// </summary>
    private IEnumerator CollapseEmptySpacesCo()
    {
        yield return new WaitForSeconds(0.25f);

        // 모든 보석의 위치 검증
        ValidateGemPositions();

        // 각 열에 대해 빈 공간 채우기
        for(int x = 0; x < width; x++)
        {
            int nullCount = 0; 

            for(int y = 0; y < height; y++)
            {
                // 1. 빈 공간 카운트
                if (allGems[x, y] == null)
                {
                    nullCount++;
                    continue;
                }
                
                // 2. 이동 불가능한 보석은 카운트 리셋
                if (!allGems[x, y].IsMovable)
                {
                    nullCount = 0;
                    continue;
                }
                
                // 3. 빈 공간이 있으면 보석 이동
                if(nullCount > 0)
                {
                    var gemToMove = allGems[x, y];
                    
                    // 이동할 위치 계산 및 유효성 검사
                    if(y - nullCount >= 0)
                    {
                        // 보석 이동 처리
                        allGems[x, y - nullCount] = gemToMove; 
                        gemToMove.gridIndex = new Vector2Int(x, y - nullCount);
                        allGems[x, y] = null;
                    }
                }
            }
        }
        
        // 위치 검증 후 빈 공간 채우기
        ValidateGemPositions();
        StartCoroutine(FillBoardCo());
    }

    /// <summary>
    /// 보드의 빈 공간을 새 보석으로 채움
    /// </summary>
    public IEnumerator FillBoardCo()
    {
        yield return new WaitForSeconds(0.2f);
        
        // 보드 리필
        RefillBoard();
        
        // 움직일 수 없는 보석 위치 고정
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
        
        // 매치 확인
        yield return new WaitForSeconds(0.2f);
        matchFinder.FindAllMatches();

        // 매치가 있으면 제거, 없으면 가능한 매치 확인
        if(matchFinder.currentMatches.Count > 0)
        {
            yield return new WaitForSeconds(0.2f);
            DestroyMatchedGems();
        }
        else
        {
            // 가능한 매치가 없으면 보드 섞기
            if (!CheckForPossibleMatches())
            {
                yield return StartCoroutine(ShuffleBoardCo());
                yield return StartCoroutine(FillBoardCo());
            }
            else
            {
                // 게임 진행 상태로 전환
                yield return new WaitForSeconds(0.2f);
                currentState = BoardState.moving;
            }
        }
    }

    /// <summary>
    /// 보드의 빈 공간을 새 보석으로 채움
    /// </summary>
    private void RefillBoard()
    {
        // 새로 생성할 스톤 블록 수 제한
        int stonesToSpawn = 0;
        int maxNewStones = 1; 
        
        // 모든 빈 공간 채우기
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (allGems[x, y] == null)
                {
                    // 스톤 블록 생성 확률 계산
                    bool createStone = false;
                    if (currentStoneCount < currentLevelData.maxStoneBlocks && 
                        stonesToSpawn < maxNewStones &&
                        Random.value < currentLevelData.stoneSpawnChance)
                    {
                        createStone = true;
                        stonesToSpawn++;
                    }

                    // 생성 위치 계산
                    Vector2Int gridPos = new Vector2Int(x, y);
                    Vector2Int spawnFrom = new Vector2Int(x, y + 5); // 위에서 떨어지는 효과

                    // 보석 생성 (스톤 블록 또는 일반 보석)
                    if (createStone)
                    {
                        // 스톤 블록 생성
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
                            // 스톤 블록 없으면 일반 보석 생성
                            int gemToUse = Random.Range(0, activeGems.Length);
                            Gem gem = GemPoolManager.instance.GetGem(activeGems[gemToUse].gemType);
                            gem.transform.position = GetWorldPosition(spawnFrom);
                            gem.SetupGem(gridPos, this);
                            allGems[x, y] = gem;
                        }
                    }
                    else
                    {
                        // 일반 보석 생성 (스톤 블록 제외)
                        int gemToUse = Random.Range(0, activeGems.Length);
                        int attempts = 0;

                        // 스톤 블록이 아닌 보석 선택
                        while (activeGems[gemToUse].gemType == Gem.GemType.Stone && attempts < MAX_ITERATIONS)
                        {
                            gemToUse = Random.Range(0, activeGems.Length);
                            attempts++;
                        }

                        // 보석 생성 및 설정
                        Gem gem = GemPoolManager.instance.GetGem(activeGems[gemToUse].gemType);
                        gem.transform.position = GetWorldPosition(spawnFrom);
                        gem.SetupGem(gridPos, this);
                        allGems[x, y] = gem;
                    }
                }
            }
        }

        // 떠도는 보석 제거
        RemoveOrphanedGems();
    }

    /// <summary>
    /// 보드에 포함되지 않은 보석을 제거
    /// </summary>
    private void RemoveOrphanedGems()
    {
        // 모든 Gem 오브젝트 찾기
        List<Gem> foundGems = new List<Gem>();
        foundGems.AddRange(Object.FindObjectsByType<Gem>(FindObjectsSortMode.None)); 

        // 보드에 있는 보석 제외
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

        // 남은 보석 파괴
        foreach (Gem gem in foundGems)
        {
            Destroy(gem.gameObject);
        }
    }

    /// <summary>
    /// 보드에 있는 스톤 블록 수 계산
    /// </summary>
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

    /// <summary>
    /// 모든 보석의 위치를 검증하고 필요시 수정
    /// </summary>
    public void ValidateGemPositions()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Gem gem = allGems[x, y];
                if (gem == null) continue;

                // 기대 위치와 실제 위치 비교
                Vector2Int expected = new Vector2Int(x, y);
                if (gem.gridIndex != expected)
                {
                    gem.gridIndex = expected;
                }
            }
        }
    }
}
