using UnityEngine;
using UnityEngine.UI;

public class CursorController : MonoBehaviour
{
    public Sprite cursorIdle; 
    public Sprite cursorClick; 

    private Image cursorImage;
    private RectTransform rectTransform;

    void Awake()
    {
        cursorImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();

        Cursor.visible = false; 
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        rectTransform.position = mousePos;

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
