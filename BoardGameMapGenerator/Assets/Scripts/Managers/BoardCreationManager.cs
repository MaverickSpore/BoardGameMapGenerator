using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] RectTransform MapArea;
    [SerializeField] ExpandableListController ExpandableListTemplate;
    [SerializeField] GameObject TilesListStartingLocation;
    [SerializeField] TileObjectPlacedController TileObjectPlacedTemplate;
    [SerializeField] List<ModeButtonController> ModeButtons;



    string GameNamesFilePath = "/GameBoardSets/";
    string GameNamesTextFileName = "SelectedGameNames.txt";
    string MapSaveFilePath = "/SavedMaps/";
    public List<string> GameNamesFromFolders;
    public List<List<string>> SubFoldersFromFolders;


    List<ExpandableListController> GamesList;
    Dictionary<string, ChildTileObjectController> SelectableTiles;


    List<GameObject> boardPieces = new List<GameObject>();

    //public bool pieceSelected;
    ChildTileObjectController selectedPiece;
    TileObjectPlacedController selectedTile;

    bool ableToPlace;
    bool isPlacedTileSelected;
    bool areTilesLocked;

    float gridSize;

    public enum Mode
    {
        Deselect,
        Delete,
        Rotate,
        Move,
        ZForward,
        ZBackward
    }

    Mode currentMode;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //pieceSelected = false;
        isPlacedTileSelected = false;
        GamesList = new List<ExpandableListController>();
        SelectableTiles = new Dictionary<string, ChildTileObjectController>();
        gridSize = 32;

        //Button newMarker = Instantiate(locationMarker, this.transform);
        //newMarker.GetComponent<PotentialLocationController>().SetSize(pieceWidth, pieceHeight);

        if (GetGameNames()) { }
        else { print("Failed to get Game Names from File"); }

        if (SetUpObjectsList()) { }
        else { print("Failed to Set Up Object List"); }

        foreach (ExpandableListController game in GamesList)
        {
            game.PressExpandButton(0);
        }
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
                    // Place Tile On Board
                    Vector3 mousePos = Input.mousePosition;
                    Sprite selectedSprite = selectedPiece.GetImage();

                    if (isPlacedTileSelected == true && selectedTile != null && selectedTile.GetHovered() && selectedSprite == selectedTile.GetSprite())
                        return;
                    //mousePos.x -= (mousePos.x % (selectedSprite.rect.width / 2.0f)) - (selectedSprite.rect.width / 4.0f);
                    //mousePos.y -= (mousePos.y % (selectedSprite.rect.height / 2.0f)) - (selectedSprite.rect.height / 4.0f);
                    mousePos.z = 0;
                    TileObjectPlacedController newPiece = Instantiate(TileObjectPlacedTemplate, mousePos, Quaternion.identity, MapArea.transform);
                    boardPieces.Add(newPiece.gameObject);
                    newPiece.SetImage(selectedPiece.GetImage());
                    newPiece.SetParentTile(selectedPiece);
                    newPiece.AdjustToGrid();
                    //newPiece.transform.position = new Vector3(mousePos.x, mousePos.y, 0);
                    selectedPiece.SetCountText(selectedPiece.GetCurrent() - 1);
                    ChildTileObjectController pairedTile = selectedPiece.GetPairedTile();
                    if (pairedTile != null)
                    {
                        if (pairedTile.GetCurrent() > 0 && selectedPiece.GetCurrent() < pairedTile.GetCurrent())
                        {
                            pairedTile.AddToCurrent(-1);
                            newPiece.SetPairedTile(pairedTile);
                        }
                    }
                    else if (selectedPiece.GetTilesPaired() != null)
                    {
                        // Get total current count from all tiles paired
                        int totalCurrent = 0;
                        foreach (ChildTileObjectController tile in selectedPiece.GetTilesPaired())
                        {
                            totalCurrent += tile.GetCurrent();
                        }
                        if (totalCurrent > 0 && selectedPiece.GetCurrent() < totalCurrent)
                        {
                            foreach (ChildTileObjectController tile in selectedPiece.GetTilesPaired())
                            {
                                if (tile.GetCurrent() > 0)
                                {
                                    tile.AddToCurrent(-1);
                                    newPiece.SetPairedTile(tile);
                                    break;
                                }
                            }
                        }
                    }
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
                        float currentYOffset = 50;
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

                        if (newExpandableList != null && SetUpSubObjects(newExpandableList, gameName)) { result = true; }
                        else { return false; }
                    }
                }
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
        List<string> subCFGStringsPair = new List<string>();
        Dictionary<string, ChildTileObjectController> tileNameDictionary = new Dictionary<string, ChildTileObjectController>();

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

                            tileNameDictionary.Add(tileFile.Name, expandableListParent.GetChildTiles()[expandableListParent.GetChildTiles().Count - 1]);
                            SelectableTiles.Add(tileFile.Name, expandableListParent.GetChildTiles()[expandableListParent.GetChildTiles().Count - 1]);
                            
                        }

                    }

                }
                else if (subCFGString.Contains("pairs"))
                {
                    subCFGStringsPair.Add(subCFGString);
                }
            }
        }

        Dictionary<string, string> tilePairs = new Dictionary<string, string>();
        // Separate the pairs into individual strings using ; and : as delimiters
        foreach (string subCFGString in subCFGStringsPair)
        {
            List<string> subCFGPairStrings = SeperateStrings(subCFGString, '=');
            if (subCFGPairStrings.Count > 0)
            {
                string subCFGPairString = subCFGPairStrings[1];

                List<string> subCFGPairStringsSemi = SeperateStrings(subCFGPairString, ';');
                if (subCFGPairStringsSemi.Count > 0)
                {
                    foreach (string subCFGPairStringSemi in subCFGPairStringsSemi)
                    {
                        // Separate the pair into individual strings using : as delimiter
                        List<string> subCFGPairStringsColon = SeperateStrings(subCFGPairStringSemi, ':');
                        if (subCFGPairStringsColon.Count == 2)
                        {
                            if (tilePairs.ContainsKey(subCFGPairStringsColon[0]))
                            {
                                tilePairs.Add(subCFGPairStringsColon[1], subCFGPairStringsColon[0]);
                            }
                            else
                            {
                                tilePairs.Add(subCFGPairStringsColon[0], subCFGPairStringsColon[1]);
                            }
                        }
                    }
                }
            }
        }

        // Pair the tiles based on the tileNamesDictionary and tilePairs Dictionary
        foreach (KeyValuePair<string, string> tileName in tilePairs)
        {
            if (tileNameDictionary.ContainsKey(tileName.Key) && tileNameDictionary.ContainsKey(tileName.Value))
            {
                tileNameDictionary[tileName.Key].SetPairedTile(tileNameDictionary[tileName.Value]);
                tileNameDictionary[tileName.Value].AddToTilesPaired(tileNameDictionary[tileName.Key]);
            }
        }

        return result;
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
    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
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
                if (subSection == null) continue;

                subSection.SetYOffset(currentSubYOffset);
                currentSubYOffset -= subSection.GetListHeight() + 25;
                contentAreaHeight += subSection.GetListHeight() + 25;
                currentYOffset -= 25;
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
        SetMode(Mode.Deselect);
    }
    public void PressClearSelection()
    {
        selectedPiece = null;
        clearSelection.interactable = false;
        ableToPlace = false;
    }
    public void TileDeleted(TileObjectPlacedController deletedPiece)
    {
        if (deletedPiece == null) return;
        boardPieces.Remove(deletedPiece.gameObject);
    }
    public void PlacedTileDeselected()
    {
        isPlacedTileSelected = false;
        selectedTile = null;
    }
    public void PlacedTileSelected(TileObjectPlacedController selectedTile)
    {
        isPlacedTileSelected = true;
        this.selectedTile = selectedTile;
    }
    public void ToggleTileLock()
    {
        areTilesLocked = !areTilesLocked;
    }

    public void PressNewMap()
    {
        NewMap();
    }
    public void PressSaveMap()
    {
        SaveMap();
    }
    public void PressLoadMap()
    {
        LoadMap();
    }
    public void PressAddPack()
    {
        AddPack();
    }
    public void PressRemovePack()
    {
        RemovePack();
    }


    // Getters
    public float GetGridSize()
    {
        return gridSize;
    }
    public bool GetTilesLocked()
    {
        return areTilesLocked;
    }
    public Mode GetMode()
    {
        return currentMode;
    }

    // Setters
    public void SetMode(Mode mode)
    {
        if (ModeButtons != null && ModeButtons.Count > 0)
        {
            foreach (ModeButtonController button in ModeButtons)
            {
                button.Deselect();
            }
        }

        currentMode = mode;
        if (mode != Mode.Deselect)
            PressClearSelection();

        // In case I want to add more functionality here, per mode
        switch (currentMode)
        {
            case Mode.Deselect:
                break;
            case Mode.Delete:
                break;
            case Mode.Rotate:
                break;
            case Mode.Move:
                break;
            case Mode.ZForward:
                break;
            case Mode.ZBackward:
                break;
        }
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
                if (str.EndsWith("\n"))
                {
                    stringsList.Add(str.Substring(0, str.Length - 1));
                }
                else if (str.EndsWith("\r"))
                {
                    stringsList.Add(str.Substring(0, str.Length - 1));
                }
                else if (str.EndsWith("\r\n"))
                {
                    stringsList.Add(str.Substring(0, str.Length - 2));
                }
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


    // Menu Bar Functions
    private void NewMap()
    {
        while (boardPieces.Count > 0)
        {
            boardPieces[0].GetComponent<TileObjectPlacedController>().PressDeleteTile(); // TODO: Add new tiles to board pieces in LoadMap function
        }
        PressClearSelection();
        SetMode(Mode.Deselect);
    }
    private void SaveMap()
    {
        string path;

#if UNITY_EDITOR
        //path = EditorUtility.SaveFilePanel("Save Map", Application.dataPath + MapSaveFilePath, "Map", "map");
        path = Application.dataPath + MapSaveFilePath + "Map.map";
#else
        //path = MaverickFileExplorer.SaveFilePanel("Save Map", Application.dataPath + MapSaveFilePath, "Map", "map");
        path = Application.dataPath + MapSaveFilePath + "Map.map";
#endif

        if (path.Length != 0)
        {
            // save the information from GamesList and boardPieces to the file using File.WriteAllText
            List<GameObject> GamesListObjectList = new List<GameObject>();
            foreach (ExpandableListController game in GamesList)
            {
                GamesListObjectList.Add(game.gameObject);
            }

            string mapSaveString = string.Empty;
            mapSaveString += "GamesList=";
            mapSaveString += File.ReadAllText(Application.dataPath + GameNamesFilePath + GameNamesTextFileName);
            mapSaveString += "\n";

            // Board Pieces
            mapSaveString += ConvertBoardPiecesToString();
            mapSaveString += "\n";

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.WriteAllText(path, mapSaveString);

            

        }


    }
    private void LoadMap()
    {
        string path;

#if UNITY_EDITOR
        //path = EditorUtility.OpenFilePanel("Load Map", Application.dataPath + MapSaveFilePath, "map");
        path = Application.dataPath + MapSaveFilePath + "Map.map";
#else
        //path = MaverickFileExplorer.SaveFilePanel("Save Map", Application.dataPath + MapSaveFilePath, "map");
        path = Application.dataPath + MapSaveFilePath + "Map.map";
#endif

        if (path.Length != 0)
        {
            NewMap();


            // load the information from the file using File.ReadAllText
            SelectableTiles.Clear();
            File.OpenRead(path);
            string mapLoadString = File.ReadAllText(path);

            List<string> mapLoadStrings = SeperateStrings(mapLoadString);
            foreach (string mapLoad in mapLoadStrings)
            {
                if (mapLoad.Contains("GamesList"))
                {
                    float yOffset = -50;
                    if (GamesList[0] != null)
                    {
                        yOffset = GamesList[0].transform.position.y;
                    }

                    while (GamesList.Count > 0)
                    {
                        Destroy(GamesList[0].gameObject);
                        GamesList.RemoveAt(0);
                    }
                    GamesList = new List<ExpandableListController>();
                    GameNamesFromFolders.Clear();
                    List<string> gamesList = SeperateStrings(mapLoad, '=');
                    if (gamesList.Count > 0)
                    {

                        GameNamesFromFolders.AddRange(SeperateStrings(gamesList[1], ','));// gamesList[1].Split(","));
                        GameNamesFromFolders.RemoveAt(GameNamesFromFolders.Count - 1);
                        if (GameNamesFromFolders.Count > 0)
                        {
                            SetUpObjectsList();
                            //ReloadGamesList();
                        }
                        foreach (ExpandableListController game in GamesList)
                        {
                            game.PressExpandButton(0);
                        }
                        GamesList[0].SetYOffset(yOffset);
                        
                        AdjustsGamesListYOffset();
                    }
                }
                else if (mapLoad.Contains("BoardPieces")) // work on this part now
                {
                    List<string> boardPiecesList = SeperateStrings(mapLoad, '=');
                    if (boardPiecesList.Count > 0)
                    {
                        boardPieces = ConvertStringToBoardPieces(boardPiecesList[1]);

                        if (boardPieces.Count > 0)
                        {
                            ReloadBoardPiecess();
                        }
                    }
                }
            }
        
        }
    }
    private void AddPack()
    {

    }
    private void RemovePack()
    {

    }

    private void ReloadGamesList()
    {
        // place all the games back on the list based on the newly changed GamesList list
        List<ExpandableListController> newGamesList = GamesList;
        GamesList = new List<ExpandableListController>();
        foreach (ExpandableListController game in newGamesList)
        {
            GamesList.Add(Instantiate(game, game.transform.position, game.transform.rotation, CategoriesContentArea.transform));
        }
    }
    private void ReloadBoardPiecess()
    {
        // place all the board pieces back on the board based on the newly changed boardPieces list

        /*List<GameObject> newBoardPieces = boardPieces;
        boardPieces = new List<GameObject>();
        foreach (GameObject piece in newBoardPieces)
        {
            boardPieces.Add(Instantiate(piece, piece.transform.position, piece.transform.rotation, MapArea.transform));
        }*/
    }
    private string ConvertBoardPiecesToString()
    {
        string boardPiecesString = "BoardPieces=";

        foreach (GameObject piece in boardPieces)
        {
            TileObjectPlacedController tile = piece.GetComponent<TileObjectPlacedController>();
            // Add tile's location, sprite, and rotation of the current image using GetCurrentImage to the string
            boardPiecesString += "Location:" + tile.transform.position.x + "," + tile.transform.position.y + "," + tile.transform.position.z + ";";
            boardPiecesString += "Sprite:" + tile.GetTileName() + ";";
            boardPiecesString += "Rotation:" + tile.GetRotation() + ";";
            boardPiecesString += "/";
        }
        return boardPiecesString;
    }
    private List<GameObject> ConvertStringToBoardPieces(string BoardPiecesString)
    {
        List<GameObject> newBoardPieces = new List<GameObject>();

        // for each loop based on BardPiecesString speparated by / delimiter
        List<string> boardPiecesList = SeperateStrings(BoardPiecesString, '/');
        boardPiecesList.RemoveAt(boardPiecesList.Count - 1);
        foreach (string boardPiece in boardPiecesList)
        {
            TileObjectPlacedController newPiece = Instantiate(TileObjectPlacedTemplate, new Vector3(0, 0, 0), Quaternion.identity, MapArea.transform);
            newBoardPieces.Add(newPiece.gameObject);
            // for each loop based on boardPiece separated by ; delimiter
            List<string> boardPieceInfoList = SeperateStrings(boardPiece, ';');
            boardPieceInfoList.RemoveAt(boardPieceInfoList.Count - 1);
            foreach (string pieceInfo in boardPieceInfoList)
            {
                List<string> strings = SeperateStrings(pieceInfo, ':');
                if (strings[0].Contains("Location"))
                {
                    if (strings[1] == null) { continue; }
                    List<string> locationStrings = SeperateStrings(strings[1], ',');
                    if (locationStrings.Count < 3) { continue; }
                    newPiece.transform.position = new Vector3(float.Parse(locationStrings[0]), float.Parse(locationStrings[1]), float.Parse(locationStrings[2]));
                }
                else if (strings[0].Contains("Sprite"))
                {
                    if (strings[1] == null) { continue; }
                    if (SelectableTiles.ContainsKey(strings[1]))
                    {
                        newPiece.SetImage(SelectableTiles[strings[1]].GetImage());
                        newPiece.SetParentTile(SelectableTiles[strings[1]]);
                        SelectableTiles[strings[1]].AddToCurrent(-1);

                        ChildTileObjectController pairedTile = SelectableTiles[strings[1]].GetPairedTile();
                        if (pairedTile != null)
                        {
                            if (pairedTile.GetCurrent() > 0 && SelectableTiles[strings[1]].GetCurrent() < pairedTile.GetCurrent())
                            {
                                pairedTile.AddToCurrent(-1);
                                newPiece.SetPairedTile(pairedTile);
                            }
                        }
                        else if (SelectableTiles[strings[1]].GetTilesPaired() != null)
                        {
                            // Get total current count from all tiles paired
                            int totalCurrent = 0;
                            foreach (ChildTileObjectController tile in SelectableTiles[strings[1]].GetTilesPaired())
                            {
                                totalCurrent += tile.GetCurrent();
                            }
                            if (totalCurrent > 0 && SelectableTiles[strings[1]].GetCurrent() < totalCurrent)
                            {
                                foreach (ChildTileObjectController tile in SelectableTiles[strings[1]].GetTilesPaired())
                                {
                                    if (tile.GetCurrent() > 0)
                                    {
                                        tile.AddToCurrent(-1);
                                        newPiece.SetPairedTile(tile);
                                        break;
                                    }
                                }
                            }
                        }


                    }
                    else
                    {
                        print("Tile Not Found");
                    }
                }
                else if (strings[0].Contains("Rotation"))
                {
                    if (strings[1] == null) { continue; }
                    int rotation = int.Parse(strings[1]);
                    while (rotation > 0)
                    {
                        newPiece.PressRotateCW();
                        rotation -= 90;
                    }
                }
            }
        }

        return newBoardPieces;
    }
}
