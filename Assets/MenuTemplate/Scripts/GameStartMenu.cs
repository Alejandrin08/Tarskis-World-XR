using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStartMenu : MonoBehaviour
{
    [Header("UI Pages")]
    public GameObject mainMenu;

    [Header("Main Menu Buttons")]
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        mainMenu.SetActive(true);

        startButton.onClick.AddListener(() => StartCoroutine(StartGameCoroutine()));
        quitButton.onClick.AddListener(QuitGame);
    }

    IEnumerator StartGameCoroutine()
    {
        DisableVRSystems();

        mainMenu.SetActive(false);

        yield return Resources.UnloadUnusedAssets();
        System.GC.Collect();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                ResetOculusSystems();
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    void DisableVRSystems()
    {
        var tubeRenderers = FindObjectsOfType<Oculus.Interaction.TubeRenderer>(true);
        foreach (var tr in tubeRenderers)
        {
            tr.enabled = false;
        }

        var cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null) cameraRig.enabled = false;
    }

    void ResetOculusSystems()
    {
        if (OVRManager.instance != null)
        {
            OVRManager.instance.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}