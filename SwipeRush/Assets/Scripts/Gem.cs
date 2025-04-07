using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 매치-3 게임의 보석을 나타내는 클래스, 보석의 속성과 동작을 정의
/// </summary>
public class Gem : MonoBehaviour
{
    /// <summary>
    /// 보석의 종류를 정의하는 열거형
    /// </summary>
    public enum GemType { Blue, Green, Sky, Pink, Yellow, Brown, Purple, Stone }
    
    public GemType gemType;          // 현재 보석의 타입

    // 보석의 속성 정의
    public bool IsMovable => gemType != GemType.Stone && !isIndestructible;    // 이동 가능 여부
    public bool IsMatchable => gemType != GemType.Stone && !isIndestructible;  // 매치 가능 여부
    public bool IsDestructible => gemType != GemType.Stone && !isIndestructible; // 파괴 가능 여부

    public bool isIndestructible = false;  // 파괴 불가능 상태 여부
    public bool isMatched;                 // 현재 매치 상태 여부
    public int scoreValue = 10;            // 보석 파괴 시 획득 점수

    [HideInInspector] public Vector2Int gridIndex;        // 보석의 그리드 좌표
    [HideInInspector] public Board board;                 // 보석이 속한 보드
    [HideInInspector] public Vector2Int previousPosition; // 이전 그리드 위치 (스와이프 취소용)

    // 입력 관련 변수
    private Vector2 startTouchPosition;    // 드래그 시작 위치
    private Vector2 endTouchPosition;      // 드래그 종료 위치
    private bool isDragging;               // 드래그 중 여부
    private float swipeAngle;              // 스와이프 각도
    private Gem otherGem;                  // 교환할 대상 보석

    public GameObject destroyEffect;       // 보석 파괴 시 생성할 이펙트
    
    private enum SwipeDirection { Right, Up, Left, Down, None } // 스와이프 방향 열거형

    /// <summary>
    /// 매 프레임마다 보석의 이동과 입력을 처리
    /// </summary>
    private void Update()
    {
        MoveTowardsTarget();
        HandleInput();
    }

    /// <summary>
    /// 보석을 목표 위치로 부드럽게 이동
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (!IsMovable) return;

        // 목표 위치 계산
        Vector3 targetPosition = GetWorldPosition(gridIndex);
        
