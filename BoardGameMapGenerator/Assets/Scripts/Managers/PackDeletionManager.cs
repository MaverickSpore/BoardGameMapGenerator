using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PackDeletionManager : MonoBehaviour
{
    public static PackDeletionManager instance;

    [SerializeField] RectTransform ContentArea;
    [SerializeField] Button DeletePacksButton;

    [SerializeField] TMP_Text NoFoldersFound;

    List<TMP_Text> FoldersList;

    readonly string GameBoardSetPath = Application.dataPath + "/GameBoardSets/";

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
        this.gameObject.SetActive(false);
        FoldersList = new List<TMP_Text>();
    }


    private void SetupFoldersList()
    {
        DirectoryInfo dirInfoPath = new(GameBoardSetPath);
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
                newFolder.GetComponent<DeleteFolderController>().SetFolder();
                FoldersList.Add(newFolder);
            }
            NoFoldersFound.gameObject.SetActive(false);
        }

        AdjustFoldersList();
    }
    private void ResetFoldersList()
    {
        FoldersList ??= new List<TMP_Text>();
        if (FoldersList.Count > 0)
        {
            foreach (TMP_Text folder in FoldersList)
            {
                Destroy(folder.gameObject);
            }
            FoldersList.Clear();
        }
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
    private void DeleteFolder(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        if (File.Exists(path + ".meta"))
        {
            File.Delete(path + ".meta");
        }
        if (PackImportManager.instance != null)
        {
            PackImportManager.instance.OpenPackImportManager();
        }
    }



    // public methods
    public void OpenPackDeletionManager()
    {
        this.gameObject.SetActive(true);
        ReloadFoldersList();
    }
    public void PressDelete(TMP_Text Folder)
    {
        DeleteFolder(GameBoardSetPath + Folder.text);

        ReloadFoldersList();

        GameSelectManager.instance.PressRefreshGames();
    }
}
