using UnityEngine;
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

        public List<GameTile> path;
        public List<Vector2Int> squares;

        public GameTile desiredGoal;
        public GameTile pathGoal;

        public float pathCost;
        public float goalRadius;
    }
}

