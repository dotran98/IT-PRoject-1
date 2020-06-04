/**
 * Controls the spawning of the NPC agents.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCControl : MonoBehaviour
{
    //all required variables
    int spawnTime = 5;
    float timer = 0f;
    bool spawning = false;
    public Transform spawn;
    public GameObject prefab; //the bot

    public int maxNumberBots; //controls the max number of bots the user wants spawned
    int count = 1;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if it is not spawning, increase the timer
        if (!spawning)
        {
            timer += Time.deltaTime;
        }

        //if the timer is greater than spawn time and there are not enough bots, spawn more and increase counter
        if(timer >= spawnTime)
        {
            if (count < maxNumberBots)
            {
                Spawn();
                count++;
            }
        }
    }

    //Spawn a NPC agent in the required spawn position
    void Spawn()
    {
        spawning = true;
        timer = 0;

        Transform location;

        location = spawn;
        Instantiate(prefab, location.position, location.rotation);

        StartCoroutine(Slow()); //slows the spawn so it waits

        spawning = false;
    }

    IEnumerator Slow()
    {
        //yield on a new YieldInstruction that waits for 1 seconds.
        yield return new WaitForSeconds(1);
    }
}
