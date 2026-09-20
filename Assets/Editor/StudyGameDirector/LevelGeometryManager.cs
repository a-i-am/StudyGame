using UnityEngine;
using UnityEngine.ProBuilder;
using System.Collections.Generic;

namespace StudyGame.Editor.Director
{
    public static class LevelGeometryManager
    {
        public static LevelGeometryData CreateDefault20x20Grid()
        {
            var data = new LevelGeometryData();
            data.GridSize = 1.0f;
            data.WallHeight = 3.0f;
            data.GenerateWalls = true;
            data.GenerateCeiling = false;

            for (int x = -10; x < 10; x++)
            {
                for (int z = -10; z < 10; z++)
                {
                    data.FloorCells.Add(new Vector2Int(x, z));
                }
            }
            return data;
        }

        /*
         * [CRITICAL MESH GENERATION RULES]
         * 1. DO NOT make faces double-sided to "fix" backface culling.
         * 2. This level editor uses a 'Sims-like' dollhouse view. The walls MUST be single-sided and face OUTWARDS.
         *    - This ensures walls facing the camera (outside) are visible.
         *    - Walls facing away from the camera (blocking the view of the interior) are naturally culled,
         *      allowing the player/designer to look INSIDE the room effortlessly.
         * 3. Winding Order:
         *    - Floor: Faces UP (+Y). Indices: 0, 2, 1 (BL, TL, BR) -> Clockwise.
         *    - Ceiling: Faces DOWN (-Y). Flipped indices so it is visible from the inside, culled from the top down.
         *    - Walls: Must face OUTWARD from the room. 
         * 4. Any future modifications to LevelGeometryManager MUST preserve this single-sided culling behavior.
         */
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
                    // Ceiling Quad (Double-sided so it can be seen from top-down editor view and from inside)
                    int vc = positions.Count;
                    positions.Add(new Vector3(x, h, z)); // 0
                    positions.Add(new Vector3(x + s, h, z)); // 1
                    positions.Add(new Vector3(x, h, z + s)); // 2
                    positions.Add(new Vector3(x + s, h, z + s)); // 3
                    
                    // Facing DOWN (-Y, visible from inside)
                    var fDown = new Face(new int[] { vc, vc+1, vc+2, vc+1, vc+3, vc+2 });
                    fDown.submeshIndex = 1;
                    faces.Add(fDown);
                    
                    // Facing UP (+Y, visible from outside/top-down)
                    var fUp = new Face(new int[] { vc, vc+2, vc+1, vc+1, vc+2, vc+3 });
                    fUp.submeshIndex = 1;
                    faces.Add(fUp);
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
                
                // Assign Default Material to fix magenta error
                var renderer = pbMesh.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material defaultMat = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
                    if (defaultMat == null) defaultMat = new Material(Shader.Find("Standard"));
                    
                    Material ceilMat = new Material(Shader.Find("Transparent/Diffuse"));
                    ceilMat.color = new Color(1, 1, 1, data.CeilingOpacity);
                    
                    Material[] mats = new Material[2];
                    mats[0] = defaultMat;
                    mats[1] = ceilMat;
                    renderer.sharedMaterials = mats;
                }
                pbMesh.ToMesh();
                pbMesh.Refresh();
                
                // Add MeshCollider for physics
                var collider = pbMesh.gameObject.GetComponent<MeshCollider>();
                if (collider == null) collider = pbMesh.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = pbMesh.GetComponent<MeshFilter>().sharedMesh;
            }
        }
    }
}
