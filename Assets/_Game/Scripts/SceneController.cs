using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public GameObject[] savedObjects;
    public GameObject winScreen;

    static readonly string[] LevelOrder = { "First", "Second", "Third" };

    bool isTransitioning;

    void Awake()
    {
        foreach (GameObject obj in savedObjects)
            DontDestroyOnLoad(obj);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isTransitioning = false;
    }

    void Update()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.name == "EndGame" || activeScene.name == "WinScreen")
        {
            foreach (GameObject obj in savedObjects)
                Destroy(obj);
        }
    }

    public void ChangeScene()
    {
        if (isTransitioning)
            return;

        string currentScene = SceneManager.GetActiveScene().name;
        int currentIndex = System.Array.IndexOf(LevelOrder, currentScene);
        if (currentIndex < 0)
            return;

        isTransitioning = true;

        if (currentIndex >= LevelOrder.Length - 1)
        {
            SceneManager.LoadScene("WinScreen");
            return;
        }

        if (winScreen != null)
            winScreen.SetActive(false);

        SceneManager.LoadScene(LevelOrder[currentIndex + 1]);
    }
}
