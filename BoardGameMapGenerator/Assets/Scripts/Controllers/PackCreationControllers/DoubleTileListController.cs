using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoubleTileListController : MonoBehaviour
{
    [SerializeField] TMP_Text MainTextToHide;
    [SerializeField] Button Tile;

    [SerializeField] Button TileUp;
    [SerializeField] Button TileDown;
    [SerializeField] Button RemoveTile;

    PackCreationManager.ImportedImageInfo TileInfo;

    float Width;
    float Height;


    public void SetTile(Sprite sprite, Vector2 size)
    {
        TileUp.gameObject.SetActive(false);
        TileDown.gameObject.SetActive(false);
        RemoveTile.gameObject.SetActive(false);

        MainTextToHide.text = "";
        Tile.gameObject.SetActive(true);
        Tile.GetComponent<Image>().sprite = sprite;
        Tile.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, size.y);
        GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, size.y);
        Width = size.x;
        Height = size.y;
    }
    public float GetWidth()
    {
        return Width;
    }
    public float GetHeight()
    {
        return Height;
    }

    public void SetTileInfo(PackCreationManager.ImportedImageInfo tileInfo)
    {
        TileInfo = tileInfo;
    }
    public PackCreationManager.ImportedImageInfo GetTileInfo()
    {
        return TileInfo;
    }
    public void SetDouble()
    {
        Tile.interactable = true;

        TileUp.gameObject.SetActive(false);
        TileDown.gameObject.SetActive(false);
        RemoveTile.gameObject.SetActive(false);
    }
    public void SetTop()
    {
        Tile.interactable = false;

        TileUp.gameObject.SetActive(false);
        TileDown.gameObject.SetActive(true);
        RemoveTile.gameObject.SetActive(true);
    }
    public void SetMiddle()
    {
        Tile.interactable = false;

        TileUp.gameObject.SetActive(true);
        TileDown.gameObject.SetActive(true);
        RemoveTile.gameObject.SetActive(true);
    }
    public void SetBottom()
    {
        Tile.interactable = false;

        TileUp.gameObject.SetActive(true);
        TileDown.gameObject.SetActive(false);
        RemoveTile.gameObject.SetActive(true);
    }
}
