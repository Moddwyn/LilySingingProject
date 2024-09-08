using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class NotePlacer : MonoBehaviour
{
    public float xOffset = -500;
    public float widthStretchMultipliter = 1;
    public List<NoteBlock.BlockPlacement> blockPlacements = new List<NoteBlock.BlockPlacement>();
    [ReadOnly] public List<NoteBlock> placedBlocks = new List<NoteBlock>();

    [HorizontalLine]

    public RectTransform noteBlockParent;
    public NoteBlock noteBlockPrefab;
    public PitchArrow pitchArrow;
    public AudioPitchEstimator estimator;

    public void PlaceNotes(float pitchMultiplier)
    {
        foreach (var blockObjs in placedBlocks)
        {
            Destroy(blockObjs.gameObject);
        }

        placedBlocks.Clear();

        foreach (var blockPlacement in blockPlacements)
        {
            // Adjust frequency based on pitchMultiplier
            float originalFrequency = GetFrequencyFromNoteName(blockPlacement.noteName, blockPlacement.octave);
            float adjustedFrequency = originalFrequency * pitchMultiplier;

            // Calculate Y position based on adjusted frequency
            float yPos = pitchArrow.MapFrequencyToPosition(adjustedFrequency, estimator.frequencyMin, estimator.frequencyMax, pitchArrow.yRange.x, pitchArrow.yRange.y);

            // Calculate the width based on the length (assuming length is in seconds and 100 units per second)
            float length = blockPlacement.endTime - blockPlacement.startTime;
            float blockWidth = length * 100f * widthStretchMultipliter;

            // Calculate the X position based on the starting time (assuming 100 units per second)
            float xPos = blockPlacement.startTime * 100f * widthStretchMultipliter;

            // Instantiate the note block
            NoteBlock noteBlock = Instantiate(noteBlockPrefab, noteBlockParent);
            RectTransform noteTransform = noteBlock.GetComponent<RectTransform>();
            noteTransform.sizeDelta = new Vector2(blockWidth, noteTransform.sizeDelta.y);
            noteTransform.anchoredPosition = new Vector2(xPos + xOffset, yPos);

            // Adjust the note name based on the pitch multiplier
            NoteBlock.Note adjustedNote = AdjustNoteByPitchMultiplier(blockPlacement.noteName, pitchMultiplier);

            NoteBlock.BlockPlacement placement = new NoteBlock.BlockPlacement
            {
                noteName = adjustedNote, // Use adjusted note name
                octave = blockPlacement.octave, // Keep octave the same, or adjust if necessary
                endTime = blockPlacement.endTime,
                startTime = blockPlacement.startTime
            };

            noteBlock.Init(placement);
            placedBlocks.Add(noteBlock);
        }
    }

    // This method adjusts the note name based on the pitchMultiplier
    public NoteBlock.Note AdjustNoteByPitchMultiplier(NoteBlock.Note originalNote, float pitchMultiplier)
    {
        // Assuming pitchMultiplier affects the note semitone-wise, we shift the note accordingly
        int semitoneShift = Mathf.RoundToInt(Mathf.Log(pitchMultiplier, 2) * 12); // Convert pitchMultiplier to semitone shift

        // Map the note to a scale position (e.g., C=0, C#=1, ..., B=11)
        int noteIndex = (int)originalNote;
        int newNoteIndex = (noteIndex + semitoneShift) % 12; // Ensure it wraps around the 12 notes

        if (newNoteIndex < 0)
        {
            newNoteIndex += 12; // Handle negative indices (if pitchMultiplier < 1)
        }

        return (NoteBlock.Note)newNoteIndex; // Return the new note
    }

    public float GetFrequencyFromNoteName(NoteBlock.Note note, int octave)
    {
        Dictionary<NoteBlock.Note, float> baseFrequencies = new Dictionary<NoteBlock.Note, float>
        {
            { NoteBlock.Note.C, 16.35f }, { NoteBlock.Note.CSharp, 17.32f }, { NoteBlock.Note.D, 18.35f }, { NoteBlock.Note.DSharp, 19.45f },
            { NoteBlock.Note.E, 20.60f }, { NoteBlock.Note.F, 21.83f }, { NoteBlock.Note.FSharp, 23.12f }, { NoteBlock.Note.G, 24.50f },
            { NoteBlock.Note.GSharp, 25.96f }, { NoteBlock.Note.A, 27.50f }, { NoteBlock.Note.ASharp, 29.14f }, { NoteBlock.Note.B, 30.87f }
        };

        if (baseFrequencies.TryGetValue(note, out float baseFreq))
        {
            // Calculate the frequency for the specified octave
            float frequency = baseFreq * Mathf.Pow(2, octave);
            return frequency;
        }

        return 0f;
    }
}
