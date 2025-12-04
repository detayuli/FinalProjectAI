using UnityEngine;
using System.Collections; // Wajib untuk Coroutine
using System.Collections.Generic;

public class AI_Mover : MonoBehaviour
{
    // Ubah nama dari 'target' menjadi 'patrolEnd' agar lebih jelas
    public Transform patrolEnd; 
    public float moveSpeed = 5f;
    public float waitTimeAtEnd = 1.0f; // Waktu tunggu di setiap ujung patroli

    private List<Node> path;
    private Grid grid;
    private int targetIndex;
    private Vector3 currentWaypoint;
    
    // Simpan posisi awal AI
    private Vector3 patrolStart; 

    void Start()
    {
        grid = FindFirstObjectByType<Grid>(); 

        if (grid == null)
        {
            Debug.LogError("Grid script not found in the scene! Cannot start patrol.");
            enabled = false;
            return;
        }
        
        // Simpan posisi awal AI saat game dimulai
        patrolStart = transform.position; 
        
        // Hapus Update() lama dan mulai Coroutine Patroli
        StartCoroutine(PatrolLoop()); 
    }

    // Coroutine utama untuk mengatur urutan patroli
    IEnumerator PatrolLoop()
    {
        // Loop abadi untuk patroli bolak-balik
        while (true)
        {
            // --- 1. Bergerak dari Start ke End ---
            yield return StartCoroutine(MoveToTarget(patrolEnd.position));
            
            // Tunggu sebentar di posisi akhir
            yield return new WaitForSeconds(waitTimeAtEnd); 

            // --- 2. Bergerak dari End kembali ke Start ---
            yield return StartCoroutine(MoveToTarget(patrolStart));

            // Tunggu sebentar di posisi awal
            yield return new WaitForSeconds(waitTimeAtEnd); 
        }
    }
    
    // Fungsi baru yang meminta jalur ke tujuan tertentu
    void RequestPath(Vector3 destination)
    {
        // Cek apakah targetnya null
        if (patrolEnd == null && destination != patrolStart)
        {
            Debug.LogError("Patrol End Target is not set!");
            path = null;
            return;
        }

        path = grid.FindPath(transform.position, destination);
        
        if (path != null && path.Count > 0)
        {
            targetIndex = 0;
            currentWaypoint = path[0].worldPosition;
        }
        else
        {
            // Jika jalur gagal ditemukan, set path ke null
            path = null;
            Debug.LogWarning($"Path not found to {destination}. Skipping this leg of patrol.");
        }
    }
    
    // Coroutine yang menangani pergerakan AI mengikuti jalur
    IEnumerator MoveToTarget(Vector3 destination)
    {
        // Minta jalur baru ke tujuan
        RequestPath(destination);

        // Tunggu hingga jalur berhasil ditemukan
        while (path == null)
        {
            yield return null; 
            // Tambahkan jeda jika jalur tidak ditemukan
            yield break;
        }

        // Mulai pergerakan
        while (targetIndex < path.Count)
        {
            // Pindah AI menuju waypoint saat ini
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, moveSpeed * Time.deltaTime);
            
            // Jika AI sudah mencapai waypoint saat ini
            if (transform.position == currentWaypoint)
            {
                targetIndex++;
                
                // Jika sudah mencapai waypoint terakhir, keluar dari loop pergerakan
                if (targetIndex >= path.Count)
                {
                    break; 
                }
                // Tetapkan waypoint berikutnya
                currentWaypoint = path[targetIndex].worldPosition;
            }
            
            yield return null; // Tunggu 1 frame
        }

        // Bersihkan jalur setelah mencapai tujuan
        path = null; 
    }
    
    // ... (OnDrawGizmos tetap sama) ...
    void OnDrawGizmos()
    {
        if (grid != null && path != null)
        {
            for (int i = targetIndex; i < path.Count; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(path[i].worldPosition, Vector3.one * (grid.nodeRadius * 2 - .2f));

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i].worldPosition);
                }
                else if (i > 0)
                {
                    Gizmos.DrawLine(path[i-1].worldPosition, path[i].worldPosition);
                }
            }
        }
    }
}