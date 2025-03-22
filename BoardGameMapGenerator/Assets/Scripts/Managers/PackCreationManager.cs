using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PackCreationManager : MonoBehaviour
{
    [Header("Instance")]
    public static PackCreationManager Instance;

    [Header("Sections - Manage Tiles")]
    [SerializeField] Image TileListMenu;
    [SerializeField] Image ImageSizesMenu;
    [SerializeField] Image PreviewImage;
    [SerializeField] Image ImportImageMenu;

    [Header("Sections - Manage Categories")]
    [SerializeField] Image ManageCategoriesMenu;
    [SerializeField] Image TilesListMenu;

    [Header("Sections - Manage Pairings")]
    [SerializeField] Image SetupPairingsMenu;
    [SerializeField] Image GeneratePackMenu;

    [Header("Sections - Zip Folder Location")]
    [SerializeField] Image ZipFolderLocationMenu;

    [Header("Dropdowns")]
    [SerializeField] TMP_Dropdown TileWidth;
    [SerializeField] TMP_Dropdown TileHeight;

    //[Header("Text Blocks")]

    [Header("Input Fields")]
    [SerializeField] TMP_InputField NewCategoryInput;

    [Header("Manage Categories Items")]
    [SerializeField] RectTransform ContentAreaManage;
    [SerializeField] TMP_Text NoCategoriesFoundManage;
    [SerializeField] RectTransform ContentAreaDouble;
    [SerializeField] TMP_Text NoTilesFoundDouble;

    [Header("Image List Items")]
    [SerializeField] RectTransform ContentAreaImage;
    [SerializeField] TMP_Text NoTilesFoundImage;
    [SerializeField] TMP_Dropdown TileWidthDropdown;
    [SerializeField] TMP_Dropdown TileHeightDropdown;
    [SerializeField] TMP_InputField TileMaxCountInput;

    [Header("Image Import Items")]
    [SerializeField] RectTransform ContentAreaImageImport;
    [SerializeField] TMP_Text NoFoldersFoundImageImport;

    [Header("Generate Pack Items")]
    [SerializeField] RectTransform ContentAreaGeneratePack;
    [SerializeField] ExpandableListController ExpandableListTemplate;
    [SerializeField] TMP_InputField PackNameInput;

    [Header("Setup Pairings Items")]
    [SerializeField] RectTransform ContentAreaSetupPairings;
    [SerializeField] TMP_Text NoTilesFoundPairing;
    [SerializeField] RectTransform ContentAreaPairingsDisplay;
    [SerializeField] TMP_Text NoPairingsFound;

    [Header("ZipFolder Location Items")]
    [SerializeField] RectTransform ContentAreaZipFolder;
    [SerializeField] TMP_Text NoFoldersFoundZipLocation;

#nullable enable
    public struct ImportedImageInfo
    {
        public string ImagePath;
        public Sprite ImageSprite;
        public string ImageName;
        public Vector2 size;
        public int maxCount;

        public override bool Equals(object? obj) => obj is ImportedImageInfo other && this.Equals(other);
        public bool Equals(ImportedImageInfo p) => ImagePath == p.ImagePath && ImageSprite == p.ImageSprite && ImageName == p.ImageName && size == p.size && maxCount == p.maxCount;
        public override int GetHashCode() => (ImagePath, ImageSprite, ImageName, size, maxCount).GetHashCode();
        public static bool operator ==(ImportedImageInfo lhs, ImportedImageInfo rhs) => lhs.Equals(rhs);
        public static bool operator !=(ImportedImageInfo lhs, ImportedImageInfo rhs) => !(lhs == rhs);
    }
#nullable disable
    public struct PairedTileInfo
    {
        public ImportedImageInfo Tile;
        public List<ImportedImageInfo> PairedTiles;
        public string Category;
    }

    List<TMP_Text> TilesListImage;
    List<TMP_Text> DoubleTilesListLeft;
    List<TMP_Text> DoubleTilesListRight;
    List<TMP_Text> CategoriesListManage;
    List<TMP_Text> FoldersListImageImport;
    List<TMP_Text> PairingsList;
    List<TMP_Text> PairingsDisplayList;
    List<TMP_Text> ZipSaveFolderDisplayList;
    List<string> CategoriesList;
    List<ImportedImageInfo> TilesList;
    //Dictionary<string, List<ImportedImageInfo>> CategoriesDictionary;
    //Dictionary<ImportedImageInfo, List<ImportedImageInfo>> PairedTiles;
    List<PairedTileInfo> PairedTiles;

    readonly string GameBoardSetPath = Application.dataPath + "/GameBoardSets/";
    string CurrentPath;
    string CurrentImagePath;
    TMP_Text CurrentSelectedTile;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        CategoriesListManage = new List<TMP_Text>();
        TilesListImage = new List<TMP_Text>();
        DoubleTilesListLeft = new List<TMP_Text>();
        DoubleTilesListRight = new List<TMP_Text>();
        FoldersListImageImport = new List<TMP_Text>();
        PairingsList = new List<TMP_Text>();
        PairingsDisplayList = new List<TMP_Text>();
        ZipSaveFolderDisplayList = new List<TMP_Text>();
        TilesList = new List<ImportedImageInfo>();
        //CategoriesDictionary = new Dictionary<string, List<ImportedImageInfo>>();
        //PairedTiles = new Dictionary<ImportedImageInfo, List<ImportedImageInfo>>();
        CategoriesList = new List<string>();
        CurrentPath = GameBoardSetPath;

        if (!Directory.Exists(GameBoardSetPath))
        {
            Directory.CreateDirectory(GameBoardSetPath);
        }


        HideMenus();
        ShowImageImportSection();
    }
    private void SetPresetSizes(int w, int h)
    {
        TileWidth.SetValueWithoutNotify(w);
        TileHeight.SetValueWithoutNotify(h);
        SetSize();
    }
    private void SetSize()
    {
        //CurrentTileSize.text = TileWidth.captionText.text + " x " + TileHeight.captionText.text;
        PreviewImage.rectTransform.sizeDelta = new Vector2(int.Parse(TileWidth.captionText.text), int.Parse(TileHeight.captionText.text));
    }

