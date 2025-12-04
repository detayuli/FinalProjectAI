using UnityEngine;
using UnityEngine.UI;

public class DynamicSplitScreen : MonoBehaviour
{
    // Objek Transform dari kedua pemain yang akan dilacak
    public Transform player1;
    public Transform player2;

    // Kamera yang akan digunakan untuk split-screen
    public Camera camera1;
    public Camera camera2;

    [Header("Pengaturan Split")]
    // Jarak minimum agar layar tidak split (dalam unit Unity)
    public float minSplitDistance = 5f; 
    
    // Kecepatan transisi kamera saat split/gabung
    public float transitionSpeed = 5f; 

    // **TAMBAHAN BARU:** Referensi ke objek GameObject Garis Pembatas UI
    [Header("Pengaturan UI")]
    public GameObject splitBorder; 
    
    // VARIABEL BARU: Untuk menyimpan RectTransform garis pembatas
    private RectTransform borderRectTransform;
    
    // VARIABEL BARU: Tinggi target (gunakan nilai besar seperti 1200 atau 2000)
    public float borderTargetHeight = 1200f; 

    private bool isSplit = false;

    void Start()
    {
        if (splitBorder != null)
        {
            borderRectTransform = splitBorder.GetComponent<RectTransform>();
            splitBorder.SetActive(false);
        }

        if (borderRectTransform == null && splitBorder != null)
        {
            Debug.LogError("SplitBorder tidak memiliki komponen RectTransform!");
            enabled = false;
        }
    }

    void Update()
    {
        if (player1 == null || player2 == null || camera1 == null || camera2 == null || borderRectTransform == null)
        {
            if (enabled) Debug.LogError("Player, Camera, atau SplitBorder belum diatur dengan benar!");
            enabled = false;
            return;
        }

        // Hitung jarak antara kedua pemain
        float distance = Vector3.Distance(player1.position, player2.position);

        // Tentukan apakah harus split atau gabung
        if (distance > minSplitDistance && !isSplit)
        {
            isSplit = true;
        }
        else if (distance <= minSplitDistance && isSplit)
        {
            isSplit = false;
        }

        // Panggil fungsi yang mengelola tampilan kamera
        UpdateCameraViews();
    }

    void UpdateCameraViews()
    {
        // ----------------------------------------------------
        // LOGIKA SMOOTHING GARIS PEMBATAS
        // ----------------------------------------------------

        // 1. Tentukan tinggi target (0 jika gabung, 1200f jika split)
        float targetHeight = isSplit ? borderTargetHeight : 0f;
        
        // 2. Lakukan Interpolasi Linear (Lerp)
        float newHeight = Mathf.Lerp(
            borderRectTransform.sizeDelta.y, 
            targetHeight, 
            Time.deltaTime * transitionSpeed * 2 // Dipercepat 2x agar transisi lebih terasa responsif
        );

        // 3. Terapkan tinggi baru ke RectTransform
        borderRectTransform.sizeDelta = new Vector2(borderRectTransform.sizeDelta.x, newHeight);

        // 4. Kontrol Visibilitas (SetActive)
        if (isSplit)
        {
            // Aktifkan garis pembatas hanya jika tingginya mulai naik
            if (!splitBorder.activeSelf && newHeight > 1f)
            {
                splitBorder.SetActive(true);
            }
        }
        else
        {
            // Nonaktifkan garis pembatas hanya jika tingginya sudah sangat dekat dengan nol
            if (splitBorder.activeSelf && newHeight < 0.1f)
            {
                splitBorder.SetActive(false);
            }
        }
        
        // ----------------------------------------------------
        // LOGIKA KAMERA & SPLIT
        // ----------------------------------------------------
        
        if (isSplit)
        {
            // --- Mode Split-Screen ---
            
            // Kamera 1: Setengah kiri
            Rect targetRect1 = new Rect(0f, 0f, 0.5f, 1f); 
            // Kamera 2: Setengah kanan
            Rect targetRect2 = new Rect(0.5f, 0f, 0.5f, 1f);

            // Transisi halus (Viewport Rect)
            camera1.rect = SmoothRectTransition(camera1.rect, targetRect1);
            camera2.rect = SmoothRectTransition(camera2.rect, targetRect2);

            SetCameraTarget(camera1, player1);
            SetCameraTarget(camera2, player2);
            
            // Aktifkan kedua kamera
            camera1.enabled = true;
            camera2.enabled = true;
        }
        else
        {
            // --- Mode Layar Gabung (Unified Screen) ---

            // Target rektangel untuk layar penuh
            Rect targetRectFull = new Rect(0f, 0f, 1f, 1f);

            // Gabungkan tampilan Kamera 1 menjadi layar penuh
            camera1.rect = SmoothRectTransition(camera1.rect, targetRectFull);
            
            // Matikan Kamera 2
            camera2.enabled = false;

            // Posisikan Kamera 1 di antara kedua pemain
            Vector3 centerPoint = (player1.position + player2.position) / 2f;
            Vector3 targetPosition = new Vector3(centerPoint.x, centerPoint.y, camera1.transform.position.z);
            
            // Gerakkan kamera 1 dengan halus
            camera1.transform.position = Vector3.Lerp(camera1.transform.position, targetPosition, Time.deltaTime * transitionSpeed);
        }
    }
    
    // Fungsi untuk membuat transisi Rect (Viewport) lebih halus
    Rect SmoothRectTransition(Rect current, Rect target)
    {
        float x = Mathf.Lerp(current.x, target.x, Time.deltaTime * transitionSpeed);
        float y = Mathf.Lerp(current.y, target.y, Time.deltaTime * transitionSpeed);
        float w = Mathf.Lerp(current.width, target.width, Time.deltaTime * transitionSpeed);
        float h = Mathf.Lerp(current.height, target.height, Time.deltaTime * transitionSpeed);
        
        return new Rect(x, y, w, h);
    }

    // Fungsi bantu untuk memastikan kamera melacak target (hanya saat split)
    void SetCameraTarget(Camera cam, Transform target)
    {
        Vector3 targetPos = new Vector3(target.position.x, target.position.y, cam.transform.position.z);
        cam.transform.position = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * transitionSpeed);
    }
}