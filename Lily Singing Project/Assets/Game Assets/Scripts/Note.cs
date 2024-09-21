using UnityEngine;

[System.Serializable]
public class Note
{
    public enum Name
    {
        C,
        CSharp,
        D,
        DSharp,
        E,
        F,
        FSharp,
        G,
        GSharp,
        A,
        ASharp,
        B
    }

    public Name noteName;
    public int octave;
    public float frequency;

    const float A4Frequency = 440f; // Standard frequency of A4
    const int A4Index = 57; // A4 is the 57th note (starting from C0 as 0)

    public static Note GetNoteFromFrequency(float freq)
    {
        // Calculate the number of semitones away from A4
        float semitoneOffset = 12 * Mathf.Log(freq / A4Frequency, 2);

        // Get the closest note index relative to C0 (index 0)
        int noteIndex = Mathf.RoundToInt(semitoneOffset) + A4Index;

        // Calculate the octave (each octave has 12 notes)
        int octave = noteIndex / 12;

        // Calculate the note name by taking modulo 12
        Name noteName = (Name)(noteIndex % 12);

        // Return the calculated note
        return new Note { noteName = noteName, octave = octave, frequency = freq };
    }

    public static float GetFrequencyFromNote(Name noteName, int octave)
    {
        // Get the index of the note (0 for C, 1 for C#, ..., 11 for B)
        int noteIndex = (int)noteName;

        // Calculate the total semitone shift from A4 (A4 is the 57th note)
        int totalNoteIndex = (octave * 12) + noteIndex;
        int semitoneOffset = totalNoteIndex - A4Index; // A4 is the reference point (A4Index = 57)

        // Calculate the frequency using the formula
        float frequency = A4Frequency * Mathf.Pow(2, semitoneOffset / 12f);

        return frequency;
    }

    public static string GetNoteNameFormatted(Name name)
    {
        switch (name)
        {
            case Name.CSharp: return "C#";
            case Name.DSharp: return "D#";
            case Name.FSharp: return "F#";
            case Name.GSharp: return "G#";
            case Name.ASharp: return "A#";
            default: return name.ToString(); // Return the note name as it is for non-sharp notes
        }
    }
}
