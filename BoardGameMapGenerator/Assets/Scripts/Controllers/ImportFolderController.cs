using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImportFolderController : MonoBehaviour
{
    [SerializeField] Button ImportButton;
    [SerializeField] Button OpenButton;

    public void SetZip()
    {
        ImportButton.gameObject.SetActive(true);
        OpenButton.gameObject.SetActive(false);
    }
    public void SetFolder()
    {
        ImportButton.gameObject.SetActive(false);
        OpenButton.gameObject.SetActive(true);
    }
}
