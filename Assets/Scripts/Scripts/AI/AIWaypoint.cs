using System.Collections.Generic;
using UnityEngine;
namespace AI
{
    public class AIWaypoint : MonoBehaviour
    {
        public AIWaypoint previousWaypoint;
        public AIWaypoint nextWaypoint;
        public List<BranchOption> branches;

        [Range(0f, 10f)]
        public float width = 1f;

        [HideInInspector]public bool isUsed = false;
        public float minWaitTime = 0f;
        public float maxWaitTime = 0f;

        public Vector3 GetPosition()
        {
            Vector3 minBound = transform.position + transform.right * width / 2f;
            Vector3 maxBound = transform.position - transform.right * width / 2f;

            return Vector3.Lerp(minBound, maxBound, Random.Range(0f, 1f));
        }
    }

    [System.Serializable]
    public class BranchOption
    {
        public AIWaypoint waypoint;
        public bool allowBranchingBack = false;
        [Range(0f, 1f)]
        public float ratio = 0.5f;

        public BranchOption(AIWaypoint _waypoint)
        {
            waypoint = _waypoint;
        }
    }
}