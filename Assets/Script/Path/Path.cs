using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace IPath
{
    public enum SearchResult
    {
        Ok,
        CantGetCloser,
        GoalOutOfRange,
        Error
    };

    public class Path
    {
        public Path()
        {
            goalRadius = -1f;
            pathCost = -1f;
        }

        public Stack<Vector3> path = new Stack<Vector3>();
        public Stack<Vector2Int> squares; 

        public Vector3 desiredGoal;
        public Vector3 pathGoal;

        public float pathCost;
        public float goalRadius;
    }
}

