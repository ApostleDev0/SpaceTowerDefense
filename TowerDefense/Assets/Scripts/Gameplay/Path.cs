using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Path : MonoBehaviour
{
    #region Serialized Fields
    [SerializeField] private Transform[] waypoints;
    #endregion

    #region Private Properties
    private LineRenderer _line;
    #endregion

    #region Public Properties
    public int WaypointCount => waypoints != null ? waypoints.Length : 0;
    #endregion

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }
    private void Update()
    {
        // check before config texture
        if(_line != null && _line.material != null)
        {
            _line.material.mainTextureOffset -= new Vector2(Time.deltaTime * 0.2f, 0);
        }
    }
    public Vector3 GetPosition(int index)
    {
        // check index of waypoints
        if (waypoints == null || index < 0 || index >= waypoints.Length)
        {
            Debug.LogWarning($"Path: Index {index} is out of range!");
            return transform.position;
        }
        return waypoints[index].position;
    }

    private void SetupLineRenderer()
    {
        if (_line == null || waypoints == null || waypoints.Length == 0) return;

        _line.positionCount = waypoints.Length;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                _line.SetPosition(i, waypoints[i].position);
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // check waypoints
        if (waypoints == null || waypoints.Length == 0)
        {
            return;
        }
        
        for(int i = 0; i < waypoints.Length; i++)
        {
            if(waypoints[i] == null)
            {
                continue;
            }
            
            // display waypoint's name
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(waypoints[i].transform.position + Vector3.up * 0.7f, waypoints[i].name, style);

            // draw line connect waypoints
            if (i <  waypoints.Length - 1 && waypoints[i+1] != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i+1].transform.position);
            }
        }
    }
#endif
}
