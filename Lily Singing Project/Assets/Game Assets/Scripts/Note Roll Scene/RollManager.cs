using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

public class RollManager : MonoBehaviour
{
    public VoiceTypeAnalyzer typeAnalyzer;
    public AudioMixer audioMixer;
    bool isRolling = false;

    [HorizontalLine]

    public AudioSource songSource;
    public float offsetStart = 1;
    public float pitchMultiplier = 1;
    [ReadOnly] public float currentTime;

    [HorizontalLine]

    public float xOffset = -500;
    public float widthStretchMultipliter = 1;
    public NoteBlock noteBlockPrefab;
    public List<NoteBlock.BlockPlacement> blockPlacements = new List<NoteBlock.BlockPlacement>();
    [ReadOnly] public List<NoteBlock> placedBlocks = new List<NoteBlock>();

    [HorizontalLine]

    public RectTransform noteBlockParent;
    public PitchArrow pitchArrow;
    public MidiFileReader midiFileReader;

    void Start()
    {
        midiFileReader.ConvertMidiFileToNoteData(() =>
        {
            blockPlacements.Clear();
            foreach (var note in midiFileReader.midiNoteDatas)
            {
                blockPlacements.Add(new NoteBlock.BlockPlacement
                {
                    note = note.note,
                    endTime = note.timeInSeconds + note.durationInSeconds,
                    startTime = note.timeInSeconds
                });
            }
        });
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartSong();
        }

        if (isRolling)
        {
            UpdateBlockPositions();
        }

        currentTime = songSource.time;
    }

    public void StartSong()
    {
        isRolling = true;
        songSource.Stop();

        pitchMultiplier = typeAnalyzer.detectedVoiceType != null ? typeAnalyzer.detectedVoiceType.suggestedPitch : 1;
        audioMixer.SetFloat("PitchShifter", pitchMultiplier);
        songSource.time = offsetStart;
        PlaceNotes(pitchMultiplier - 0.3f);

        songSource.Play();
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

    void PlaceNotes(float pitchMultiplier)
    {
        foreach (var blockObjs in placedBlocks)
        {
            Destroy(blockObjs.gameObject);
        }

        placedBlocks.Clear();

        foreach (var blockPlacement in blockPlacements)
        {
            // Adjust frequency based on pitchMultiplier
            float originalFrequency = blockPlacement.note.frequency;
            float adjustedFrequency = originalFrequency * pitchMultiplier;

            // Calculate Y position based on adjusted frequency
            float minFrequency = typeAnalyzer.detectedVoiceType.minFrequency - 50;
            float maxFrequency = typeAnalyzer.detectedVoiceType.maxFrequency + 50;

            print($"Song {midiFileReader.midiFileName} has min freq: {minFrequency}, max freq: {maxFrequency}");
            float yPos = MapFrequencyToPosition(adjustedFrequency, minFrequency, maxFrequency, pitchArrow.yRange.x, pitchArrow.yRange.y);

            // Calculate the width based on the length (assuming length is in seconds and 100 units per second)
            float length = blockPlacement.endTime - blockPlacement.startTime;
            float blockWidth = length * 100f * widthStretchMultipliter;

            // Calculate the X position based on the starting time (assuming 100 units per second)
            float xPos = blockPlacement.startTime * 100f * widthStretchMultipliter;

            // Adjust the note name based on the pitch multiplier
            NoteBlock.BlockPlacement adjustedPlacement = AdjustNoteAndOctaveByPitchMultiplier(
                                                            blockPlacement.note,
                                                            pitchMultiplier,
                                                            blockPlacement.startTime,
                                                            blockPlacement.endTime
                                                        );


            NoteBlock noteBlock = Instantiate(noteBlockPrefab, noteBlockParent);
            RectTransform noteTransform = noteBlock.GetComponent<RectTransform>();
            noteTransform.sizeDelta = new Vector2(blockWidth, noteTransform.sizeDelta.y);
            noteTransform.anchoredPosition = new Vector2(xPos + xOffset, yPos);

            noteBlock.Init(adjustedPlacement); // Initialize with adjusted note and octave
            placedBlocks.Add(noteBlock);

        }
    }

    // This method adjusts the note name based on the pitchMultiplier
    NoteBlock.BlockPlacement AdjustNoteAndOctaveByPitchMultiplier(Note originalNote, float pitchMultiplier, float startTime, float endTime)
    {
        // Assuming pitchMultiplier affects the note semitone-wise, we shift the note accordingly
        int semitoneShift = Mathf.RoundToInt(Mathf.Log(pitchMultiplier, 2) * 12); // Convert pitchMultiplier to semitone shift

        // Map the note name to a scale position (C = 0, C# = 1, ..., B = 11)
        int noteIndex = (int)originalNote.noteName;  // This is the new way to get the note index using Note.Name
        int totalSemitoneShift = noteIndex + semitoneShift;

        // Calculate the new octave based on the semitone shift
        int octaveChange = totalSemitoneShift / 12;  // Determine how many octaves to shift
        int newNoteIndex = totalSemitoneShift % 12;  // Wrap the note within one octave

        if (newNoteIndex < 0)
        {
            newNoteIndex += 12; // Handle negative indices (if pitchMultiplier < 1)
            octaveChange--;      // Adjust octave accordingly
        }

        // Adjust the original octave
        int newOctave = originalNote.octave + octaveChange;

        // Create a new Note with the adjusted note name and octave
        Note adjustedNote = new Note
        {
            noteName = (Note.Name)newNoteIndex,  // Convert back to the Note.Name enum
            octave = newOctave
        };

        // Return the updated BlockPlacement with the new note, octave, and provided start/end times
        return new NoteBlock.BlockPlacement
        {
            note = adjustedNote,
            startTime = startTime,  // Use the passed startTime
            endTime = endTime       // Use the passed endTime
        };
    }

    public static float MapFrequencyToPosition(float frequency, float minFreq, float maxFreq, float minY, float maxY)
    {
        float t = Mathf.InverseLerp(minFreq, maxFreq, frequency);
        return Mathf.Lerp(minY, maxY, t);
    }
}
