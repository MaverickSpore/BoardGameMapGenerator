using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PackCreationManager : MonoBehaviour
{
    public void PressReturn()
    {
        SceneManager.LoadScene(0);
    }
}
