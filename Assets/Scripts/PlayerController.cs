using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Variabel publik untuk KeyCode (dapat diatur di Inspector)
    public KeyCode moveUp = KeyCode.W;
    public KeyCode moveDown = KeyCode.S;
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;

    public Animator anim;
    public float moveSpeed = 5f; // Memberi nilai default
    private Rigidbody2D rb;

    private float x;
    private float y;

    private Vector2 input;
    private bool moving;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        
        // Pastikan komponen Rigidbody2D ada
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component missing on the player!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Animate();
    }

    private void FixedUpdate()
    {
        // Menerapkan kecepatan gerakan pada Rigidbody
        rb.velocity = input * moveSpeed;
    }

    void GetInput()
    {
        // Mengatur nilai x dan y berdasarkan KeyCode yang DITEKAN
        x = 0;
        y = 0;

        if (Input.GetKey(moveUp))
        {
            y = 1;
        }
        else if (Input.GetKey(moveDown))
        {
            y = -1;
        }

        if (Input.GetKey(moveRight))
        {
            x = 1;
        }
        else if (Input.GetKey(moveLeft))
        {
            x = -1;
        }
        
        // Masukkan input ke dalam Vector2 'input'
        input = new Vector2(x, y);
        
        // Normalisasi agar kecepatan diagonal tidak lebih cepat
        input.Normalize(); 
    }

    private void Animate()
    {
        // Cek apakah ada input gerakan sama sekali (magnitude > 0)
        // Kita menggunakan nilai x atau y mentah sebelum normalisasi untuk cek ini
        moving = (x != 0 || y != 0); 
        
        // Kirim status moving ke Animator (Asumsi: bool parameter di Animator bernama "moving")
        if (anim != null)
        {
            anim.SetBool("moving", moving);

            if (moving)
            {
                // Kirim nilai input mentah (x dan y) ke Animator untuk transisi arah
                // (Asumsi: float parameter di Animator bernama "X" dan "Y")
                anim.SetFloat("X", x);
                anim.SetFloat("Y", y);
            }
        }
    }
}