using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ManageScene : MonoBehaviour
{
    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadLevelWithDelay(levelName));
    }

    private IEnumerator LoadLevelWithDelay(string levelName)
    {
        yield return new WaitForSeconds(5f);

        SceneManager.LoadScene(levelName);
    }
}
