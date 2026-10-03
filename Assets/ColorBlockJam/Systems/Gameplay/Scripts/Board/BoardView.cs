using System.Collections.Generic;
using ColorBlockJam.Gameplay.Logic;
using ColorBlockJam.Level;
using UnityEngine;
using UnityEngine.Rendering;

namespace ColorBlockJam.Gameplay
{
    public sealed class BoardView : MonoBehaviour
    {
        private readonly List<Mesh> builtMeshes = new();
        private readonly List<DoorView> doorViews = new();
        private Transform staticParts;
        private Transform doorParts;
        private float cellSize;

        public Bounds WorldBounds { get; private set; }

        public void Build(Board board, BoardArt art, BlockPalette palette, GameplayConfig config)
        {
            cellSize = config.CellSize;
            staticParts = new GameObject("Board Parts").transform;
            staticParts.SetParent(transform, false);
            doorParts = new GameObject("Doors").transform;
            doorParts.SetParent(transform, false);

            var builder = new BoardMeshBuilder(board, art, palette, cellSize);
            var doors = new List<DoorMesh>();
            AddRenderer(staticParts, "Board", builder.BuildBoard(doors), art.GroundMaterial, art.WallMaterial);
            AddRenderer(staticParts, "Floor", builder.BuildFloor(), art.FloorMaterial).shadowCastingMode = ShadowCastingMode.Off;

            foreach (var door in doors)
            {
                var doorRoot = new GameObject(door.Mesh.name).transform;
                doorRoot.SetParent(doorParts, false);
                doorRoot.localPosition = door.Pivot;
                AddRenderer(doorRoot, "Mesh", door.Mesh, art.DoorMaterial);

                var doorView = doorRoot.gameObject.AddComponent<DoorView>();
                doorView.Initialize(door.Side, door.From, door.To, config);
                doorViews.Add(doorView);
            }

            var walls = 4f * art.WallHalfThickness;
            var size = new Vector3((board.Width + walls) * cellSize, cellSize, (board.Height + walls) * cellSize);
            WorldBounds = new Bounds(CellToWorld(new Vector2(board.Width * 0.5f, board.Height * 0.5f)), size);
        }

        public void PlayDoorEntry(BoardDoor door)
        {
            foreach (var doorView in doorViews)
            {
                if (doorView.Covers(door.Side, door.Start))
                {
                    doorView.PlayEntry();
                    return;
                }
            }
        }

        public Vector3 CellToWorld(Vector2 cell)
        {
            return transform.position + new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
        }

        public bool TryScreenToCell(Camera viewCamera, Vector2 screenPoint, float height, out Vector2 cell)
        {
            var ray = viewCamera.ScreenPointToRay(screenPoint);
            var plane = new Plane(Vector3.up, transform.position + Vector3.up * height);
            if (!plane.Raycast(ray, out var distance))
            {
                cell = default;
                return false;
            }

            var local = ray.GetPoint(distance) - transform.position;
            cell = new Vector2(local.x / cellSize, local.z / cellSize);
            return true;
        }

        public void Clear()
        {
            foreach (var mesh in builtMeshes)
            {
                Destroy(mesh);
            }

            builtMeshes.Clear();
            doorViews.Clear();
            if (staticParts != null)
            {
                Destroy(staticParts.gameObject);
            }

            if (doorParts != null)
            {
                Destroy(doorParts.gameObject);
            }
        }

        private void OnDestroy()
        {
            foreach (var mesh in builtMeshes)
            {
                Destroy(mesh);
            }
        }

        private MeshRenderer AddRenderer(Transform parent, string partName, Mesh mesh, params Material[] materials)
        {
            builtMeshes.Add(mesh);
            var part = new GameObject(partName);
            part.transform.SetParent(parent, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var meshRenderer = part.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterials = materials;
            return meshRenderer;
        }
    }
}
