using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator startAnimator;
    [SerializeField] private string startTriggerName = "Start";
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Settings")]
    [SerializeField] private float sceneLoadDelay = 1.5f;

    private bool isStarting = false;

    public void StartGameAnimation()
    {
        if (isStarting) return;
        isStarting = true;

        if (startAnimator != null)
            startAnimator.SetBool(startTriggerName, true);

        Invoke(nameof(StartGame), sceneLoadDelay);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
