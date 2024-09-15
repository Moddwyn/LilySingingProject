using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class NoteBlock : MonoBehaviour
{
    [ReadOnly] public BlockPlacement blockPlacement;
    public TMP_Text noteText;

    public void Init(BlockPlacement placement)
    {
        blockPlacement = placement;
        SetNoteText(Note.GetNoteNameFormatted(blockPlacement.note.noteName) + blockPlacement.note.octave);
    }

    public void SetNoteText(string note)
    {
        noteText.text = note;
    }

    [System.Serializable]
    public class BlockPlacement
    {
        public Note note;
        public float startTime;
        public float endTime;
    }
}