#if true // Set Up Categories Section
    
    // Manage Categories Methods
    private void SetupCategoriesListManage()
    {
        if (CategoriesList.Count == 0)
        {
            NoCategoriesFoundManage.gameObject.SetActive(true);
        }
        else
        {
            if (CategoriesListManage.Count == 0)
            {
                foreach (string category in CategoriesList)
                {
                    TMP_Text newFolder = Instantiate(NoCategoriesFoundManage, ContentAreaManage.transform);
                    newFolder.text = category;
                    newFolder.GetComponent<ManageCategoriesController>().SetFolder();
                    newFolder.GetComponent<ManageCategoriesController>().SetMiddle();
                    CategoriesListManage.Add(newFolder);
                }
                NoCategoriesFoundManage.gameObject.SetActive(false);
                if (CategoriesListManage.Count > 1)
                {
                    CategoriesListManage[0].GetComponent<ManageCategoriesController>().SetTop();
                    CategoriesListManage[^1].GetComponent<ManageCategoriesController>().SetBottom();
                }
                else if (CategoriesListManage.Count == 1)
                {
                    CategoriesListManage[0].GetComponent<ManageCategoriesController>().SetNeither();
                }
            }
        }

        AdjustCategoriesListManage();
    }
    private void ResetCategoriesListManage()
    {
        CategoriesListManage ??= new List<TMP_Text>();
        /*if (CategoriesListManage.Count > 0)
        {
            foreach (TMP_Text folder in CategoriesListManage)
            {
                Destroy(folder.gameObject);
            }
            CategoriesListManage.Clear();
        }*/
        NoCategoriesFoundManage.gameObject.SetActive(true);
    }
    private void ReloadCategoriesListManage()
    {
        ResetCategoriesListManage();
        SetupCategoriesListManage();
    }
    private void AdjustCategoriesListManage()
    {
        if (CategoriesListManage.Count <= 0) { return; }
        float startingY = NoCategoriesFoundManage.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;
        foreach (TMP_Text folder in CategoriesListManage)
        {
            folder.gameObject.SetActive(true);
            if (folder.GetComponent<DoubleTileListController>() != null)
            {
                yOffset -= folder.GetComponent<DoubleTileListController>().GetHeight() * 0.5f;
            }
            else if (folder != CategoriesListManage[0])
            {
                yOffset -= 25.0f;
            }
            folder.GetComponent<RectTransform>().transform.position = new Vector3(NoCategoriesFoundManage.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            if (folder.GetComponent<DoubleTileListController>() != null)
            {
                yOffset -= folder.GetComponent<DoubleTileListController>().GetHeight() * 0.5f + 10;
                if (folder == CategoriesListManage[1])
                {
                    folder.GetComponent<DoubleTileListController>().SetTop();
                }
                else
                {
                    folder.GetComponent<DoubleTileListController>().SetMiddle();
                    if (folder == CategoriesListManage[^1])
                        folder.GetComponent<DoubleTileListController>().SetBottom();
                }
            }
            else
            {
                yOffset -= 50.0f;
                if (folder.GetComponent<ManageCategoriesController>() != null)
                {
                    if (folder == CategoriesListManage[0])
                        folder.GetComponent<ManageCategoriesController>().SetTop();
                    else
                        folder.GetComponent<ManageCategoriesController>().SetMiddle();

                    if (IsLastCategory(folder))
                        folder.GetComponent<ManageCategoriesController>().SetBottom();
                    if (folder == CategoriesListManage[0] && IsLastCategory(folder))
                        folder.GetComponent<ManageCategoriesController>().SetNeither();
                }
            }
        }
        ContentAreaManage.sizeDelta = new Vector2(ContentAreaManage.sizeDelta.x, Mathf.Abs(yOffset) + 25);
        NoCategoriesFoundManage.gameObject.SetActive(false);
    }

    // Double Tiles List Methods
    private void SetupDoubletilesList()
    {
        if (TilesList.Count == 0)
        {
            NoTilesFoundDouble.gameObject.SetActive(true);
        }
        else
        {
            if (DoubleTilesListLeft.Count + DoubleTilesListRight.Count != TilesList.Count)
            {
                if (DoubleTilesListLeft.Count > 0)
                {
                    foreach (TMP_Text tile in DoubleTilesListLeft)
                    {
                        if (tile == null) { continue; }
                        Destroy(tile.gameObject);
                    }
                    DoubleTilesListLeft.Clear();
                }
                if (DoubleTilesListRight.Count > 0)
                {
                    foreach (TMP_Text tile in DoubleTilesListRight)
                    {
                        if (tile == null) { continue; }
                        Destroy(tile.gameObject);
                    }
                    DoubleTilesListRight.Clear();
                }


                int count = 0;
                foreach (ImportedImageInfo tile in TilesList)
                {
                    count++;
                    TMP_Text newTile = Instantiate(NoTilesFoundDouble, ContentAreaDouble.transform);
                    switch (count % 2)
                    {
                        case 0:
                            newTile.GetComponent<DoubleTileListController>().SetTile(tile.ImageSprite, tile.size);
                            DoubleTilesListRight.Add(newTile);
                            break;
                        case 1:
                            newTile.GetComponent<DoubleTileListController>().SetTile(tile.ImageSprite, tile.size);
                            DoubleTilesListLeft.Add(newTile);
                            break;
                    }
                    newTile.GetComponent<DoubleTileListController>().SetTileInfo(tile);
                    newTile.GetComponent<RectTransform>().sizeDelta = new Vector2(tile.size.x, tile.size.y);
                    newTile.name = tile.ImagePath + "_" + tile.size.x + "x" + tile.size.y;
                }
            }
            else
            {
            }
            NoTilesFoundDouble.gameObject.SetActive(false);
        }
        AdjustDoubleTilesList();
    }
    private void ResetDoubleTilesList()
    {
        DoubleTilesListLeft ??= new List<TMP_Text>();
        DoubleTilesListRight ??= new List<TMP_Text>();
        NoTilesFoundDouble.gameObject.SetActive(true);
    }
    private void ReloadDoubleTilesList()
    {
        ResetDoubleTilesList();
        SetupDoubletilesList();
    }
    private void AdjustDoubleTilesList()
    {
        if (DoubleTilesListLeft.Count <= 0 && DoubleTilesListRight.Count <= 0) { return; }
        float startingY = NoTilesFoundDouble.GetComponent<RectTransform>().transform.position.y;
        // Left Side
        float yOffsetLeft = NoTilesFoundDouble.GetComponent<RectTransform>().transform.localPosition.y;
        foreach (TMP_Text tile in DoubleTilesListLeft)
        {
            if (tile == null) { continue; }
            yOffsetLeft -= tile.GetComponent<DoubleTileListController>().GetHeight() * 0.5f;
            tile.GetComponent<RectTransform>().transform.position = new Vector3(NoTilesFoundDouble.GetComponent<RectTransform>().transform.position.x  - 128, startingY + yOffsetLeft);
            yOffsetLeft -= tile.GetComponent<DoubleTileListController>().GetHeight() * 0.5f;
        }

        // Right Side
        float yOffsetRight = NoTilesFoundDouble.GetComponent<RectTransform>().transform.localPosition.y;
        foreach (TMP_Text tile in DoubleTilesListRight)
        {
            if (tile == null) { continue; }
            yOffsetRight -= tile.GetComponent<DoubleTileListController>().GetHeight() * 0.5f;
            tile.GetComponent<RectTransform>().transform.position = new Vector3(NoTilesFoundDouble.GetComponent<RectTransform>().transform.position.x + 128, startingY + yOffsetRight);
            yOffsetRight -= tile.GetComponent<DoubleTileListController>().GetHeight() * 0.5f;
        }

        float yOffset = Mathf.Abs(Mathf.Min(yOffsetLeft, yOffsetRight));
        ContentAreaDouble.sizeDelta = new Vector2(ContentAreaDouble.sizeDelta.x, yOffset  + 25);
    }


    public void PressAddTileToCategory(TMP_Text tile)
    {
        if (CategoriesListManage.Count == 0) { return; }
        if (GetTileFromTileList(tile).ImagePath == "") { return; }
        TilesList.Remove(GetTileFromTileList(tile));
        DoubleTilesListLeft.Remove(tile);
        DoubleTilesListRight.Remove(tile);
        tile.transform.SetParent(ContentAreaManage.transform);
        // TODO: Add Tile to Category
        CategoriesListManage.Add(tile);
        AdjustCategoriesListManage();
        // TODO: Remove Tile from TilesList
        ReloadDoubleTilesList();
    }
    public void PressTileUp(TMP_Text tile)
    {
        int index = CategoriesListManage.FindIndex(x => x == tile);
        if (index > 1 && CategoriesListManage[index - 1] != null)
        {
            (CategoriesListManage[index], CategoriesListManage[index - 1]) = (CategoriesListManage[index - 1], CategoriesListManage[index]);
            AdjustCategoriesListManage();
        }
    }
    public void PressTileDown(TMP_Text tile)
    {
        int index = CategoriesListManage.FindIndex(x => x == tile);
        if (index < CategoriesListManage.Count - 1 && CategoriesListManage[index + 1] != null)
        {
            (CategoriesListManage[index], CategoriesListManage[index + 1]) = (CategoriesListManage[index + 1], CategoriesListManage[index]);
            AdjustCategoriesListManage();
        }
    }
    private void RemoveTileFromCategory(TMP_Text tile)
    {
        // Add tile to DoubleTilesListLeft or DoubleTilesListRight, whichever has less tiles
        TilesList.Add(tile.GetComponent<DoubleTileListController>().GetTileInfo());
        tile.transform.SetParent(ContentAreaDouble.transform);
        tile.GetComponent<DoubleTileListController>().SetDouble();
        if (DoubleTilesListLeft.Count <= DoubleTilesListRight.Count)
        {
            DoubleTilesListLeft.Add(tile);
        }
        else
        {
            DoubleTilesListRight.Add(tile);
        }
        // Remove tile from CategoriesListManage
        //Destroy(tile.gameObject);
        CategoriesListManage.Remove(tile);
        print("Tile Removed: " + tile.name);
    }
    public void PressTileRemove(TMP_Text tile)
    {
        RemoveTileFromCategory(tile);
        ReloadCategoriesListManage();
        ReloadDoubleTilesList();
    }
    public void PressCategoryUp(TMP_Text category)
    {
        List<TMP_Text> tiles = new();
        List<TMP_Text> aboveTiles = new();
        int index = CategoriesListManage.FindIndex(x => x == category);
        if (index == 0) { return; }
        int aboveIndex = 0;
        tiles.Add(CategoriesListManage[index]);
        for (int i = index + 1; i < CategoriesListManage.Count; i++)
        {
            if (CategoriesListManage[i].GetComponent<DoubleTileListController>() != null)
            {
                tiles.Add(CategoriesListManage[i]);
            }
            if (CategoriesListManage[i].GetComponent<ManageCategoriesController>() != null)
            {
                break;
            }
        }
        for (int i = index - 1; i >= 0; i--)
        {
            if (CategoriesListManage[i].GetComponent<DoubleTileListController>() != null)
            {
                aboveTiles.Add(CategoriesListManage[i]);
            }
            if (CategoriesListManage[i].GetComponent<ManageCategoriesController>() != null)
            {
                aboveTiles.Add(CategoriesListManage[i]);
                aboveTiles.Reverse();
                aboveIndex = i;
                break;
            }
        }
        // Swap the two lists in CategoriesListManage
        for (int i = 0; i < tiles.Count; i++)
        {
            CategoriesListManage[aboveIndex + i] = tiles[i];
        }
        for (int i = 0; i < aboveTiles.Count; i++)
        {
            CategoriesListManage[aboveIndex + tiles.Count + i] = aboveTiles[i];
        }
        ReloadCategoriesListManage();
    }
    public void PressCategoryDown(TMP_Text category)
    {
        List<TMP_Text> tiles = new();
        List<TMP_Text> belowTiles = new();
        int index = CategoriesListManage.FindIndex(x => x == category);
        if (index == CategoriesListManage.Count - 1) { return; }
        int belowIndex = 0;
        tiles.Add(CategoriesListManage[index]);
        for (int i = index + 1; i < CategoriesListManage.Count; i++)
        {
            if (CategoriesListManage[i].GetComponent<DoubleTileListController>() != null)
            {
                tiles.Add(CategoriesListManage[i]);
            }
            if (CategoriesListManage[i].GetComponent<ManageCategoriesController>() != null)
            {
                break;
            }
        }
        belowIndex = index + tiles.Count;
        belowTiles.Add(CategoriesListManage[belowIndex]);
        for (int i = belowIndex + 1; i < CategoriesListManage.Count; i++)
        {
            if (CategoriesListManage[i].GetComponent<DoubleTileListController>() != null)
            {
                belowTiles.Add(CategoriesListManage[i]);
            }
            if (CategoriesListManage[i].GetComponent<ManageCategoriesController>() != null)
            {
                break;
            }
        }
        // Swap the two lists in CategoriesListManage
        for (int i = 0; i < belowTiles.Count; i++)
        {
            CategoriesListManage[index + i] = belowTiles[i];
        }
        for (int i = 0; i < tiles.Count; i++)
        {
            CategoriesListManage[index + belowTiles.Count + i] = tiles[i];
        }
        ReloadCategoriesListManage();
    }
    public void PressCategoryDelete(TMP_Text category)
    {
        int index = CategoriesListManage.FindIndex(x => x == category);

        if (index < 0 || index >= CategoriesListManage.Count) 
        {
            Destroy(category.gameObject);
            return; 
        }
        if (index == 0 && CategoriesListManage.Count == 1)
        {
            CategoriesListManage.Clear();
            CategoriesList.Clear();
            Destroy(category.gameObject);
            ReloadCategoriesListManage();
            return;
        }

        CategoriesListManage.RemoveAt(index);
        CategoriesList.Remove(category.text);
        while (CategoriesListManage.Count > index)
        {
            if (CategoriesListManage[index].GetComponent<DoubleTileListController>() != null)
                RemoveTileFromCategory(CategoriesListManage[index]);
            else
                break;
        }
        Destroy(category.gameObject);
        ReloadCategoriesListManage();
        ReloadDoubleTilesList();
    }
#endif

#if true // Set Up Tile Creation Section

    // TileList Methods
    private void SetupTilesListImage()
    {
        if (TilesList.Count == 0)
        {
            NoTilesFoundImage.gameObject.SetActive(true);
        }
        else
        {
            foreach (ImportedImageInfo tile in TilesList)
            {
                TMP_Text newFolder = Instantiate(NoTilesFoundImage, ContentAreaImage.transform);
                //newFolder.GetComponent<TileListController>().SetInfo(tile.size, tile.maxCount, tile.ImageSprite);
                newFolder.GetComponent<TileListController>().SetInfo(tile);
                TilesListImage.Add(newFolder);
            }
            NoTilesFoundImage.gameObject.SetActive(false);
        }

        AdjustTilesListImage();
    }
    private void ResetTilesListImage()
    {
        TilesListImage ??= new List<TMP_Text>();
        if (TilesListImage.Count > 0)
        {
            foreach (TMP_Text tile in TilesListImage)
            {
                Destroy(tile.gameObject);
            }
        }
        TilesListImage.Clear();
        NoTilesFoundImage.gameObject.SetActive(true);
    }
    private void ReloadTilesListImage()
    {
        ResetTilesListImage();
        SetupTilesListImage();
    }
    private void AdjustTilesListImage()
    {
        if (TilesListImage.Count <= 0) { return; }
        TilesListImage[0].GetComponent<RectTransform>().transform.position = NoTilesFoundImage.GetComponent<RectTransform>().transform.position;
        float startingY = NoTilesFoundImage.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;
        foreach (TMP_Text folder in TilesListImage)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            yOffset -= 260;
        }
        ContentAreaImage.GetComponent<RectTransform>().sizeDelta = new Vector2(ContentAreaImage.GetComponent<RectTransform>().sizeDelta.x, Mathf.Abs(yOffset) + 25);
    }
    
    // Image Import Methods
    private void SetupFoldersListImageImport()
    {
        DirectoryInfo dirInfoPath = new(CurrentPath);
        DirectoryInfo[] folders = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        FileInfo[] jpgFiles = dirInfoPath.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);
        FileInfo[] pngFiles = dirInfoPath.GetFiles("*.png", SearchOption.TopDirectoryOnly);
        if (folders.Length == 0)
        {
            NoFoldersFoundImageImport.gameObject.SetActive(true);
        }
        else
        {
            foreach (DirectoryInfo folder in folders)
            {
                TMP_Text newFolder = Instantiate(NoFoldersFoundImageImport, ContentAreaImageImport.transform);
                newFolder.text = folder.Name;
                newFolder.GetComponent<ImportImageController>().SetFolder();
                FoldersListImageImport.Add(newFolder);
            }
            NoFoldersFoundImageImport.gameObject.SetActive(false);
        }

        if (jpgFiles.Length > 0)
        {
            foreach (FileInfo file in jpgFiles)
            {
                TMP_Text newFolder = Instantiate(NoFoldersFoundImageImport, ContentAreaImageImport.transform);
                newFolder.text = file.Name;
                newFolder.GetComponent<ImportImageController>().SetImage();
                FoldersListImageImport.Add(newFolder);
            }
            NoFoldersFoundImageImport.gameObject.SetActive(false);
        }

        if (pngFiles.Length > 0)
        {
            foreach (FileInfo file in pngFiles)
            {
                TMP_Text newFolder = Instantiate(NoFoldersFoundImageImport, ContentAreaImageImport.transform);
                newFolder.text = file.Name;
                newFolder.GetComponent<ImportImageController>().SetImage();
                FoldersListImageImport.Add(newFolder);
            }
            NoFoldersFoundImageImport.gameObject.SetActive(false);
        }

        AdjustFoldersListImageImport();
    }
    private void ResetFoldersListImageImport()
    {
        FoldersListImageImport ??= new List<TMP_Text>();
        if (FoldersListImageImport.Count > 0)
        {
            foreach (TMP_Text folder in FoldersListImageImport)
            {
                Destroy(folder.gameObject);
            }
            FoldersListImageImport.Clear();
        }
        NoFoldersFoundImageImport.gameObject.SetActive(true);
    }
    private void ReloadFoldersListImageImport()
    {
        ResetFoldersListImageImport();
        SetupFoldersListImageImport();
    }
    private void AdjustFoldersListImageImport()
    {
        if (FoldersListImageImport.Count <= 0) { return; }
        float startingY = NoFoldersFoundImageImport.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;
        foreach (TMP_Text folder in FoldersListImageImport)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            yOffset -= 50;
        }
        ContentAreaImageImport.GetComponent<RectTransform>().sizeDelta = new Vector2(ContentAreaImageImport.GetComponent<RectTransform>().sizeDelta.x, Mathf.Abs(yOffset) + 25); 
    }
    private void GetDrivesImageImport()
    {
        ResetFoldersListImageImport();
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
            TMP_Text newFolder = Instantiate(NoFoldersFoundImageImport, ContentAreaImageImport.transform);
            newFolder.text = drive.Name;
            newFolder.GetComponent<ImportImageController>().SetFolder();
            FoldersListImageImport.Add(newFolder);
        }
        NoFoldersFoundImageImport.gameObject.SetActive(false);
        AdjustFoldersListImageImport();
    }
    private Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }
    public void PressExitFolderImageImport()
    {
        ExitFolder();
        if (CurrentPath == "")
        {
            GetDrivesImageImport();
            return;
        }
        ReloadFoldersListImageImport();
    }
