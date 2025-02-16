using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteFolderController : MonoBehaviour
{
    [SerializeField] Button DeleteButton;
    public void SetOther()
    {
        DeleteButton.gameObject.SetActive(false);
    }
    public void SetFolder()
    {
        DeleteButton.gameObject.SetActive(true);
    }
}
