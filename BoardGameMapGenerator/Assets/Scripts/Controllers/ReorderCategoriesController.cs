using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReorderCategoriesController : MonoBehaviour
{
    [SerializeField] Button ReorderUpButton;
    [SerializeField] Button ReorderDownButton;
    public void SetNeither()
    {
        ReorderUpButton.gameObject.SetActive(false);
        ReorderDownButton.gameObject.SetActive(false);
    }
    public void SetBoth()
    {
        ReorderUpButton.gameObject.SetActive(true);
        ReorderDownButton.gameObject.SetActive(true);
    }
    public void SetUp()
    {
        ReorderUpButton.gameObject.SetActive(true);
        ReorderDownButton.gameObject.SetActive(false);
    }
    public void SetDown()
    {
        ReorderUpButton.gameObject.SetActive(false);
        ReorderDownButton.gameObject.SetActive(true);
    }
}
