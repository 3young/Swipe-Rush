using System;
using Unity.VisualScripting;
using UnityEngine;

public class Gem : MonoBehaviour
{
    [HideInInspector] public Vector2Int gridIndex; // 그리드 좌표 저장
    [HideInInspector] public Board board; // 보드 참조

    private Vector2 firstTouchPosition; // 스와이프 시작 위치
    private Vector2 finalTouchPosition; // 스와이프 종료 위치
    private bool mousePressed; // 마우스 클릭 여부
    private float swipeAngle = 0; // 스와이프 각도

    private Gem otherGem; // 이동할 위치의 보석
    public Vector2Int previousGirdIndex; // 이전 그리드 좌표

    private void Update()
    {
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

        // 마우스 입력 감지
        if (mousePressed && Input.GetMouseButtonUp(0))
        {
            mousePressed = false;
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 화면 좌표를 월드 좌표로 반환
            CalculateAngle();
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
        previousGirdIndex = position;
        board = theBoard;
        transform.position = GetWorldPosition(gridIndex);
    }

    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
        mousePressed = true;
    }

    // 이동 각도 계산 함수
    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * Mathf.Rad2Deg; // 드 점 사이의 각도를 라디안 단위로 반환

        if(Vector3.Distance(firstTouchPosition, finalTouchPosition) > 0.5f)
        {
            MoveGem();
        }
    }

    // 보석 이동 함수
    private void MoveGem()
    {
        previousGirdIndex = gridIndex;

        if (swipeAngle >= -45 && swipeAngle < 45 && gridIndex.x < board.width - 1)
        {
            otherGem = board.allGems[gridIndex.x + 1, gridIndex.y];
            otherGem.gridIndex.x--;
            gridIndex.x++;
        }

        else if(swipeAngle >= 45 && swipeAngle < 135 && gridIndex.y < board.height-1)
        {
            otherGem = board.allGems[gridIndex.x, gridIndex.y + 1];
            otherGem.gridIndex.y--;
            gridIndex.y++;
        }

        else if(swipeAngle >= 135 || swipeAngle < -135 && gridIndex.x > 0)
        {
            otherGem = board.allGems[gridIndex.x - 1, gridIndex.y];
            otherGem.gridIndex.x++;
            gridIndex.x--;
        }

        else if(swipeAngle >= -135 && swipeAngle < -45 && gridIndex.y > 0)
        {
            otherGem = board.allGems[gridIndex.x, gridIndex.y - 1];
            otherGem.gridIndex.y++;
            gridIndex.y--;
        }

        board.allGems[gridIndex.x, gridIndex.y] = this;
        board.allGems[otherGem.gridIndex.x, otherGem.gridIndex.y] = otherGem;
    }

}
