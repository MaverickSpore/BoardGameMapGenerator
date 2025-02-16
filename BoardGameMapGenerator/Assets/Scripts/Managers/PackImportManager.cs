using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PackImportManager : MonoBehaviour
{
    public PackImportManager instance;

    [SerializeField] RectTransform ContentArea;
    [SerializeField] Button ImportZipButton;

    [SerializeField] TMP_Text NoFoldersFound;

    List<TMP_Text> FoldersList;

    public string CurrentPath;
    string GameBoardSetPath = Application.dataPath + "/GameBoardSets/";

    private void Start()
    {
        instance = this;
        this.gameObject.SetActive(false);
        FoldersList = new List<TMP_Text>();
        CurrentPath = GameBoardSetPath;
    }

    private void SetupFoldersList()
    {
        DirectoryInfo dirInfoPath = new(CurrentPath);
        DirectoryInfo[] folders = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        if (folders.Length == 0)
        {
            NoFoldersFound.gameObject.SetActive(true);
        }
        else
        {
            foreach (DirectoryInfo folder in folders)
            {
                TMP_Text newFolder = Instantiate(NoFoldersFound, ContentArea.transform);
                newFolder.text = folder.Name;
                newFolder.GetComponent<ImportFolderController>().SetFolder();
                FoldersList.Add(newFolder);
            }

        }
        DirectoryInfo dirInfoZipPath = new(CurrentPath);
        FileInfo[] zipFolders = dirInfoZipPath.GetFiles("*.zip", SearchOption.TopDirectoryOnly);

        if (zipFolders.Length > 0)
        {
            foreach (FileInfo folder in zipFolders)
            {
                TMP_Text newFolder = Instantiate(NoFoldersFound, ContentArea.transform);
                newFolder.text = folder.Name;
                newFolder.GetComponent<ImportFolderController>().SetZip();
                FoldersList.Add(newFolder);
            }
        }


        NoFoldersFound.gameObject.SetActive(false);
        AdjustFoldersList();
    }
    private void ResetFoldersList()
    {
        foreach (TMP_Text folder in FoldersList)
        {
            Destroy(folder.gameObject);
        }
        FoldersList.Clear();
        NoFoldersFound.gameObject.SetActive(true);
    }
    private void ReloadFoldersList()
    {
        ResetFoldersList();
        SetupFoldersList();
    }
    private void AdjustFoldersList()
    {
        if (FoldersList.Count <= 0) { return; }
        FoldersList[0].GetComponent<RectTransform>().transform.position = NoFoldersFound.GetComponent<RectTransform>().transform.position;
        float yOffset = FoldersList[0].GetComponent<RectTransform>().transform.position.y;
        foreach(TMP_Text folder in FoldersList)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, yOffset);
            yOffset -= 50;//folder.GetComponent<RectTransform>().sizeDelta.y;
        }
        ContentArea.GetComponent<RectTransform>().sizeDelta = new Vector2(ContentArea.GetComponent<RectTransform>().sizeDelta.x, 50 * (FoldersList.Count + 2));
    }
    private void GetDrives()
    {
        ResetFoldersList();
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
            TMP_Text newFolder = Instantiate(NoFoldersFound, ContentArea.transform);
            newFolder.text = drive.Name;
            FoldersList.Add(newFolder);
        }
        NoFoldersFound.gameObject.SetActive(false);
        AdjustFoldersList();
    }
    private void ImportZipFile(string zipPath, string destinationPath)
    {
        FileStream fileStream = new(zipPath, FileMode.Open, FileAccess.Read);
        if (fileStream == null) { return; }
        ZipArchive archive = new(fileStream, ZipArchiveMode.Read);
        if (archive == null) { return; }
        
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string entryPath = GetParentFolder(destinationPath) + entry.FullName;
            if (entryPath.EndsWith("/"))
            {
                Directory.CreateDirectory(entryPath);
                continue;
            }
            if (File.Exists(entryPath))
            {
                File.Delete(entryPath);
            }
            entry.ExtractToFile(entryPath);
            if (IsBuildableFolder(entryPath))
            {
                // Add to GameBoardSets
            }
            else
            {
                // Delete entryPath File
                if (File.Exists(entryPath))
                {
                    File.Delete(entryPath);
                }
            }
        }
        fileStream.Close();
        
    }
    private bool IsBuildableFolder(string zipPath)
    {
        // for later user feedback
        return true;
    }
    
    // public methods
    public void PressImportPack()
    {
        this.gameObject.SetActive(true);
        ReloadFoldersList();
    }
    public void PressReturnToSelect()
    {
        this.gameObject.SetActive(false);
        GameSelectManager.instance.PressRefreshGames();
    }
    public void PressOpenFolder(TMP_Text Folder)
    {
        CurrentPath += Folder.text + "/";
        ReloadFoldersList();
    }
    public void PressImportZip(TMP_Text Folder)
    {
        // Import Zip Folder To GameBoardSets Folder
        string zipPath = CurrentPath + Folder.text;
        string destinationPath = GameBoardSetPath + Folder.text;

        ImportZipFile(zipPath, destinationPath);

        ReloadFoldersList();

        GameSelectManager.instance.PressRefreshGames();
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
            GetDrives();
            return;
        }

        if (!CurrentPath.EndsWith("/"))
            CurrentPath += "/";
        
        ReloadFoldersList();
    }



    // Helper Fucntions

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
    private string GetParentFolder(string path)
    {
        string result;
        List<string> strings = SeperateStrings(path, '/');
        if (strings.Count <= 0) { return ""; }
        if (strings[^1] == "") { strings.RemoveAt(strings.Count - 1); }
        strings.RemoveAt(strings.Count - 1);

        result = string.Join("/", strings);
        if (!result.EndsWith("/"))
            result += "/";

        return result;
    }
}
