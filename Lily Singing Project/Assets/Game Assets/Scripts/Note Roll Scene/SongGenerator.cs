using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class SongGenerator : MonoBehaviour
{
    [ReadOnly] public List<NoteBlock.BlockPlacement> blockPlacements = new List<NoteBlock.BlockPlacement>();
}
