using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager
{
    public Dictionary<int, GameObject> birdFlock;

    public Manager()
    {
        birdFlock = new Dictionary<int, GameObject>();
    }
}
