using UnityEngine;
using UnityEditor;
using AI;
public class AIWaypointManagerWindow : EditorWindow
{
    [MenuItem("Window/AI/Waypoint Editor")]
    public static void Open()
    {
        GetWindow<AIWaypointManagerWindow>();
    }

    public Transform waypointRoot;
    public bool drawGizmos = true;

    private void OnGUI()
    {
        SerializedObject sobj = new SerializedObject(this);

        EditorGUILayout.BeginHorizontal("box");

        EditorGUILayout.PropertyField(sobj.FindProperty("waypointRoot"));

        EditorGUILayout.PropertyField(sobj.FindProperty("drawGizmos"));

        if (waypointRoot == null)
        {
            EditorGUILayout.HelpBox("Assign a root transform.", MessageType.Warning);
        }

        if (GUILayout.Button("Find nearest Waypoint"))
        {
            GameObject[] waypoints = GameObject.FindGameObjectsWithTag("Waypoint");
            GameObject[] o = Selection.gameObjects;
            AISpawnpoint w;
            foreach (GameObject obj in o)
            {
                if (obj.TryGetComponent<AISpawnpoint>(out w))
                {
                    Undo.RecordObject(w, "Get nearest Waypoint");
                    float d = Mathf.Infinity;
                    GameObject closest = null;
                    foreach (GameObject g in waypoints)
                    {
                        float di = (g.transform.position - obj.transform.position).sqrMagnitude;
                        if (di < d)
                        {
                            d = di;
                            closest = g;
                        }
                    }
                    w.firstWaypoint = closest.GetComponent<AIWaypoint>();
                    PrefabUtility.RecordPrefabInstancePropertyModifications(w);
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        if (waypointRoot != null)
        {
            EditorGUILayout.BeginHorizontal("box");
            DrawButtons();
            EditorGUILayout.EndHorizontal();
        }

        sobj.ApplyModifiedProperties();
    }

    private void DrawButtons()
    {
        if (GUILayout.Button("Create Waypoint"))
        {
            CreateWaypoint();
        }

        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<AIWaypoint>())
        {
            if (GUILayout.Button("Create Waypoint Before"))
            {
                CreateWaypointBefore();
            }
            if (GUILayout.Button("Create Waypoint After"))
            {
                CreateWaypointAfter();
            }
            if (GUILayout.Button("Create Branch"))
            {
                CreateBranch();
            }
            if (GUILayout.Button("Remove Waypoint"))
            {
                RemoveWaypoint();
            }
        }
    }

    private void CreateWaypoint()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(AIWaypoint));
        waypointObject.tag = "Waypoint";
        waypointObject.transform.SetParent(waypointRoot, false);
        AIWaypoint waypoint = waypointObject.GetComponent<AIWaypoint>();
        if (waypointRoot.childCount > 1)
        {
            waypoint.previousWaypoint = waypointRoot.GetChild(waypointRoot.childCount - 2).GetComponent<AIWaypoint>();
            waypoint.previousWaypoint.nextWaypoint = waypoint;
            waypoint.transform.position = waypoint.previousWaypoint.transform.position;
            waypoint.transform.forward = waypoint.previousWaypoint.transform.forward;
        }
        Selection.activeGameObject = waypoint.gameObject;
    }
    private void CreateWaypointBefore()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(AIWaypoint));
        waypointObject.tag = "Waypoint";
        waypointObject.transform.SetParent(waypointRoot, false);
        AIWaypoint newWaypoint = waypointObject.GetComponent<AIWaypoint>();

        AIWaypoint selectedWaypoint = Selection.activeGameObject.GetComponent<AIWaypoint>();
        waypointObject.transform.position = selectedWaypoint.transform.position;
        waypointObject.transform.forward = selectedWaypoint.transform.forward;
        if (selectedWaypoint.previousWaypoint != null)
        {
            newWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
            selectedWaypoint.previousWaypoint.nextWaypoint = newWaypoint;
        }
        newWaypoint.nextWaypoint = selectedWaypoint;
        selectedWaypoint.previousWaypoint = newWaypoint;

        newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());
        Selection.activeGameObject = newWaypoint.gameObject;
    }
    private void CreateWaypointAfter()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(AIWaypoint)){ tag = "Waypoint" };
        waypointObject.transform.SetParent(waypointRoot, false);
        AIWaypoint newWaypoint = waypointObject.GetComponent<AIWaypoint>();

        AIWaypoint selectedWaypoint = Selection.activeGameObject.GetComponent<AIWaypoint>();
        waypointObject.transform.position = selectedWaypoint.transform.position;
        waypointObject.transform.forward = selectedWaypoint.transform.forward;
        if (selectedWaypoint.nextWaypoint != null)
        {
            newWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint;
            selectedWaypoint.nextWaypoint.previousWaypoint = newWaypoint;
        }
        newWaypoint.previousWaypoint = selectedWaypoint;
        selectedWaypoint.nextWaypoint = newWaypoint;

        newWaypoint.transform.SetSiblingIndex(selectedWaypoint.transform.GetSiblingIndex());
        Selection.activeGameObject = newWaypoint.gameObject;
    }
    private void RemoveWaypoint()
    {
        AIWaypoint selectedWaypoint = Selection.activeGameObject.GetComponent<AIWaypoint>();
        if (selectedWaypoint.nextWaypoint != null)
        {
            selectedWaypoint.nextWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
        }
        if (selectedWaypoint.previousWaypoint != null)
        {
            selectedWaypoint.previousWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint;
            Selection.activeGameObject = selectedWaypoint.previousWaypoint.gameObject;
        }

        DestroyImmediate(selectedWaypoint.gameObject);
    }

    private void CreateBranch()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(AIWaypoint));
        waypointObject.tag = "Waypoint";
        waypointObject.transform.SetParent(waypointRoot, false);
        AIWaypoint newWaypoint = waypointObject.GetComponent<AIWaypoint>();
        AIWaypoint branchedFrom = Selection.activeGameObject.GetComponent<AIWaypoint>();
        branchedFrom.branches.Add(new BranchOption(newWaypoint));
        newWaypoint.transform.position = branchedFrom.transform.position;
        newWaypoint.transform.forward = branchedFrom.transform.forward;
        Selection.activeGameObject = newWaypoint.gameObject;
    }
}


