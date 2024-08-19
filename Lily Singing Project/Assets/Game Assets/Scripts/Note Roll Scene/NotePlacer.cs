using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;
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

    void Start()
    {
        PlaceNotes();
    }

    void PlaceNotes()
    {
        foreach (var blockPlacement in blockPlacements)
        {
            float frequency = GetFrequencyFromNoteName(blockPlacement.noteName, blockPlacement.octave);
            float yPos = pitchArrow.MapFrequencyToPosition(frequency, estimator.frequencyMin, estimator.frequencyMax, pitchArrow.yRange.x, pitchArrow.yRange.y);

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

            NoteBlock.BlockPlacement placement = new NoteBlock.BlockPlacement
            {
                noteName = blockPlacement.noteName,
                octave = blockPlacement.octave,
                endTime = blockPlacement.endTime,
                startTime = blockPlacement.startTime
            };

            noteBlock.Init(placement);
            placedBlocks.Add(noteBlock);
        }
    }


    float GetFrequencyFromNoteName(NoteBlock.Note note, int octave)
    {
        // Define note frequencies for one octave
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


