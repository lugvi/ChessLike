using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Controller for a single position, attached to the position prefab, used to control the color and other properties of the position
/// </summary>
public class PositionController : MonoBehaviour 
{   
    Material mat; // Reference to the material for changing color 
    public Vector2 pos; // Position of this object in the game world

    void Awake()
    {
        mat = this.GetComponent<MeshRenderer>().material; // Get the material of this position
        pos = new Vector2((int)transform.position.x, (int)transform.position.y); // Set the position of this object in the game world
    }
    public void SetColor(Color color) => mat.color = color; // Set the color of the position
}