using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obs : MonoBehaviour
{
    Vector3 pos;

    void Start()
    {
        this.pos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        this.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        this.transform.position = this.pos;
        this.transform.rotation = Quaternion.identity;
    }
}
