using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectController : MonoBehaviour
{
    [SerializeField] Button OpenButton;
    [SerializeField] Button SaveButton;
    [SerializeField] Button LoadButton;

    public void SetFolder()
    {
        SaveButton.gameObject.SetActive(false);
        LoadButton.gameObject.SetActive(false);
        OpenButton.gameObject.SetActive(true);
    }
    public void SetSave()
    {
        OpenButton.gameObject.SetActive(false);
        LoadButton.gameObject.SetActive(false);
        SaveButton.gameObject.SetActive(true);
    }
    public void SetLoad()
    {
        OpenButton.gameObject.SetActive(false);
        SaveButton.gameObject.SetActive(false);
        LoadButton.gameObject.SetActive(true);
    }
}
