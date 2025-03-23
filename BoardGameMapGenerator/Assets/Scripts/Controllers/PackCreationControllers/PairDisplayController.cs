using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PairDisplayController : MonoBehaviour
{
    [SerializeField] TMP_Text MainTextToHide;
    [SerializeField] Image MasterTile;
    [SerializeField] Image PairTile;
    [SerializeField] Button Remove;

    private PackCreationManager.ImportedImageInfo TileInfo;
    private PackCreationManager.ImportedImageInfo PairedTileInfo;

    private string Category;

    public void SetPair(Sprite master, Sprite pair)
    {
        MainTextToHide.text = "";
        MasterTile.gameObject.SetActive(true);
        PairTile.gameObject.SetActive(true);
        Remove.gameObject.SetActive(true);
        MasterTile.sprite = master;
        PairTile.sprite = pair;
    }
    public void SetTileInfo(PackCreationManager.ImportedImageInfo tileInfo, PackCreationManager.ImportedImageInfo pairedTileInfo)
    {
        TileInfo = tileInfo;
        PairedTileInfo = pairedTileInfo;
    }
    public PackCreationManager.ImportedImageInfo GetTileInfo()
    {
        return TileInfo;
    }
    public PackCreationManager.ImportedImageInfo GetPairedTileInfo()
    {
        return PairedTileInfo;
    }
    public void SetCategory(string category)
    {
        Category = category;
    }
}
