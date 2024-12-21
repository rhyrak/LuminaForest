using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static void TransitionToTutorial()
    {
        SceneManager.LoadScene("GameScene");
    }
}
