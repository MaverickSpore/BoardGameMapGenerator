using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardCreationManager : MonoBehaviour
{

    public static BoardCreationManager instance;
    [SerializeField] float pieceWidth;
    [SerializeField] float pieceHeight;

    [SerializeField] Button locationMarker;


    List<GameObject> boardPieces = new List<GameObject>();

    public bool pieceSelected;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        pieceSelected = false;

        Button newMarker = Instantiate(locationMarker, this.transform);
        newMarker.GetComponent<PotentialLocationController>().SetSize(pieceWidth, pieceHeight);
    }
}
