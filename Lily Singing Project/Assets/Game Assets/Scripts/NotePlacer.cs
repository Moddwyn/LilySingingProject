using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotePlacer : MonoBehaviour
{
    public MidiFIleReader midiFIleReader;
    public float timeToXFactor = 100f; // Factor to convert timeInSeconds to x position
    public int octaveOffset = 0; // Octave offset
    public int pitchOffset = 0; // Pitch offset in semitones

    [Space(30)]

    public PitchArrow pitchArrow;
    public AudioPitchEstimator estimator;
    public GameObject objectToSpawn; // The GameObject to spawn
    public Transform parentTransform; // Optional: Parent transform for the spawned objects

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnAllNotes();
        }
    }

    public void SpawnAllNotes()
    {
        for (int i = 0; i < midiFIleReader.midiNotes.Count; i++)
        {
            MidiNoteData noteData = midiFIleReader.midiNotes[i];
            string adjustedNoteName = AdjustNoteOctave(noteData.fullNoteName, octaveOffset);
            adjustedNoteName = AdjustNotePitch(adjustedNoteName, pitchOffset);
            float frequency = GetFrequencyFromNoteName(adjustedNoteName);
            float xPos = noteData.timeInSeconds * timeToXFactor;
            SpawnGameObjectAtNoteYPosition(frequency, xPos, adjustedNoteName);
        }
    }

    string AdjustNoteOctave(string noteName, int octaveOffset)
    {
        string notePart = noteName.Substring(0, noteName.Length - 1);
        int originalOctave = int.Parse(noteName.Substring(noteName.Length - 1));
        int newOctave = originalOctave + octaveOffset;
        return notePart + newOctave;
    }

    string AdjustNotePitch(string noteName, int pitchOffset)
    {
        string[] notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        string notePart = noteName.Substring(0, noteName.Length - 1);
        int octave = int.Parse(noteName.Substring(noteName.Length - 1));

        int noteIndex = System.Array.IndexOf(notes, notePart);
        int newIndex = noteIndex + pitchOffset;

        while (newIndex < 0)
        {
            newIndex += 12;
            octave -= 1;
        }

        while (newIndex >= 12)
        {
            newIndex -= 12;
            octave += 1;
        }

        string newNotePart = notes[newIndex];
        return newNotePart + octave;
    }

    float GetFrequencyFromNoteName(string noteName)
    {
        // Extract note and octave from noteName (e.g., "C#4")
        string notePart = noteName.Substring(0, noteName.Length - 1);
        int octave = int.Parse(noteName.Substring(noteName.Length - 1));

        // Define note frequencies for one octave
        Dictionary<string, float> baseFrequencies = new Dictionary<string, float>
        {
            { "C", 16.35f }, { "C#", 17.32f }, { "D", 18.35f }, { "D#", 19.45f },
            { "E", 20.60f }, { "F", 21.83f }, { "F#", 23.12f }, { "G", 24.50f },
            { "G#", 25.96f }, { "A", 27.50f }, { "A#", 29.14f }, { "B", 30.87f }
        };

        if (baseFrequencies.TryGetValue(notePart, out float baseFreq))
        {
            // Calculate the frequency for the specified octave
            float frequency = baseFreq * Mathf.Pow(2, octave);
            return frequency;
        }

        return 0f;
    }

    void SpawnGameObjectAtNoteYPosition(float frequency, float xPos, string noteName)
    {
        float yPos = pitchArrow.MapFrequencyToPosition(frequency, estimator.frequencyMin, estimator.frequencyMax, pitchArrow.minYPos, pitchArrow.maxYPos);

        // Create a new UI element in the canvas
        GameObject newObject = Instantiate(objectToSpawn, parentTransform);
        RectTransform rectTransform = newObject.GetComponent<RectTransform>();
        rectTransform.GetChild(0).GetComponent<TMP_Text>().text = noteName;

        // Set the anchored position
        Vector2 anchoredPosition = new Vector2(xPos, yPos);
        rectTransform.anchoredPosition = anchoredPosition;
    }
}
