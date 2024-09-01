using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

public class RollManager : MonoBehaviour
{
    public NotePlacer notePlacer;
    public VoiceTypeAnalyzer voiceTypeAnalyzer;
    public AudioPitchEstimator pitchEstimator;
    public AudioMixer audioMixer;
    bool isRolling = false;

    [HorizontalLine]

    public AudioSource songSource;
    public float offsetStart = 1;
    [Range(-2, 2)] public int octaveOffset = 0;
    [ReadOnly] public float currentTime;

    // New field for song's original note
    public string originalNote = "C4"; // Example default value

    // Predefined note frequencies (for simplicity, not exhaustive)
    private Dictionary<string, float> noteFrequencies = new Dictionary<string, float>()
    {
        {"C4", 261.63f}, {"C#4", 277.18f}, {"D4", 293.66f}, {"D#4", 311.13f},
        {"E4", 329.63f}, {"F4", 349.23f}, {"F#4", 369.99f}, {"G4", 392.00f},
        {"G#4", 415.30f}, {"A4", 440.00f}, {"A#4", 466.16f}, {"B4", 493.88f},
        // Add other notes as needed
    };

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRolling = true;
            songSource.Stop();
            ApplyVoiceTypePitchShift(); // Apply the pitch shift before playing
            songSource.time = offsetStart;
            songSource.Play();
        }

        if (isRolling)
        {
            UpdateBlockPositions();
        }

        currentTime = songSource.time;
    }

    void ApplyVoiceTypePitchShift()
    {
        if (!noteFrequencies.ContainsKey(originalNote))
        {
            Debug.LogError("Original note is not recognized!");
            return;
        }

        float originalFrequency = noteFrequencies[originalNote];
        float pitchMultiplier = 1f;

        // Determine pitch multiplier based on the detected voice type
        switch (voiceTypeAnalyzer.detectedVoiceType)
        {
            case "Soprano (Female)":
                pitchMultiplier = CalculatePitchShift(originalFrequency, 261.63f, octaveOffset); // Soprano range C4 to C6
                break;
            case "Mezzo-Soprano (Female)":
                pitchMultiplier = CalculatePitchShift(originalFrequency, 196.00f, octaveOffset); // Mezzo-Soprano range G3 to G5
                break;
            case "Alto (Female)":
                pitchMultiplier = CalculatePitchShift(originalFrequency, 130.81f, octaveOffset); // Alto range C3 to C5
                break;
            case "Tenor (Male)":
                pitchMultiplier = CalculatePitchShift(originalFrequency, 130.81f, octaveOffset); // Tenor range C3 to C5
                break;
            case "Baritone (Male)":
                pitchMultiplier = CalculatePitchShift(originalFrequency, 98.00f, octaveOffset); // Baritone range G2 to G4
                break;
            case "Bass (Male)":
                pitchMultiplier = CalculatePitchShift(originalFrequency, 82.41f, octaveOffset); // Bass range E2 to E4
                break;
            default:
                pitchMultiplier = Mathf.Pow(2, octaveOffset); // No shift if voice type is unknown
                break;
        }

        // Apply the calculated pitch multiplier to the audio mixer
        audioMixer.SetFloat("PitchShifter", pitchMultiplier);
    }

    float CalculatePitchShift(float originalFrequency, float targetFrequency, int octaveOffset)
    {
        // Calculate the pitch shift multiplier needed
        float multiplier = targetFrequency / originalFrequency;

        // Apply octave offset
        multiplier *= Mathf.Pow(2, octaveOffset);

        return multiplier;
    }

    void UpdateBlockPositions()
    {
        foreach (NoteBlock noteBlock in notePlacer.placedBlocks)
        {
            float startTime = noteBlock.blockPlacement.startTime;
            float timeDifference = startTime - currentTime;
            float xPos = timeDifference * 100f * notePlacer.widthStretchMultipliter + notePlacer.xOffset;

            RectTransform noteTransform = noteBlock.GetComponent<RectTransform>();
            noteTransform.anchoredPosition = new Vector2(xPos, noteTransform.anchoredPosition.y);
        }
    }
}
