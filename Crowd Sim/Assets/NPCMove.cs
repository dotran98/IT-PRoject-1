using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCMove : MonoBehaviour
{
    public GameObject[] wps;
    public GameObject sp;
    public NavMeshAgent agent;


    //use this for initialisation
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.tag = "AI";
        int d = UnityEngine.Random.Range(0, wps.Length);
        agent.SetDestination(wps[d].transform.position);
    }

    //update is called once per frame
    private void Update()
    {

        if (agent.remainingDistance < 0.5)
        {
            int d = UnityEngine.Random.Range(0, wps.Length);
            agent.SetAreaCost(4, 20);
            agent.SetDestination(wps[d].transform.position);

        }
        else if (agent.hasPath)
        {
            agent.isStopped = false;

            Vector3 toTarget = agent.steeringTarget - this.transform.position;
            float turnAngle = Vector3.Angle(this.transform.forward, toTarget);
            agent.acceleration = turnAngle * agent.speed;
        }
    }
}
