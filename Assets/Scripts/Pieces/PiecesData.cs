using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pieces;
using UnityEngine;

[CreateAssetMenu(fileName = "PiecesData", menuName = "Game Data/Pieces Data")]
public class PiecesData : ScriptableObject
{
    public PieceValue[] types;

    
    public Sprite GetImage(PieceType type)
    {
        return types.First(x => x.Type == type).Image;
    }
    

    [System.Serializable]
    public struct PieceValue
    {
        public Sprite Image;
        public PieceType Type;
    }
}
