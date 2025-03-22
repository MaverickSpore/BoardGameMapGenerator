using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TileListController : MonoBehaviour
{
    [SerializeField] TMP_Text MainTextToHide;
    [SerializeField] Image TileImage;
    [SerializeField] TMP_Text SizeText;
    [SerializeField] TMP_Text MaxText;
    [SerializeField] Button RemoveTileButton;

    PackCreationManager.ImportedImageInfo TileInfo;

    private void SetMaxText(int max)
    {
        MaxText.text = "Max: " + max.ToString();
    }
    private void SetSizeText(Vector2 size)
    {
        SizeText.text = size.x + "x" + size.y;
        //GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, size.y);
    }
    private void SetImage(Sprite sprite, Vector2 size)
    {
        TileImage.sprite = sprite;
        TileImage.rectTransform.sizeDelta = size;
    }

    private void ShowTile()
    {
        MainTextToHide.text = "";
        TileImage.gameObject.SetActive(true);
        SizeText.gameObject.SetActive(true);
        MaxText.gameObject.SetActive(true);
        RemoveTileButton.gameObject.SetActive(true);
    }
    public void SetInfo(PackCreationManager.ImportedImageInfo tileInfo)
    {
        TileInfo = tileInfo;
        SetSizeText(TileInfo.size);
        SetMaxText(TileInfo.maxCount);
        SetImage(TileInfo.ImageSprite, TileInfo.size);

        ShowTile();
    }
    public void SetInfo(Vector2 size, int max, Sprite tileImage)
    {
        SetSizeText(size);
        SetMaxText(max);
        SetImage(tileImage, size);

        ShowTile();
    }
    public void SetInfo(string width, string height, int max, Sprite tileImage)
    {
        SizeText.text = width + "x" + height;
        SetMaxText(max);
        SetImage(tileImage, new Vector2(int.Parse(width), int.Parse(height)));

        ShowTile();
    }
    public PackCreationManager.ImportedImageInfo GetTileInfo()
    {
        if (TileInfo.IsUnityNull())
        {
            return new PackCreationManager.ImportedImageInfo();
        }

        return TileInfo;
    }
}
