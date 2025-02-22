using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class PackCreationManager : MonoBehaviour
{
    [Header("Instance")]
    public static PackCreationManager instance;

    [Header("Images")]
    [SerializeField] Image AddCategoryMenu;
    [SerializeField] Image RemoveCategoryMenu;
    [SerializeField] Image ReorderCategoryMenu;
    [SerializeField] Image ImportImageMenu;
    [SerializeField] Image GeneratePackMenu;
    [SerializeField] Image SetupPairingsMenu;
    [SerializeField] Image PreviewImage;

    [Header("Dropdowns")]
    [SerializeField] TMP_Dropdown CategoryDropdown;
    [SerializeField] TMP_Dropdown TileWidth;
    [SerializeField] TMP_Dropdown TileHeight;

    [Header("Text Blocks")]
    [SerializeField] TMP_Text CurrentTileSize;

    [Header("Input Fields")]
    [SerializeField] TMP_InputField NewCategoryInput;

    [Header("Remove Category Items")]
    [SerializeField] RectTransform ContentAreaRemove;
    [SerializeField] TMP_Text NoCategoriesFoundRemove;

    [Header("Reorder Categories Items")]
    [SerializeField] RectTransform ContentAreaReorder;
    [SerializeField] TMP_Text NoCategoriesFoundReorder;

    [Header("Image Import Items")]
    [SerializeField] RectTransform ContentAreaImageImport;
    [SerializeField] TMP_Text NoFoldersFoundImageImport;

    [Header("Generate Pack Items")]
    [SerializeField] RectTransform ContentAreaGeneratePack;
    [SerializeField] ExpandableListController ExpandableListTemplate;
    [SerializeField] TMP_InputField PackNameInput;

    [Header("Setup Pairings Items")]
    [SerializeField] RectTransform ContentAreaSetupPairings;

    struct ImportedImageInfo
    {
        public string ImagePath;
        public Vector2 size;
        public int maxCount;
    }

    List<TMP_Text> CategoriesListRemove;
    List<TMP_Text> CategoriesListReorder;
    List<TMP_Text> FoldersListImageImport;
    List<ExpandableListController> ObjectListGenerate;
    List<ExpandableListController> ObjectListPairings;
    List<string> CategoriesList;
    List<string> TempCategoriesList;
    Dictionary<string, List<ImportedImageInfo>> CategoriesDictionary;
    Dictionary<string, List<string>> PairedTiles;

    readonly string GameBoardSetPath = Application.dataPath + "/GameBoardSets/";
    string CurrentPath;
    string CurrentImagePath;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        CategoriesListRemove = new List<TMP_Text>();
        CategoriesListReorder = new List<TMP_Text>();
        FoldersListImageImport = new List<TMP_Text>();
        ObjectListGenerate = new List<ExpandableListController>();
        ObjectListPairings = new List<ExpandableListController>();
        CategoriesDictionary = new Dictionary<string, List<ImportedImageInfo>>();
        PairedTiles = new Dictionary<string, List<string>>();
        CategoriesList = new List<string>();
        TempCategoriesList = new List<string>();
        CurrentPath = GameBoardSetPath;

        if (!Directory.Exists(GameBoardSetPath))
        {
            Directory.CreateDirectory(GameBoardSetPath);
        }


        HideMenus();
        PressTileSizeChanged();
    }
    private void HideMenus()
    {
        AddCategoryMenu.gameObject.SetActive(false);
        RemoveCategoryMenu.gameObject.SetActive(false);
        ReorderCategoryMenu.gameObject.SetActive(false);
        ImportImageMenu.gameObject.SetActive(false);
        GeneratePackMenu.gameObject.SetActive(false);
        SetupPairingsMenu.gameObject.SetActive(false);
    }
    private void SetPresetSizes(int w, int h)
    {
        TileWidth.SetValueWithoutNotify(w);
        TileHeight.SetValueWithoutNotify(h);
        SetSize();
    }
    private void SetSize()
    {
        CurrentTileSize.text = TileWidth.captionText.text + " x " + TileHeight.captionText.text;
        PreviewImage.rectTransform.sizeDelta = new Vector2(int.Parse(TileWidth.captionText.text), int.Parse(TileHeight.captionText.text));
    }
    private bool IsInDropdown(TMP_Dropdown dropdown, string option)
    {
        bool result = false;

        foreach(TMP_Dropdown.OptionData data in dropdown.options)
        {
            if (data.text.ToLower() == option.ToLower())
            {
                result = true;
                break;
            }
        }

        return result;
    }
    
    // RemoveCategoryList Methods
    private void SetupCategoriesListRemove()
    {
        if (CategoriesList.Count == 0)
        {
            NoCategoriesFoundRemove.gameObject.SetActive(true);
        }
        else
        {
            foreach (string category in CategoriesList)
            {
                TMP_Text newFolder = Instantiate(NoCategoriesFoundRemove, ContentAreaRemove.transform);
                newFolder.text = category;
                newFolder.GetComponent<RemoveCategoryController>().SetFolder();
                CategoriesListRemove.Add(newFolder);
            }
            NoCategoriesFoundRemove.gameObject.SetActive(false);
        }

        AdjustCategoriesListRemove();
    }
    private void ResetCategoriesListRemove()
    {
        CategoriesListRemove ??= new List<TMP_Text>();
        if (CategoriesListRemove.Count > 0)
        {
            foreach (TMP_Text folder in CategoriesListRemove)
            {
                Destroy(folder.gameObject);
            }
            CategoriesListRemove.Clear();
        }
        NoCategoriesFoundRemove.gameObject.SetActive(true);
    }
    private void ReloadCategoriesListRemove()
    {
        ResetCategoriesListRemove();
        SetupCategoriesListRemove();
    }
    private void AdjustCategoriesListRemove()
    {
        if (CategoriesListRemove.Count <= 0) { return; }
        CategoriesListRemove[0].GetComponent<RectTransform>().transform.position = NoCategoriesFoundRemove.GetComponent<RectTransform>().transform.position;
        float yOffset = CategoriesListRemove[0].GetComponent<RectTransform>().transform.position.y;
        foreach (TMP_Text folder in CategoriesListRemove)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, yOffset);
            yOffset -= 50;//folder.GetComponent<RectTransform>().sizeDelta.y;
        }
        ContentAreaRemove.GetComponent<RectTransform>().sizeDelta = new Vector2(ContentAreaRemove.GetComponent<RectTransform>().sizeDelta.x, 50 * (CategoriesListRemove.Count + 2));
    }

    // ReorderCategoryList Methods
    private void SetupCategoriesListReorder()
    {
        if (CategoriesList.Count == 0)
        {
            NoCategoriesFoundReorder.gameObject.SetActive(true);
        }
        else
        {
            foreach (string category in CategoriesList)
            {
                TMP_Text newFolder = Instantiate(NoCategoriesFoundReorder, ContentAreaReorder.transform);
                newFolder.text = category;
                newFolder.GetComponent<ReorderCategoriesController>().SetBoth();
                CategoriesListReorder.Add(newFolder);
            }
            NoCategoriesFoundReorder.gameObject.SetActive(false);
            if (CategoriesListReorder.Count > 1)
            {
                CategoriesListReorder[0].GetComponent<ReorderCategoriesController>().SetDown();
                CategoriesListReorder[^1].GetComponent<ReorderCategoriesController>().SetUp();
            }
            else if (CategoriesListReorder.Count == 1)
            {
                CategoriesListReorder[0].GetComponent<ReorderCategoriesController>().SetNeither();
            }
        }

        AdjustCategoriesListReorder();
    }
    private void ResetCategoriesListReorder()
    {
        CategoriesListReorder ??= new List<TMP_Text>();
        if (CategoriesListReorder.Count > 0)
        {
            foreach (TMP_Text folder in CategoriesListReorder)
            {
                Destroy(folder.gameObject);
            }
            CategoriesListReorder.Clear();
        }
        NoCategoriesFoundReorder.gameObject.SetActive(true);
    }
    private void ReloadCategoriesListReorder()
    {
        ResetCategoriesListReorder();
        SetupCategoriesListReorder();
    }
    private void AdjustCategoriesListReorder()
    {
        if (CategoriesListReorder.Count <= 0) { return; }
        CategoriesListReorder[0].GetComponent<RectTransform>().transform.position = NoCategoriesFoundReorder.GetComponent<RectTransform>().transform.position;
        float yOffset = CategoriesListReorder[0].GetComponent<RectTransform>().transform.position.y;
        foreach (TMP_Text folder in CategoriesListReorder)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, yOffset);
            yOffset -= 50;//folder.GetComponent<RectTransform>().sizeDelta.y;
        }
        ContentAreaReorder.GetComponent<RectTransform>().sizeDelta = new Vector2(ContentAreaReorder.GetComponent<RectTransform>().sizeDelta.x, 50 * (CategoriesListReorder.Count + 2));
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
        FoldersListImageImport[0].GetComponent<RectTransform>().transform.position = NoFoldersFoundImageImport.GetComponent<RectTransform>().transform.position;
        float yOffset = FoldersListImageImport[0].GetComponent<RectTransform>().transform.position.y;
        foreach (TMP_Text folder in FoldersListImageImport)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, yOffset);
            yOffset -= 50;//folder.GetComponent<RectTransform>().sizeDelta.y;
        }
        ContentAreaImageImport.GetComponent<RectTransform>().sizeDelta = new Vector2(ContentAreaImageImport.GetComponent<RectTransform>().sizeDelta.x, 50 * (FoldersListImageImport.Count + 2));
    }
    private void GetDrivesImageImport()
    {
        ResetFoldersListImageImport();
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
            TMP_Text newFolder = Instantiate(NoFoldersFoundImageImport, ContentAreaImageImport.transform);
            newFolder.text = drive.Name;
            FoldersListImageImport.Add(newFolder);
        }
        NoFoldersFoundImageImport.gameObject.SetActive(false);
        AdjustFoldersListImageImport();
    }
    /*private void ImportImage(string sourcePath, string destinationPath)
    {
        //File.Copy(sourcePath, destinationPath, true);
    }*/
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

    // GeneratePack Methods
    private void SetupCategoriesListGenerate()
    {
        if (CategoriesDictionary.Count == 0)
        {
        }
        else
        {
            foreach(KeyValuePair<string, List<ImportedImageInfo>> category in CategoriesDictionary)
            {
                ExpandableListController newExpList = Instantiate(ExpandableListTemplate, ContentAreaGeneratePack.transform);
                newExpList.gameObject.SetActive(true);
                newExpList.GetComponent<ExpandableListController>().Init();
                newExpList.GetComponent<ExpandableListController>().SetListName(category.Key);
                foreach (ImportedImageInfo imageInfo in category.Value)
                {
                    Texture2D texture = LoadTexture(imageInfo.ImagePath);
                    if (texture == null) { continue; }
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    newExpList.GetComponent<ExpandableListController>().AddChildSprite(ref sprite, imageInfo.size.x, imageInfo.size.y, imageInfo.ImagePath);
                    newExpList.GetComponent<ExpandableListController>().GetChildTiles()[^1].GetComponent<ChildTileObjectController>().SetSize(imageInfo.size);
                    //newExpList.GetComponent<ExpandableListController>().AddListItem(imageInfo.ImagePath, imageInfo.size);
                }

                ObjectListGenerate.Add(newExpList);
            }
        }

        AdjustCategoriesListGenerate();
    }
    private void ResetCategoriesListGenerate()
    {
        ObjectListGenerate ??= new List<ExpandableListController>();
        if (ObjectListGenerate.Count > 0)
        {
            foreach (ExpandableListController expList in ObjectListGenerate)
            {
                Destroy(expList.gameObject);
            }
            ObjectListGenerate.Clear();
        }
    }
    /*private void RecursiveExpandableListDeletion(ExpandableListController expList)
    {
        for (int i = expList.GetChildList().Count - 1; i >= 0; i--)
        {
            RecursiveExpandableListDeletion(expList.GetChildList()[i]);
        }
        for (int i = expList.GetChildTiles().Count - 1; i >= 0; i--)
        {
            if (SelectableTiles.ContainsKey(expList.GetChildTiles()[i].GetSpriteName()))
            {
                SelectableTiles.Remove(expList.GetChildTiles()[i].GetSpriteName());
            }
            Destroy(expList.GetChildTiles()[i].gameObject);
        }
        Destroy(expList.gameObject);
    }*/
    private void ReloadCategoriesListGenerate()
    {
        ResetCategoriesListGenerate();
        SetupCategoriesListGenerate();
    }
    private void AdjustCategoriesListGenerate()
    {
        if (ObjectListGenerate.Count <= 0) { return; }
        //ObjectListGenerate[0].GetComponent<RectTransform>().transform.position = ContentAreaGeneratePack.GetComponent<RectTransform>().transform.position;
        float yOffset = ObjectListGenerate[0].GetComponent<RectTransform>().transform.position.y - 50;
        float contentHeight = 0;
        foreach (ExpandableListController expList in ObjectListGenerate)
        {
            expList.SetYOffset(yOffset);
            //expList.GetComponent<RectTransform>().transform.position = new Vector3(expList.GetComponent<RectTransform>().transform.position.x, yOffset);
            yOffset -= expList.GetListHeight();
            contentHeight += expList.GetListHeight();
        }
        ContentAreaGeneratePack.GetComponent<RectTransform>().sizeDelta = new Vector2(ExpandableListTemplate.GetComponent<RectTransform>().sizeDelta.x, contentHeight);
    }

    // SetupPairings Methods
    private void SetupCategoriesListPairings()
    {
        if (CategoriesDictionary.Count == 0)
        {
        }
        else
        {
            foreach (KeyValuePair<string, List<ImportedImageInfo>> category in CategoriesDictionary)
            {
                ExpandableListController newExpList = Instantiate(ExpandableListTemplate, ContentAreaSetupPairings.transform);
                newExpList.gameObject.SetActive(true);
                newExpList.GetComponent<ExpandableListController>().Init();
                newExpList.GetComponent<ExpandableListController>().SetListName(category.Key);
                foreach (ImportedImageInfo imageInfo in category.Value)
                {
                    Texture2D texture = LoadTexture(imageInfo.ImagePath);
                    if (texture == null) { continue; }
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    newExpList.GetComponent<ExpandableListController>().AddChildSprite(ref sprite, imageInfo.size.x, imageInfo.size.y, imageInfo.ImagePath);
                    newExpList.GetComponent<ExpandableListController>().GetChildTiles()[^1].GetComponent<ChildTileObjectController>().SetSize(imageInfo.size);
                    //newExpList.GetComponent<ExpandableListController>().AddListItem(imageInfo.ImagePath, imageInfo.size);
                }

                ObjectListPairings.Add(newExpList);
            }
        }

        AdjustCategoriesListPairings();
    }
    private void ResetCategoriesListPairings()
    {
        ObjectListPairings ??= new List<ExpandableListController>();
        if (ObjectListPairings.Count > 0)
        {
            foreach (ExpandableListController expList in ObjectListPairings)
            {
                Destroy(expList.gameObject);
            }
            ObjectListPairings.Clear();
        }
    }
    private void ReloadCategoriesListPairings()
    {
        ResetCategoriesListPairings();
        SetupCategoriesListPairings();
    }
    private void AdjustCategoriesListPairings()
    {
        if (ObjectListPairings.Count <= 0) { return; }
        float yOffset = ContentAreaSetupPairings.transform.position.y - 50;
        float contentHeight = 0;
        foreach (ExpandableListController expList in ObjectListPairings)
        {
            if (expList == null) { continue; }
            expList.SetYOffset(yOffset);
            yOffset -= expList.GetListHeight() + 25;
            contentHeight += expList.GetListHeight() + 25;
        }
        ContentAreaSetupPairings.sizeDelta = new Vector2(ContentAreaSetupPairings.sizeDelta.x, contentHeight + 100);
    }









    // Public Methods
    public void UpdateCategories(List<string> categories)
    {
        CategoryDropdown.options.Clear();
        foreach (string category in categories)
        {
            CategoryDropdown.options.Add(new TMP_Dropdown.OptionData(category));
        }
    }
    public void PressRemoveCategory(TMP_Text category)
    {
        if (IsInDropdown(CategoryDropdown, category.text))
        {
            CategoryDropdown.options.Remove(CategoryDropdown.options.Find(x => x.text.ToLower() == category.text.ToLower()));
            CategoryDropdown.RefreshShownValue();
            CategoriesList.Remove(category.text);
            ReloadCategoriesListRemove();
        }
    }
    public void AdjustExpandableListOffsets()
    {
        if (GeneratePackMenu.isActiveAndEnabled)
        {
            AdjustCategoriesListGenerate();
        }
        if (SetupPairingsMenu.isActiveAndEnabled)
        {
            AdjustCategoriesListPairings();
        }
    }


    // Button Presses
    public void PressSelectCategory()
    {

    }
    public void PressAddCategory()
    {
        HideMenus();
        AddCategoryMenu.gameObject.SetActive(true);
    }
    public void PressRemoveCategory()
    {
        HideMenus();
        RemoveCategoryMenu.gameObject.SetActive(true);
        ReloadCategoriesListRemove();
    }
    public void PressReorderCategories()
    {
        HideMenus();
        ReorderCategoryMenu.gameObject.SetActive(true);
        ReloadCategoriesListReorder();
        TempCategoriesList = new List<string>(CategoriesList);
    }
    public void PressGeneratePackButton()
    {
        HideMenus();
        SetupPairingsMenu.gameObject.SetActive(true);
        ReloadCategoriesListPairings();
    }

    public void PressImport()
    {
        HideMenus();
        ImportImageMenu.gameObject.SetActive(true);
        ReloadFoldersListImageImport();
    }
    public void PressAddTile()
    {
        if (string.IsNullOrEmpty(CategoryDropdown.captionText.text)) { return; }
        if (!CategoriesDictionary.ContainsKey(CategoryDropdown.captionText.text))
        {
            ImportedImageInfo imageInfo = new() { ImagePath = CurrentImagePath, size = new Vector2(int.Parse(TileWidth.captionText.text), int.Parse(TileHeight.captionText.text)) };
            List<ImportedImageInfo> imagePaths = new() { imageInfo };
            CategoriesDictionary.Add(CategoryDropdown.captionText.text, imagePaths);
        }
        else
        {
            ImportedImageInfo imageInfo = new() { ImagePath = CurrentImagePath, size = new Vector2(int.Parse(TileWidth.captionText.text), int.Parse(TileHeight.captionText.text)) };
            CategoriesDictionary[CategoryDropdown.captionText.text].Add(imageInfo);
        }


        Dictionary<string, List<ImportedImageInfo>> tempCategoriesDictionary = new();

        foreach (string category in CategoriesList)
        {
            if (CategoriesDictionary.ContainsKey(category))
            {
                tempCategoriesDictionary.Add(category, CategoriesDictionary[category]);
            }
        }
        CategoriesDictionary = new Dictionary<string, List<ImportedImageInfo>>(tempCategoriesDictionary);






    }
    public void PressRemoveTile()
    {

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
        if (CategoryDropdown.options.Contains(new TMP_Dropdown.OptionData(NewCategoryInput.text))) { return; }
        if (IsInDropdown(CategoryDropdown, NewCategoryInput.text)) { return; }
        CategoryDropdown.options.Add(new TMP_Dropdown.OptionData(NewCategoryInput.text));
        if (CategoryDropdown.options.Count > 1)
            CategoryDropdown.SetValueWithoutNotify(CategoryDropdown.options.Count - 1);
        else
            CategoryDropdown.SetValueWithoutNotify(0);
        CategoryDropdown.RefreshShownValue();
        CategoriesList.Add(NewCategoryInput.text);
    }
    public void PressConfirmReorder()
    {
        TempCategoriesList = new List<string>(CategoriesList);
        Dictionary<string, List<ImportedImageInfo>> tempCategoriesDictionary = new();

        CategoryDropdown.options.Clear();
        foreach (string category in CategoriesList)
        {
            CategoryDropdown.options.Add(new TMP_Dropdown.OptionData(category));
            if (CategoriesDictionary.ContainsKey(category))
            {
                tempCategoriesDictionary.Add(category, CategoriesDictionary[category]);
            }
        }
        CategoryDropdown.RefreshShownValue();
        CategoriesDictionary = new Dictionary<string, List<ImportedImageInfo>>(tempCategoriesDictionary);

        HideMenus();
    }
    public void PressCancelReorder()
    {
        CategoriesList = new List<string>(TempCategoriesList);
        ReloadCategoriesListReorder();
        HideMenus();
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
        ReloadFoldersListImageImport();
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
    public void PressExitFolder()
    {
        List<string> strings = SeperateStrings(CurrentPath, '/');
        if (strings.Count <= 0) { return; }
        if (strings[^1] == "") { strings.RemoveAt(strings.Count - 1); }
        strings.RemoveAt(strings.Count - 1);

        CurrentPath = string.Join("/", strings);

        if (CurrentPath == "")
        {
            GetDrivesImageImport();
            return;
        }

        if (!CurrentPath.EndsWith("/"))
            CurrentPath += "/";

        ReloadFoldersListImageImport();
    }
    public void PressReorderUp(TMP_Text Category)
    {
        int index = CategoriesList.FindIndex(x => x == Category.text);
        if (index == 0) { return; }
        (CategoriesList[index - 1], CategoriesList[index]) = (CategoriesList[index], CategoriesList[index - 1]);
        ReloadCategoriesListReorder();
    }
    public void PressReorderDown(TMP_Text Category)
    {
        int index = CategoriesList.FindIndex(x => x == Category.text);
        if (index == CategoriesList.Count - 1) { return; }
        (CategoriesList[index + 1], CategoriesList[index]) = (CategoriesList[index], CategoriesList[index + 1]);
        ReloadCategoriesListReorder();
    }
    public void PressConfirmSetupPairings()
    {
        HideMenus();
        GeneratePackMenu.gameObject.SetActive(true);
        ReloadCategoriesListGenerate();
    }
    public void PressCancelSetupPairings()
    {
        PairedTiles.Clear();
        HideMenus();
    }
    public void PressConfirmGeneratePack()
    {
        if (string.IsNullOrEmpty(PackNameInput.text)) { return; }
        if (CategoriesDictionary == null) { return; }
        if (CategoriesDictionary.Count == 0) { return; }
        string PackPath = GameBoardSetPath + "/" + PackNameInput.text;
        if (Directory.Exists(PackPath))
        {
            Directory.Delete(PackPath, true);
        }
        if (!Directory.Exists(PackPath))
        {
            Directory.CreateDirectory(PackPath);
        }
        int itemIndex = 0;
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
        }
        HideMenus();
    }
    public void PressCancelGeneratePack()
    {
        HideMenus();
    }


    public void PressReturn()
    {
        SceneManager.LoadScene(0);
    }


    // Helper Methods
    List<string> SeperateStrings(string fullString, char delimiter = '\n')
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
}
