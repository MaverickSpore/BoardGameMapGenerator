using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasMovementManager : MonoBehaviour
{
    [Header("Instance")]
    public static CanvasMovementManager instance;

    [Header("Camera")]
    [SerializeField] RectTransform MoveableCanvas;
    [SerializeField] RectTransform CanvasToCenterOn;

    [Header("Zoom Variables")]
    [Range(0.1f, 5f)][SerializeField] float ZoomMulti;
    [Range(0.5f, 1)][SerializeField] float ZoomMin;
    [Range(1, 2)][SerializeField] float ZoomMax;

    private float Zoom = 1;
    private Vector2 BaseScale;
    private Vector2 PrevMousePos;

    private Vector2 DragMin;
    private Vector2 DragMax;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
    // Start is called before the first frame update
    void Start()
    {
        BaseScale = MoveableCanvas.localScale;
        DragMin = new Vector2(-1000, -1000);
        DragMax = new Vector2(1000 + Screen.width, 1000 + Screen.height);
    }

    // Update is called once per frame
    void Update()
    {
        if (BoardCreationManager.Instance == null)
            return;
        if (BoardCreationManager.Instance.GetMode() != BoardCreationManager.Mode.CameraMovement)
            return;
        if (!BoardCreationManager.Instance.IsInMapArea())
            return;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Zoom += scroll * ZoomMulti;
            Zoom = Mathf.Clamp(Zoom, ZoomMin, ZoomMax);
            MoveableCanvas.localScale = BaseScale * Zoom;
            
            Vector2 zoomCenter = new Vector2(CanvasToCenterOn.sizeDelta.x / 2, CanvasToCenterOn.sizeDelta.y / 2);
            Vector2 mousePos = (Vector2)Input.mousePosition - zoomCenter;
            MoveableCanvas.anchoredPosition -= mousePos * scroll;
        }
        DragScreen();
    }
    private void DragScreen()
    {
        // Dragging the screen with the mouse
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (BoardCreationManager.Instance != null)
            {
                float gridSize = BoardCreationManager.Instance.GetGridSize();
                mousePos.x -= (mousePos.x % gridSize) - gridSize;
                mousePos.y -= (mousePos.y % gridSize) - gridSize;
            }
            if (PrevMousePos == Vector2.zero)
            {
                PrevMousePos = mousePos;
                return;
            }
            Vector2 diff = (mousePos - PrevMousePos);
            MoveableCanvas.anchoredPosition += diff;
            MoveableCanvas.anchoredPosition = new Vector2(Mathf.Clamp(MoveableCanvas.anchoredPosition.x, DragMin.x, DragMax.x), Mathf.Clamp(MoveableCanvas.anchoredPosition.y, DragMin.y, DragMax.y));
            PrevMousePos = mousePos;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            PrevMousePos = Vector2.zero;
        }
    }
}
