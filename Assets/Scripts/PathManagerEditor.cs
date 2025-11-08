using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathManager))]
public class PathManagerEditor : Editor
{
    [SerializeField]
    PathManager pathManager;

    [SerializeField]
    List<Waypoint> thePath;
    List<int> toDelete;

    Waypoint selectedPoint = null;
    bool doRepaint = true;

    private void OnSceneGUI()
    {
        thePath = pathManager.GetPath();
        DrawPath(thePath);
    }

    private void OnEnable()
    {
        pathManager = target as PathManager;
        toDelete = new List<int>();
    }

    override public void OnInspectorGUI()
    {
        this.serializedObject.Update();
        thePath = pathManager.GetPath();

        base.OnInspectorGUI();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Path");

        DrawGUIForPoints();

        // Button for adding a point to the path
        if (GUILayout.Button("Add Point to Path"))
        {
            pathManager.CreateAddPoint();
        }

        EditorGUILayout.EndVertical();
        SceneView.RepaintAll();
    }



    public void DrawPath(List<Waypoint> path)
    {
        if (path!=null)
        {
            int current = 0;
            foreach(Waypoint wp in path)
            { doRepaint = DrawPoint(wp);
            int next = (current + 1) % path.Count;
                Waypoint wpnext = path[next];
                DrawPathLine(wp, wpnext);

                current++;

            }


        }
        if (doRepaint) Repaint();

    }

public void DrawPathLine(Waypoint p1,Waypoint p2)
    {
        Color c = Handles.color;
        Handles.color = Color.gray;
        Handles.DrawLine(p1.GetPos(), p2.GetPos());
        Handles.color = c;
    }


    public bool DrawPoint(Waypoint p)
{
    bool isChanged = false;

    if (selectedPoint == p)
    {
        Color c = Handles.color;
        Handles.color = Color.green;

        EditorGUI.BeginChangeCheck();
            Vector3 oldpos = p.GetPos();

        Vector3 newPos = Handles.PositionHandle(p.pos, Quaternion.identity);

            float handleSize = HandleUtility.GetHandleSize(newPos);
        Handles.SphereHandleCap(1, newPos, Quaternion.identity, 0.25f * handleSize, EventType.Repaint);
        if (EditorGUI.EndChangeCheck())
        {
            p.pos = newPos;
            isChanged = true;
        }

        Handles.color = c;
    }
    else
    {
        Vector3 currPos = p.GetPos();
        float handleSize = HandleUtility.GetHandleSize(currPos);
        if (Handles.Button(currPos, Quaternion.identity, 0.25f * handleSize, 0.25f * handleSize, Handles.SphereHandleCap))
        {
            isChanged = true;
            selectedPoint = p;
        }
    }

    return isChanged;
}
    void DrawGUIForPoints()
    {
        if (thePath != null && thePath.Count > 0)
        {
            for (int i = 0; i < thePath.Count; i++)
            {
               Waypoint p = thePath[i];
                EditorGUILayout.BeginHorizontal();
           

                Vector3 oldPos = p.GetPos();
                Vector3 newPos = EditorGUILayout.Vector3Field("", oldPos);
                p.SetPos(newPos);

                // the Delete button
                if (GUILayout.Button("-", GUILayout.Width(25)))
                {
                    toDelete.Add(i); // add the index to our delete list
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        if (toDelete.Count > 0)
        {
            foreach (int i in toDelete)
                thePath.RemoveAt(i); // remove from the path
            toDelete.Clear(); // clear the delete list for the next time
        }
    }





}
