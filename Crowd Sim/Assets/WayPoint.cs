/*** 
 * Sets up the waypoint gizmo. Only used in the Unity scene. Not in game mode.
 * Invisible waypoints that the NPC agents use to navigate the mesh.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    [SerializeField]
    protected float debugDrawRadius = 1.0F;

    public virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, debugDrawRadius);
    }
}
