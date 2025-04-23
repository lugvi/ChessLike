using System.Collections.Generic;
using UnityEngine;

/// <summary> controller for a single piece, attached to the piece prefab </summary>
public class PieceController : MonoBehaviour
{
    public List<Vector2> validMoves; // List of valid moves for the piece on the map

    public Vector2 currentpos; // Current position of the piece on the map

    public LineRenderer lineRenderer; // Reference to the LineRenderer component for drawing movement lines

    [ContextMenu("Fill Valid Positions")]
    void FillValidPositions()
    {
        this.validMoves = new(); // Clear the list before filling it
        foreach(var a in transform.GetComponentsInChildren<PositionController>())
        {
           this.validMoves.Add(a.transform.localPosition);
        }
    }
    
}