#endif

#if true // Set Up Pairings Section

    // SetupPairings Methods
    private void SetupCategoriesListPairings()
    {
        if (PairingsList.Count == 0)
        {
            foreach (TMP_Text item in CategoriesListManage)
            {
                if (item.GetComponent<DoubleTileListController>() != null)
                {
                    TMP_Text newTile = Instantiate(NoTilesFoundPairing, ContentAreaSetupPairings.transform);
                    newTile.GetComponent<PairingsListController>().SetTile(item.GetComponent<DoubleTileListController>().GetTileInfo().ImageSprite, item.GetComponent<DoubleTileListController>().GetTileInfo().size);
                    newTile.GetComponent<PairingsListController>().SetTileInfo(item.GetComponent<DoubleTileListController>().GetTileInfo());
                    newTile.GetComponent<PairingsListController>().SetNeutral();
                    PairingsList.Add(newTile);
                }
                else if (item.GetComponent<ManageCategoriesController>() != null)
                {
                    PairingsList.Add(item);
                    item.transform.SetParent(ContentAreaSetupPairings.transform);
                }
            }
        }
        else
        {
        }

        AdjustCategoriesListPairings();
    }
    private void ResetCategoriesListPairings()
    {
        PairingsList ??= new List<TMP_Text>();
        NoTilesFoundPairing.gameObject.SetActive(true);
    }
    private void ReloadCategoriesListPairings()
    {
        ResetCategoriesListPairings();
        SetupCategoriesListPairings();
    }
    private void AdjustCategoriesListPairings()
    {
        if (PairingsList.Count <= 0) { return; }
        float startingY = NoTilesFoundPairing.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;
        foreach (TMP_Text folder in PairingsList)
        {
            folder.gameObject.SetActive(true);
            if (folder.GetComponent<PairingsListController>() != null)
            {
                yOffset -= folder.GetComponent<PairingsListController>().GetHeight() * 0.5f;
            }
            else if (folder != PairingsList[0])
            {
                yOffset -= 25.0f;
            }
            folder.GetComponent<RectTransform>().transform.position = new Vector3(NoTilesFoundPairing.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            if (folder.GetComponent<PairingsListController>() != null)
            {
                yOffset -= folder.GetComponent<PairingsListController>().GetHeight() * 0.5f + 10;
            }
            else
            {
                yOffset -= 50.0f;
                if (folder.GetComponent<ManageCategoriesController>() != null)
                {
                    folder.GetComponent<ManageCategoriesController>().SetNeither();
                }
            }
        }
        ContentAreaSetupPairings.sizeDelta = new Vector2(ContentAreaSetupPairings.sizeDelta.x, Mathf.Abs(yOffset) + 25);
        NoTilesFoundPairing.gameObject.SetActive(false);
    }

    // SetupPairingsDisplay Methods
    private void SetupPairingsDisplay()
    {
        PairedTiles ??= new List<PairedTileInfo>();
        if (PairedTiles.Count <= 0) return;

        foreach (PairedTileInfo pair in PairedTiles)
        {
            foreach (ImportedImageInfo tile in pair.PairedTiles)
            {
                TMP_Text newPair = Instantiate(NoPairingsFound, ContentAreaPairingsDisplay.transform);
                newPair.GetComponent<PairDisplayController>().SetPair(pair.Tile.ImageSprite, tile.ImageSprite);
                newPair.GetComponent<PairDisplayController>().SetTileInfo(pair.Tile, tile);
                newPair.GetComponent<PairDisplayController>().SetCategory(pair.Category);
                PairingsDisplayList.Add(newPair);
            }
        }

        AdjustPairingsDisplay();
    }
    private void ResetPairingsDisplay()
    {
        PairingsDisplayList ??= new List<TMP_Text>();
        if (PairingsDisplayList.Count > 0)
        {
            foreach (TMP_Text pair in PairingsDisplayList)
            {
                Destroy(pair.gameObject);
            }
            PairingsDisplayList.Clear();
        }
        NoPairingsFound.gameObject.SetActive(true);
    }
    private void ReloadPairingsDisplay()
    {
        ResetPairingsDisplay();
        SetupPairingsDisplay();
    }
    private void AdjustPairingsDisplay()
    {
        if (PairingsDisplayList.Count <= 0) { return; }
        float startingY = NoPairingsFound.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;
        foreach (TMP_Text pair in PairingsDisplayList)
        {
            pair.gameObject.SetActive(true);
            yOffset -= 64;
            pair.GetComponent<RectTransform>().transform.position = new Vector3(NoPairingsFound.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            yOffset -= 64;
        }
        ContentAreaPairingsDisplay.sizeDelta = new Vector2(ContentAreaPairingsDisplay.sizeDelta.x, Mathf.Abs(yOffset) + 25);
        NoPairingsFound.gameObject.SetActive(false);
    }

    private TMP_Text GetTileCategory(TMP_Text tile)
    {
        int index = PairingsList.FindIndex(x => x == tile);
        for (int i = index - 1; i >= 0; i--)
        {
            if (PairingsList[i].GetComponent<ManageCategoriesController>() != null)
            {
                return PairingsList[i];
            }
        }
        return null;
    }
    public void PressTilePairing(TMP_Text tile)
    {
        if (tile.GetComponent<PairingsListController>() == null) { return; }
        if (CurrentSelectedTile == null)
        {
            if (tile.GetComponent<PairingsListController>().IsAvailable())
            {
                tile.GetComponent<PairingsListController>().SetSelected();
                CurrentSelectedTile = tile; 
                PairingsDisableOtherCategories(tile);
            }
        }
        else
        {
            if (tile.GetComponent<PairingsListController>().IsAvailable())
            {
                /*
                if (PairedTiles.ContainsKey(tile.GetComponent<PairingsListController>().GetTileInfo()))
                {
                    if (!PairedTiles[tile.GetComponent<PairingsListController>().GetTileInfo()].Contains(CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo()))
                        PairedTiles[tile.GetComponent<PairingsListController>().GetTileInfo()].Add(CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo());
                }
                else if (PairedTiles.ContainsKey(CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo()))
                {
                    if (!PairedTiles[CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo()].Contains(tile.GetComponent<PairingsListController>().GetTileInfo()))
                        PairedTiles[CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo()].Add(tile.GetComponent<PairingsListController>().GetTileInfo());
                }
                else
                    PairedTiles[CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo()] = new List<ImportedImageInfo> { tile.GetComponent<PairingsListController>().GetTileInfo() };
                */

                // Lines 932-943 using PairedTileInfo PairedTiles object
                int index = PairedTiles.FindIndex(x => x.Tile == tile.GetComponent<PairingsListController>().GetTileInfo());
                if (index >= 0 && index < PairedTiles.Count)
                {
                    int pairIndex = PairedTiles[index].PairedTiles.FindIndex(x => x == CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo());
                    if (pairIndex < 0 || pairIndex > PairedTiles[index].PairedTiles.Count)
                        PairedTiles[index].PairedTiles.Add(CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo());
                }
                else if ((index = PairedTiles.FindIndex(x => x.Tile == CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo())) >= 0 && index < PairedTiles.Count)
                {
                    int pairIndex = PairedTiles[index].PairedTiles.FindIndex(x => x == tile.GetComponent<PairingsListController>().GetTileInfo());
                    if (pairIndex < 0 || pairIndex > PairedTiles[index].PairedTiles.Count)
                        PairedTiles[index].PairedTiles.Add(tile.GetComponent<PairingsListController>().GetTileInfo());
                }
                else
                {
                    PairedTiles.Add(new PairedTileInfo { Tile = CurrentSelectedTile.GetComponent<PairingsListController>().GetTileInfo(), PairedTiles = new List<ImportedImageInfo> { tile.GetComponent<PairingsListController>().GetTileInfo() }, Category = GetTileCategory(tile).text });
                }
                
                
                
                
                
                
                
                
                
                CurrentSelectedTile = null;
                EnableAllPairingTiles();
                ReloadPairingsDisplay();
            }
        }
    }
    private void PairingsDisableOtherCategories(TMP_Text tile)
    {
        int index = PairingsList.FindIndex(x => x == tile);
        if (index < 0 || index >= PairingsList.Count) { return; }
        int topIndex = 0;
        int bottomIndex = 0;
        for (int i = index - 1; i >= 0; i--)
        {
            if (PairingsList[i].GetComponent<ManageCategoriesController>() != null)
            {
                topIndex = i;
                break;
            }
        }
        for (int i = index + 1; i < PairingsList.Count; i++)
        {
            if (PairingsList[i].GetComponent<ManageCategoriesController>() != null)
            {
                bottomIndex = i;
                break;
            }
        }

        if (topIndex != 0)
        {
            for (int i = 0; i < topIndex; i++)
            {
                if (PairingsList[i].GetComponent<PairingsListController>() != null)
                    PairingsList[i].GetComponent<PairingsListController>().SetUnavailable();
            }
        }
        if (bottomIndex != 0)
        {
            for (int i = bottomIndex; i < PairingsList.Count; i++)
            {
                if (PairingsList[i].GetComponent<PairingsListController>() != null)
                    PairingsList[i].GetComponent<PairingsListController>().SetUnavailable();
            }
        }
    }
    private void EnableAllPairingTiles()
    {
        if (PairingsList.Count == 0) { return; }

        for (int i = 0; i < PairingsList.Count; i++)
        {
            if (PairingsList[i].GetComponent<PairingsListController>() != null)
            {
                PairingsList[i].GetComponent<PairingsListController>().SetNeutral();
            }
        }
    }

    public void PressRemovePairing(TMP_Text tilePair)
    {
        ImportedImageInfo tile = tilePair.GetComponent<PairDisplayController>().GetTileInfo();
        ImportedImageInfo pair = tilePair.GetComponent<PairDisplayController>().GetPairedTileInfo();
        //if (PairedTiles.ContainsKey(tile))
        int index = PairedTiles.FindIndex(x => x.Tile == tile);
        if (index >= 0 && index < PairedTiles.Count)
        {
            PairedTiles[index].PairedTiles.Remove(pair);
            if (PairedTiles[index].PairedTiles.Count <= 0)
            {
                PairedTiles.RemoveAt(index);
            }
        }
        ReloadPairingsDisplay();
    }

    /*public void PressTestShowPairings()
    {
        foreach (KeyValuePair<ImportedImageInfo, List<ImportedImageInfo>> pair in PairedTiles)
        {
            print(pair.Key.ImageName + ": ");
            foreach (ImportedImageInfo tile in pair.Value)
            {
                print("\t" + pair.Key.ImageName + ": " + tile.ImageName);
            }
        }
    }*/
#endif

#if true // Set Up ZipFolder Save Location Section

    private void SetupZipfolderList()
    {
        DirectoryInfo dirInfoPath = new(CurrentPath);
        DirectoryInfo[] folders = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        if (folders.Length == 0)
        {
            NoFoldersFoundZipLocation.gameObject.SetActive(true);
        }
        else
        {
            foreach (DirectoryInfo folder in folders)
            {
                TMP_Text newFolder = Instantiate(NoFoldersFoundZipLocation, ContentAreaZipFolder.transform);
                newFolder.text = folder.Name;
                newFolder.GetComponent<ZipSaveLocationController>().SetFolder();
                ZipSaveFolderDisplayList.Add(newFolder);
            }
            NoFoldersFoundZipLocation.gameObject.SetActive(false);
        }

        AdjustZipfolderList();
    }
    private void ResetZipfolderList()
    {
        ZipSaveFolderDisplayList ??= new List<TMP_Text>();
        if (ZipSaveFolderDisplayList.Count > 0)
        {
            foreach (TMP_Text folder in ZipSaveFolderDisplayList)
            {
                Destroy(folder.gameObject);
            }
            ZipSaveFolderDisplayList.Clear();
        }
        NoFoldersFoundZipLocation.gameObject.SetActive(true);
    }
    private void ReloadZipfolderList()
    {
        ResetZipfolderList();
        SetupZipfolderList();
    }
    private void AdjustZipfolderList()
    {
        if (ZipSaveFolderDisplayList.Count <= 0) { return; }
        float startingY = NoFoldersFoundZipLocation.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;
        foreach (TMP_Text folder in ZipSaveFolderDisplayList)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            yOffset -= 50;
        }
        ContentAreaZipFolder.sizeDelta = new Vector2(ContentAreaZipFolder.sizeDelta.x, Mathf.Abs(yOffset) + 25);
        NoFoldersFoundZipLocation.gameObject.SetActive(false);
    }
    private void GetDrivesZipSave()
    {
        ResetZipfolderList();
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
            TMP_Text newFolder = Instantiate(NoFoldersFoundZipLocation, ContentAreaZipFolder.transform);
            newFolder.text = drive.Name;
            newFolder.GetComponent<ZipSaveLocationController>().SetFolder();
            ZipSaveFolderDisplayList.Add(newFolder);
        }
        NoFoldersFoundZipLocation.gameObject.SetActive(false);
        AdjustZipfolderList();
    }
    public void PressExitFolderZipSave()
    {
        ExitFolder();
        if (CurrentPath == "")
        {
            GetDrivesZipSave();
            return;
        }
        ReloadZipfolderList();
    }
    public void PressSaveZip()
    {
        // Copy generated pack in PackNameInput.text folder to zip folder at CurrentPath
        if (string.IsNullOrEmpty(CurrentPath)) { return; }
        if (string.IsNullOrEmpty(PackNameInput.text)) { return; }
        string PackPath = GameBoardSetPath + "/" + PackNameInput.text;
        if (!Directory.Exists(PackPath)) { return; }
        string ZipPath = CurrentPath + PackNameInput.text + ".zip";
        if (File.Exists(ZipPath)) { return; }
        ZipFile.CreateFromDirectory(PackPath, ZipPath);
        PressReturn();
    }

