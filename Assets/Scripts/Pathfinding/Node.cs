using UnityEngine;

public class Node
{
    // Apakah node ini bisa dilalui?
    public bool isWalkable;
    // Posisi di dunia nyata (World Position)
    public Vector3 worldPosition;
    // Posisi X dan Y dalam grid
    public int gridX;
    public int gridY;

    // --- Nilai untuk Algoritma BFS/Flood Fill ---
    
    // G Cost: Jarak dari node awal (digunakan untuk melacak jumlah langkah/kedalaman)
    public int gCost; 
    // H Cost dan F Cost dihilangkan

    // Node Induk (Parent Node) untuk merekonstruksi jalur
    public Node parent;

    // F Cost (get property) DIHAPUS

    public Node(bool _walkable, Vector3 _worldPos, int _gridX, int _gridY)
    {
        isWalkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }
}