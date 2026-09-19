# Mesh Generation Constraints & Winding Order Rules

```csharp
// [STRICT REQUIREMENT]
// DO NOT use double-sided quads for walls. Single-sided outward facing normals ONLY.
// This enables backface culling from exterior views (Sims-style dollhouse).

void AddQuad(Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr, bool flip = false)
{
    int v = positions.Count;
    positions.Add(bl); // 0
    positions.Add(br); // 1
    positions.Add(tl); // 2
    positions.Add(tr); // 3
    
    // Default Clockwise Winding: 0 -> 2 -> 1 (BL -> TL -> BR)
    // Cross Product: (v2 - v0) x (v1 - v0)
    // Result: Normal faces OUTWARD / UPWARD
    if (flip)
    {
        // Counter-Clockwise Winding: 0 -> 1 -> 2 (BL -> BR -> TL)
        // Result: Normal faces INWARD / DOWNWARD
        faces.Add(new Face(new int[] { v, v+1, v+2, v+1, v+3, v+2 }));
    }
    else
    {
        faces.Add(new Face(new int[] { v, v+2, v+1, v+1, v+2, v+3 }));
    }
}
```

```math
// Vector Mapping Constraints:
// Floor (+Y Normal): bl=(x,0,z), br=(x+s,0,z), tl=(x,0,z+s), tr=(x+s,0,z+s) | flip=false
// Ceiling (-Y Normal): bl=(x,h,z), br=(x+s,h,z), tl=(x,h,z+s), tr=(x+s,h,z+s) | flip=true
// North Wall (+Z Normal): bl=(x,0,z+s), br=(x+s,0,z+s), tl=(x,h,z+s), tr=(x+s,h,z+s) | flip=false
// South Wall (-Z Normal): bl=(x+s,0,z), br=(x,0,z), tl=(x+s,h,z), tr=(x,h,z) | flip=false
// East Wall (+X Normal): bl=(x+s,0,z+s), br=(x+s,0,z), tl=(x+s,h,z+s), tr=(x+s,h,z) | flip=false
// West Wall (-X Normal): bl=(x,0,z), br=(x,0,z+s), tl=(x,h,z), tr=(x,h,z+s) | flip=false
```
