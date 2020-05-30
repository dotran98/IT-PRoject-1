using UnityEngine;
using UnityEngine.AI;
using OpenDis.Dis1998;
using OpenDis.Core;
using EspduSender;
using System.Net;

public class NPCMove : MonoBehaviour
{
    public GameObject[] wps;
    public GameObject sp;
    public NavMeshAgent agent;
    public Sender sender;
    public Vector3 prev_location;
    EntityStatePdu espdu;
    public bool sendNewEspdu;
    public float[] change;
    public Vector3 first_location;
    public float threshold = 2.0f;          // The threshold of position change above which an Espdu is sent; make it configurable?

    private int crossingMask;

    //use this for initialisation
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        int d = UnityEngine.Random.Range(0, wps.Length);
        crossingMask = 1 << NavMesh.GetAreaFromName("Crossing");
        agent.SetDestination(wps[d].transform.position);
        prev_location = this.agent.transform.position;

        change = new float[2];
        change[0] = 0;
        change[1] = 0;

        // Setting espdu's settings
        espdu = new EntityStatePdu();
        espdu.ExerciseID = 1;

        // Setting EntityID settings for this Bird
        EntityID eid = espdu.EntityID;
        eid.Site = 0;
        eid.Application = 1;
        eid.Entity = 2;
        EntityType entityType = espdu.EntityType;
        entityType.EntityKind = 3;      // 3 means "lifeform" (as opposed to platform, environment etc)
        entityType.Country = 13;        // These are Aussies
        entityType.Domain = 1;          // Land (vs air, surface, subsurface, space)
        entityType.Category = 130;      // Civilian (Entity Kind)

        // Setting the Espdu sender's target IP
        sender = new Sender(IPAddress.Parse("192.168.1.117"), 62040, 62040);        // Configure to target IP address/ports in .json?
        Sender.StartBroadcast();
    }

    //update is called once per frame
    private void Update()
    {
        NavMeshHit hit;
        if(!agent.SamplePathPosition(NavMesh.AllAreas, 0.0f, out hit))
        {
            if ((hit.mask & crossingMask) != 0)
            {
                int random = UnityEngine.Random.Range(8, 15);
                agent.speed = random;
            }
            else
            {
                agent.speed = 5;
            }
        }

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

        // Simple dead reckoning algorithm below; checking to see if distance moved since last espdu (change) > threshold value
        change[0] += Mathf.Abs(this.agent.transform.position.x - prev_location.x);
        prev_location.x = this.agent.transform.position.x;
        change[1] += Mathf.Abs(this.agent.transform.position.y - prev_location.y);
        prev_location.y = this.agent.transform.position.y;

        if ((change[0] > threshold) | (change[1] > threshold))
        {
            change[0] = 0;
            change[1] = 0;
            this.sendNewEspdu = true;
        }
        else
        {
            this.sendNewEspdu = false;
        }

        // Sending the new Espdu if necessary (if Dead Reckoning threshold passed)
        if (this.sendNewEspdu)
        {
            // Declaring the position of the Bot (in WSP - World Space Position)
            Vector3Double loc = espdu.EntityLocation;                                   // Issues here

            loc.X = this.agent.transform.position.x;
            loc.Y = this.agent.transform.position.y;
            loc.Z = 0.0;


            if (espdu.EntityLocation.X.Equals(null) | espdu.EntityLocation.Y.Equals(null))
            {
                Debug.LogError("Espdu's location value is NULL!!!");
            }

            // Declaring the Bot's velocity
            Vector3Float vel = espdu.EntityLinearVelocity;

            vel.X = this.agent.velocity.x;
            vel.Y = this.agent.velocity.y;
            vel.Z = 0.0f;

            if (espdu.EntityLinearVelocity.X.Equals(null) | espdu.EntityLinearVelocity.Y.Equals(null))
            {
                Debug.LogError("Espdu's linear velocity value is NULL!!!");
            }

            // Declaring the DeadReckoning Algorithm to be used (R, P, W)
            espdu.DeadReckoningParameters.DeadReckoningAlgorithm = (byte)2;

            // Sending the Espdu
            espdu.Timestamp = DisTime.DisRelativeTimestamp;

            // Prepare output
            DataOutputStream dos = new DataOutputStream(Endian.Big);
            espdu.MarshalAutoLengthSet(dos);

            // Transmit broadcast messages
            Sender.SendMessages(dos.ConvertToBytes());
            string mess = string.Format("Message sent with TimeStamp [{0}] Time Of[{1}]", espdu.Timestamp, (espdu.Timestamp >> 1));
            Debug.Log(mess);
            this.sendNewEspdu = false;
        }
    }
}