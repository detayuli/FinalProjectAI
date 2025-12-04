using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIChase : MonoBehaviour
{
    private Transform player;  
    public float chaseSpeed = 3f;  
    public float distanceBetween = 5f; 
    public float distance;

    void Start()
    {
        // Cari player berdasarkan tag
        GameObject p = GameObject.FindGameObjectWithTag("Player1");

        if (p != null)
        {
            player = p.transform;
        }
        else
        {
            Debug.LogWarning("Player with tag 'Player' not found!");
        }
    }

    void Update()
    {
        if (player == null) return; // Tidak ada player → AI diam

        distance = Vector2.Distance(transform.position, player.position);

        if (distance < distanceBetween)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                chaseSpeed * Time.deltaTime
            );

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }
}
