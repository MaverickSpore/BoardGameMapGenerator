using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelectManager : MonoBehaviour
{
    public static GameSelectManager instance;

    int GameSelected;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        GameSelected = 0;
    }

    public void SetGameSelected(int selection)
    {
        GameSelected = selection;
    }
    public int GetGameSelected()
    {
        return GameSelected;
    }
    public void PressBuildButton()
    {
        SceneManager.LoadScene(GameSelected + 1);
    }
}
