using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public GameObject[] Waypoints;

    private void OnDrawGizmos()
    {
        if(Waypoints.Length > 0)
        {
            for(int i = 0; i < Waypoints.Length; i++)
            {
                if(i <  Waypoints.Length - 1)
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawLine(Waypoints[i].transform.position, Waypoints[i+1].transform.position);

                }
                
            }
        }
    }
}
