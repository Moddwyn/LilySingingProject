using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;

public class RollManager : MonoBehaviour
{
    public MidiFileReader midiFileReader;
    public VoiceTypeAnalyzer typeAnalyzer;
    public SongManager songManager;
    public AudioSource songSource;

    [HorizontalLine]

    public float offsetStart = 1;
    public float blocksPitchOffset = -0.3f;
    [ReadOnly] public float currentTime;
    [ReadOnly] bool isRolling = false;

    [HorizontalLine]

    public float xOffset = -500;
    public float widthStretchMultipliter = 1;
    public NoteBlock noteBlockPrefab;
    public List<NoteBlock.BlockPlacement> blockPlacements = new List<NoteBlock.BlockPlacement>();
    [ReadOnly] public List<NoteBlock> placedBlocks = new List<NoteBlock>();

    [HorizontalLine]

    public RectTransform noteBlockParent;
    public PitchArrow pitchArrow;

    public void StartSong()
    {
        GenerateBlocksByMidi(midiFileReader.midiNoteDatas);
        PlaceNotes();

        songSource.Stop();
        songSource.clip = midiFileReader.currentSongData.soundClip;
        isRolling = true;
        songSource.time = offsetStart;
        songSource.Play();
    }

    void Update()
    {
        if (isRolling)
        {
            UpdateBlockPositions();
        }

        currentTime = songSource.time;
    }

    public void GenerateBlocksByMidi(List<MidiNoteData> midiNoteDatas)
    {
        blockPlacements.Clear(); // Clear existing block placements

        // Loop through all the MIDI notes to generate block placements with adjusted semitone
        foreach (var noteData in midiNoteDatas)
        {
            // Adjust the frequency based on the semitone offset using the static function from SongManager
            float adjustedFrequency = SongManager.OffsetFrequencyBySemitone(noteData.note.frequency, songManager.semitoneOffset);

            // Get the adjusted note after applying the frequency shift
            Note adjustedNote = Note.GetNoteFromFrequency(adjustedFrequency);

            // Create a new block placement with the adjusted note and times
            blockPlacements.Add(new NoteBlock.BlockPlacement
            {
                note = adjustedNote, // Use the adjusted note
                startTime = noteData.timeInSeconds, // Keep the start time as per the MIDI data
                endTime = noteData.timeInSeconds + noteData.durationInSeconds // Keep the end time
            });
        }
    }

    void UpdateBlockPositions()
    {
        foreach (NoteBlock noteBlock in placedBlocks)
        {
            float startTime = noteBlock.blockPlacement.startTime;
            float timeDifference = startTime - currentTime;
            float xPos = timeDifference * 100f * widthStretchMultipliter + xOffset;

            RectTransform noteTransform = noteBlock.GetComponent<RectTransform>();
            noteTransform.anchoredPosition = new Vector2(xPos, noteTransform.anchoredPosition.y);
        }
    }

    void PlaceNotes()
    {
        foreach (var blockObjs in placedBlocks)
        {
            Destroy(blockObjs.gameObject);
        }

        placedBlocks.Clear();

        foreach (var blockPlacement in blockPlacements)
        {
            // Calculate Y position based on adjusted frequency
            float minFrequency = songManager.currentLowestSongAdjustedNote.frequency;
            float maxFrequency = songManager.currentHighestSongAdjustedNote.frequency;

            float yPos = MapFrequencyToPosition(blockPlacement.note.frequency, minFrequency, maxFrequency, pitchArrow.yRange.x, pitchArrow.yRange.y);

            // Calculate the width based on the length (assuming length is in seconds and 100 units per second)
            float length = blockPlacement.endTime - blockPlacement.startTime;
            float blockWidth = length * 100f * widthStretchMultipliter;

            // Calculate the X position based on the starting time (assuming 100 units per second)
            float xPos = blockPlacement.startTime * 100f * widthStretchMultipliter;

            NoteBlock noteBlock = Instantiate(noteBlockPrefab, noteBlockParent);
            RectTransform noteTransform = noteBlock.GetComponent<RectTransform>();
            noteTransform.sizeDelta = new Vector2(blockWidth, noteTransform.sizeDelta.y);
            noteTransform.anchoredPosition = new Vector2(xPos + xOffset, yPos);

            noteBlock.Init(blockPlacement);
            placedBlocks.Add(noteBlock);
        }
    }

    public static float MapFrequencyToPosition(float frequency, float minFreq, float maxFreq, float minY, float maxY)
    {
        float t = Mathf.InverseLerp(minFreq, maxFreq, frequency);
        return Mathf.Lerp(minY, maxY, t);
    }
}
