using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModeButtonController : MonoBehaviour
{
    [SerializeField] Image Border;
    [SerializeField] BoardCreationManager.Mode mode;

    // Start is called before the first frame update
    void Start()
    {
        Deselect();
    }
    public void PressModeButton()
    {
        BoardCreationManager.instance.SetMode(mode);
        if (mode == BoardCreationManager.Mode.Deselect)
            return;
        Selected();
    }

    public void Deselect()
    {
        GetComponent<RectTransform>().sizeDelta = Border.GetComponent<RectTransform>().sizeDelta;
        //Border.color = Color.white;
        Border.color = Color.clear;
    }

    private void Selected()
    {
        Vector2 size = Border.GetComponent<RectTransform>().sizeDelta;
        size.Scale(new Vector2(0.9f, 0.9f));
        GetComponent<RectTransform>().sizeDelta = size;
        Border.color = Color.green;
    }
}
