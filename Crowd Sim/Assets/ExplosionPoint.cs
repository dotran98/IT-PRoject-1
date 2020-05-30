using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPoint : MonoBehaviour
{
    public GameObject explosionPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                Instantiate(explosionPrefab, hit.point, rot);
            }

        }
    }
}
