using UnityEngine;
using System.Collections.Generic;
// Hapus using System.Diagnostics; karena tidak lagi diperlukan di mode Play

public class Grid : MonoBehaviour
{
    // Variabel untuk Debugging dan Pengaturan
    public bool displayGridGizmos;
    public LayerMask unwalkableMask; // Layer yang dianggap sebagai rintangan
    public Vector2 gridWorldSize; // Ukuran area grid di dunia nyata
    public float nodeRadius; // Radius setiap node/titik
    
    // Visualisasi Debug (Opsional)
    public Transform startPosition; // Titik awal AI
    public Transform targetPosition; // Titik tujuan AI

    Node[,] grid; // Array 2D yang menyimpan semua node

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    // --- Bagian A: Pembuatan Grid ---
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.up * gridWorldSize.y/2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                // Cek apakah ada rintangan di lokasi ini
                bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius, unwalkableMask);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    // --- Bagian B: Algoritma BFS (Flood Fill) BARU ---
    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = NodeFromWorldPoint(startPos);
        Node targetNode = NodeFromWorldPoint(targetPos);
        
        // Menggunakan Queue untuk BFS (memastikan eksplorasi langkah demi langkah)
        Queue<Node> queue = new Queue<Node>();
        // Menggunakan HashSet untuk Node yang sudah dikunjungi/dievaluasi
        HashSet<Node> visited = new HashSet<Node>();
        
        queue.Enqueue(startNode);
        visited.Add(startNode);
        
        // Reset G Cost (depth) untuk startNode (opsional, tapi baik untuk kejelasan)
        startNode.gCost = 0;

        while (queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();

            if (currentNode == targetNode)
            {
                // Jalur ditemukan! Rekonstruksi dan kembalikan jalur.
                return RetracePath(startNode, targetNode);
            }

            foreach (Node neighbour in GetNeighbours(currentNode))
            {
                if (!neighbour.isWalkable || visited.Contains(neighbour))
                {
                    continue;
                }
                
                // Set node parent untuk retrace
                neighbour.parent = currentNode;
                
                // Catat jumlah langkah (G Cost)
                neighbour.gCost = currentNode.gCost + 1;

                // Tandai sebagai sudah dikunjungi dan masukkan ke antrian
                visited.Add(neighbour);
                queue.Enqueue(neighbour);
            }
        }
        
        // Jika loop selesai tanpa menemukan target
        return new List<Node>(); 
    }

    // --- Bagian C: Helper Functions ---
    
    // Mendapatkan semua node tetangga (atas, bawah, kiri, kanan, dan diagonal)
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue; // Lewati node itu sendiri

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                // Pastikan node tetangga berada di dalam batas grid
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    // Mengubah posisi dunia nyata menjadi Node di grid
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x - transform.position.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.y - transform.position.y + gridWorldSize.y / 2) / gridWorldSize.y;
        
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }
    
    // Fungsi GetDistance dihilangkan dari perhitungan FindPath, tapi tetap harus ada
    // jika Anda ingin menggunakannya untuk hal lain atau mempertahankan struktur kode.
    // Jika tidak diperlukan, Anda bisa menghapusnya, TAPI jika Anda menggunakan
    // `GetDistance` di tempat lain, JANGAN HAPUS.
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
    
    // Merekonstruksi jalur dari node tujuan kembali ke node awal
    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse(); // Balikkan jalur agar dari awal ke akhir
        return path;
    }
    
    // --- Bagian D: Debug Visualisasi ---
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null && displayGridGizmos)
        {
            // Pengecekan keamanan
            if (startPosition == null) return;
            
            Node playerNode = NodeFromWorldPoint(startPosition.position);
            foreach (Node n in grid)
            {
                // Warna rintangan merah, bisa dilalui putih
                Gizmos.color = (n.isWalkable) ? Color.white : Color.red;
                
                // Warna node AI (Start) menjadi biru
                if (playerNode == n) Gizmos.color = Color.cyan;
                
                // Warna node target menjadi kuning
                if (targetPosition != null && NodeFromWorldPoint(targetPosition.position) == n) Gizmos.color = Color.yellow;
                
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
            }
        }
    }
}