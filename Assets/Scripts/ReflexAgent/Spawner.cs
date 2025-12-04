using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject target;
    // Variabel maxSpawns diubah namanya menjadi maxActiveObjects
    public int maxActiveObjects = 10; 
    public float spawnTime = 1f; // Jeda waktu antara setiap spawn
    public LayerMask spawnLayer;
    
    // Waktu hancur objek
    public float destroyAfterSeconds = 5f; 

    // Daftar untuk melacak objek aktif (lebih aman daripada hanya hitungan)
    private List<GameObject> activeTargets = new List<GameObject>(); 

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Spawning());
    }

    IEnumerator Spawning()
    {
        // Loop ini akan berjalan selamanya
        while (true)
        {
            // Cek: Apakah jumlah objek aktif (activeTargets.Count) kurang dari batas?
            if (activeTargets.Count < maxActiveObjects)
            {
                Spawn();
            }
            
            // Tunggu jeda waktu sebelum mencoba spawn lagi
            yield return new WaitForSeconds(spawnTime); 
        }
    }

    void Spawn()
    {
        // Coba untuk menemukan lokasi spawn yang valid (Hanya 1x coba per 'spawnTime')
        float randX = Random.Range(-6f, 6f);
        float randY = Random.Range(-3.5f, 3.5f);

        Vector2 randomLocation = new Vector2(randX, randY);

        Collider2D hit = Physics2D.OverlapCircle(randomLocation, 4, spawnLayer);
        
        // Jika lokasi kosong
        if (hit == null) 
        {
            // 1. Instantiate objek
            GameObject newTarget = Instantiate(target, randomLocation, Quaternion.identity);
            
            // 2. Tambahkan ke daftar objek aktif
            activeTargets.Add(newTarget);

            // 3. Panggil Coroutine untuk menghancurkan objek DAN menghapusnya dari daftar
            StartCoroutine(DestroyAndRemove(newTarget, destroyAfterSeconds));
        }
        // Jika lokasi tidak kosong, tidak terjadi apa-apa, dan loop akan menunggu 'spawnTime' lagi
    }

    // Coroutine baru untuk menangani penghancuran dan manajemen daftar
    IEnumerator DestroyAndRemove(GameObject targetToDestroy, float delay)
    {
        // Tunggu selama waktu destroy
        yield return new WaitForSeconds(delay); 

        // Cek apakah objek masih ada sebelum menghancurkannya (mencegah error)
        if (targetToDestroy != null)
        {
            // Hancurkan objek (Destroy)
            Destroy(targetToDestroy);
            
            // Hapus objek dari daftar pelacak
            activeTargets.Remove(targetToDestroy);
        }
        // Jika objek sudah null (mungkin dihancurkan oleh skrip lain), cukup hapus dari daftar
        else if (activeTargets.Contains(targetToDestroy))
        {
            activeTargets.Remove(targetToDestroy);
        }
    }
}