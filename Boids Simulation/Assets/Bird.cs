using System.Collections;
using System.Collections.Generic;
using System.Net;
using EspduSender;
using OpenDis.Core;
using OpenDis.Dis1998;
using OpenDis.Enumerations;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public GameObject manager;

    public Vector2 location;

    public Vector2 velocity;

    float detectDistance;

    Vector3 goalPosition = Vector3.zero;

    Vector2 added_velo;

    public Vector2 prev_location;

    EntityStatePdu espdu;

    public bool sendNewEspdu;

    public float threshold = 5.0f; // The threshold of position change above which an Espdu is sent

    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<Rigidbody2D>().velocity = new Vector2(1, 1);
        location =
            new Vector2(this.gameObject.transform.position.x,
                this.gameObject.transform.position.y);
        detectDistance = 25.0f;
        prev_location = this.location; // This will be reset if the DeadReckoning threshold is passed and the Espdu is sent

        // Setting espdu's settings
        espdu = new EntityStatePdu();
        espdu.ExerciseID = 1;

        // Setting EntityID settings for this Bird
        EntityID eid = espdu.EntityID;
        eid.Site = 0;
        eid.Application = 1;
        eid.Entity = 2;
        EntityType entityType = espdu.EntityType;
        entityType.EntityKind = 3; // 3 means "lifeform" (as opposed to platform, environment etc)
        entityType.Country = 13; // These are Aussie birds ;)
        entityType.Domain = 2; // Air (vs land, surface, subsurface, space)
        entityType.Category = 0; // Other (it's a bird)
    }

    Vector2 seek(Vector2 target)
    {
        return (target - location);
    }

    Vector2 rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    void applyVelo(Vector2 added_velo)
    {
        Vector2 new_velo =
            this.GetComponent<Rigidbody2D>().velocity + added_velo;
        float angle = Vector2.Angle(new_velo, this.velocity);

        if (angle > 5.0f)
        {
            Vector2 dir = rotate(this.velocity, 5.0f).normalized;

            if (Vector2.Angle(dir, new_velo) > angle)
            {
                dir = rotate(this.velocity, -5.0f).normalized;
            }
            float mag =
                new_velo.magnitude *
                this.velocity.magnitude *
                Mathf.Sin(angle * Mathf.Deg2Rad) /
                (
                new_velo.magnitude * Mathf.Sin((angle - 5) * Mathf.Deg2Rad) +
                this.velocity.magnitude * Mathf.Sin(5 * Mathf.Deg2Rad)
                );

            this.GetComponent<Rigidbody2D>().velocity = dir * mag;
        }
        if (
            this.GetComponent<Rigidbody2D>().velocity.magnitude >
            manager.GetComponent<Flock>().maxVelo
        )
        {
            this.GetComponent<Rigidbody2D>().velocity =
                this.GetComponent<Rigidbody2D>().velocity.normalized;
            this.GetComponent<Rigidbody2D>().velocity *=
                manager.GetComponent<Flock>().maxVelo;
        }
    }

    Vector2 avoidObs()
    {
        Vector2 new_velo = Vector2.zero;
        Vector2 redirect = Vector2.zero;
        foreach (GameObject obs in manager.GetComponent<Flock>().obstacle)
        {
            Vector3 pos = this.transform.position;
            Vector3 obsPos = obs.transform.position;

            float dis = Vector3.Distance(pos, obsPos);

            if (dis <= this.detectDistance)
            {
                Vector3 dir = pos - obsPos;
                Vector2 direction2D = new Vector2(dir.x, dir.y);
                float angle = Vector2.Angle(this.velocity, direction2D);

                if (angle < 90)
                {
                    if (manager.GetComponent<Flock>().seekGoal)
                    {
                        Vector2 standard = goalPosition - obsPos;
                        float det =
                            standard.x * (-direction2D.y) -
                            standard.y * (-direction2D.x);
                        if (det < 0)
                            redirect =
                                -Vector2.Perpendicular(direction2D).normalized;
                        else
                            redirect =
                                Vector2.Perpendicular(direction2D).normalized;
                    }
                    else
                        redirect =
                            Vector2.Perpendicular(direction2D).normalized;
                }

                if (dis <= 5)
                {
                    new_velo = redirect * 5;
                }
                else
                {
                    new_velo = redirect * 2;
                }
            }
        }
        return new_velo;
    }

    Vector2 separate()
    {
        Vector2 new_velo = Vector2.zero;

        foreach (GameObject i in manager.GetComponent<Flock>().birds)
        {
            if (this.name != i.name)
            {
                Vector2 pos = i.GetComponent<Bird>().location;
                float distance = Vector2.Distance(this.location, pos);
                bool isNeighbor = distance < this.detectDistance ? true : false;

                if (isNeighbor)
                {
                    new_velo += (this.location - pos) / distance;
                }
            }
        }
        if (new_velo.magnitude != 0)
            return new_velo.normalized;
        else
            return Vector2.zero;
    }

    Vector2 align()
    {
        float neighborDis = this.detectDistance;
        Vector2 sum = Vector2.zero;
        Vector2 avg;
        int count = 0;

        foreach (GameObject i in manager.GetComponent<Flock>().birds)
        {
            if (this.name == i.name) continue;

            float temp =
                Vector2
                    .Distance(this.location, i.GetComponent<Bird>().location);

            if (temp < neighborDis)
            {
                sum += i.GetComponent<Bird>().velocity;
                count++;
            }
        }
        if (count == 0)
            avg = Vector2.zero;
        else
            avg = (sum / count) - velocity;

        return avg.normalized;
    }

    Vector2 cohesion()
    {
        float neighborDis = this.detectDistance;
        Vector2 sum = Vector2.zero;
        int count = 0;

        foreach (GameObject i in manager.GetComponent<Flock>().birds)
        {
            if (i.name == this.name) continue;

            float dis =
                Vector2
                    .Distance(this.GetComponent<Bird>().location,
                    i.GetComponent<Bird>().location);

            if (dis < neighborDis)
            {
                sum += i.GetComponent<Bird>().location;
                count++;
            }
        }

        sum = count == 0 ? Vector2.zero : seek(sum / count);

        return sum;
    }

    void flock()
    {
        location = transform.position;
        velocity = this.GetComponent<Rigidbody2D>().velocity;

        Vector2 ali = align();
        Vector2 co = cohesion();
        Vector2 sep = separate();
        Vector2 avoid = avoidObs();

        float[] w = { 1.0f, 1.5f, 2.0f, 1.0f };
        added_velo = w[0] * ali + w[1] * co + w[2] * sep + w[3] * avoid;
        if (manager.GetComponent<Flock>().seekGoal)
        {
            Vector2 goal = seek(goalPosition);
            added_velo += goal;
        }

        applyVelo (added_velo);
    }

    void stayInBorder()
    {
        Vector3 stageDimensions =
            Camera
                .main
                .ScreenToWorldPoint(new Vector3(Screen.width,
                    Screen.height,
                    0));

        if (this.location.x >= stageDimensions.x)
        {
            this.location = this.transform.position;
            this.transform.position = new Vector3(-location.x, -location.y, 0);
        }
        if (this.location.x <= -stageDimensions.x)
        {
            this.location = this.transform.position;
            this.transform.position = new Vector3(-location.x, -location.y, 0);
        }
        if (this.location.y >= stageDimensions.y)
        {
            this.location = this.transform.position;
            this.transform.position = new Vector3(-location.x, -location.y, 0);
        }
        if (this.location.y <= -stageDimensions.y)
        {
            this.location = this.transform.position;
            this.transform.position = new Vector3(-location.x, -location.y, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        goalPosition = manager.transform.position;
        transform.up = this.velocity;

        if (this.velocity.magnitude == 0)
        {
            this.GetComponent<Rigidbody2D>().velocity =
                new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        }

        flock();
        goalPosition = manager.transform.position;
        stayInBorder();

        this.GetComponent<Rigidbody2D>().rotation =
            this.GetComponent<Rigidbody2D>().angularVelocity * Time.deltaTime;

        if (
            (this.location.x - this.prev_location.x) > threshold // If position in x axis > threshold value, send Espdu
        )
        {
            this.prev_location = this.location;
            this.sendNewEspdu = true;
        }
        else if (
            (this.location.y - this.prev_location.y) > threshold // If position in y axis > threshold value, send Espdu
        )
        {
            this.prev_location = this.location;
            this.sendNewEspdu = true;
        }
        else
        {
            this.sendNewEspdu = false;
        }

        for (int i = 0; i < 1; i++)
        {
            // Declaring the position of the Bird (in WSP - World Space Position)
            Vector3Double loc = espdu.EntityLocation;
            loc.X = this.location.x;
            loc.Y = this.location.y;
            loc.Z = 0.0;

            // Declaring the Bird's velocity
            Vector3Float vel = espdu.EntityLinearVelocity;
            vel.X = this.velocity.x;
            vel.Y = this.velocity.y;
            vel.Z = 0.0f;

            // Declaring the DeadReckoning Algorithm to be used (R, P, W)
            espdu.DeadReckoningParameters.DeadReckoningAlgorithm = (byte) 2;

            //// THIS IS IN TESTING PHASE --------------------------------------------------------------------
            //// Setting the angular velocity of the Bird (as above)
            //Vector3Float angvelo = new Vector3Float
            //{
            //    X = this.GetComponent<Rigidbody2D>().angularVelocity,
            //    Y = 0.0f,
            //    Z = 0.0f
            //};
            //espdu.DeadReckoningParameters.EntityAngularVelocity = angvelo;
            //// END OF TESTING BLOCK ------------------------------------------------------------------------
            if (this.sendNewEspdu)
            {
                espdu.Timestamp = DisTime.DisRelativeTimestamp;

                // Prepare output
                DataOutputStream dos = new DataOutputStream(Endian.Big);
                espdu.MarshalAutoLengthSet (dos);

                // Transmit broadcast messages
                Sender.SendMessages(dos.ConvertToBytes());
                string mess =
                    string
                        .Format("Message sent with TimeStamp [{0}] Time Of[{1}]",
                        espdu.Timestamp,
                        (espdu.Timestamp >> 1));
                Debug.Log (mess);
            }
        }
    }
}
