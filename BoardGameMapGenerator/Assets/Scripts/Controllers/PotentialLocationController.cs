using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotentialLocationController : MonoBehaviour
{
    float height;
    float width;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSize(float width, float height)
    {
        this.width = width;
        this.height = height;

        this.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(width, height);
    }

    void LocationSelected()
    {
        if (BoardCreationManager.instance.pieceSelected)
        {

        }
    }
}