        // 목표 위치와 현재 위치의 차이가 있으면 이동
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, board.gemSpeed * Time.deltaTime);
        }
        else
        {
            // 정확한 위치로 설정
            transform.position = targetPosition;
            
            // 보드 배열 업데이트
            if (board.allGems[gridIndex.x, gridIndex.y] != null && board.allGems[gridIndex.x, gridIndex.y] != this)
            {
                board.ValidateGemPositions();
                return;
            }
            board.allGems[gridIndex.x, gridIndex.y] = this;
        }
    }

    /// <summary>
    /// 사용자 입력을 처리
    /// </summary>
    private void HandleInput()
    {
        // 마우스 버튼을 놓으면 스와이프 처리
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            if (board.currentState == Board.BoardState.moving)
            {
                endTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환
    /// </summary>
    /// <returns>월드 좌표</returns>
    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * board.tileSize, gridPosition.y * board.tileSize, -0.1f) + (Vector3)board.boardOffset;
    }

    /// <summary>
    /// 보석을 초기화하고 설정
    /// </summary>
    public void SetupGem(Vector2Int position, Board theBoard)
    {
        // 기본 속성 설정
        gridIndex = position;
        board = theBoard;
        previousPosition = position;

        // 상태 초기화
        isMatched = false;
        isIndestructible = false;
        scoreValue = 10;

        // 스톤 블록 특수 처리
        if (gemType == GemType.Stone)
        {
            isIndestructible = true;
            scoreValue = 0;
            isMatched = false;
        }
    }

    /// <summary>
    /// 마우스 클릭 시 시작 위치를 기록
    /// </summary>
    private void OnMouseDown()
    {
        if (board.currentState == Board.BoardState.moving)
        {
            startTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
    }

    /// <summary>
    /// 스와이프 각도를 계산하고 충분한 거리가 있으면 보석 이동 시도
    /// </summary>
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(endTouchPosition.y - startTouchPosition.y, endTouchPosition.x - startTouchPosition.x) * Mathf.Rad2Deg;
        if (Vector3.Distance(startTouchPosition, endTouchPosition) > 0.5f)
        {
            TrySwapGem();
        }
    }

    /// <summary>
    /// 스와이프 방향에 따라 인접한 보석과 교환 시도
    /// </summary>
    private void TrySwapGem()
    {
        if (!IsMovable) return;

        previousPosition = gridIndex;
        SwipeDirection direction = GetSwipeDirection();

        // 인접한 보석 가져오기 시도
        if (!TryGetAdjacentGem(direction, out otherGem)) return;

        // 인접한 보석과 위치 교환
        SwapWithGem(otherGem);
        StartCoroutine(ValidateMoveCo());
    }

    /// <summary>
    /// 스와이프 각도를 방향으로 변환
    /// </summary>
    /// <returns>스와이프 방향</returns>
    private SwipeDirection GetSwipeDirection()
    {
        if (swipeAngle >= -45 && swipeAngle < 45) return SwipeDirection.Right;
        if (swipeAngle >= 45 && swipeAngle < 135) return SwipeDirection.Up;
        if (swipeAngle >= 135 || swipeAngle < -135) return SwipeDirection.Left;
        if (swipeAngle >= -135 && swipeAngle < -45) return SwipeDirection.Down;
        return SwipeDirection.None;
    }

    /// <summary>
    /// 지정된 방향의 인접한 보석을 가져옴
    /// </summary>
    /// <returns>인접한 보석이 존재하고 이동 가능하면 true</returns>
    private bool TryGetAdjacentGem(SwipeDirection direction, out Gem adjacentGem)
    {
        adjacentGem = null;
        Vector2Int targetPos = gridIndex;

        // 방향에 따른 타겟 위치 계산
        switch (direction)
        {
            case SwipeDirection.Right: if (gridIndex.x >= board.width - 1) return false; targetPos.x++; break;
            case SwipeDirection.Up: if (gridIndex.y >= board.height - 1) return false; targetPos.y++; break;
            case SwipeDirection.Left: if (gridIndex.x <= 0) return false; targetPos.x--; break;
            case SwipeDirection.Down: if (gridIndex.y <= 0) return false; targetPos.y--; break;
        }

        // 인접한 보석 확인 및 이동 가능 여부 체크
        adjacentGem = board.allGems[targetPos.x, targetPos.y];
        return adjacentGem != null && adjacentGem.IsMovable;
    }

    /// <summary>
    /// 현재 보석과 다른 보석의 위치를 교환
    /// </summary>
    /// <param name="otherGem">교환할 대상 보석</param>
    private void SwapWithGem(Gem otherGem)
    {
        // 그리드 인덱스 교환
        Vector2Int tempIndex = gridIndex;
        gridIndex = otherGem.gridIndex;
        otherGem.gridIndex = tempIndex;

        // 보드 배열 업데이트
        board.allGems[gridIndex.x, gridIndex.y] = this;
        board.allGems[otherGem.gridIndex.x, otherGem.gridIndex.y] = otherGem;
    }

    /// <summary>
    /// 보석 이동 후 매치를 확인하고 처리
    /// 매치가 없으면 원래 위치로 되돌림
    /// </summary>
    /// <returns>코루틴 열거자</returns>
    public IEnumerator ValidateMoveCo()
    {
        board.currentState = Board.BoardState.waiting;
        yield return new WaitForSeconds(0.5f);

        // 매치 검사
        board.matchFinder.FindAllMatches();
        
        if (board.matchFinder.currentMatches.Count > 0)
        {
            // 매치가 있으면 매치된 보석 제거
            board.DestroyMatchedGems();
        }
        else if (otherGem != null)
        {
            // 매치가 없으면 원래 위치로 되돌림
            otherGem.gridIndex = gridIndex;
            gridIndex = previousPosition;

            board.allGems[gridIndex.x, gridIndex.y] = this;
            board.allGems[otherGem.gridIndex.x, otherGem.gridIndex.y] = otherGem;

            yield return new WaitForSeconds(0.5f);
            board.currentState = Board.BoardState.moving;

            // 가능한 매치가 없으면 보드 섞기
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
