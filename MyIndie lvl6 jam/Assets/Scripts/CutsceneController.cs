using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float delayBeforeNextScene = 5f; 
    [SerializeField] private string nextSceneName;

    private void Start()
    {
        StartCoroutine(LoadNextSceneAfterDelay());
    }

    private System.Collections.IEnumerator LoadNextSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeNextScene);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("⚠️ Next scene name is not set in CutsceneController!");
    }
}
