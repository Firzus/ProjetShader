using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TMP_Text text;


    [SerializeField] private float initialYVelocity = 7f;
    [SerializeField] private float initialXVelocityRange = 3f;
    [SerializeField] private float lifeTime = 0.8f;


    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = new Vector2(Random.Range(-initialXVelocityRange, initialXVelocityRange), initialYVelocity);
        Destroy(gameObject, lifeTime);
    }

    public void SetMessage(string message)
    {
        text.SetText(message);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
