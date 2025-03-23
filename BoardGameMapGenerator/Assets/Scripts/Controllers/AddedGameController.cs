using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddedGameController : MonoBehaviour
{
    [SerializeField] Button removeButton;
    [SerializeField] TMP_Text CountText;

    public void ShowButton()
    {
        removeButton.gameObject.SetActive(true);
    }
    public void SetCount(int count)
    {
        CountText.gameObject.SetActive(true);
        CountText.text = "x" + count.ToString();
    }
    public int GetCount()
    {
        string count = CountText.text;
        count = count.Substring(1);
        return int.Parse(count);
    }
}
