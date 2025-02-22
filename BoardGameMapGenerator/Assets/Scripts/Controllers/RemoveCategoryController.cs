using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RemoveCategoryController : MonoBehaviour
{
    [SerializeField] Button RemoveCategoryButton;
    public void SetOther()
    {
        RemoveCategoryButton.gameObject.SetActive(false);
    }
    public void SetFolder()
    {
        RemoveCategoryButton.gameObject.SetActive(true);
    }
}
