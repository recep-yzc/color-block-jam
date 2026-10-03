using System;
using System.Collections.Generic;

namespace ColorBlockJam.Gameplay.Logic
{
    internal sealed class SolverStates : IEqualityComparer<int>
    {
        private const int Offset = 128;
        private const byte Gone = byte.MaxValue;

        private readonly int length;
        private byte[] data;

        public SolverStates(int blockCount, int capacity)
        {
            length = Math.Max(1, blockCount * 2);
            data = new byte[Math.Max(16, capacity) * length];
        }

        public int Count { get; private set; }

        public int Next => Count;

        public void Write(Board board)
        {
            var needed = (Count + 1) * length;
            if (data.Length < needed)
            {
                Array.Resize(ref data, Math.Max(needed, data.Length * 2));
            }

            var offset = Count * length;
            var blocks = board.Blocks;
            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                data[offset + i * 2] = block.IsCleared ? Gone : (byte)(block.Position.X + Offset);
                data[offset + i * 2 + 1] = block.IsCleared ? Gone : (byte)(block.Position.Y + Offset);
            }
        }

        public int Commit()
        {
            return Count++;
        }

        public void Restore(int index, Board board, GridPoint[] positions, bool[] cleared)
        {
            var offset = index * length;
            for (var i = 0; i < positions.Length; i++)
            {
                var x = data[offset + i * 2];
                cleared[i] = x == Gone;
                positions[i] = cleared[i] ? board.Blocks[i].Position : new GridPoint(x - Offset, data[offset + i * 2 + 1] - Offset);
            }

            board.SetState(positions, cleared);
        }

        public bool Equals(int first, int second)
        {
            var a = first * length;
            var b = second * length;
            for (var i = 0; i < length; i++)
            {
                if (data[a + i] != data[b + i])
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(int index)
        {
            unchecked
            {
                var hash = (int)2166136261;
                var offset = index * length;
                for (var i = 0; i < length; i++)
                {
                    hash = (hash ^ data[offset + i]) * 16777619;
                }

                return hash;
            }
        }
    }
}
