using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        Button playButton = root.Q<Button>("PlayButton");
        Button quitButton = root.Q<Button>("QuitButton");

        playButton.clicked += () => SceneManager.LoadScene("Scene");
        quitButton.clicked += () =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        };
    }
}