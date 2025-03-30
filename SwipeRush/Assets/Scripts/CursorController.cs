using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 커스텀 마우스 커서를 제어하는 클래스
/// 게임 내에서 기본 커서 대신 커스텀 이미지 사용
/// </summary>
public class CursorController : MonoBehaviour
{
    /// <summary>유휴 상태일 때 표시할 커서 이미지</summary>
    public Sprite cursorIdle; 
    /// <summary>클릭 상태일 때 표시할 커서 이미지</summary>
    public Sprite cursorClick; 

    private Image cursorImage;
    private RectTransform rectTransform;

    /// <summary>
    /// 컴포넌트 초기화 및 기본 커서 숨김 처리 수행
    /// </summary>
    void Awake()
    {
        cursorImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        Cursor.visible = false; // 기본 커서 숨김
    }

    /// <summary>
    /// 매 프레임마다 커서 위치를 업데이트하고 상태에 따라 이미지 변경
    /// </summary>
    void Update()
    {
        // 마우스 위치로 커서 이동
        Vector2 mousePos = Input.mousePosition;
        rectTransform.position = mousePos;

        // 클릭 상태에 따라 이미지 변경
        if (Input.GetMouseButton(0))
        {
            cursorImage.sprite = cursorClick;
        }
        else
        {
            cursorImage.sprite = cursorIdle;
        }
    }
}