#endif

#if true // Public Methods

    public void PressRemoveCategory(TMP_Text category)
    {
        if (CategoriesList.Contains(category.text)/*IsInDropdown(CategoryDropdown, category.text)*/)
        {
            //CategoryDropdown.options.Remove(CategoryDropdown.options.Find(x => x.text.ToLower() == category.text.ToLower()));
            //CategoryDropdown.RefreshShownValue();
            CategoriesList.Remove(category.text);
            ReloadCategoriesListManage();
        }
    }

    // Button Presses
    public void PressSelectCategory()
    {

    }
    public void PressImport()
    {
        HideMenus();
        ImportImageMenu.gameObject.SetActive(true);
        ReloadFoldersListImageImport();
    }
    public void PressAddTile()
    {
        if (string.IsNullOrEmpty(TileMaxCountInput.text)) { return; }
        if (string.IsNullOrEmpty(CurrentImagePath)) { return; }
        string imageName = GetNameFromPath(CurrentImagePath);
        ImportedImageInfo imageInfo = new() { ImagePath = CurrentImagePath, ImageSprite = PreviewImage.sprite, ImageName = imageName, size = new Vector2(int.Parse(TileWidth.captionText.text), int.Parse(TileHeight.captionText.text)), maxCount = int.Parse(TileMaxCountInput.text) };
        if (IsTileInTileList(imageInfo)) { return; }
        if (IsTileInCategoriesList(imageInfo)) { return; }
        TilesList.Add(imageInfo);

        ReloadTilesListImage();
    }
    public void PressRemoveTile(TMP_Text tileText)
    {
        if (TilesList.Contains(tileText.GetComponent<TileListController>().GetTileInfo()))
        {
            TilesList.Remove(tileText.GetComponent<TileListController>().GetTileInfo());
            ReloadTilesListImage();
        }
    }


    public void PressTileSizeChanged()
    {
        SetPresetSizes(TileWidth.value, TileHeight.value);
    }
    public void PressWorldTile()
    {
        SetPresetSizes(5, 5);
        SetSize();
    }
    public void PressObjectiveTile()
    {
        SetPresetSizes(3, 2);
        SetSize();
    }
    public void PressDoorTile()
    {
        SetPresetSizes(2, 2);
        SetSize();
    }
    public void PressConfirmCategoryName()
    {
        if (string.IsNullOrEmpty(NewCategoryInput.text)) { return; }
        if (CategoriesList.Contains(NewCategoryInput.text)) { return; }
        CategoriesList.Add(NewCategoryInput.text);
        TMP_Text newFolder = Instantiate(NoCategoriesFoundManage, ContentAreaManage.transform);
        newFolder.text = NewCategoryInput.text;
        newFolder.GetComponent<ManageCategoriesController>().SetFolder();
        newFolder.GetComponent<ManageCategoriesController>().SetNeither();
        CategoriesListManage.Add(newFolder);
        //ReloadCategoriesListManage();
        AdjustCategoriesListManage();
    }
    public void PressConfirmImageImport()
    {
        // add image from CurrentImagePath to current category
        if (string.IsNullOrEmpty(CurrentImagePath)) { PressCancelImageImport(); return; }
        ImportImageMenu.gameObject.SetActive(false);
    }
    public void PressCancelImageImport()
    {
        if (string.IsNullOrEmpty(CurrentImagePath)) { return; }
        CurrentImagePath = "";
        PreviewImage.sprite = null;
        HideMenus();
    }
    public void PressOpenFolder(TMP_Text Folder)
    {
        CurrentPath += Folder.text + "/";
        if (ImportImageMenu.isActiveAndEnabled)
            ReloadFoldersListImageImport();
        if (ZipFolderLocationMenu.isActiveAndEnabled)
            ReloadZipfolderList();
    }
    public void PressImportImage(TMP_Text Image)
    {
        // Import Image To ?? Location
        string imagePath = CurrentPath + Image.text;

        Texture2D texture = LoadTexture(imagePath);
        if (texture == null) { return; }
        PreviewImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        CurrentImagePath = imagePath;
        //ImportImage(imagePath, destinationPath);

        ReloadFoldersListImageImport();

        //PressRefreshGames();
    }
    public void PressReorderUp(TMP_Text Category)
    {
        int index = CategoriesList.FindIndex(x => x == Category.text);
        if (index == 0) { return; }
        (CategoriesList[index - 1], CategoriesList[index]) = (CategoriesList[index], CategoriesList[index - 1]);
        ReloadCategoriesListManage();
    }
    public void PressReorderDown(TMP_Text Category)
    {
        int index = CategoriesList.FindIndex(x => x == Category.text);
        if (index == CategoriesList.Count - 1) { return; }
        (CategoriesList[index + 1], CategoriesList[index]) = (CategoriesList[index], CategoriesList[index + 1]);
        ReloadCategoriesListManage();
    }
    public void PressCancelGeneratePack()
    {
        HideMenus();
    }
