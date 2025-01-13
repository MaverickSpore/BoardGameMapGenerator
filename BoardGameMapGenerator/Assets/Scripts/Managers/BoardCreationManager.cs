using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardCreationManager : MonoBehaviour
{

    public static BoardCreationManager instance;
    [Range(1, 200)][SerializeField] float pieceHeight;
    [Range(1, 100)][SerializeField] float textHeight;

    [SerializeField] Canvas mainCanvas;
    [SerializeField] Button locationMarker;
    [SerializeField] Button clearSelection;
    [SerializeField] RectTransform CategoriesContentArea;
    [SerializeField] TMP_Text categoriesTemplate;
    [SerializeField] ExpandableListController ExpandableListTemplate;
    [SerializeField] GameObject TilesListStartingLocation;
    [SerializeField] TileObjectPlacedController TileObjectPlacedTemplate;

    

    string GameNamesFilePath = "/GameBoardSets/";
    string GameNamesTextFileName = "SelectedGameNames.txt";
    public List<string> GameNamesFromFolders;
    public List<List<string>> SubFoldersFromFolders;


    List<ExpandableListController> GamesList;


    List<GameObject> boardPieces = new List<GameObject>();

    //public bool pieceSelected;
    ChildTileObjectController selectedPiece;

    bool ableToPlace;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //pieceSelected = false;
        GamesList = new List<ExpandableListController>();

        //Button newMarker = Instantiate(locationMarker, this.transform);
        //newMarker.GetComponent<PotentialLocationController>().SetSize(pieceWidth, pieceHeight);

        if (GetGameNames()) { }
        else { print("Failed to get Game Names from File"); }

        if (SetUpObjectsList()) { }
        else { print("Failed to Set Up Object List"); }


        //SetCategoriesText();
    }
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (selectedPiece != null && selectedPiece.GetCurrent() > 0)
            {
                if (!ableToPlace)
                {
                    ableToPlace = true;
                }
                else
                {
                    Vector3 mousePos = Input.mousePosition;
                    Sprite selectedSprite = selectedPiece.GetImage();
                    mousePos.x -= mousePos.x % selectedSprite.rect.width;
                    mousePos.y -= mousePos.y % selectedSprite.rect.width;
                    mousePos.z = 0;
                    TileObjectPlacedController newPiece = Instantiate(TileObjectPlacedTemplate, mousePos, Quaternion.identity, mainCanvas.transform);
                    boardPieces.Add(newPiece.gameObject);
                    newPiece.SetImage(selectedPiece.GetImage());
                    //newPiece.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
                    selectedPiece.SetCountText(selectedPiece.GetCurrent() - 1);
                    if (selectedPiece.GetCurrent() <= 0)
                    {
                        PressClearSelection();
                    }
                }
            }
        }
    }
    bool GetGameNames()
    { 
        GameNamesFromFolders = new List<string>();
        string gameNamesText = File.ReadAllText(Application.dataPath + GameNamesFilePath + GameNamesTextFileName);
        if (gameNamesText.Length <= 0) { return false; }

        GameNamesFromFolders.AddRange(gameNamesText.Split(","));
        GameNamesFromFolders.RemoveAt(GameNamesFromFolders.Count - 1);

        return true;
    }


    bool SetUpObjectsList()
    {
        bool result = true;


        foreach (string gameName in GameNamesFromFolders)
        {
            DirectoryInfo dirInfoPath = new DirectoryInfo(Application.dataPath + GameNamesFilePath + "/" + gameName + "/");
            FileInfo[] gameCFG = dirInfoPath.GetFiles("cfg", SearchOption.TopDirectoryOnly);

            if (gameCFG.Length != 1)
            {
                return false;
            }

            // Get cfg String from folder
            string configString = string.Empty;
            FileInfo configFile = new FileInfo(Application.dataPath + GameNamesFilePath + "/" + gameName + "/" + "cfg");
            if (configFile.Exists)
            {
                FileStream configFileStream = configFile.OpenRead();
                StreamReader configFileReader = new StreamReader(configFileStream);
                configString = configFileReader.ReadToEnd();

                //print(configFile.FullName + ": " + configString);
            }
            // cfg String from folder has been gotten

            List<string> CFGStringsEnter = SeperateStrings(configString);
            //ExpandableListController newExpandableListOld = null;

            if (CFGStringsEnter.Count > 0)
            {
                //print("Each CFG String: ");
                foreach (string CFGString in CFGStringsEnter)
                {
                    //print("\t" + testCFGString);

                    if (CFGString.Contains("name"))
                    {
                        List<string> CFGNameStrings = SeperateStrings(CFGString, '=');
                        if (CFGNameStrings[1] == null) { continue; };
                        ExpandableListController newExpandableList = Instantiate(ExpandableListTemplate, TilesListStartingLocation.transform);
                        newExpandableList.SetListName(CFGNameStrings[1]);
                        newExpandableList.Init();
                        float currentYOffset = -50;
                        if (GamesList.Count > 0)
                        {
                            currentYOffset = GamesList[0].transform.position.y;
                        }
                        foreach (ExpandableListController game in GamesList)
                        {
                            currentYOffset += game.GetListHeight();
                        }
                        newExpandableList.SetYOffset(currentYOffset);
                        GamesList.Add(newExpandableList);

                        /*// Seeing if SetUpSubObject needs to not be separate

                        DirectoryInfo subDirInfoPath = new DirectoryInfo(Application.dataPath + GameNamesFilePath + "/" + gameName + "/");
                        DirectoryInfo[] subFoldersDirInfo = subDirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
                        if (subFoldersDirInfo.Length == 0) return false;
                        // subFoldersDirInfo should be all subfolders: Tiles, Doors, Cars, Other Tokens


                        //float yOffset = 0;
                        foreach (DirectoryInfo subFolder in subFoldersDirInfo)
                        {



                            FileInfo[] subGameCFG = subFolder.GetFiles("cfg", SearchOption.TopDirectoryOnly);

                            if (subGameCFG.Length != 1)
                            {
                                return false;
                            }

                            // Get cfg String from folder
                            string subConfigString = string.Empty;
                            FileInfo subConfigFile = new FileInfo(subFolder.FullName + "/" + "cfg");
                            if (subConfigFile.Exists)
                            {
                                FileStream configFileStream = subConfigFile.OpenRead();
                                StreamReader configFileReader = new StreamReader(configFileStream);
                                subConfigString = configFileReader.ReadToEnd();

                                //print(configFile.FullName + ": " + configString);
                            }
                            // cfg String from folder has been gotten

                            List<string> subCFGStringsEnter = SeperateStrings(subConfigString);

                            if (subCFGStringsEnter.Count > 0)
                            {
                                //print("Each CFG String: ");
                                foreach (string subCFGString in subCFGStringsEnter)
                                {
                                    //print("\t" + testCFGString);

                                    if (subCFGString.Contains("name"))
                                    {
                                        List<string> subCFGNameStrings = SeperateStrings(subCFGString, '=');
                                        if (subCFGNameStrings[1] == null) { continue; };
                                        ExpandableListController newSubExpandableList = Instantiate(ExpandableListTemplate, newExpandableList.GetContentArea().transform);
                                        newSubExpandableList.Init();
                                        //yOffset = newExpandableList.GetListHeight();
                                        newSubExpandableList.AddYOffset(newExpandableList.GetListHeight());
                                        //print("YOffset: " + newExpandableList.GetListHeight());
                                        newExpandableList.AddChildList(ref newSubExpandableList);
                                        newSubExpandableList.SetListName(subCFGNameStrings[1]);

                                    }
                                }
                                //print("String Ended");
                            }




                        }

                        // End of SetUpSubObjects */

                        if (newExpandableList != null && SetUpSubObjects(newExpandableList, gameName)) { result = true; }
                        else { return false; }
                    }

                    /*
                    List<string> testCFGStringsEq = SeperateStrings(testCFGString, '=');
                    if (testCFGStringsEq.Count > 0)
                    {
                        print("\t\tEach CFG SubString: ");
                        foreach (string testCFGSubString in testCFGStringsEq)
                        {
                            print("\t\t\t" + testCFGSubString);
                        }
                        print("\t\tSubString Ended");
                    }
                    */
                }
                //print("String Ended");
            }

        }

        return result;
    }
    bool SetUpSubObjects(ExpandableListController expandableListParent, string gameName = "Default Game Name")
    {
        bool result = true;

        if (expandableListParent == null)
        {
            print("List Parent Was NULL!!");
            return false;
        }



        DirectoryInfo subDirInfoPath = new DirectoryInfo(Application.dataPath + GameNamesFilePath + "/" + gameName + "/");
        DirectoryInfo[] subFoldersDirInfo = subDirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        if (subFoldersDirInfo.Length == 0) return false;
        // subFoldersDirInfo should be all subfolders: Tiles, Doors, Cars, Other Tokens


        //float yOffset = 0;
        foreach (DirectoryInfo subFolder in subFoldersDirInfo)
        {



            FileInfo[] subGameCFG = subFolder.GetFiles("cfg", SearchOption.TopDirectoryOnly);

            if (subGameCFG.Length != 1)
            {
                return false;
            }

            // Get cfg String from folder
            string subConfigString = string.Empty;
            FileInfo subConfigFile = new FileInfo(subFolder.FullName + "/" + "cfg");
            if (subConfigFile.Exists)
            {
                FileStream configFileStream = subConfigFile.OpenRead();
                StreamReader configFileReader = new StreamReader(configFileStream);
                subConfigString = configFileReader.ReadToEnd();

                //print(configFile.FullName + ": " + configString);
            }
            // cfg String from folder has been gotten

            List<string> subCFGStringsEnter = SeperateStrings(subConfigString);

            if (subCFGStringsEnter.Count > 0)
            {
                //print("Each CFG String: ");
                foreach (string subCFGString in subCFGStringsEnter)
                {
                    //print("\t" + testCFGString);

                    if (subCFGString.Contains("name"))
                    {
                        List<string> subCFGNameStrings = SeperateStrings(subCFGString, '=');
                        if (subCFGNameStrings[1] == null) { continue; };
                        ExpandableListController newSubExpandableList = Instantiate(ExpandableListTemplate, expandableListParent.GetContentArea().transform);
                        newSubExpandableList.Init();
                        //yOffset = newExpandableList.GetListHeight();
                        newSubExpandableList.SetYOffset(expandableListParent.transform.position.y - expandableListParent.GetListHeight());
                        //print("YOffset: " + newExpandableList.GetListHeight());
                        expandableListParent.AddChildList(ref newSubExpandableList);
                        newSubExpandableList.SetListName(subCFGNameStrings[1]);

                        if (SetUpTilesInSubList(newSubExpandableList, subFolder)) { }
                        else { print("Tiles In Sub List Not Set Up"); return false; }

                    }
                }
                //print("String Ended");
            }




        }



        return result;
    }
    bool SetUpTilesInSubList(ExpandableListController expandableListParent, DirectoryInfo subFolder)
    {
        bool result = true;

        if (expandableListParent == null)
            return false;
        if (subFolder == null)
            return false;


        FileInfo[] subGameCFG = subFolder.GetFiles("cfg", SearchOption.TopDirectoryOnly);

        if (subGameCFG.Length != 1)
        {
            return false;
        }

        // Get cfg String from folder
        string subConfigString = string.Empty;
        FileInfo subConfigFile = new FileInfo(subFolder.FullName + "/" + "cfg");
        if (subConfigFile.Exists)
        {
            FileStream configFileStream = subConfigFile.OpenRead();
            StreamReader configFileReader = new StreamReader(configFileStream);
            subConfigString = configFileReader.ReadToEnd();
        }
        // cfg String from folder has been gotten

        List<string> subCFGStringsEnter = SeperateStrings(subConfigString);

        if (subCFGStringsEnter.Count > 0)
        {
            foreach (string subCFGString in subCFGStringsEnter)
            {
                if (subCFGString.Contains("max"))
                {
                    List<string> subCFGMaxStrings = SeperateStrings(subCFGString, '=');
                    if (subCFGMaxStrings[1] == null) { continue; };

                    List<string> maxStrings = SeperateStrings(subCFGMaxStrings[1], ';');

                    foreach(string maxString in maxStrings)
                    {
                        List<string> tileCombo = SeperateStrings(maxString, ':');
                        // tileCombo[0] = tile file name, tileCombo[1] = tile max count

                        //print("Folder: " + subFolder.FullName);
                        //print("Tile File Name: " + tileCombo[0]);

                        if (tileCombo.Count < 2 || tileCombo[0] == null) { continue; }

                        FileInfo[] tileFiles = subFolder.GetFiles(tileCombo[0], SearchOption.TopDirectoryOnly);
                        if (tileFiles.Length != 1)
                        {
                            print("This tile has the incorrect amount of files associated.");
                            return false;
                        }

                        foreach (FileInfo tileFile in tileFiles)
                        {
                            Sprite newSprite;
                            Texture2D spriteTexture = LoadTexture(tileFile.FullName);
                            newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0));
                            
                            expandableListParent.AddChildSprite(ref newSprite, tileCombo[1], tileFile.Name);
                            
                        }

                    }

                }
            }
        }






        return result;
    }
    


    bool LoadGameSubFoldersOld()
    {
        SubFoldersFromFolders = new List<List<string>>();

        foreach (string gameName in GameNamesFromFolders)
        {
            //print("Zip Folder: " + GameNamesFilePath + "/" + gameName.text);
            DirectoryInfo dirInfoPath = new DirectoryInfo(Application.dataPath + GameNamesFilePath + "/" + gameName);
            DirectoryInfo[] gameFolders = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            if (gameFolders.Length == 0) return false;

            // Get cfg String from folder
            string configString = string.Empty;
            FileInfo configFile = new FileInfo(Application.dataPath + GameNamesFilePath + "/" + gameName + "/" + "cfg");
            if (configFile.Exists)
            {
                FileStream configFileStream = configFile.OpenRead();
                StreamReader configFileReader = new StreamReader(configFileStream);
                configString = configFileReader.ReadToEnd();

                //print(configFile.FullName + ": " + configString);
            }
            // cfg String from folder has been gotten

            List<string> testCFGStringsEnter = SeperateStrings(configString);

            if (testCFGStringsEnter.Count > 0)
            {
                print("Each CFG String: ");
                foreach (string testCFGString in testCFGStringsEnter)
                {
                    print("\t" + testCFGString);
                    List<string> testCFGStringsEq = SeperateStrings(testCFGString, '=');
                    if (testCFGStringsEq.Count > 0)
                    {
                        print("\t\tEach CFG SubString: ");
                        foreach (string testCFGSubString in testCFGStringsEq)
                        {
                            print("\t\t\t" + testCFGSubString);
                        }
                        print("\t\tSubString Ended");
                    }
                }
                print("String Ended");
            }



            List<string> subFoldersList = new List<string>();
            foreach (DirectoryInfo folder in gameFolders) { subFoldersList.Add(folder.Name.Substring(0, folder.Name.Length)); /*print(gameName.Name);*/ };
            SubFoldersFromFolders.Add(subFoldersList);
        }
        //print(dirInfoPath.FullName);


        return true;
    }
    void SetCategoriesTextOld()
    {
        ///// CHANGE TO INDIVIDUAL OBJECTS, LIKE THE GAME SELECT SCREEN /////
        ///// IT WILL MAKE ADDING SELECTABLE IMAGES MUCH EASIER... DUHH /////
        /////           ^^  I believe I already did this  ^^            /////


        
        /*

        */
        categoriesTemplate.text = string.Empty;
        categoriesTemplate.text = categoriesTemplate.text + "\n";
        float subFoldersCount = 1;
        float contentYTotal = 0;

            categoriesTemplate.GetComponent<CategoryController>().HidePiece();
            categoriesTemplate.gameObject.SetActive(false);

        for (int i = 0; i < GameNamesFromFolders.Count; i++)
        {
            //categoriesTemplate.text = categoriesTemplate.text + "--" + GameNamesFromFolders[i] + "--\n";

            TMP_Text newText = Instantiate(categoriesTemplate, CategoriesContentArea.transform);
            newText.gameObject.SetActive(true);

            contentYTotal -= textHeight;

            newText.transform.position = new Vector3(newText.transform.position.x, contentYTotal, newText.transform.position.z);
            newText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(250, textHeight);
            CategoriesContentArea.sizeDelta = new Vector2(CategoriesContentArea.sizeDelta.x, (contentYTotal) + 100);
            newText.text = GameNamesFromFolders[i];

            foreach (string category in SubFoldersFromFolders[i])
            {
                //categoriesTemplate.text = categoriesTemplate.text + "-" + category + "-\n";

                TMP_Text newSubText = Instantiate(categoriesTemplate, CategoriesContentArea.transform);
                newSubText.gameObject.SetActive(true);

                contentYTotal -= textHeight;

                newSubText.transform.position = new Vector3(newSubText.transform.position.x, contentYTotal, newSubText.transform.position.z);
                newSubText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(250, textHeight);
                CategoriesContentArea.sizeDelta = new Vector2(CategoriesContentArea.sizeDelta.x, (contentYTotal) + 100);
                newSubText.text = category;

                List<Sprite> subObjects = LoadObjectsInSubFolder(GameNamesFromFolders[i], category);

                if (subObjects.Count > 0) { }
                else { continue; }

                contentYTotal -= pieceHeight;
                foreach (Sprite subObject in subObjects)
                {
                    TMP_Text newSubObject = Instantiate(categoriesTemplate, CategoriesContentArea.transform);
                    newSubObject.gameObject.SetActive(true);

                    contentYTotal -= subObject.texture.height;

                    newSubObject.GetComponent<CategoryController>().ShowPiece();
                    newSubObject.GetComponent<CategoryController>().SetImageSprite(subObject);

                    newSubObject.transform.position = new Vector3(newSubObject.transform.position.x, contentYTotal + (subObject.texture.height / 2.0f), newSubObject.transform.position.z);
                    newSubObject.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(subObject.texture.width, subObject.texture.height);
                    CategoriesContentArea.sizeDelta = new Vector2(CategoriesContentArea.sizeDelta.x, (contentYTotal) + 100);
                    //newSubObject.text = category;
                    contentYTotal -= textHeight;
                }



                subFoldersCount++;
            }
            //categoriesTemplate.text = categoriesTemplate.text + "\n";
            subFoldersCount += 1;
        }
        //CategoriesContentArea.sizeDelta = new Vector2(CategoriesContentArea.sizeDelta.x, 40 * (subFoldersCount + GameNamesFromFolders.Count));
        CategoriesContentArea.sizeDelta = new Vector2(CategoriesContentArea.sizeDelta.x, -contentYTotal);
        /*
        */
    }
    
    
    List<Sprite> LoadObjectsInSubFolder(string Game, string Category)
    {
        List<Sprite> result = new List<Sprite>();

        DirectoryInfo dirInfoPath = new DirectoryInfo(Application.dataPath + GameNamesFilePath + "/" + Game + "/" + Category + "/");
        FileInfo[] folderObjects = dirInfoPath.GetFiles("*.jpg", SearchOption.AllDirectories);
        if (folderObjects.Length == 0) return new List<Sprite>();

        foreach (FileInfo folderObject in folderObjects) 
        {
            Sprite newSprite;
            Texture2D spriteTexture = LoadTexture(folderObject.FullName);
            newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0));
            result.Add(newSprite); 
            /*print(gameName.Name);*/ 
        };
        
        return result;
    }
    Texture2D LoadTexture(string FilePath)
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










    // Public Functions

    public void AdjustsGamesListYOffset()
    {
        if (GamesList.Count <= 0) return;

        float currentYOffset = GamesList[0].transform.position.y;
        float contentAreaHeight = 0;

        foreach (ExpandableListController game in GamesList)
        {
            game.SetYOffset(currentYOffset);

            float currentSubYOffset = currentYOffset - game.GetTextHeight();
            foreach (ExpandableListController subSection in game.GetChildList())
            {
                subSection.SetYOffset(currentSubYOffset);
                currentSubYOffset -= subSection.GetListHeight();
                contentAreaHeight += subSection.GetListHeight();
            }

            currentYOffset -= game.GetListHeight();
            contentAreaHeight += game.GetTextHeight();
        }

        CategoriesContentArea.sizeDelta = new Vector2(CategoriesContentArea.sizeDelta.x, contentAreaHeight + 100);
    }

    public void SetPieceSelected(ChildTileObjectController selected)
    {
        selectedPiece = selected;
        clearSelection.interactable = true;
        ableToPlace = false;
    }
    public void PressClearSelection()
    {
        selectedPiece = null;
        clearSelection.interactable = false;
        ableToPlace = false;
    }




    // Helper Fucntions

    List<string> SeperateStrings(string fullString, char delimiter = '\n')
    {
        if (string.IsNullOrEmpty(fullString))
        {
            return new List<string>();
        }

        List<string> stringsList = new List<string>();

        bool hasDelim = fullString.Contains(delimiter);
        if (hasDelim)
        {
            string[] tempStringArr = fullString.Split(delimiter);
            foreach (string str in tempStringArr)
            {
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
