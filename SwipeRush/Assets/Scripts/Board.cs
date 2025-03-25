using UnityEngine;

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

    void Update()
    {
        matchFinder.FindAllMatches();
    }

    // 보드 설정 함수
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
                
                SpawnGem(new Vector2Int(x, y), gems[gemToUse], position);
            }
        }
    }

    // 보석 생성 함수
    private void SpawnGem(Vector2Int gridPosition, Gem gemToSpawn, Vector2 worldPosition)
    {
        // 보석을 월드 좌표에 생성
        Gem gem = Instantiate(gemToSpawn, new Vector3(worldPosition.x, worldPosition.y, -0.1f), Quaternion.identity);
        gem.transform.parent = transform;
        gem.name = $"Gem ({gridPosition.x}, {gridPosition.y})";
        // 보석의 그리드 좌표 설정
        allGems[gridPosition.x, gridPosition.y] = gem;
        // 보석의 그리드 좌표와 보드 참조 설정
        gem.SetupGem(gridPosition, this);
    }

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
}
