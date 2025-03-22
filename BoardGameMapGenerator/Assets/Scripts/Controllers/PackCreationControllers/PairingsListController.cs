using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PairingsListController : MonoBehaviour
{

    [SerializeField] TMP_Text MainTextToHide;
    [SerializeField] Button Tile;

    PackCreationManager.ImportedImageInfo TileInfo;

    private enum Mode
    {
        Neutral,
        Selected,
        Unavailable
    }
    Mode CurrentMode;

    float Width;
    float Height;
    public void SetTile(Sprite sprite, Vector2 size)
    {
        MainTextToHide.text = "";
        Tile.gameObject.SetActive(true);
        Tile.GetComponent<Image>().sprite = sprite;
        Tile.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, size.y);
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, size.y);
        Width = size.x;
        Height = size.y;
    }
    public void SetTileInfo(PackCreationManager.ImportedImageInfo tileInfo)
    {
        TileInfo = tileInfo;
    }
    public PackCreationManager.ImportedImageInfo GetTileInfo()
    {
        return TileInfo;
    }
    public string GetTileName()
    {
        return TileInfo.ImageName;
    }
    public float GetWidth()
    {
        return Width;
    }
    public float GetHeight()
    {
        return Height;
    }

    public void SetNeutral()
    {
        CurrentMode = Mode.Neutral;
        Tile.interactable = true;
    }
    public void SetSelected()
    {
        CurrentMode = Mode.Selected;
        Tile.interactable = false;
    }
    public void SetUnavailable()
    {
        CurrentMode = Mode.Unavailable;
        Tile.interactable = false;
    }
    public bool IsAvailable()
    {
        return CurrentMode == Mode.Neutral;
    }
}
