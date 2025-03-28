using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Gem : MonoBehaviour
{
    public enum GemType { Blue, Green, Sky, Pink, Yellow, Brown, Purple, Stone };
    public GemType gemType;
    
    // 각 보석 타입의 특성을 명확하게 분리
    public bool IsMovable => gemType != GemType.Stone && !isIndestructible;
    public bool IsMatchable => gemType != GemType.Stone && !isIndestructible;
    public bool IsDestructible => gemType != GemType.Stone && !isIndestructible;
    
    public bool isIndestructible = false;

    public bool isMatched;
    public int scoreValue = 10;

    [HideInInspector] public Vector2Int gridIndex; // 그리드 좌표 저장
    [HideInInspector] public Board board; // 보드 참조
    [HideInInspector] public Vector2Int previousPosition; // 이전 위치 저장

    private Vector2 startTouchPosition; // 스와이프 시작 위치
    private Vector2 endTouchPosition; // 스와이프 종료 위치
    private bool isDragging; // 마우스 클릭 여부
    private float swipeAngle = 0; // 스와이프 각도

    private Gem otherGem; // 이동할 위치의 보석

    public GameObject destroyEffect;

    private void Update()
    {
        MoveTowardsTarget();
        HandleInput();
    }

    private void MoveTowardsTarget()
    {
        // 속성을 사용하여 조건 체크
        if (!IsMovable)
        {
            return;
        }

        Vector3 targetPosition = GetWorldPosition(gridIndex);

        // 보석 이동
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, board.gemSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
            board.allGems[gridIndex.x, gridIndex.y] = this;
        }
    }

    private void HandleInput()
    {
        // 마우스 입력 감지
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            if(board.currentState == Board.BoardState.moving)
            {
                endTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 화면 좌표를 월드 좌표로 반환
                CalculateAngle();
            }
        }
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * board.tileSize, gridPosition.y * board.tileSize, -0.1f) + (Vector3)board.boardOffset;
    }

    // 보석의 그리드 좌표와 보드 설정하는 함수
    // 보석 생성 시 호출 -> 보석 위치와 보드 초기화
    public void SetupGem(Vector2Int position, Board theBoard)
    {
        gridIndex = position;
        board = theBoard;

        if (gemType == GemType.Stone)
        {
            isIndestructible = true;
            scoreValue = 0;
            isMatched = false; // 스톤은 매치될 수 없음을 확실히 함
        }
    }

    private void OnMouseDown()
    {
        if (board.currentState == Board.BoardState.moving)
        {
            startTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
    }

    // 이동 각도 계산 함수
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(endTouchPosition.y - startTouchPosition.y, endTouchPosition.x - startTouchPosition.x) * Mathf.Rad2Deg; // 두 점 사이의 각도를 라디안 단위로 반환

        if (Vector3.Distance(startTouchPosition, endTouchPosition) > 0.5f)
        {
            TrySwapGem();
        }
    }

    // 보석 이동 함수
    private void TrySwapGem()
    {
        // 속성을 사용하여 조건 체크
        if (!IsMovable)
        {
            Debug.Log($"스왑 시도 취소: {name} - 스톤이거나 파괴 불가능함");
            return;
        }

        previousPosition = gridIndex;

        if (swipeAngle >= -45 && swipeAngle < 45 && gridIndex.x < board.width - 1)
        {
            otherGem = board.allGems[gridIndex.x + 1, gridIndex.y];
            
            // 대상 보석이 null이거나 이동 불가능하면 스왑하지 않음
            if (otherGem == null || !otherGem.IsMovable) 
                return;

            otherGem.gridIndex.x--;
            gridIndex.x++;
        }
        else if (swipeAngle >= 45 && swipeAngle < 135 && gridIndex.y < board.height - 1)
        {
            otherGem = board.allGems[gridIndex.x, gridIndex.y + 1];
            
            // 대상 보석이 null이거나 이동 불가능하면 스왑하지 않음
            if (otherGem == null || !otherGem.IsMovable) 
                return;

            otherGem.gridIndex.y--;
            gridIndex.y++;
        }
        else if ((swipeAngle >= 135 || swipeAngle < -135) && gridIndex.x > 0)
        {
            otherGem = board.allGems[gridIndex.x - 1, gridIndex.y];
            
            // 대상 보석이 null이거나 이동 불가능하면 스왑하지 않음
            if (otherGem == null || !otherGem.IsMovable) 
                return;

            otherGem.gridIndex.x++;
            gridIndex.x--;
        }
        else if (swipeAngle >= -135 && swipeAngle < -45 && gridIndex.y > 0)
        {
            otherGem = board.allGems[gridIndex.x, gridIndex.y - 1];
            
            // 대상 보석이 null이거나 이동 불가능하면 스왑하지 않음
            if (otherGem == null || !otherGem.IsMovable) 
                return;

            otherGem.gridIndex.y++;
            gridIndex.y--;
        }
        else
        {
            return;
        }

        board.allGems[gridIndex.x, gridIndex.y] = this;
        board.allGems[otherGem.gridIndex.x, otherGem.gridIndex.y] = otherGem;

        StartCoroutine(CheckAndRevertMoveCo());
    }

    public IEnumerator CheckAndRevertMoveCo()
    {
        board.currentState = Board.BoardState.waiting;

        yield return new WaitForSeconds(0.5f);
        board.matchFinder.FindAllMatches(); // 매치 검사

        // 매치된 보석이 있는지 확인
        if (board.matchFinder.currentMatches.Count > 0)
        {
            board.DestroyMatches();
        }
        else if (otherGem != null)
        {
            // 보석 위치 원래대로 되돌리기
            otherGem.gridIndex = gridIndex;
            gridIndex = previousPosition;

            board.allGems[gridIndex.x, gridIndex.y] = this;
            board.allGems[otherGem.gridIndex.x, otherGem.gridIndex.y] = otherGem;

            yield return new WaitForSeconds(0.5f);
            board.currentState = Board.BoardState.moving;

            // 매치 안 된 후 가능한 매치도 없다면 -> 셔플
            if (!board.CheckForPossibleMatches())
            {
                board.StartCoroutine(board.ShuffleBoardCo());
                board.StartCoroutine(board.FillBoardCo());
            }
        }
        else
        {
            board.currentState = Board.BoardState.moving;
        }
    }
}
