using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChildTileObjectController : MonoBehaviour
{
    [SerializeField] Image currentImage;
    [SerializeField] TMP_Text countText;

    public ChildTileObjectController pairedTile;
    List<ChildTileObjectController> tilesPaired;

    int current;
    int max;
    int singleMax;
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
    public string GetSpriteName()
    {
        return name;
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
    public void SetSingleMax(int max)
    {
        singleMax = max;
    }
    public int GetSingleMax()
    {
        return singleMax;
    }
    public int GetCurrent()
    {
        return current;
    }
    public int GetMax()
    {
        return max;
    }
    public int AddToCurrent(int add)
    {
        current += add;
        countText.text = current + " / " + max;
        return current;
    }
    public void PressChildTile()
    {
        if (GetCurrent() > 0)
            BoardCreationManager.instance.SetPieceSelected(this);
    }

    public void SetPairedTile(ChildTileObjectController pairedTile)
    {
        this.pairedTile = pairedTile;
    }
    public ChildTileObjectController GetPairedTile()
    {
        return pairedTile;
    }
    public void SetTilesPaired(List<ChildTileObjectController> tilesPaired)
    {
        this.tilesPaired = tilesPaired;
    }
    public void AddToTilesPaired(ChildTileObjectController tile)
    {
        tilesPaired ??= new List<ChildTileObjectController>();
        tilesPaired.Add(tile);
    }
    public List<ChildTileObjectController> GetTilesPaired()
    {
        tilesPaired ??= new List<ChildTileObjectController>();
        return tilesPaired;
    }
}
