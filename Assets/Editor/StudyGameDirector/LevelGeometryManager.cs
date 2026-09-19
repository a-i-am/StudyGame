using UnityEngine;
using UnityEngine.ProBuilder;
using System.Collections.Generic;

namespace StudyGame.Editor.Director
{
    public static class LevelGeometryManager
    {
        public static void Rebuild(LevelGeometryData data)
        {
            var oldGo = GameObject.Find("PB_LevelGeometry");
            if (oldGo != null)
            {
                Object.DestroyImmediate(oldGo);
            }

            if (data.FloorCells == null || data.FloorCells.Count == 0) return;

            List<Vector3> positions = new List<Vector3>();
            List<Face> faces = new List<Face>();

            float s = data.GridSize;
            float h = data.WallHeight;
            
            HashSet<Vector2Int> cellSet = new HashSet<Vector2Int>(data.FloorCells);

            foreach (var cell in data.FloorCells)
            {
                float x = cell.x * s;
                float z = cell.y * s;

                // Floor Quad (Facing UP: +Y)
                int v = positions.Count;
                positions.Add(new Vector3(x, 0, z)); // 0
                positions.Add(new Vector3(x + s, 0, z)); // 1
                positions.Add(new Vector3(x, 0, z + s)); // 2
                positions.Add(new Vector3(x + s, 0, z + s)); // 3
                faces.Add(new Face(new int[] { v, v+2, v+1, v+1, v+2, v+3 }));

                if (data.GenerateCeiling)
                {
                    // Ceiling Quad (Facing DOWN: -Y)
                    int vc = positions.Count;
                    positions.Add(new Vector3(x, h, z)); // 0
                    positions.Add(new Vector3(x + s, h, z)); // 1
                    positions.Add(new Vector3(x, h, z + s)); // 2
                    positions.Add(new Vector3(x + s, h, z + s)); // 3
                    faces.Add(new Face(new int[] { vc, vc+1, vc+2, vc+1, vc+3, vc+2 }));
                }

                if (data.GenerateWalls)
                {
                    // North (z+1)
                    if (!cellSet.Contains(new Vector2Int(cell.x, cell.y + 1)))
                    {
                        int vw = positions.Count;
                        positions.Add(new Vector3(x, 0, z + s)); // BL
                        positions.Add(new Vector3(x + s, 0, z + s)); // BR
                        positions.Add(new Vector3(x, h, z + s)); // TL
                        positions.Add(new Vector3(x + s, h, z + s)); // TR
                        faces.Add(new Face(new int[] { vw, vw+2, vw+1, vw+1, vw+2, vw+3 })); 
                    }
                    // South (z-1)
                    if (!cellSet.Contains(new Vector2Int(cell.x, cell.y - 1)))
                    {
                        int vw = positions.Count;
                        positions.Add(new Vector3(x + s, 0, z)); // BL
                        positions.Add(new Vector3(x, 0, z)); // BR
                        positions.Add(new Vector3(x + s, h, z)); // TL
                        positions.Add(new Vector3(x, h, z)); // TR
                        faces.Add(new Face(new int[] { vw, vw+2, vw+1, vw+1, vw+2, vw+3 })); 
                    }
                    // East (x+1)
                    if (!cellSet.Contains(new Vector2Int(cell.x + 1, cell.y)))
                    {
                        int vw = positions.Count;
                        positions.Add(new Vector3(x + s, 0, z + s)); // BL
                        positions.Add(new Vector3(x + s, 0, z)); // BR
                        positions.Add(new Vector3(x + s, h, z + s)); // TL
                        positions.Add(new Vector3(x + s, h, z)); // TR
                        faces.Add(new Face(new int[] { vw, vw+2, vw+1, vw+1, vw+2, vw+3 })); 
                    }
                    // West (x-1)
                    if (!cellSet.Contains(new Vector2Int(cell.x - 1, cell.y)))
                    {
                        int vw = positions.Count;
                        positions.Add(new Vector3(x, 0, z)); // BL
                        positions.Add(new Vector3(x, 0, z + s)); // BR
                        positions.Add(new Vector3(x, h, z)); // TL
                        positions.Add(new Vector3(x, h, z + s)); // TR
                        faces.Add(new Face(new int[] { vw, vw+2, vw+1, vw+1, vw+2, vw+3 })); 
                    }
                }
            }

            if (positions.Count > 0)
            {
                var pbMesh = ProBuilderMesh.Create(positions, faces);
                pbMesh.gameObject.name = "PB_LevelGeometry";
                
                var col = pbMesh.gameObject.AddComponent<MeshCollider>();
                col.sharedMesh = pbMesh.GetComponent<MeshFilter>().sharedMesh;

                pbMesh.ToMesh();
                pbMesh.Refresh();
            }
        }
    }
}
