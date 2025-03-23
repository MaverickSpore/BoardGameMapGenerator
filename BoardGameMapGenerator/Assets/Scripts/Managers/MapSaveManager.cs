using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class MapSaveManager : MonoBehaviour
{
    [Header("Instance")]
    public static MapSaveManager Instance;

    [Header("Menu Items")]
    [SerializeField] RectTransform ContentArea;
    [SerializeField] TMP_Text NoFoldersFound;
    [SerializeField] Image NewMapName;
    [SerializeField] TMP_InputField NewMapInput;
    [SerializeField] Image OverWriteConfirmation;

    List<TMP_Text> foldersList;

    string DefaultMapPath = Application.dataPath + "/Maps/";
    string CurrentPath;

    bool IsLoad;
    string NewMapString = "New Map";
    TMP_Text CurrentOverwriteText;

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
        CurrentPath = DefaultMapPath;
        foldersList = new List<TMP_Text>();
        CloseMenu();
    }



    public void OpenSaveMenu()
    {
        IsLoad = false;
        OpenMenu();
    }
    public void OpenLoadMenu()
    {
        IsLoad = true;
        OpenMenu();
    }

    private void SetupFolderList()
    {
        if (CurrentPath == "")
        {
            GetDrives();
            return;
        }
        DirectoryInfo dirInfoPath = new(CurrentPath);

        if (!dirInfoPath.Exists)
        {
            Directory.CreateDirectory(CurrentPath);
            NoFoldersFound.gameObject.SetActive(true);
            return;
        }

        DirectoryInfo[] folders = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        FileInfo[] mapFiles = dirInfoPath.GetFiles("*.map", SearchOption.TopDirectoryOnly);

        if (folders.Length <= 0 && mapFiles.Length <= 0)
        {
            if (IsLoad)
            {
                NoFoldersFound.gameObject.SetActive(true);
            }
            else
            {
                AddSaveOption();
                AdjustFolderList();
            }
            return;
        }

        if (!IsLoad)
        {
            AddSaveOption();
        }
        foreach (DirectoryInfo folder in folders)
        {
            TMP_Text newFolder = Instantiate(NoFoldersFound, ContentArea.transform);
            newFolder.text = folder.Name;
            newFolder.GetComponent<MapSelectController>().SetFolder();
            foldersList.Add(newFolder);
        }
        foreach (FileInfo mapFile in mapFiles)
        {
            TMP_Text newFolder = Instantiate(NoFoldersFound, ContentArea.transform);
            newFolder.text = mapFile.Name;
            if (IsLoad)
                newFolder.GetComponent<MapSelectController>().SetLoad();
            else
                newFolder.GetComponent<MapSelectController>().SetSave();
            foldersList.Add(newFolder);
        }

        NoFoldersFound.gameObject.SetActive(false);

        AdjustFolderList();
    }
    private void ResetFolderList()
    {
        foreach (TMP_Text folder in foldersList)
        {
            Destroy(folder.gameObject);
        }
        foldersList.Clear();
        NoFoldersFound.gameObject.SetActive(true);
    }
    private void ReloadFolderList()
    {
        ResetFolderList();
        SetupFolderList();
    }
    private void AdjustFolderList()
    {
        if (foldersList.Count <= 0) { return; }
        float startingY = NoFoldersFound.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;
        foreach (TMP_Text folder in foldersList)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            yOffset -= 50;
        }
        ContentArea.sizeDelta = new Vector2(ContentArea.sizeDelta.x, Mathf.Abs(yOffset) + 100);
        NoFoldersFound.gameObject.SetActive(false);
    }

    private void AddSaveOption()
    {
        TMP_Text newFolder = Instantiate(NoFoldersFound, ContentArea.transform);
        newFolder.text = NewMapString; // Will add input textbox later
        newFolder.GetComponent<MapSelectController>().SetSave();
        foldersList.Add(newFolder);
    }

    private void GetDrives()
    {
        ResetFolderList();
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
            TMP_Text newFolder = Instantiate(NoFoldersFound, ContentArea.transform);
            newFolder.text = drive.Name;
            newFolder.GetComponent<MapSelectController>().SetFolder();
            foldersList.Add(newFolder);
        }
        NoFoldersFound.gameObject.SetActive(false);
        AdjustFolderList();
    }
    public void PressExitFolder()
    {
        CurrentPath = ExitFolder(CurrentPath);
        if (CurrentPath == "")
        {
            GetDrives();
            return;
        }
        ReloadFolderList();
    }
    private string ExitFolder(string path)
    {
        string result = "";
        List<string> strings = SeperateStrings(path, '/');
        if (strings.Count <= 0) { return ""; }
        if (strings[^1] == "") { strings.RemoveAt(strings.Count - 1); }
        strings.RemoveAt(strings.Count - 1);

        result = string.Join("/", strings);
        if (result == "")
            return "";
        if (!result.EndsWith("/"))
            result += "/";
        return result;
    }
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

    public void PressOpenFolder(TMP_Text folder)
    {
        CurrentPath += folder.text + "/";
        ReloadFolderList();
    }
    public void PressLoadMap(TMP_Text map)
    {
        if (BoardCreationManager.Instance == null) { return; }
        BoardCreationManager.Instance.LoadMap(CurrentPath + map.text);
        CloseMenu();
    }
    public void PressSaveMap(TMP_Text map = null)
    {
        if (BoardCreationManager.Instance == null) { return; }
        if (map == null || map.text == NewMapString)
        {
            NewMapName.gameObject.SetActive(true);
        }
        else
        {
            CurrentOverwriteText = map;
            OverWriteConfirmation.gameObject.SetActive(true);
            return;
        }
    }
    public void PressConfirmSave()
    {
        if (NewMapInput.text != null)
        {
            BoardCreationManager.Instance.SaveMap(CurrentPath + NewMapInput.text + ".map");
            NewMapInput.text = "";
            CloseMenu();
        }
    }
    public void PressConfirmOverwrite()
    {
        BoardCreationManager.Instance.SaveMap(CurrentPath + CurrentOverwriteText.text);
        CloseMenu();
    }
    public void PressCancelOverwrite()
    {
        OverWriteConfirmation.gameObject.SetActive(false);
    }
    public void PressCancel()
    {
        CloseMenu();
    }

    private void OpenMenu()
    {
        this.gameObject.SetActive(true);
        ReloadFolderList();
    }
    private void CloseMenu()
    {
        NewMapName.gameObject.SetActive(false);
        OverWriteConfirmation.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

}
