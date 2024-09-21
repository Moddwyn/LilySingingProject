using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PitchArrow : MonoBehaviour
{
    public VoiceFrequencyAnalyzer frequencyAnalyzer;
    public VoiceTypeAnalyzer typeAnalyzer;
    public SongManager songManager;
    public MidiFileReader midiFileReader;
    public AudioSource audioSource;

    [Space(20)]
    public float moveSpeed = 1;
    public Vector2 yRange = new Vector2(-420, 420);
    public RectTransform arrowTransform;
    public TMP_Text noteText;

    void Update()
    {
        if (typeAnalyzer.detectedVoiceType == null) return;

        float frequency = frequencyAnalyzer.frequency;
        Note note = Note.GetNoteFromFrequency(frequency);
        if (frequency > 0)
        {
            float minFrequency = songManager.currentLowestSongAdjustedNote.frequency;
            float maxFrequency = songManager.currentHighestSongAdjustedNote.frequency;

            float newYPos = RollManager.MapFrequencyToPosition(note.frequency, minFrequency, maxFrequency, yRange.x, yRange.y);
            SetArrowPosition(newYPos);

            noteText.text = $"{Note.GetNoteNameFormatted(note.noteName)}{note.octave} | {(int)frequency}Hz";
        }
    }

    void SetArrowPosition(float yPos)
    {
        Vector2 newPos = arrowTransform.anchoredPosition;
        newPos.y = yPos;
        arrowTransform.anchoredPosition = Vector2.MoveTowards(arrowTransform.anchoredPosition, newPos, Time.deltaTime * 1000 * moveSpeed);
    }
}
