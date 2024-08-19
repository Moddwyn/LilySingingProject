using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PitchArrow : MonoBehaviour
{
    public AudioPitchEstimator estimator;
    public AudioSource audioSource;

    [Space(20)]
    public float moveSpeed = 1;
    public Vector2 yRange = new Vector2(-420, 420);
    public RectTransform arrowTransform;
    public TMP_Text noteText;

    void Update()
    {
        float frequency = GetFrequency();
        if (frequency > 0)
        {
            float newYPos = MapFrequencyToPosition(frequency, estimator.frequencyMin, estimator.frequencyMax, yRange.x, yRange.y);
            SetArrowPosition(newYPos);

            noteText.text = $"{GetNameFromFrequency(frequency)} | {(int)frequency}Hz";
        }

    }

    public float MapFrequencyToPosition(float frequency, float minFreq, float maxFreq, float minY, float maxY)
    {
        float t = Mathf.InverseLerp(minFreq, maxFreq, frequency);
        return Mathf.Lerp(minY, maxY, t);
    }

    void SetArrowPosition(float yPos)
    {
        Vector2 newPos = arrowTransform.anchoredPosition;
        newPos.y = yPos;
        arrowTransform.anchoredPosition = Vector2.MoveTowards(arrowTransform.anchoredPosition, newPos, Time.deltaTime * 1000 * moveSpeed);
    }

    public string GetNameFromFrequency(float frequency)
    {
        var noteNumber = Mathf.RoundToInt(12 * Mathf.Log(frequency / 440) / Mathf.Log(2) + 69);
        string[] names = {
        "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };
        int octave = (noteNumber / 12) - 1; // Calculate the octave number
        string noteName = names[noteNumber % 12];
        return noteName + octave; // Combine note name and octave
    }

    public string GetNoteNameFromCurrFrequency() => GetNameFromFrequency(GetFrequency());

    public float GetFrequency() => estimator.Estimate(audioSource);
}
