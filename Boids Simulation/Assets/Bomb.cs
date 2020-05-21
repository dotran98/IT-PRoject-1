﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    float bombPower;

    // Start is called before the first frame update
    void Start()
    {
        bombPower = 600000.0f;
    }

    void FixedUpdate()
    {
        Object.Destroy(this.gameObject, 1.0f);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Vector3 bomb = this.transform.position;
        Vector3 target = col.gameObject.transform.position;

        Vector3 force = target - bomb;

        float dis = Vector3.Distance(bomb, target);

        force = force.normalized / dis * bombPower;
        col.gameObject.GetComponent<Rigidbody2D>().AddForce(force);
    }
}
