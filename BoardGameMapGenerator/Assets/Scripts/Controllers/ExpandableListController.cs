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
    [SerializeField] TMP_Text ListCountText;

    List<ExpandableListController> childLists;
    List<ChildTileObjectController> childTiles;

    public float listHeight;
    string FileName;
    int ListCount;

    void Start()
    {
        ListCount = 1;
        if (expandButton == null) { print("Expand Button is NULL"); }
        if (expandButtonText == null) { print("Expand Button Text is NULL"); }
        if (content == null) { print("Expandable List Content is NULL"); }


        listHeight = expandButton.GetComponent<RectTransform>().rect.size.y;
        expandableListText.GetComponent<RectTransform>().sizeDelta = new Vector2 (expandableListText.GetComponent<RectTransform>().sizeDelta.x, listHeight);

        if (childLists != null)
        {
            foreach (ExpandableListController child in childLists)
            {
                if (child != null)
                {
                    child.PressExpandButton(0);
                }
            }
        }
    }
    public void Init()
    {
        childLists = new List<ExpandableListController>();
        childTiles = new List<ChildTileObjectController>();
        FileName = "";
        ListCount = 1;
        PressExpandButton();
    }
    public void PressExpandButton(int mode = -1)
    {
        if (expandButtonText.text == ">" || mode == 1)
        {
            expandButtonText.text = "v";

            BoardCreationManager.instance.AdjustsGamesListYOffset();
            ExpandContent();
        }
        else if (mode == 0)
        {
            expandButtonText.text = ">";
            CollapseContent();

            BoardCreationManager.instance.AdjustsGamesListYOffset();
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
    public void SetFileName(string name)
    {
        FileName = name;
    }
    public string GetFileName()
    {
        return FileName;
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
        if (!(childLists == null || childLists.Count == 0) && IsExpanded())
        {
            //print("ChildLists was NOT NULL");
            foreach (ExpandableListController child in childLists)
            {
                listHeight += child.GetListHeight();
            }
        }
        if (!(childTiles == null || childTiles.Count == 0) && IsExpanded())
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
        newTile.SetSingleMax(count);

        AddChildTile(ref newTile);
    }

    void AddChildTile(ref ChildTileObjectController childTile)
    {
        childTiles.Add(childTile);
        BoardCreationManager.instance.AdjustsGamesListYOffset();
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


    public bool IsExpanded()
    {
        return expandButtonText.text == "v";
    }

    public List<ChildTileObjectController> GetChildTiles()
    {
        return childTiles;
    }

    public void AddToCount(int count)
    {
        ListCount += count;
        if (ListCount > 1)
        {
            ListCountText.gameObject.SetActive(true);
            ListCountText.text = "x" + ListCount.ToString();
        }
        else
        {
            ListCountText.gameObject.SetActive(false);
        }
    }
    public void SetCount(int count)
    {
        ListCount = count;
        if (ListCount > 1)
        {
            ListCountText.gameObject.SetActive(true);
            ListCountText.text = "x" + ListCount.ToString();
        }
        else
        {
            ListCountText.gameObject.SetActive(false);
        }
    }
    public int GetCount()
    {
        return ListCount;
    }
}
