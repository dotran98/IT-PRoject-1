/**
 * Controls the explosion effect.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCExplode : MonoBehaviour
{

    //bomb-making variables
    public GameObject bomb;
    public float power = 10.0f;
    public float radius = 5.0f;
    public float upforce = 1.0f;
 

    // Start is called before the first frame update
    void Start()
    {
        //if the bomb is enabled on the camera, set the detonate
        if (bomb == enabled)
        {
            Invoke("Detonate", 0);
        }
    }

    void FixedUpdate()
    {
        
    }

    //detonate and add force within a certain radius (using a particle system as the visual)
    void Detonate()
    {
        Vector3 explosionPosition = bomb.transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPosition, radius);
        ParticleSystem exp = this.GetComponent<ParticleSystem>();

        foreach(Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if(rb != null) //if there are no agents in the area, do not detonate, else play the explosion and add force
            {
                exp.Play();
                rb.AddExplosionForce(power, explosionPosition, radius, upforce, ForceMode.Impulse);
            }

        }
    }

}


