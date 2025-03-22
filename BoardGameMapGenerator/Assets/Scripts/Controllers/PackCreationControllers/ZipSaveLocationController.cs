using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZipSaveLocationController : MonoBehaviour
{
    [SerializeField] Button OpenButton;

    public void SetFolder()
    {
        OpenButton.gameObject.SetActive(true);
    }
}
