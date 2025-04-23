using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor.SpeedTree.Importer;
using UnityEngine;
using Random = UnityEngine.Random; // Use Unity's Random class for random number generation

public class GameManager : MonoBehaviour
{

#region Singleton
        public static GameManager Instance; // Singleton instance of the GameManager
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // Assign the singleton instance
            }
            else
            {
                Destroy(gameObject); // Destroy duplicate instances
            }
        }
#endregion

    [SerializeField] List<MapController> Maps; // Spawnable maps in the game
    [SerializeField] MapController currentMap; // Reference to the current map
    [SerializeField] List<PieceController> PlayerPrefabs; // possible player pieces
    [SerializeField] CinemachineCamera cinemachineCamera;

    [SerializeField] DifficultySettings currentDifficulty;

    PieceController playerPiece; // Reference to the piece being controlled by the player

    Dictionary<PieceController, Vector2> piecesToMove = new(); // List of pieces to move

    TurnState turnState; // Current state of the game turn

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) //Detect clicks on positions to move player piece
        {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit); // Cast a ray from the camera to the mouse position
            if (hit.collider != null) // Check if the ray hit an object
            {
                var pos = hit.collider.GetComponent<PositionController>(); // Get the PositionController component from the hit object
                if (pos != null) // Check if the object has a PositionController component
                {
                    SelectPosition(pos); // Select the position
                }
            }
        }
    }

    public void Restart()
    {
        piecesToMove.Clear();
        if(currentMap != null) // Check if the current map is already initialized
        {
            Destroy(currentMap.gameObject); // Destroy the current map if it exists
        }

        var map = Maps[Random.Range(0, Maps.Count)]; // Get a random map from the list of maps

        currentMap = Instantiate(map); // Instantiate the map prefab

        currentMap.Initialize();
        turnState = TurnState.PickingPosition; // Set the initial turn state to picking position
        UpdatePositionDisplay(); // Update the position display for the map
        cinemachineCamera.Follow = currentMap.startPositions[currentMap.startPositions.Count/2].transform; // Set the camera to follow the map
    }

    public void NextTurn()
    {
        CheckWinLose();
        switch(turnState) // Check the current turn state
        {
            case TurnState.PickingPosition: 
                break;
            case TurnState.Player:
                OnPlayerTurn(); // Call the function to handle player turn
                break;
            case TurnState.AI: 
                OnAITurn(); // Call the function to handle AI turn
                break;
        }
        UpdatePositionDisplay();
    }

    void OnPlayerTurn()
    {   
        //choose which pieces will move on next turn
        foreach(var piece in piecesToMove.Keys)  // Reset the line renderer positions for all pieces
        {
            piece.lineRenderer.positionCount = 0; // Clear the line renderer positions
        }
        piecesToMove.Clear(); // Clear the list of pieces to move
        if(currentMap.currentPieces.Count == 0)
            return;
        for(int i = 0; i < currentDifficulty.simulMoves; i++)
        {
            var piece = currentMap.currentPieces[Random.Range(0, currentMap.currentPieces.Count)]; // Get a random piece from the list of current pieces        
            
            var targetPosition = GetRandomValidMove(piece); // Get a random valid move for the piece
            piecesToMove.TryAdd(piece, targetPosition); // Add the piece and its valid move to the list of pieces to move
            //draw a line from the piece to the position
            piece.lineRenderer.positionCount = 2; // Set the number of positions for the line renderer
            piece.lineRenderer.SetPositions(new Vector3[] { piece.currentpos, targetPosition}); // Set the positions for the line renderer
        }
    }


    Vector2 GetRandomValidMove(PieceController piece) // Get a random valid move for the piece
    {
        var validMoves = new List<Vector2>(piece.validMoves); // Create a new list of valid moves for the piece 

        for(int i = 0; i < validMoves.Count; i++)
        {
            var move = validMoves[Random.Range(0, validMoves.Count)]; // Get a random move this piece can make
            //check if the move is within map bounds and not occupied by another piece
            if(currentMap.positions.TryGetValue(piece.currentpos - move, out var position) && !currentMap.currentPieces.Any(p => p.currentpos == position.pos))
                return position.pos;
        }
        return piece.currentpos; // Return the current position of the piece if no valid move is found
    }

    void OnAITurn()
    {
        foreach(var piece in piecesToMove.Keys)
        {
            TryMovePieceToPos(piece, piecesToMove[piece]); // Move the piece to the corresponding position
        }
        turnState = TurnState.Player;
        NextTurn();
    }

    public void SelectPosition(PositionController position)
    {
        switch(turnState) // Check the current turn state
        {
            case TurnState.PickingPosition:
                if (position != null && currentMap.startPositions.Contains(position)) // Check if the position is valid
                {
                    playerPiece = Instantiate(PlayerPrefabs.First(), position.transform.position, Quaternion.identity, currentMap.transform); // Instantiate the player piece at the selected position
                    playerPiece.currentpos = position.pos; // Set the current position of the player piece
                    cinemachineCamera.Follow = playerPiece.transform; // Set the camera to follow the player piece
                    turnState = TurnState.Player; // Change to player turn state
                    NextTurn(); // Move to the next turn
                }
                else
                {
                    UIManager.instance.ShowPopup("Invalid Position", 0.5f);
                }
                break;
            case TurnState.Player:
                if (position != null && TryMovePieceToPos(playerPiece, position.pos))
                {
                    CheckTakes();       //Check if player can take any pieces before AI moves
                    turnState = TurnState.AI; // Change to AI turn state
                    NextTurn(); // Move to the next turn if the piece is moved successfully
                }
                break;
        }
    }

    void CheckWinLose()
    {
        if(playerPiece.currentpos == currentMap.kingPiece.currentpos) // Check if the player piece has reached the king piece
        {
            UIManager.instance.DisplayGameOver(true);
        }
        else if(currentMap.currentPieces.Any(piece => piece != currentMap.kingPiece && piece.validMoves.Any(move => piece.currentpos - move == playerPiece.currentpos))) // Check if all pieces have reached the king piece
        {
            UIManager.instance.DisplayGameOver(false);
        }
    }

    void CheckTakes()
    {
        foreach(var piece in currentMap.currentPieces) 
        {
           if(playerPiece.currentpos == piece.currentpos)
           {
                foreach(var pos in piece.validMoves) //Add valid moves from the taken piece to the player piece
                {
                    if(!playerPiece.validMoves.Contains(pos))
                    {
                        playerPiece.validMoves.Add(pos);
                    }
                }
                currentMap.currentPieces.Remove(piece); // Remove the AI piece from the list of current pieces
                Destroy(piece.gameObject); // Destroy the AI piece
                UIManager.instance.ShowPopup($"{piece.gameObject.name} Taken", 0.5f); // Show a popup indicating the piece was taken
                return;
           }
        }
    }

    bool TryMovePieceToPos(PieceController piece, Vector2 position)
    {
        if (position != null && piece.validMoves.Contains(piece.currentpos - position)) // Check if the position is valid
        {
            piece.currentpos = position; // Set the current position of the piece
            piece.transform.position = position; // Move the piece to the selected position
            return true;
        }
        else
        {
            UIManager.instance.ShowPopup("Invalid Move", 0.5f); // Show a popup if the move is invalid
            return false; // Return false if the move is invalid
        }
    }

    public void UpdatePositionDisplay()
    {
        currentMap.positions.Values.ToList().ForEach(pos => pos.SetColor(Color.white)); // Set the color of all positions to white});
        
        if(turnState == TurnState.PickingPosition)//if the game is in the picking position state set starting positions to green
            currentMap.startPositions.ForEach(pos => pos.SetColor(Color.green));
        else
            ColorPieceMoves(Color.green, playerPiece); // Set the color of the player positions to green
        
        if(currentDifficulty.showEnemyRange)
            currentMap.currentPieces.ForEach(piece => {
                ColorPieceMoves(Color.red, piece); // Set the color of the AI moves to red
            });

        void ColorPieceMoves(Color color, PieceController piece) // Set the color of the piece moves to the specified color
        {
            piece.validMoves.ForEach(pos => {
                if(currentMap.positions.TryGetValue(piece.currentpos - pos, out var pp)) // Set the piece in the corresponding position
                {
                    pp.SetColor(color);
                }
            });
        }   
    }
}

public enum TurnState
{
    PickingPosition,
    Player,
    AI
}
