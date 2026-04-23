using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Fade OUT at scene start
        StartCoroutine(Fade(1f, 0f));
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        yield return Fade(0f, 1f); // fade IN
        SceneManager.LoadScene(sceneName);
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1,0));
    }

    private IEnumerator Fade(float start, float end)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            fadeCanvas.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        fadeCanvas.alpha = end;
    }
}