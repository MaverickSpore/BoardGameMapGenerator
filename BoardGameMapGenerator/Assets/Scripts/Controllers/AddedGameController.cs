using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddedGameController : MonoBehaviour
{
    [SerializeField] Button removeButton;

    public void HideButton()
    {
        removeButton.gameObject.SetActive(false);
    }
    public void ShowButton()
    {
        removeButton.gameObject.SetActive(true);
    }
}
