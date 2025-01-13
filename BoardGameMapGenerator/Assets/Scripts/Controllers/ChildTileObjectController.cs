using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChildTileObjectController : MonoBehaviour
{
    [SerializeField] Image currentImage;
    [SerializeField] TMP_Text countText;

    int current;
    int max;
    public void SetImage(Sprite newImage, string imageName = "DefaultName")
    {
        currentImage.sprite = newImage;
        currentImage.GetComponent<RectTransform>().sizeDelta = new Vector2 (newImage.rect.width, newImage.rect.height);
        name = imageName;
    }
    public Sprite GetImage()
    {
        return currentImage.sprite;
    }
    public float GetHeight()
    {
        return currentImage.GetComponent<RectTransform>().sizeDelta.y + countText.GetComponent<RectTransform>().sizeDelta.y;
    }
    public void SetYOffset(float newYOffset)
    {
        gameObject.transform.position = new Vector3(gameObject.transform.position.x, newYOffset - (currentImage.GetComponent<RectTransform>().sizeDelta.y * 0.4f), gameObject.transform.position.z);
    }
    public void SetCountText(int current, int max = -1)
    {
        this.current = current;
        if (max >= 0)
            this.max = max;
        countText.text = this.current + " / " + this.max;
    }
    public int GetCurrent()
    {
        return current;
    }
    public void PressChildTile()
    {
        BoardCreationManager.instance.SetPieceSelected(this);
    }
}
