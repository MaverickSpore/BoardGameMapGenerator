using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TileObjectPlacedController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image mainImageLeaveWhite;
    [SerializeField] Image currentImage;

    public ChildTileObjectController parentTile;
    public ChildTileObjectController pairedTile;

    bool isDragged;
    bool isHovered;
    bool isButtonHovered;
    int currentRotation;

    Vector3 currentLocation;


    private void Start()
    {
        isDragged = false;
        //isHovered = true;
        isButtonHovered = false;
        currentRotation = 0;
        currentLocation = transform.position;
    }
    public void SetImage(Sprite newImage)
    {
        currentImage.sprite = newImage;
        float imageWidth = newImage.rect.width;
        float imageHeight = newImage.rect.height;

        float gridSize = BoardCreationManager.instance.GetGridSize();
        float widthAdjust = imageWidth % gridSize;
        float heightAdjust = imageHeight % gridSize;
        if (widthAdjust < gridSize / 4)
            imageWidth -= widthAdjust;
        else
            imageWidth += gridSize - widthAdjust;
        if (heightAdjust < gridSize / 4)
            imageHeight -= heightAdjust;
        else
            imageHeight += gridSize - heightAdjust;

        currentImage.GetComponent<RectTransform>().sizeDelta = new Vector2(imageWidth, imageHeight);
        mainImageLeaveWhite.GetComponent<RectTransform>().sizeDelta = new Vector2(imageWidth, imageHeight);

        AdjustToGrid();
    }
    public Image GetCurrentImage()
    {
        return currentImage;
    }
    public void AdjustToGrid()
    {
        float gridSize = BoardCreationManager.instance.GetGridSize();
        switch (currentRotation)
        {
            case 0:
            case 180:
                mainImageLeaveWhite.GetComponent<RectTransform>().sizeDelta = new Vector2(currentImage.GetComponent<RectTransform>().sizeDelta.x, currentImage.GetComponent<RectTransform>().sizeDelta.y);
                transform.position = new Vector2(transform.position.x - ((transform.position.x % (gridSize)) - (gridSize / 4.0f)), transform.position.y - ((transform.position.y % (gridSize)) - (gridSize / 4.0f)));
                break;
            case 90:
            case 270:
                mainImageLeaveWhite.GetComponent<RectTransform>().sizeDelta = new Vector2(currentImage.GetComponent<RectTransform>().sizeDelta.y, currentImage.GetComponent<RectTransform>().sizeDelta.x);
                transform.position = new Vector2(transform.position.x - ((transform.position.y % (gridSize)) - (gridSize / 4.0f)), transform.position.y - ((transform.position.x % (gridSize)) - (gridSize / 4.0f)));
                break;
        }
    }

    public Sprite GetSprite()
    {
        return currentImage.sprite;
    }
    public string GetTileName()
    {
        return parentTile.GetSpriteName();
    }
    public Vector2 GetSize()
    {
        switch (currentRotation)
        {
            case 0:
            case 180:
            default:
                return new Vector2(currentImage.GetComponent<RectTransform>().sizeDelta.x, currentImage.GetComponent<RectTransform>().sizeDelta.y);
            case 90:
            case 270:
                return new Vector2(currentImage.GetComponent<RectTransform>().sizeDelta.y, currentImage.GetComponent<RectTransform>().sizeDelta.x);
        }
    }
    public int GetRotation()
    {
        return currentRotation;
    }
    public void SetParentTile(ChildTileObjectController parent)
    {
        parentTile = parent;
    }

    private void Update()
    {
        //if (!BoardCreationManager.instance.GetTilesLocked())
        if (BoardCreationManager.instance.GetMode() == BoardCreationManager.Mode.Move)
        {
            if (isDragged)
            {
                Vector3 mousePos = Input.mousePosition;
                float gridSize = BoardCreationManager.instance.GetGridSize();
                mousePos.x -= (mousePos.x % gridSize) - (gridSize / 4.0f);
                mousePos.y -= (mousePos.y % gridSize) - (gridSize / 4.0f);
                transform.position = new Vector3(mousePos.x, mousePos.y, -1);
                //AdjustToGrid();

                if (currentLocation != transform.position)
                {
                    BoardCreationManager.instance.PressClearSelection();
                    currentLocation = transform.position;
                }

            }
            if (isHovered)
            {
                if (!isButtonHovered)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        isDragged = true;
                    }
                }
                if (Input.GetMouseButtonUp(0))
                {
                    isDragged = false;
                }
                if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftShift))
                {
                    PressDeleteTile();
                }
                if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl))
                {
                    PressRotateCW();
                }
            }
        }

        if (BoardCreationManager.instance.GetMode() == BoardCreationManager.Mode.Rotate)
        {
            if (isHovered)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    PressRotateCW();
                }
                if (Input.GetMouseButtonDown(1))
                {
                    PressRotateCCW();
                }
            }
        }

        if (BoardCreationManager.instance.GetMode() == BoardCreationManager.Mode.Delete)
        {
            if (isHovered)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    PressDeleteTile();
                }
            }
        }

        if (BoardCreationManager.instance.GetMode() == BoardCreationManager.Mode.ZForward)
        {
            if (isHovered)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    PressZForward();
                }
            }
        }

        if (BoardCreationManager.instance.GetMode() == BoardCreationManager.Mode.ZBackward)
        {
            if (isHovered)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    PressZBack();
                }
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
    public void SetButtonHovered(bool isHovered)
    {
        this.isButtonHovered = isHovered;
    }
    public bool GetHovered()
    {
        return isHovered;
    }

    public void PressChildTile()
    {
        //BoardCreationManager.instance.SetPieceSelected(this);
    }

    public void PressDeleteTile()
    {
        parentTile.AddToCurrent(1);
        BoardCreationManager.instance.TileDeleted(this);
        if (pairedTile != null)
            pairedTile.AddToCurrent(1);
        Destroy(gameObject);
    }
    public void PressRotateCW()
    {
        currentImage.transform.Rotate(new Vector3(0, 0, -90));
        currentRotation += 90;
        if (currentRotation == 360)
            currentRotation = 0;

        AdjustToGrid();

    }
    public void PressRotateCCW()
    {
        currentImage.transform.Rotate(new Vector3(0, 0, 90));
        currentRotation -= 90;
        if (currentRotation == -90)
            currentRotation = 270;

        AdjustToGrid();

    }

    public void PressZForward()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            transform.SetAsLastSibling();
        else
        {
            if (transform.GetSiblingIndex() < transform.parent.childCount - 1)
                transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
        }
    }
    public void PressZBack()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            transform.SetAsFirstSibling();
        else
        {
            if (transform.GetSiblingIndex() > 0)
                transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
        }
    }

    public ChildTileObjectController GetPairedTile()
    {
        return pairedTile;
    }
    public void SetPairedTile(ChildTileObjectController pairedTile)
    {
        this.pairedTile = pairedTile;
    }

}
