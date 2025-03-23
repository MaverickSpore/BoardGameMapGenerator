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
    public static PackImportManager instance;

    [SerializeField] RectTransform ContentArea;
    [SerializeField] Button ImportZipButton;

    [SerializeField] TMP_Text NoFoldersFound;

    List<TMP_Text> FoldersList;

    public string CurrentPath;
    readonly string GameBoardSetPath = Application.dataPath + "/GameBoardSets/";

    private enum ErrorCode
    {
        SUCCESS = 1,
        NOFOLDER = 2,
        NOZIP = 3,
        MULTIPLECFG = 403,
        NOCFG = 404,
        CFGNOGAMENAME = 405,
        CFGNONAME = 406,
        CFGBADMAX = 407,
        CFGBADPAIRS = 408
    }

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
            NoFoldersFound.gameObject.SetActive(false);
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
            NoFoldersFound.gameObject.SetActive(false);
        }
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
        float startingY = NoFoldersFound.GetComponent<RectTransform>().transform.position.y;
        float yOffset = 0;
        foreach (TMP_Text folder in FoldersList)
        {
            folder.GetComponent<RectTransform>().transform.position = new Vector3(folder.GetComponent<RectTransform>().transform.position.x, startingY + yOffset);
            yOffset -= 50;
        }
        ContentArea.sizeDelta = new Vector2(ContentArea.sizeDelta.x, Mathf.Abs(yOffset) - NoFoldersFound.GetComponent<RectTransform>().transform.localPosition.y);
    }
    private void GetDrives()
    {
        ResetFoldersList();
        DriveInfo[] allDrives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in allDrives)
        {
            TMP_Text newFolder = Instantiate(NoFoldersFound, ContentArea.transform);
            newFolder.text = drive.Name;
            newFolder.GetComponent<ImportFolderController>().SetFolder();
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

        string masterFolder = archive.Entries[0].FullName;
        masterFolder = GetParentFolder(destinationPath) + masterFolder;

        while (GetParentFolder(masterFolder) != GameBoardSetPath)
        {
            masterFolder = GetParentFolder(masterFolder);
            if (masterFolder == GameBoardSetPath) { return; }
            if (masterFolder.Length <= GameBoardSetPath.Length) { return; }
        }

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
        }
        fileStream.Close();
        ErrorCode result = IsBuildableFolder(masterFolder);
        if (result == ErrorCode.SUCCESS)
        {
            // Pop up "Good Pack" message
            if (GameSelectManager.instance != null)
            {
                GameSelectManager.instance.ShowGoodImport();
            }
        }
        else
        {
            // Pop up "Bad Pack" message
            if (Directory.Exists(masterFolder))
            {
                Directory.Delete(masterFolder, true);
            }
            if (File.Exists(masterFolder[..^1] + ".meta"))
            {
                File.Delete(masterFolder[..^1] + ".meta");
            }
            if (GameSelectManager.instance != null)
            {
                string ErrorString = "";

                switch (result)
                {
                    case ErrorCode.NOFOLDER:
                        ErrorString = "NO FOLDERS FOUND";
                        break;
                    case ErrorCode.NOZIP:
                        ErrorString = "NO ZIP FILE FOUND";
                        break;
                    case ErrorCode.NOCFG:
                        ErrorString = "CFG FILE MISSING - EVERY FOLDER REQUIRES A CFG FILE";
                        break;
                    case ErrorCode.MULTIPLECFG:
                        ErrorString = "MULTIPLE CFG FILES FOUND IN SINGLE FOLDER";
                        break;
                    case ErrorCode.CFGNOGAMENAME:
                        ErrorString = "MAIN CFG FILE REQUIRES GAME NAME";
                        break;
                    case ErrorCode.CFGNONAME:
                        ErrorString = "ALL CFG FILES REQUIRE CATEGORY NAME";
                        break;
                    case ErrorCode.CFGBADMAX:
                        ErrorString = "CFG FILE - MAX VALUES FORMATTED INCORRECTLY";
                        break;
                    case ErrorCode.CFGBADPAIRS:
                        ErrorString = "CFG FILE - PAIRS VALUES FORMATTED INCORRECTLY";
                        break;
                }

                ErrorString += "\nPlease Use Pack Creator For An Easy Buildable Pack";


                GameSelectManager.instance.ShowBadImport(ErrorString);
            }
        }
    }
    private ErrorCode IsBuildableFolder(string zipPath)
    {
        if (!Directory.Exists(zipPath))
            return ErrorCode.NOZIP;
        DirectoryInfo dirInfoPath = new(zipPath);
        FileInfo[] cfgFiles = dirInfoPath.GetFiles("cfg", SearchOption.TopDirectoryOnly);
        if (cfgFiles.Length > 1)
            return ErrorCode.MULTIPLECFG;
        if (cfgFiles.Length == 0)
            return ErrorCode.NOCFG;

        // Check cfg for name section
        string configString = "";
        FileInfo configFileInfo = new(zipPath + "/" + "cfg");
        if (!configFileInfo.Exists) return ErrorCode.NOCFG;
        if (configFileInfo.Exists)
        {
            FileStream configFileStream = configFileInfo.OpenRead();
            StreamReader configFileReader = new(configFileStream);
            configString = configFileReader.ReadToEnd();
            configFileReader.Close();
        }
        List<string> cfgStrings = SeperateStrings(configString, '\n');
        bool hasGameName = false;
        
        foreach (string cfgString in cfgStrings)
        {
            if (cfgString.Contains("name="))
            {
                hasGameName = true;
                List<string> nameSplit = SeperateStrings(cfgString,'=');
                if (nameSplit.Count != 2)
                    return ErrorCode.CFGNONAME;
                if (string.IsNullOrEmpty(nameSplit[1]))
                    return ErrorCode.CFGNONAME;
            }
        }
        if (!hasGameName)
            return ErrorCode.CFGNOGAMENAME;

        DirectoryInfo[] folders = dirInfoPath.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        if (folders.Length == 0)
            return ErrorCode.NOFOLDER;

        // Check each folder for cfg files
        foreach (DirectoryInfo folder in folders)
        {
            FileInfo[] cfgFilesInFolder = folder.GetFiles("cfg", SearchOption.TopDirectoryOnly);
            if (cfgFilesInFolder.Length > 1)
                return ErrorCode.MULTIPLECFG;
            if (cfgFilesInFolder.Length == 0)
                return ErrorCode.NOCFG;

            // Check cfg file for sections
            string subConfigString = "";
            FileInfo subConfigFile = new(folder.FullName + "/" + "cfg");
            if (!subConfigFile.Exists) return ErrorCode.NOCFG;
            if (subConfigFile.Exists)
            {
                FileStream configFileStream = subConfigFile.OpenRead();
                StreamReader configFileReader = new(configFileStream);
                subConfigString = configFileReader.ReadToEnd();
                configFileReader.Close();
            }
            List<string> cfgSubStrings = SeperateStrings(subConfigString, '\n');
            bool hasName = false;
            bool hasMax = false;
            bool hasPairs = false;
            foreach (string cfgString in cfgSubStrings)
            {
                // Check for name
                if (cfgString.Contains("name="))
                {
                    hasName = true;
                    List<string> nameSplit = SeperateStrings(cfgString,'=');
                    if (nameSplit.Count != 2)
                        return ErrorCode.CFGNONAME;
                    if (string.IsNullOrEmpty(nameSplit[1]))
                        return ErrorCode.CFGNONAME;
                }
                // Check for max
                if (cfgString.Contains("max="))
                {
                    hasMax = true;
                    List<string> maxSplit = SeperateStrings(cfgString,'=');
                    if (maxSplit.Count != 2)
                        return ErrorCode.CFGBADMAX;
                    if (string.IsNullOrEmpty(maxSplit[1]))
                        return ErrorCode.CFGBADMAX;
                    List<string> maxStrings = SeperateStrings(maxSplit[1], ';');
                    // Check for each maxStrings having 2 values
                    foreach (string maxString in maxStrings)
                    {
                        if (string.IsNullOrEmpty(maxString))
                            continue;
                        List<string> maxValues = SeperateStrings(maxString, ':');
                        if (maxValues.Count != 2)
                            return ErrorCode.CFGBADMAX;
                        if (string.IsNullOrEmpty(maxValues[0]) || string.IsNullOrEmpty(maxValues[1]))
                            return ErrorCode.CFGBADMAX;
                        // Check if maxValues[1] is an integer
                        if (!int.TryParse(maxValues[1], out _))
                            return ErrorCode.CFGBADMAX;
                    }
                }
                // Check for pairs
                if (cfgString.Contains("pairs="))
                {
                    hasPairs = true;
                    List<string> pairsSplit = SeperateStrings(cfgString,'=');
                    if (pairsSplit.Count != 2)
                        return ErrorCode.CFGBADPAIRS;
                    if (string.IsNullOrEmpty(pairsSplit[1]))
                        return ErrorCode.CFGBADPAIRS;
                    List<string> pairsStrings = SeperateStrings(pairsSplit[1], ';');
                    // Check for each pairsStrings having 2 values
                    foreach (string pairsString in pairsStrings)
                    {
                        if (string.IsNullOrEmpty(pairsString))
                            continue;
                        List<string> pairsValues = SeperateStrings(pairsString, ':');
                        if (pairsValues.Count != 2)
                            return ErrorCode.CFGBADPAIRS;
                        if (string.IsNullOrEmpty(pairsValues[0]) || string.IsNullOrEmpty(pairsValues[1]))
                            return ErrorCode.CFGBADPAIRS;
                    }
                }
            }
            if (!hasName)
                return ErrorCode.CFGNONAME;
            if (!hasMax)
                return ErrorCode.CFGBADMAX;
            if (!hasPairs)
                return ErrorCode.CFGBADPAIRS;
        }
        // for later user feedback
        return ErrorCode.SUCCESS;
    }
    
    // public methods
    public void OpenPackImportManager()
    {
        this.gameObject.SetActive(true);
        ReloadFoldersList();
    }
    public void PressReturnToSelect()
    {
        print("PackImportManager - PressReturnToSelect Has Been Used");
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
        if (PackDeletionManager.instance != null)
        {
            PackDeletionManager.instance.OpenPackDeletionManager();
        }

        GameSelectManager.instance.PressRefreshGames();
    }
    public void PressExitFolder()
    {
        CurrentPath = GetParentFolder(CurrentPath);

        if (CurrentPath == "" || CurrentPath == "/")
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
