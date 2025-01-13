using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileObjectPlacedController : MonoBehaviour
{
    [SerializeField] Image currentImage;
    public void SetImage(Sprite newImage)
    {
        currentImage.sprite = newImage;
        currentImage.GetComponent<RectTransform>().sizeDelta = new Vector2(newImage.rect.width, newImage.rect.height);
    }

    public void PressChildTile()
    {
        //BoardCreationManager.instance.SetPieceSelected(this);
    }
}
