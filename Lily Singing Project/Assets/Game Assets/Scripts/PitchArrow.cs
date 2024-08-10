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
    public float minYPos;
    public float maxYPos;
    public RectTransform arrowTransform;
    public TMP_Text noteText;

    public string GetNoteNameFromCurrFrequency() => GetNameFromFrequency(GetFrequency());

    public float GetFrequency() => estimator.Estimate(audioSource);

    void Update()
    {
        float frequency = GetFrequency();
        if (frequency > 0)
        {
            float newYPos = MapFrequencyToPosition(frequency, estimator.frequencyMin, estimator.frequencyMax, minYPos, maxYPos);
            SetArrowPosition(newYPos);

            noteText.text = $"{GetNameFromFrequency(frequency)} | {(int)frequency}Hz";
        }

    }

    public float MapFrequencyToPosition(float frequency, float minFreq, float maxFreq, float minY, float maxY)
    {
        // Ensure frequency is within the min and max range
        frequency = Mathf.Clamp(frequency, minFreq, maxFreq);

        // Map the frequency to the Y position
        float normalizedFrequency = (frequency - minFreq) / (maxFreq - minFreq);
        float yPos = Mathf.Lerp(minY, maxY, normalizedFrequency);

        return yPos;
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
}
