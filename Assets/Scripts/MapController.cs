using System.Collections.Generic;
using UnityEngine;

/// <summary> controller for a single map, attached to the map prefab </summary>
public class MapController : MonoBehaviour
{
    [SerializeField] List<PositionController> mapPositions; // List of map all map positions, filled from the editor
    [SerializeField] List<PieceToCount> allPieces; // List of pieces and their max amount available for this map

    public List<PieceController> currentPieces = new(); // List of pieces currently in play on this map

    [SerializeField] List<PositionController> kingPositions = new(); // List of positions king can spawn on

    public List<PositionController> startPositions = new(); // List of valid start positions

    [SerializeField] PieceController kingPiecePrefab; // king prefab

    public PieceController kingPiece; // reference to king prefab instance
    public Dictionary<Vector2, PositionController> positions; // helper for easier lookup of map positions based on their coordinates


    public void Initialize()
    {
        positions = new();              // Initialize the dictionary for positions
        foreach (var a in mapPositions) // Fill the dictionary with positions and their corresponding PositionController
        {
            positions.Add(a.transform.position, a);
        }
        SpawnPieces();
    }
    public void SpawnPieces()
    {
        if (positions == null || positions.Count == 0) // Check if the positions list is empty
        {
            FillValidPositions(); // Fill the positions list if it is empty
        }
        var temp = new List<Vector2>(positions.Keys); // Create a temporary list of positions to avoid modifying the original list
        temp.RemoveAll(pos => startPositions.Contains(positions[pos])); // Remove all starting positions from the list so enemy pieces dont spawn there
        
        var kingPos = kingPositions[Random.Range(0, kingPositions.Count)].pos;
        kingPiece = SpawnPiece(kingPiecePrefab, kingPos); // Spawn the king piece at a random king position
        
        foreach (var pieceData in allPieces) //Spawn all opposing pieces
        {
            for (int i = 0; i < pieceData.count; i++) // Spawn the number of pieces specified
            {
                var pos = temp[Random.Range(0, temp.Count)]; // Get a random position from the list
                SpawnPiece(pieceData.piece, pos); // Spawn the piece at the random position
            }
        }

        PieceController SpawnPiece(PieceController prefab, Vector2 pos) // Spawn the pieces at the specified positions
        {
            var piece = Instantiate(prefab, pos, Quaternion.identity, this.transform); // Instantiate the piece at the specified position
            piece.currentpos = pos; // Set the current position of the piece
            currentPieces.Add(piece); // Add the piece to the list of current pieces
            temp.Remove(pos); // Remove the position from the list to avoid piece overlap
            return piece; // Return the spawned piece
        }
    }

    /// <summary>
    /// Fills the list of valid positions using the locations of gameobjects with PositionController components on them.
    /// called from the editor only
    /// </summary>
    [ContextMenu("Fill Valid Positions")]
    public void FillValidPositions()
    {
        mapPositions = new(); // Clear the list before filling it
        foreach(var a in this.GetComponentsInChildren<PositionController>())
        {
           mapPositions.Add(a);
        }
    }
}


/// <summary>
/// Helper for easy display and modification of piece and the count for a specific map through the inspector
/// </summary>
[System.Serializable]
public struct PieceToCount
{
    public PieceController piece; // Piece to be placed on the map
    public int count; // Number of pieces to be placed on the map
}
