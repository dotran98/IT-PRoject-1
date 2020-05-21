using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Bird : MonoBehaviour
{
    public GameObject manager;

    public Vector2 location;

    public Vector2 velocity;

    float detectDistance;

    Vector2 goalPosition = Vector2.zero;

    Vector2 curForce;

    void Start()
    {
        this.velocity =
            new Vector2(Random.Range(3.0f, 5.0f), Random.Range(3.0f, 5.0f));
        location =
            new Vector2(this.gameObject.transform.position.x,
                this.gameObject.transform.position.y);
        detectDistance = 20.0f;
    }

    Vector2 seek(Vector2 target)
    {
        return (target - location).normalized;
    }

    void applyForce(Vector2 f)
    {
        Vector3 force = new Vector3(f.x, f.y, 0);
        if (force.magnitude > manager.GetComponent<Flock>().maxForce)
        {
            force = f.normalized;
            force *= manager.GetComponent<Flock>().maxForce;
        }

        this.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Force);

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
        Vector2 avoidForce = Vector2.zero;
        Vector2 redirect = Vector2.zero;
        foreach (GameObject obs in manager.GetComponent<Flock>().obstacle)
        {
            Vector3 pos = this.transform.position;
            Vector3 obsPos = obs.transform.position;

            float dis = Vector3.Distance(pos, obsPos);

            if (dis <= this.detectDistance)
            {
                Vector3 dir = obsPos - pos;
                Vector2 direction2D = new Vector2(dir.x, dir.y);
                if (Vector2.Angle(this.velocity, direction2D) < 90)
                {
                    if (direction2D.x < 0)
                        redirect = Vector2.Perpendicular(direction2D);
                    else
                        redirect = -Vector2.Perpendicular(direction2D);

                    if (dis <= 5)
                    {
                        avoidForce = redirect / dis * 50;
                    }
                    else
                    {
                        avoidForce = redirect / dis * 10;
                    }
                }
            }
        }
        return avoidForce;
    }

    Vector2 separate()
    {
        Vector2 repulsiveForce = Vector2.zero;

        foreach (GameObject i in manager.GetComponent<Flock>().birds)
        {
            if (this.name != i.name)
            {
                Vector2 pos = i.GetComponent<Bird>().location;
                float distance = Vector2.Distance(this.location, pos);
                bool isNeighbor = distance < this.detectDistance ? true : false;

                if (isNeighbor)
                {
                    repulsiveForce += (this.location - pos) / distance;
                }
            }
        }
        if (repulsiveForce.magnitude != 0)
            return repulsiveForce.normalized;
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
        {
            avg = (sum / count) - velocity;
            avg = avg.normalized;
        }
        return avg;
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

        float[] w = { 1.0f, 1.0f, 1.5f, 1.0f };
        curForce = w[0] * ali + w[1] * co + w[2] * sep + w[3] * avoid;
        if (manager.GetComponent<Flock>().seekGoal)
        {
            Vector2 goal = seek(goalPosition);
            curForce += goal;
        }

        applyForce (curForce);
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
            float new_x = -stageDimensions.x;
            this.transform.position = new Vector3(new_x, -location.y, 0);
        }
        if (this.location.x <= -stageDimensions.x)
        {
            this.location = this.transform.position;
            float new_x = stageDimensions.x;
            this.transform.position = new Vector3(new_x, -location.y, 0);
        }
        if (this.location.y >= stageDimensions.y)
        {
            this.location = this.transform.position;
            float new_y = -stageDimensions.y;
            this.transform.position = new Vector3(-location.x, new_y, 0);
        }
        if (this.location.y <= -stageDimensions.y)
        {
            this.location = this.transform.position;
            float new_y = stageDimensions.y;
            this.transform.position = new Vector3(-location.x, new_y, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        goalPosition = manager.transform.position;

        flock();
        stayInBorder();
    }
}