#endif

#if true // Continue Button Presses

    public void PressContinueTiles()
    {
        HideMenus();
        ShowManageCategoriesSection();
    }
    public void PressContinueCategories()
    {
        HideMenus();
        ShowManagePairingsSection();
    }
    public void PressConfirmGeneratePack()
    {
        if (string.IsNullOrEmpty(PackNameInput.text)) { return; }
        if (PairingsList == null) { return; }
        if (PairingsList.Count == 0) { return; }
        string PackPath = GameBoardSetPath + "/" + PackNameInput.text;
        if (Directory.Exists(PackPath))
        {
            Directory.Delete(PackPath, true);
        }
        if (!Directory.Exists(PackPath))
        {
            Directory.CreateDirectory(PackPath);
        }

        // Create Main CFG File
        // 1) Game Name - name=Game Name
        string cfgFullStringMainFolder = "name=" + PackNameInput.text;
        // NOT IMPLEMENTED YET 2) Image Path - image=imageName

        // Create CFG File with cfgFullStringMainFolder in PackPath Directory
        File.WriteAllText(PackPath + "/cfg", cfgFullStringMainFolder);


        int itemIndex = 0;
        Dictionary<string, List<PairedTileInfo>> categorizedPairs = new();
        PairedTiles ??= new List<PairedTileInfo>();
        foreach (PairedTileInfo pair in PairedTiles)
        {
            if (pair.Category != null)
            {
                if (categorizedPairs.ContainsKey(pair.Category))
                {
                    categorizedPairs[pair.Category].Add(pair);
                }
                else
                {
                    categorizedPairs[pair.Category] = new List<PairedTileInfo> { pair };
                }
            }
        }

        Dictionary<string, List<ImportedImageInfo>> CategoriesDictionary = new();
        foreach (TMP_Text item in PairingsList)
        {
            if (item.GetComponent<ManageCategoriesController>() != null)
            {
                if (CategoriesDictionary.ContainsKey(item.text))
                {
                    continue;
                }
                CategoriesDictionary[item.text] = new List<ImportedImageInfo>();
            }
            else if (item.GetComponent<PairingsListController>() != null)
            {
                if (CategoriesDictionary.ContainsKey(GetTileCategory(item).text))
                {
                    CategoriesDictionary[GetTileCategory(item).text].Add(item.GetComponent<PairingsListController>().GetTileInfo());
                }
            }
        }


        foreach (string category in CategoriesDictionary.Keys)
        {
            itemIndex++;
            string categoryPath = PackPath + "/0" + itemIndex + "." + category;
            if (Directory.Exists(categoryPath))
            {
                Directory.Delete(categoryPath, true);
            }
            if (!Directory.Exists(categoryPath))
            {
                Directory.CreateDirectory(categoryPath);
            }
            foreach (ImportedImageInfo imageInfo in CategoriesDictionary[category])
            {
                Texture2D texture = LoadTexture(imageInfo.ImagePath);
                if (texture == null) { continue; }

                RenderTexture rt = new((int)imageInfo.size.x, (int)imageInfo.size.y, 24);
                RenderTexture.active = rt;
                Graphics.Blit(texture, rt);
                Texture2D result = new((int)imageInfo.size.x, (int)imageInfo.size.y);
                result.ReadPixels(new Rect(0, 0, (int)imageInfo.size.x, (int)imageInfo.size.y), 0, 0);
                result.Apply();
                texture = result;

                byte[] texBytes;

                List<string> strings = SeperateStrings(imageInfo.ImagePath, '/');
                if (strings.Count > 0)
                {
                    if (strings[^1] == "")
                    {
                        if (strings[^2].ToLower().Contains("jpg"))
                        {
                            texBytes = texture.EncodeToJPG();
                        }
                        else if (strings[^2].ToLower().Contains("png"))
                        {
                            texBytes = texture.EncodeToPNG();
                        }
                        else
                        {
                            print("ImagePath Error ^2: " + imageInfo.ImagePath);
                            continue;
                        }
                        string imageName = strings[^2];
                        //File.Copy(imageInfo.ImagePath, categoryPath + "/" + imageName, true);
                        File.WriteAllBytes(categoryPath + "/" + imageName, texBytes);
                    }
                    else
                    {
                        if (strings[^1].ToLower().Contains("jpg"))
                        {
                            texBytes = texture.EncodeToJPG();
                        }
                        else if (strings[^1].ToLower().Contains("png"))
                        {
                            texBytes = texture.EncodeToPNG();
                        }
                        else
                        {
                            print("ImagePath Error ^1: " + imageInfo.ImagePath);
                            continue;
                        }
                        string imageName = strings[^1];
                        //File.Copy(imageInfo.ImagePath, categoryPath + "/" + imageName, true);
                        File.WriteAllBytes(categoryPath + "/" + imageName, texBytes);
                    }
                }
            }

            // Create CFG File
            string cfgFullString = "";
            // 1) Category Name - name=Category Name
            cfgFullString += "name=" + category;
            // 2) Max Counts - max=imageName:#;imageName:#;imageName:#
            if (CategoriesDictionary.ContainsKey(category))
            {
                cfgFullString += "\nmax=";
                foreach (ImportedImageInfo imageInfo in CategoriesDictionary[category])
                {
                    cfgFullString += imageInfo.ImageName + ":" + imageInfo.maxCount + ";";
                }
                cfgFullString = cfgFullString[..^1];
            }
            // 3) Pairs - pairs=imageName:imageName;imageName:imageName;imageName:imageName
            if (categorizedPairs.ContainsKey(category))
            {
                // Example Pair String - pairs=Dalle A Recto.JPG:Dalle A Verso.JPG;Dalle B Recto.JPG:Dalle B Verso.JPG
                cfgFullString += "\npairs=";
                foreach (PairedTileInfo pair in categorizedPairs[category])
                {
                    string pairString = "";
                    for (int i = 0; i < pair.PairedTiles.Count; i++)
                    {
                        pairString += pair.Tile.ImageName + ":" + pair.PairedTiles[i].ImageName + ";";
                    }
                    cfgFullString += pairString;
                }
                cfgFullString = cfgFullString[..^1];
            }

            // Write cfgFullString to cfg file within CategoryPath/
            File.WriteAllText(categoryPath + "/cfg", cfgFullString);
        }
        ShowZipSaveLocationSection();
    }
