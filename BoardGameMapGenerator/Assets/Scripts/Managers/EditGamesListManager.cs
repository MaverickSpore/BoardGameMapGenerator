using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EditGamesListManager : MonoBehaviour
{
    public static EditGamesListManager instance;

    [SerializeField] TMP_Text SelectableGameTemplate;
    [SerializeField] Button AddGameTemplateButton;
    [SerializeField] Button RemoveGameTemplateButton;
    [SerializeField] RectTransform GamesListContentArea;

    Dictionary<string, string> GameNamesFromFolders;
    List<TMP_Text> GamesList;

    public List<string> BoardCreationGameNames;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        GameNamesFromFolders = new Dictionary<string, string>();
        BoardCreationGameNames = new List<string>();
        GamesList = new List<TMP_Text>();
        this.gameObject.SetActive(false);

    }
    
    public void PressAddMenu()
    {
        PressCancel();
        this.gameObject.SetActive(true);
        AddGameTemplateButton.gameObject.SetActive(true);
        RemoveGameTemplateButton.gameObject.SetActive(false);
        if (LoadGameNames())
        {
            MenuOpened();
        }
    }
    public void PressRemoveMenu()
    {
        PressCancel();
        this.gameObject.SetActive(true);
        AddGameTemplateButton.gameObject.SetActive(false);
        RemoveGameTemplateButton.gameObject.SetActive(true);
        if (PullGameNames())
        {
            MenuOpened();
        }
    }
    private void MenuOpened()
    {
        BoardCreationManager.instance.CollapseAllLists();
        BoardCreationGameNames.Clear();
        foreach (string gameName in BoardCreationManager.instance.GameNamesFromFile)
        {
            BoardCreationGameNames.Add(gameName);
        }
        if (GamesList.Count > 0) { ClearList(); }
        SetupGamesList();
    }


    private void SetupGamesList()
    {
        if (GameNamesFromFolders.Count == 0) { return; }
        float yPosition = SelectableGameTemplate.transform.position.y;

        foreach (string gameName in GameNamesFromFolders.Keys)
        {
            TMP_Text newGame = Instantiate(SelectableGameTemplate, GamesListContentArea.transform);
            newGame.transform.position = new Vector3(newGame.transform.position.x, yPosition, newGame.transform.position.z);
            newGame.text = gameName;
            yPosition -= 75;
            newGame.name = gameName;
            GamesList.Add(newGame);
        }

        SelectableGameTemplate.gameObject.SetActive(false);
    }

    public void PressAddGame(TMP_Text gameName)
    {
        if (GameNamesFromFolders.ContainsKey(gameName.text))
        {
            BoardCreationManager.instance.AddToExpandableLists(GameNamesFromFolders[gameName.text]);
            BoardCreationManager.instance.AddToGameNamesList(GameNamesFromFolders[gameName.text]);
            GameNamesFromFolders.Remove(gameName.text);
            ClearList();
            SetupGamesList();
        }
    }
    public void PressRemoveGame(TMP_Text gameName)
    {
        if (GameNamesFromFolders.ContainsKey(gameName.text))
        {
            BoardCreationManager.instance.GameNamesFromFile.Remove(GameNamesFromFolders[gameName.text]);
            if (!BoardCreationManager.instance.GameNamesFromFile.Contains(GameNamesFromFolders[gameName.text]))
            {
                GameNamesFromFolders.Remove(gameName.text);
            }
            BoardCreationManager.instance.UpdateExpandableLists();
            ClearList();
            SetupGamesList();
        }
    }

    public void PressConfirm()
    {
        BoardCreationManager.instance.UpdateExpandableLists();
        BoardCreationGameNames.Clear();
        this.gameObject.SetActive(false);
    }
    public void PressCancel()
    {
        if (BoardCreationGameNames.Count > 0)
        {
            BoardCreationManager.instance.GameNamesFromFile.Clear();
            //BoardCreationManager.instance.GameNamesFromFile.AddRange(BoardCreationGameNames);
            foreach (string gameName in BoardCreationGameNames)
            {
                BoardCreationManager.instance.GameNamesFromFile.Add(gameName);
            }
        }
        BoardCreationManager.instance.UpdateExpandableLists();
        ClearList();
        GameNamesFromFolders.Clear();
        this.gameObject.SetActive(false);


    }




    bool LoadGameNames()
    {
        GameNamesFromFolders.Clear();
        DirectoryInfo dirInfoPath = new(Application.dataPath + BoardCreationManager.instance.GameNamesFilePath);
        DirectoryInfo[] gameNames = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        if (gameNames.Length == 0) return false;
        foreach (DirectoryInfo gameName in gameNames) {
            if (Directory.Exists(gameName.FullName))
            {
                //if (BoardCreationManager.instance.GameNamesFromFile.Contains(gameName.Name)) { continue; }
                GameNamesFromFolders.Add(BoardCreationManager.instance.GetGameName(gameName.FullName), gameName.Name);
            } 
        };

        return true;
    }
    bool PullGameNames()
    {
        GameNamesFromFolders.Clear();
        //GameNamesFromFolders.Keys.AddRange(BoardCreationManager.instance.GameNamesFromFolders);
        foreach (string gameName in BoardCreationManager.instance.GameNamesFromFile)
        {
            if (GameNamesFromFolders.ContainsKey(BoardCreationManager.instance.GetGameName(Application.dataPath + BoardCreationManager.instance.GameNamesFilePath + "/" + gameName))) { continue; }
            GameNamesFromFolders.Add(BoardCreationManager.instance.GetGameName(Application.dataPath + BoardCreationManager.instance.GameNamesFilePath + "/" + gameName), gameName);
        }
        if (GameNamesFromFolders == null) { return false; }
        return true;
    }
    private void ClearList()
    {
        while (GamesList.Count > 0)
        {
            Destroy(GamesList[0].gameObject);
            GamesList.RemoveAt(0);
        }
        SelectableGameTemplate.gameObject.SetActive(true);
    }
}
