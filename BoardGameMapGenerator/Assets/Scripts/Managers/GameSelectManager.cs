using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSelectManager : MonoBehaviour
{
    public static GameSelectManager instance;

    [SerializeField] TMP_Dropdown gameNamesDropdown;
    [SerializeField] RectTransform gamesAddedContentArea;

    [Range(1,100)][SerializeField] float addedGamesHeight;
    [SerializeField] TMP_Text noGameAddedText;
    public List<TMP_Text> gamesAdded;
    TMP_Text toCompare;

    int GameSelected;

    
    string GameNamesFilePath = "/GameBoardSets/";
    string GameNamesTextFileName = "SelectedGameNames.txt";
    public List<string> GameNamesFromFolders;
    public List<string> FrontFoldersFromFolders;
    public List<string> SubFoldersFromFolders;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        GameSelected = 0;
        gamesAdded = new List<TMP_Text>() { noGameAddedText };
        gamesAdded[0].GetComponent<AddedGameController>().HideButton();
        if (LoadGameNames()) 
        { 
            gameNamesDropdown.AddOptions(GameNamesFromFolders); 
        }
        else { print("Game Name Loading Error - Game Names Not Loaded"); }
    }

    public void AddGameSelected(int selection, string selectionName)
    {
        if (gamesAdded.Count == 1)
        {
            noGameAddedText.GetComponent<AddedGameController>().ShowButton();
            noGameAddedText.gameObject.SetActive(false);
            gamesAdded.Remove(noGameAddedText);
        }

        GameSelected = selection;
        TMP_Text newText = Instantiate(noGameAddedText, gamesAddedContentArea.transform);
        newText.gameObject.SetActive(true);
        
        newText.transform.position = new Vector3(newText.transform.position.x, newText.transform.position.x - (addedGamesHeight * gamesAdded.Count) - 75 + gamesAddedContentArea.localPosition.y, newText.transform.position.z);
        newText.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(250, addedGamesHeight);
        gamesAddedContentArea.sizeDelta = new Vector2(gamesAddedContentArea.sizeDelta.x, (addedGamesHeight * gamesAdded.Count) + 100);
        newText.text = selectionName;


        gamesAdded.Add(newText);

    }
    public void RemoveGameSelected(TMP_Text removedObject)
    {
        toCompare = removedObject;
        int loc = gamesAdded.FindIndex(IsMatch);
        gamesAdded.Remove(removedObject);
        Destroy(removedObject.gameObject);
        for (int i = loc; i < gamesAdded.Count; i++)
        {
            gamesAdded[i].transform.position = new Vector3(gamesAdded[i].transform.position.x, gamesAdded[i].transform.position.y + addedGamesHeight, gamesAdded[i].transform.position.z);
        }
        gamesAddedContentArea.sizeDelta = new Vector2(gamesAddedContentArea.sizeDelta.x, (addedGamesHeight * gamesAdded.Count) + 100);
        if (gamesAdded.Count <= 0)
        {
            noGameAddedText.gameObject.SetActive(true);
            noGameAddedText.GetComponent<AddedGameController>().HideButton();
            gamesAdded.Add(noGameAddedText);
        }
    }
    bool IsMatch(TMP_Text compare)
    {
        return compare == toCompare;
    }
    public int GetGameSelected()
    {
        return GameSelected;
    }
    public void PressBuildButton()
    {
        if (gamesAdded[0] == noGameAddedText) return;

        string gamesAddedText = "";
        foreach (TMP_Text gameName in gamesAdded)
        {
            gamesAddedText += gameName.text + ",";
        }

        File.WriteAllText(Application.dataPath + GameNamesFilePath + GameNamesTextFileName, gamesAddedText);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void PressAddGame()
    {
        AddGameSelected(gameNamesDropdown.value, gameNamesDropdown.options[gameNamesDropdown.value].text);
    }

    public void PressRefreshGames()
    {
        //gameNamesDropdown.ClearOptions();
        gameNamesDropdown.options.Clear();
        GameNamesFromFolders.Clear();
        if (LoadGameNames()) { gameNamesDropdown.AddOptions(GameNamesFromFolders); }
        else { print("Game Name Loading Error - Game Names Not Loaded"); }
    }


    // File Loading Section
    bool LoadGameNames()
    {
        GameNamesFromFolders = new List<string>();
        DirectoryInfo dirInfoPath = new DirectoryInfo(Application.dataPath + GameNamesFilePath);
        DirectoryInfo[] gameNames = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        if (gameNames.Length == 0) return false;
        foreach (DirectoryInfo gameName in gameNames) { if (Directory.Exists(gameName.FullName)) { GameNamesFromFolders.Add(gameName.Name.Substring(0, gameName.Name.Length)); /*print(gameName.Name);*/ } };

        return true;
    }
    public void PressTestFeature()
    {
        if (gamesAdded.Count <= 0 || gamesAdded[0] == noGameAddedText) return;
        if (LoadGameSubFolders()) { }
        else { print("Game Subfolders Loading Error - Game Subfolders Not Loaded"); }
    }
    bool LoadGameSubFolders()
    {
        FrontFoldersFromFolders = new List<string>();
        SubFoldersFromFolders = new List<string>();
        foreach (TMP_Text gameName in gamesAdded)
        {
            if (gameName == noGameAddedText) continue;
            //print("Zip Folder: " + GameNamesFilePath + "/" + gameName.text);
            DirectoryInfo dirInfoPath = new DirectoryInfo(Application.dataPath + GameNamesFilePath + "/" + gameName.text);
            DirectoryInfo[] gameFolders = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
            if (gameFolders.Length == 0) return false;

            foreach (DirectoryInfo folder in gameFolders) { SubFoldersFromFolders.Add(folder.Name.Substring(0, folder.Name.Length)); /*print(gameName.Name);*/ };
        }
        //print(dirInfoPath.FullName);


        return true;
    }

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
