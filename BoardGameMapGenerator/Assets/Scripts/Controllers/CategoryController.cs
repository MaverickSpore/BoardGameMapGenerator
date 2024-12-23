using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CategoryController : MonoBehaviour
{
    [SerializeField] Button PieceButton;

    public void HidePiece()
    {
        PieceButton.gameObject.SetActive(false);
    }
    public void ShowPiece()
    {
        PieceButton.gameObject.SetActive(true);
    }
    public void SetImageSprite(Sprite sprite)
    {
        //PieceButton.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.textureRect.width, sprite.textureRect.height);
        PieceButton.image.sprite = sprite;
    }
}
