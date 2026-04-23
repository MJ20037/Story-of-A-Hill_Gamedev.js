using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    public static EndGameManager Instance;

    [Header("Trigger")]
    public int endDepth;

    [Header("References")]
    public GridManager grid;
    public GameObject landslidePrefab;
    public CanvasGroup fadeCanvas;

    private bool triggered;
    public bool IsTriggered => triggered;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (triggered) return;

        if (grid.GetMaxDepthReached() >= endDepth)
        {
            TriggerEnd();
        }
    }

    public void TriggerEnd()
    {
        triggered = true;
        AudioDirector.Instance?.BeginEnding();
        GameStats.Instance?.MarkEnd();
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        // 1️⃣ Continuous shake
        CameraShake.Instance.StartShake(0.3f, 8f);

        // 2️⃣ Workers panic
        Worker[] workers = FindObjectsOfType<Worker>();
        foreach (var w in workers)
        {
            w.TriggerPanic();
        }

        yield return new WaitForSeconds(3f);

        // 3️⃣ Landslide spawn
        SpawnLandslide();

        yield return new WaitForSeconds(3f);

        // 4️⃣ Fade to black
        yield return StartCoroutine(FadeToBlack());
    }

    void SpawnLandslide()
    {
        Camera cam = Camera.main;

        float halfWidth = cam.orthographicSize * cam.aspect;

        float spawnX = cam.transform.position.x + halfWidth + 2f;

        Instantiate(landslidePrefab, new Vector3(spawnX, 0f, 0f), Quaternion.identity);
    }

    IEnumerator FadeToBlack()
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = t;
            yield return null;
        }
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene("Ending");
    }
}