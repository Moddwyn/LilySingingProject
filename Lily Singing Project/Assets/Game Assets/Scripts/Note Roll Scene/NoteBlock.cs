using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class NoteBlock : MonoBehaviour
{
    [ReadOnly] public BlockPlacement blockPlacement;
    public TMP_Text noteText;

    public enum Note
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

    public void Init(BlockPlacement placement)
    {
        blockPlacement = placement;
        SetNoteText(blockPlacement.noteName.ToString() + blockPlacement.octave);
    }

    public void SetNoteText(string note)
    {
        noteText.text = note;
    }

    [System.Serializable]
    public class BlockPlacement
    {
        public Note noteName;
        public int octave;
        public float startTime;
        public float endTime;
    }
}
