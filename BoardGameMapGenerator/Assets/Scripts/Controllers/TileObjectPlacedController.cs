using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TileObjectPlacedController : MonoBehaviour
{
    [SerializeField] Image currentImage;
    [SerializeField] Button testVariable;

    int testInt;
    public void SetImage(Image newImage)
    {
        currentImage = newImage;
    }
}
