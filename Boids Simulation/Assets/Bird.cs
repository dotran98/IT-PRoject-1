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

    Vector3 goalPosition = Vector3.zero;

    Vector2 added_velo;

    void Start()
    {
        this.GetComponent<Rigidbody2D>().velocity = new Vector2(1, 1);
        location =
            new Vector2(this.gameObject.transform.position.x,
                this.gameObject.transform.position.y);
        detectDistance = 25.0f;
    }

    Vector2 seek(Vector2 target)
    {
        return (target - location).normalized;
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
                Vector3 dir = obsPos - pos;
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
        {
            avg = (sum / count) - velocity;
        }
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
        transform.up = this.velocity;

        if (this.velocity.magnitude == 0)
        {
            this.GetComponent<Rigidbody2D>().velocity =
                new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        }

        flock();
        stayInBorder();
    }
}
