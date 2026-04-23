using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SongLine
{
    public TextMeshProUGUI targetText;

    [TextArea(3, 10)]
    public string text;

    public float triggerTime;

    [Header("Typewriter")]
    public bool useTypewriter;

    [Header("Fade Settings")]
    public bool fadeOut;        // enable fade
    public float duration = 2f; // how long text stays before fading
    public float fadeTime = 1f; // fade duration

    [HideInInspector]
    public bool triggered;
}

public class SongSubtitleManager : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Typewriter Settings")]
    public float typewriteDelay = 0.05f;

    public List<SongLine> lines = new List<SongLine>();

    private Dictionary<TextMeshProUGUI, Coroutine> activeCoroutines = new();

    void Update()
    {
        if (!audioSource.isPlaying) return;

        float currentTime = audioSource.time;

        foreach (var line in lines)
        {
            if (!line.triggered && currentTime >= line.triggerTime)
            {
                ShowText(line);
                line.triggered = true;
            }
        }
    }

    void ShowText(SongLine line)
    {
        if (line.targetText == null) return;

        // Stop previous coroutine on this TMP
        if (activeCoroutines.ContainsKey(line.targetText) && activeCoroutines[line.targetText] != null)
        {
            StopCoroutine(activeCoroutines[line.targetText]);
        }

        Coroutine c;

        if (line.useTypewriter)
            c = StartCoroutine(TypeTextRoutine(line));
        else
            c = StartCoroutine(InstantTextRoutine(line));

        activeCoroutines[line.targetText] = c;
    }

    IEnumerator TypeTextRoutine(SongLine line)
    {
        var target = line.targetText;
        target.text = "";

        SetAlpha(target, 1f);

        foreach (char c in line.text)
        {
            target.text += c;
            yield return new WaitForSeconds(typewriteDelay);
        }

        if (line.fadeOut)
            yield return StartCoroutine(FadeOutRoutine(line));
    }

    IEnumerator InstantTextRoutine(SongLine line)
    {
        var target = line.targetText;

        target.text = line.text;
        SetAlpha(target, 1f);

        if (line.fadeOut)
            yield return StartCoroutine(FadeOutRoutine(line));
    }

    IEnumerator FadeOutRoutine(SongLine line)
    {
        yield return new WaitForSeconds(line.duration);

        var target = line.targetText;
        float t = 0f;

        Color original = target.color;

        while (t < line.fadeTime)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / line.fadeTime);
            target.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        target.text = "";
        SetAlpha(target, 1f); // reset for next use
    }

    void SetAlpha(TextMeshProUGUI target, float alpha)
    {
        Color c = target.color;
        target.color = new Color(c.r, c.g, c.b, alpha);
    }

    public void ResetAll()
    {
        foreach (var line in lines)
            line.triggered = false;

        foreach (var pair in activeCoroutines)
        {
            if (pair.Value != null)
                StopCoroutine(pair.Value);
        }

        activeCoroutines.Clear();
    }
}