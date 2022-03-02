//Script copyied from platformer project, edited by gabby
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    //name of the scene to load on button click
    public string LevelToLoad = "";
    // add this function to the button onclick in the editor
    public void LevelLoad()
    {
        SceneManager.LoadScene(LevelToLoad);
    }
}
