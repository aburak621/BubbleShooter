using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    /**
     * Reloads the game scene.
     */
    public void Reload()
    {
        SceneManager.LoadScene(0);
    }
}
