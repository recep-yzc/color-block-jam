using ColorBlockJam.Level;
using UnityEngine;

namespace ColorBlockJam.Gameplay
{
    internal readonly struct DoorMesh
    {
        public readonly BoardSide Side;
        public readonly int From;
        public readonly int To;
        public readonly Vector3 Pivot;
        public readonly Mesh Mesh;

        public DoorMesh(BoardSide side, int from, int to, Vector3 pivot, Mesh mesh)
        {
            Side = side;
            From = from;
            To = to;
            Pivot = pivot;
            Mesh = mesh;
        }
    }
}
