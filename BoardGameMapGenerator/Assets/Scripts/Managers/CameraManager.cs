using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CameraManager instance;
    [Header("Zoom")]
    public Camera mainCamera;
    [SerializeField] GameObject parent;
    public float zoom;
    [Range(0.1f, 2)][SerializeField] float pinchMulti;
    [Range(1, 10)][SerializeField] float zoomMulti;
    [Range(400,500)][SerializeField] float zoomMin;
    [Range(600,800)][SerializeField] float zoomMax;

    [Header("Drag")]
    [Range(1, 50)][SerializeField] float dragMulti;

    Vector3 dragStop;

    KeyCode ZoomIn = KeyCode.Equals;
    KeyCode ZoomOut = KeyCode.Minus;

    float touchDistanceNew;
    float touchDistanceOld;


    public bool dragged;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        dragStop = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        zoom = mainCamera.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (BoardCreationManager.Instance == null)
            return;
        if (BoardCreationManager.Instance.GetMode() != BoardCreationManager.Mode.CameraMovement)
            return;


        CameraMovement();
        if (Input.mouseScrollDelta.y != 0 || Input.GetKey(ZoomIn) || Input.GetKey(ZoomOut))
        {
            CameraZoomPC();
            mainCamera.orthographicSize = zoom;
        }
        if (Input.touchCount >= 2)
        {
            CameraZoomMobile();
            mainCamera.orthographicSize = zoom;
        }
        else
        {
            touchDistanceOld = 0;
            touchDistanceNew = 0;
        }
    }
    void CameraMovement()
    {
        if (Input.GetMouseButtonDown(0))// && !mainMenu.isMousedOver() && !optionsMenu.isMousedOver())
        {
            dragged = true;
            //Debug.Log("Dragging Started");
            return;
        }
        if (Input.GetMouseButtonUp(0))
        {
            dragged = false;
            dragStop = parent.transform.position;
            return;
        }

        if (dragged && Input.touchCount < 2)
        {
            Vector3 moveTo = parent.transform.position - new Vector3(Input.GetAxis("Mouse X") * dragMulti * (mainCamera.orthographicSize / 100), Input.GetAxis("Mouse Y") * dragMulti * (mainCamera.orthographicSize / 100));

            parent.transform.position = Vector3.MoveTowards(parent.transform.position, moveTo, 5000 * Time.deltaTime);
        }
        if (!dragged)
        {
            parent.transform.position = Vector3.MoveTowards(parent.transform.position, dragStop, 5000 * Time.deltaTime);
        }
    }
    void CameraZoomPC()
    {
        if (Input.mouseScrollDelta != new Vector2(0.0f, 0.0f))
        {
            zoom -= Input.mouseScrollDelta.y;
            zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
        }
        else if (Input.GetKey(ZoomIn))
        {
            zoom -= 0.1f;
            zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
        }
        else if (Input.GetKey(ZoomOut))
        {
            zoom -= 0.1f;
            zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
        }
    }
    void CameraZoomMobile()
    {
        Vector2 touch0 = Input.GetTouch(0).position;
        Vector2 touch1 = Input.GetTouch(1).position;
        float distance = Mathf.Clamp(Vector2.Distance(touch0, touch1), Screen.width / 10, Screen.width / 2);
        touchDistanceNew = distance;
        if (touchDistanceOld == 0)
        {
            touchDistanceOld = distance;
        }
        float distanceDiff = touchDistanceNew - touchDistanceOld;
        zoom -= distanceDiff / 150;
        zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);

        if (dragMulti < 2.5f)
        {
            dragMulti = 2.5f;
        }
        else if (dragMulti > 20f)
        {
            dragMulti = 20f;
        }
    }
}
