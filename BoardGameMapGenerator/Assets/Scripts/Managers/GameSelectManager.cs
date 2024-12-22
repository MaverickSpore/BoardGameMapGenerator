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
    List<string> GameNamesFromFolders;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        GameSelected = 0;
        gamesAdded = new List<TMP_Text>() { noGameAddedText };
        gamesAdded[0].GetComponent<AddedGameController>().HideButton();
        if (LoadGameNames()) { gameNamesDropdown.AddOptions(GameNamesFromFolders); }
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
        SceneManager.LoadScene(GameSelected + 1);
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
        //print(dirInfoPath.FullName);
        FileInfo[] gameNames = dirInfoPath.GetFiles("*.zip", SearchOption.AllDirectories);
        
        if (gameNames.Length == 0) return false;
        foreach (FileInfo gameName in gameNames) { GameNamesFromFolders.Add(gameName.Name.Substring(0,gameName.Name.Length - 4)); /*print(gameName.Name);*/ };

        return true;
    }
}
