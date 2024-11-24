using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropDownController : MonoBehaviour
{
    public void SetManagerSelection()
    {
        GameSelectManager.instance.SetGameSelected(this.gameObject.GetComponent<TMP_Dropdown>().value);
    }
}
