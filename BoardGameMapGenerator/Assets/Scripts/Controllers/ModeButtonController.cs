using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModeButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image Border;
    [SerializeField] BoardCreationManager.Mode mode;
    [SerializeField] string ModeMessage;

    // Start is called before the first frame update
    void Start()
    {
        Deselect();
    }
    public void PressModeButton()
    {
        BoardCreationManager.Instance.SetMode(mode);
        if (mode == BoardCreationManager.Mode.Deselect)
            return;
        Selected();
    }

    public void Deselect()
    {
        if (Border == null)
            return;
        GetComponent<RectTransform>().sizeDelta = Border.GetComponent<RectTransform>().sizeDelta;
        Border.color = Color.clear;
    }

    private void Selected()
    {
        Vector2 size = Border.GetComponent<RectTransform>().sizeDelta;
        size.Scale(new Vector2(0.9f, 0.9f));
        GetComponent<RectTransform>().sizeDelta = size;
        Border.color = Color.white;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        BoardCreationManager.Instance.DisplayModeMessage(ModeMessage);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        BoardCreationManager.Instance.DisplayModeMessage("");
    }
}
