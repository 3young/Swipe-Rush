using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Gem : MonoBehaviour
{
    public enum GemType { Blue, Green, Sky, Pink, Yellow, Brown, Purple, Stone }
    public GemType gemType;

    public bool IsMovable => gemType != GemType.Stone && !isIndestructible;
    public bool IsMatchable => gemType != GemType.Stone && !isIndestructible;
    public bool IsDestructible => gemType != GemType.Stone && !isIndestructible;

    public bool isIndestructible = false;
    public bool isMatched;
    public int scoreValue = 10;

    [HideInInspector] public Vector2Int gridIndex;
    [HideInInspector] public Board board;
    [HideInInspector] public Vector2Int previousPosition;

    private Vector2 startTouchPosition;
    private Vector2 endTouchPosition;
    private bool isDragging;
    private float swipeAngle;
    private Gem otherGem;

    public GameObject destroyEffect;
    private enum SwipeDirection { Right, Up, Left, Down, None }

    private void Update()
    {
        MoveTowardsTarget();
        HandleInput();
    }

    private void MoveTowardsTarget()
    {
        if (!IsMovable) return;

        Vector3 targetPosition = GetWorldPosition(gridIndex);
        if (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, board.gemSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
            if (board.allGems[gridIndex.x, gridIndex.y] != null && board.allGems[gridIndex.x, gridIndex.y] != this)
            {
                Debug.LogError($"보석 위치 충돌: {name} vs {board.allGems[gridIndex.x, gridIndex.y].name}");
                board.ValidateGemPositions();
                return;
            }
            board.allGems[gridIndex.x, gridIndex.y] = this;
        }
    }

    private void HandleInput()
    {
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

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * board.tileSize, gridPosition.y * board.tileSize, -0.1f) + (Vector3)board.boardOffset;
    }

    public void SetupGem(Vector2Int position, Board theBoard)
    {
        gridIndex = position;
        board = theBoard;
        previousPosition = position;

        isMatched = false;
        isIndestructible = false;
        scoreValue = 10;

        if (gemType == GemType.Stone)
        {
            isIndestructible = true;
            scoreValue = 0;
            isMatched = false;
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

    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(endTouchPosition.y - startTouchPosition.y, endTouchPosition.x - startTouchPosition.x) * Mathf.Rad2Deg;
        if (Vector3.Distance(startTouchPosition, endTouchPosition) > 0.5f)
        {
            TrySwapGem();
        }
    }

    private void TrySwapGem()
    {
        if (!IsMovable) return;

        previousPosition = gridIndex;
        SwipeDirection direction = GetSwipeDirection();

        if (!TryGetAdjacentGem(direction, out otherGem)) return;

        SwapWithGem(otherGem);
        StartCoroutine(ValidateMoveCo());
    }

    private SwipeDirection GetSwipeDirection()
    {
        if (swipeAngle >= -45 && swipeAngle < 45) return SwipeDirection.Right;
        if (swipeAngle >= 45 && swipeAngle < 135) return SwipeDirection.Up;
        if (swipeAngle >= 135 || swipeAngle < -135) return SwipeDirection.Left;
        if (swipeAngle >= -135 && swipeAngle < -45) return SwipeDirection.Down;
        return SwipeDirection.None;
    }

    private bool TryGetAdjacentGem(SwipeDirection direction, out Gem adjacentGem)
    {
        adjacentGem = null;
        Vector2Int targetPos = gridIndex;

        switch (direction)
        {
            case SwipeDirection.Right: if (gridIndex.x >= board.width - 1) return false; targetPos.x++; break;
            case SwipeDirection.Up: if (gridIndex.y >= board.height - 1) return false; targetPos.y++; break;
            case SwipeDirection.Left: if (gridIndex.x <= 0) return false; targetPos.x--; break;
            case SwipeDirection.Down: if (gridIndex.y <= 0) return false; targetPos.y--; break;
        }

        adjacentGem = board.allGems[targetPos.x, targetPos.y];
        return adjacentGem != null && adjacentGem.IsMovable;
    }

    private void SwapWithGem(Gem otherGem)
    {
        Vector2Int tempIndex = gridIndex;
        gridIndex = otherGem.gridIndex;
        otherGem.gridIndex = tempIndex;

        board.allGems[gridIndex.x, gridIndex.y] = this;
        board.allGems[otherGem.gridIndex.x, otherGem.gridIndex.y] = otherGem;
    }

    public IEnumerator ValidateMoveCo()
    {
        board.currentState = Board.BoardState.waiting;
        yield return new WaitForSeconds(0.5f);

        board.matchFinder.FindAllMatches();
        if (board.matchFinder.currentMatches.Count > 0)
        {
            board.DestroyMatchedGems();
        }
        else if (otherGem != null)
        {
            otherGem.gridIndex = gridIndex;
            gridIndex = previousPosition;

            board.allGems[gridIndex.x, gridIndex.y] = this;
            board.allGems[otherGem.gridIndex.x, otherGem.gridIndex.y] = otherGem;

            yield return new WaitForSeconds(0.5f);
            board.currentState = Board.BoardState.moving;

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
