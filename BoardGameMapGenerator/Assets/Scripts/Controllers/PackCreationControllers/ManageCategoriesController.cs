using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManageCategoriesController : MonoBehaviour
{

    [SerializeField] Button RemoveCategoryButton;
    [SerializeField] Button ReorderUpButton;
    [SerializeField] Button ReorderDownButton;
    public void SetNeither()
    {
        ReorderUpButton.gameObject.SetActive(false);
        ReorderDownButton.gameObject.SetActive(false);
    }
    public void SetTop()
    {
        ReorderUpButton.gameObject.SetActive(false);
        ReorderDownButton.gameObject.SetActive(true);
    }
    public void SetMiddle()
    {
        ReorderUpButton.gameObject.SetActive(true);
        ReorderDownButton.gameObject.SetActive(true);
    }
    public void SetBottom()
    {
        ReorderUpButton.gameObject.SetActive(true);
        ReorderDownButton.gameObject.SetActive(false);
    }
    public void SetOther()
    {
        RemoveCategoryButton.gameObject.SetActive(false);
    }
    public void SetFolder()
    {
        RemoveCategoryButton.gameObject.SetActive(true);
    }
}