[InitializeOnLoad()]
public class AIWaypointEditor
{
    static AIWaypointEditor()
    {    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]

    public static void OnDrawSceneGizmo(AIWaypoint waypoint, GizmoType gizmoType)
    {
        if (!EditorWindow.HasOpenInstances<AIWaypointManagerWindow>())
        { return; }
        AIWaypointManagerWindow a = EditorWindow.GetWindow<AIWaypointManagerWindow>();
        if(!a.drawGizmos)
        { return; }

        if ((gizmoType & GizmoType.Selected) != 0)
        {
            Gizmos.color = Color.yellow;
        }
        else
        {
            Gizmos.color = Color.yellow * 0.5f;
        }

        float mlt = Mathf.Clamp(waypoint.width, 1f, 2.5f);

        Gizmos.DrawSphere(waypoint.transform.position, 0.3f * mlt);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(waypoint.transform.position + (waypoint.transform.right * waypoint.width / 2f),
            waypoint.transform.position - (waypoint.transform.right * waypoint.width / 2f));
        if (waypoint.previousWaypoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 offset = waypoint.transform.right * waypoint.width / 2f;
            Vector3 offsetTo = waypoint.previousWaypoint.transform.right * waypoint.previousWaypoint.width / 2f;

            Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.previousWaypoint.transform.position + offsetTo);
        }
        if (waypoint.nextWaypoint != null)
        {
            Gizmos.color = Color.green;
            Vector3 offset = waypoint.transform.right * -waypoint.width / 2f;
            Vector3 offsetTo = waypoint.nextWaypoint.transform.right * -waypoint.nextWaypoint.width / 2f;

            Gizmos.DrawLine(waypoint.transform.position + offset, waypoint.nextWaypoint.transform.position + offsetTo);
        }

        if (waypoint.branches != null)
        {
            for (int i = 0; i < waypoint.branches.Count; ++i)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(waypoint.transform.position, waypoint.branches[i].waypoint.transform.position);
            }
        }
    }
}