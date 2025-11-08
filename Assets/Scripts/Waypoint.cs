using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Waypoint
{
    [SerializeField]
    public Vector3 pos;
    public void SetPos(Vector3 newPos)
    {
        pos = newPos;

    }
    public Vector3 GetPos()
    {
        return pos;

    }
    public Waypoint()
    {
        pos = new Vector3(0, 0, 0);
    }
}