#endif

#if true // Return Button Presses

    public void PressReturn()
    {
        SceneManager.LoadScene(0);
    }
    public void PressReturnImages()
    {
        SceneManager.LoadScene(0);
    }
    public void PressReturnCategories()
    {
        HideMenus();
        ShowImageImportSection();
    }
#endif

#if true // Helper Methods

    private List<string> SeperateStrings(string fullString, char delimiter = '\n')
    {
        if (string.IsNullOrEmpty(fullString))
        {
            return new List<string>();
        }

        List<string> stringsList = new();

        bool hasDelim = fullString.Contains(delimiter);
        if (hasDelim)
        {
            string[] tempStringArr = fullString.Split(delimiter);
            foreach (string str in tempStringArr)
            {
                if (str.EndsWith("\n") || str.EndsWith("\r"))
                    stringsList.Add(str[..^1]);
                else if (str.EndsWith("\r\n"))
                    stringsList.Add(str[..^2]);
                else
                    stringsList.Add(str);
            }
        }
        else
        {
            stringsList.Add(fullString);
        }

        return stringsList;
    }
    private string GetNameFromPath(string path)
    {
        List<string> strings = SeperateStrings(path, '/');
        if (strings.Count <= 0) { return ""; }
        if (strings[^1] == "") { strings.RemoveAt(strings.Count - 1); }
        return strings[^1];
    }
    private ImportedImageInfo GetTileFromTileList(TMP_Text tileText)
    {
        ImportedImageInfo result = new();
        if (tileText == null) { return result; }

        if (tileText.GetComponent<DoubleTileListController>() == null) { return result; }
        ImportedImageInfo tileInfo = tileText.GetComponent<DoubleTileListController>().GetTileInfo();


        foreach(ImportedImageInfo tile in TilesList)
        {
            if (tile.ImagePath == tileInfo.ImagePath)
            {
                if (tile.size == tileInfo.size)
                {
                    if (tile.maxCount == tileInfo.maxCount)
                    {
                        if (tile.ImageSprite == tileInfo.ImageSprite)
                        {
                            return tile;
                        }
                    }
                }
            }
        }
        return result;
    }
    private bool IsTileInTileList(ImportedImageInfo tile)
    {
        foreach (ImportedImageInfo tileInfo in TilesList)
        {
            if (tileInfo.ImagePath == tile.ImagePath)
            {
                if (tileInfo.size == tile.size)
                {
                    if (tileInfo.maxCount == tile.maxCount)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    private bool IsTileInCategoriesList(ImportedImageInfo tile)
    {
        foreach (TMP_Text item in CategoriesListManage)
        {
            if (item.GetComponent<DoubleTileListController>() != null)
            {
                ImportedImageInfo tileInfo = item.GetComponent<DoubleTileListController>().GetTileInfo();
                if (tileInfo.ImagePath == tile.ImagePath)
                {
                    if (tileInfo.size == tile.size)
                    {
                        if (tileInfo.maxCount == tile.maxCount)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    // Show/Hide Menu Methods
    private void HideMenus()
    {
        ManageCategoriesMenu.gameObject.SetActive(false);
        TilesListMenu.gameObject.SetActive(false);
        ImportImageMenu.gameObject.SetActive(false);
        ImageSizesMenu.gameObject.SetActive(false);
        TileListMenu.gameObject.SetActive(false);
        GeneratePackMenu.gameObject.SetActive(false);
        SetupPairingsMenu.gameObject.SetActive(false);
        ZipFolderLocationMenu.gameObject.SetActive(false);
    }
    private void ShowImageImportSection()
    {
        TileListMenu.gameObject.SetActive(true);
        ImageSizesMenu.gameObject.SetActive(true);
        ImportImageMenu.gameObject.SetActive(true);
        ReloadFoldersListImageImport();
        ReloadTilesListImage();
    }
    private void ShowManageCategoriesSection()
    {
        HideMenus();
        ManageCategoriesMenu.gameObject.SetActive(true);
        TilesListMenu.gameObject.SetActive(true);
        ReloadCategoriesListManage();
        ReloadDoubleTilesList();
    }
    private void ShowManagePairingsSection()
    {
        HideMenus();
        SetupPairingsMenu.gameObject.SetActive(true);
        GeneratePackMenu.gameObject.SetActive(true);
        ReloadCategoriesListPairings();
        ReloadPairingsDisplay();
    }
    private void ShowZipSaveLocationSection()
    {
        //HideMenus(); 
        PackNameInput.interactable = false;
        ZipFolderLocationMenu.gameObject.SetActive(true);
        CurrentPath = GameBoardSetPath;
        ReloadZipfolderList();
    }
    private bool IsLastCategory(TMP_Text category)
    {
        int index = CategoriesListManage.FindIndex(x => x == category);
        for (int i = index + 1; i < CategoriesListManage.Count; i++)
        {
            if (CategoriesListManage[i].GetComponent<ManageCategoriesController>() != null)
            {
                return false;
            }
        }
        return true;
    }
    private void ExitFolder()
    {
        List<string> strings = SeperateStrings(CurrentPath, '/');
        if (strings.Count <= 0) { return; }
        if (strings[^1] == "") { strings.RemoveAt(strings.Count - 1); }
        strings.RemoveAt(strings.Count - 1);

        CurrentPath = string.Join("/", strings);
        if (CurrentPath == "")
            return;
        if (!CurrentPath.EndsWith("/"))
            CurrentPath += "/";
    }
#endif
}
