using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BoardCreationManager : MonoBehaviour
{

    public static BoardCreationManager instance;
    [Range(1, 200)][SerializeField] float pieceHeight;
    [Range(1, 100)][SerializeField] float textHeight;

    [SerializeField] Button locationMarker;
    [SerializeField] RectTransform CategoriesContentArea;
    [SerializeField] TMP_Text categoriesTemplate;

    

    string GameNamesFilePath = "/GameBoardSets/";
    string GameNamesTextFileName = "SelectedGameNames.txt";
    public List<string> GameNamesFromFolders;
    public List<List<string>> SubFoldersFromFolders;



    List<GameObject> boardPieces = new List<GameObject>();

    public bool pieceSelected;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        pieceSelected = false;

        //Button newMarker = Instantiate(locationMarker, this.transform);
        //newMarker.GetComponent<PotentialLocationController>().SetSize(pieceWidth, pieceHeight);

        if (GetGameNames()) { }
        else { print("Failed to get Game Names from File"); }

        if (LoadGameSubFolders()) { }
        else { print("Failed to get Game Subfolders using Game Names"); }

        SetCategoriesText();
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

    bool LoadGameSubFolders()
    {
        SubFoldersFromFolders = new List<List<string>>();

        foreach (string gameName in GameNamesFromFolders)
        {
            //print("Zip Folder: " + GameNamesFilePath + "/" + gameName.text);
            DirectoryInfo dirInfoPath = new DirectoryInfo(Application.dataPath + GameNamesFilePath + "/" + gameName);
            DirectoryInfo[] gameFolders = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            if (gameFolders.Length == 0) return false;

            List<string> subFoldersList = new List<string>();
            foreach (DirectoryInfo folder in gameFolders) { subFoldersList.Add(folder.Name.Substring(0, folder.Name.Length)); /*print(gameName.Name);*/ };
            SubFoldersFromFolders.Add(subFoldersList);
        }
        //print(dirInfoPath.FullName);


        return true;
    }

    void SetCategoriesText()
    {
        ///// CHANGE TO INDIVIDUAL OBJECTS, LIKE THE GAME SELECT SCREEN /////
        ///// IT WILL MAKE ADDING SELECTABLE IMAGES MUCH EASIER... DUHH /////


        
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
}
