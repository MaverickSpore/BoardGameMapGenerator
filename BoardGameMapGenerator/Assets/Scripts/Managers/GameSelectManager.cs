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

    [SerializeField] TMP_Text noGameAddedText;

    [SerializeField] Image GoodImportImage;
    [SerializeField] Image BadImportImage;
    [SerializeField] TMP_Text BadImportMessage;

    [SerializeField] Button AddGameButton;
    [SerializeField] Button BuildButton;
    [SerializeField] Button PackManagerButton;
    [SerializeField] Button PackCreationButton;

    public List<TMP_Text> gamesAdded;

    int GameSelected;

    
    readonly string GameNamesFilePath = "/GameBoardSets/";
    readonly string GameNamesTextFileName = "SelectedGameNames.txt";
    public Dictionary<string, string> GameNamesFromFolders;
    public List<string> FrontFoldersFromFolders;
    public List<string> SubFoldersFromFolders;


    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        GameSelected = 0;
        gamesAdded = new List<TMP_Text>();
        if (LoadGameNames()) 
        { 
            List<string> gameNames = new();
            foreach (string gameName in GameNamesFromFolders.Keys)
            {
                gameNames.Add(gameName);
            }
            gameNamesDropdown.AddOptions(gameNames); 
        }
        else { print("Game Name Loading Error - Game Names Not Loaded"); }
        GoodImportImage.gameObject.SetActive(false);
        BadImportImage.gameObject.SetActive(false);
    }

    public void AddGameSelected(int selection, string selectionName)
    {
        GameSelected = selection;
        TMP_Text game = IsInGamesAdded(selectionName);
        if (game != null)
        {
            game.GetComponent<AddedGameController>().SetCount(game.GetComponent<AddedGameController>().GetCount() + 1);
            return;
        }
        
        TMP_Text newText = Instantiate(noGameAddedText, gamesAddedContentArea.transform);
        newText.text = selectionName;
        newText.GetComponent<AddedGameController>().ShowButton();


        gamesAdded.Add(newText);

        AdjustGamesList();
    }
    public void AdjustGamesList()
    {
        if (gamesAdded.Count <= 0)
        {
            noGameAddedText.gameObject.SetActive(true);
            return;
        }
        float startingY = noGameAddedText.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;

        foreach (TMP_Text game in gamesAdded)
        {
            game.gameObject.SetActive(true);
            game.GetComponent<RectTransform>().transform.position = new Vector3(noGameAddedText.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            yOffset -= 50;
        }
        gamesAddedContentArea.sizeDelta = new Vector2(gamesAddedContentArea.sizeDelta.x, Mathf.Abs(yOffset) + 25);
        noGameAddedText.gameObject.SetActive(false);
    }
    public void RemoveGameSelected(TMP_Text removedObject)
    {
        int loc = gamesAdded.FindIndex(x => x == removedObject);
        if (gamesAdded[loc].GetComponent<AddedGameController>().GetCount() > 1)
            gamesAdded[loc].GetComponent<AddedGameController>().SetCount(gamesAdded[loc].GetComponent<AddedGameController>().GetCount() - 1);
        else
        {
            gamesAdded.Remove(removedObject);
            Destroy(removedObject.gameObject);
            AdjustGamesList();
        }
    }
    public void PressBuildButton()
    {
        if (gamesAdded.Count <= 0) return;
        if (gamesAdded[0] == noGameAddedText) return;

        string gamesAddedText = "";
        foreach (TMP_Text gameName in gamesAdded)
        {
            for (int i = 0; i < gameName.GetComponent<AddedGameController>().GetCount(); i++)
                gamesAddedText += GameNamesFromFolders[gameName.text] + ",";
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
        gameNamesDropdown.options.Clear();
        GameNamesFromFolders.Clear();
        if (LoadGameNames())
        {
            List<string> gameNames = new();
            foreach (string gameName in GameNamesFromFolders.Keys)
            {
                gameNames.Add(gameName);
            }
            gameNamesDropdown.AddOptions(gameNames);
        }
        else { print("Game Name Loading Error - Game Names Not Loaded"); }

        for(int i = gamesAdded.Count - 1; i >= 0; i--)
        {
            if (!GameNamesFromFolders.ContainsKey(gamesAdded[i].text))
            {
                Destroy(gamesAdded[i].gameObject);
                gamesAdded.RemoveAt(i);
            }
        }
        AdjustGamesList();
    }
    public void PressCreatePack()
    {
        SceneManager.LoadScene(2);
    }

    // File Loading Section
    bool LoadGameNames()
    {
        GameNamesFromFolders = new();
        DirectoryInfo dirInfoPath = new(Application.dataPath + GameNamesFilePath);
        DirectoryInfo[] gameNames = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        if (gameNames.Length == 0) return false;
        foreach (DirectoryInfo gameName in gameNames)
        {
            if (Directory.Exists(gameName.FullName))
            {
                GameNamesFromFolders.Add(GetGameName(gameName.FullName), gameName.Name);
            }
        };

        return true;
    }


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

    private string GetGameName(string gameFolderPath)
    {
        string subConfigString = "";
        FileInfo subConfigFile = new(gameFolderPath + "/" + "cfg");
        if (!subConfigFile.Exists) return "No Config File Found";
        if (subConfigFile.Exists)
        {
            FileStream configFileStream = subConfigFile.OpenRead();
            StreamReader configFileReader = new(configFileStream);
            subConfigString = configFileReader.ReadToEnd();
            configFileReader.Close();
        }

        List<string> subCFGStringsEnter = SeperateStrings(subConfigString);
        if (subCFGStringsEnter.Count <= 0) return "Config File Empty";

        foreach (string stringEnter in subCFGStringsEnter)
        {
            if (stringEnter.Contains("name"))
            {
                List<string> nameStrings = SeperateStrings(stringEnter, '=');
                if (nameStrings.Count <= 0) return "No Names Found";
                if (nameStrings.Count > 1)
                {
                    return nameStrings[1];
                }
            }
        }

        return "End of GetGameName - No Name Found";
    }

    public void PressPackManager()
    {
        if (PackImportManager.instance != null)
        {
            PackImportManager.instance.OpenPackImportManager();
        }
        if (PackDeletionManager.instance != null)
        {
            PackDeletionManager.instance.OpenPackDeletionManager();
        }
        GoodImportImage.gameObject.SetActive(false);
        BadImportImage.gameObject.SetActive(false);

        AddGameButton.interactable = false;
        BuildButton.interactable = false;
        PackManagerButton.interactable = false;
        PackCreationButton.interactable = false;
    }
    public void PressReturnToGameSelect()
    {
        if (PackImportManager.instance != null)
        {
            PackImportManager.instance.gameObject.SetActive(false);
        }
        if (PackDeletionManager.instance != null)
        {
            PackDeletionManager.instance.gameObject.SetActive(false);
        }
        GoodImportImage.gameObject.SetActive(false);
        BadImportImage.gameObject.SetActive(false);

        AddGameButton.interactable = true;
        BuildButton.interactable = true;
        PackManagerButton.interactable = true;
        PackCreationButton.interactable = true;
    }
    public void ShowGoodImport()
    {
        GoodImportImage.gameObject.SetActive(true);
    }
    public void ShowBadImport(string ErrorString = "Unknown Error")
    {
        BadImportImage.gameObject.SetActive(true);
        BadImportMessage.text = ErrorString;
    }

    private TMP_Text IsInGamesAdded(string gameName)
    {
        foreach (TMP_Text game in gamesAdded)
        {
            if (game.text == gameName) return game;
        }
        return null;
    }
}
