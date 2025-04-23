using UnityEngine;

[CreateAssetMenu(fileName = "DifficultySettings", menuName = "Scriptable Objects/DifficultySettings")]
public class DifficultySettings : ScriptableObject
{
    public int simulMoves;      //Number of pieces AI can move on its turn at once
    public bool showEnemyRange; //Show where AI pieces can move
}
