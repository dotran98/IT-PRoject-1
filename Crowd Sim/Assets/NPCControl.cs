using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCControl : MonoBehaviour
{
    int spawnTime = 5;
    float timer = 0f;
    bool spawning = false;
    public Transform spawn;
    public GameObject prefab;

    public int maxNumberBots;
    int count = 1;

    public GameObject bomb;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!spawning)
        {
            timer += Time.deltaTime;
        }

        if(timer >= spawnTime)
        {
            if (count < maxNumberBots)
            {
                Spawn();
                count++;
            }
        }
    }

    void Spawn()
    {
        spawning = true;
        timer = 0;

        Transform location;

        location = spawn;
        Instantiate(prefab, location.position, location.rotation);

        StartCoroutine(Slow());

        spawning = false;
    }

    IEnumerator Slow()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(1);
    }
}
