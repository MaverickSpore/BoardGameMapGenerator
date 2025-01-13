using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpandableListController : MonoBehaviour
{
    // Set this up to work similarly to the MapEditor Expandable List

    [SerializeField] TMP_Text expandableListText;
    [SerializeField] Button expandButton;
    [SerializeField] TMP_Text expandButtonText;
    [SerializeField] GameObject content;
    [SerializeField] ChildTileObjectController tileTemplate;

    List<ExpandableListController> childLists;
    List<ChildTileObjectController> childTiles;

    public float listHeight;

    void Start()
    {
        if (expandButton == null) { print("Expand Button is NULL"); }
        if (expandButtonText == null) { print("Expand Button Text is NULL"); }
        if (content == null) { print("Expandable List Content is NULL"); }


        listHeight = expandButton.GetComponent<RectTransform>().rect.size.y;
    }
    public void Init()
    {
        childLists = new List<ExpandableListController>();
        childTiles = new List<ChildTileObjectController>();
        PressExpandButton();
    }
    public void PressExpandButton()
    {
        if (expandButtonText.text == ">")
        {
            expandButtonText.text = "v";

            BoardCreationManager.instance.AdjustsGamesListYOffset();
            ExpandContent();
        }
        else
        {
            expandButtonText.text = ">";
            CollapseContent();

            BoardCreationManager.instance.AdjustsGamesListYOffset();
        }
    }
    public GameObject GetContentArea()
    {
        return content;
    }
    public void SetListName(string name)
    {
        expandableListText.text = name;
    }
    public string GetListName() 
    { 
        return expandableListText.text; 
    }
    public void SetYOffset(float offset)
    {
        gameObject.transform.position = new Vector3 (gameObject.transform.position.x, offset, gameObject.transform.position.z);
    }
    public float GetTextHeight()
    {
        return expandableListText.GetComponent<RectTransform>().rect.size.y;
    }
    public float GetListHeight()
    {
        listHeight = expandButton.GetComponent<RectTransform>().rect.size.y;
        if (!(childLists == null || childLists.Count == 0 || expandButtonText.text == ">"))
        {
            //print("ChildLists was NOT NULL");
            foreach (ExpandableListController child in childLists)
            {
                listHeight += child.GetListHeight();
            }
        }
        if (!(childTiles == null || childTiles.Count == 0 || expandButtonText.text == ">"))
        {
            foreach (ChildTileObjectController tile in childTiles)
            {
                listHeight += tile.GetHeight();
            }
        }
        return listHeight;
    }

    public void AddChildList(ref ExpandableListController child)
    {
        childLists.Add(child);
    }
    public void RemoveChildList(ref ExpandableListController child)
    {
        childLists.Remove(child);
    }
    public List<ExpandableListController> GetChildList()
    {
        return childLists;
    }

    public void AddChildSprite(ref Sprite childSprite, string tileCount, string spriteName = "DefaultName")
    {
        ChildTileObjectController newTile = Instantiate(tileTemplate, content.transform);
        newTile.SetImage(childSprite, spriteName);
        newTile.SetYOffset(transform.position.y - GetListHeight());

        int count = int.Parse(tileCount);
        newTile.SetCountText(count, count);

        AddChildTile(ref newTile);
    }
    public void RemoveChildSprite(ref Sprite childSprite)
    {

    }

    void AddChildTile(ref ChildTileObjectController childTile)
    {
        childTiles.Add(childTile);
        BoardCreationManager.instance.AdjustsGamesListYOffset();
    }
    void RemoveChildTile(ref ChildTileObjectController childTile)
    {
        childTiles.Remove(childTile);
    }



    void ExpandContent()
    {
        //print("Content Expanded");

        content.SetActive(true);
    }

    void CollapseContent()
    {
        //print("Content Collapsed");

        content.SetActive(false);
    }

}
