using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StudyGame.Editor.Director
{
    [InitializeOnLoad]
    public static class NodeProxyManager
    {
        private static Dictionary<string, GameObject> _proxies = new Dictionary<string, GameObject>();
        private static Transform _root;

        private static void EnsureRoot()
        {
            if (_root == null)
            {
                var go = GameObject.Find("Director_ProxyRoot");
                if (go == null)
                {
                    go = new GameObject("Director_ProxyRoot");
                    // Optionally hide in hierarchy if desired, but we might want users to see it in Editor mode.
                    go.hideFlags = HideFlags.DontSave; 
                }
                _root = go.transform;
            }
        }

        public static void UpdateProxy(string nodeId, Vector3 position, Color color, DirectorNodeType type)
        {
            EnsureRoot();

            if (!_proxies.TryGetValue(nodeId, out GameObject proxy) || proxy == null)
            {
                string prefabPath = UnityEditor.EditorPrefs.GetString($"StudyGameDirector_PrefabPath_{type}", "Assets/Prefabs");
                string assetPath = $"{prefabPath}/{type.ToString()}.prefab";
                
                GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                
                if (prefab != null)
                {
                    proxy = Object.Instantiate(prefab);
                    proxy.name = $"NodeProxy_{nodeId}";
                    // Ensure it has a collider for Raycast
                    if (proxy.GetComponent<Collider>() == null)
                    {
                        proxy.AddComponent<BoxCollider>();
                    }
                }
                else
                {
                    // Fallback to Capsule
                    proxy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    proxy.name = $"NodeProxy_{nodeId}";
                    Object.DestroyImmediate(proxy.GetComponent<CapsuleCollider>()); // Re-add Box for easier clicking
                    proxy.AddComponent<BoxCollider>();
                }
                
                proxy.transform.SetParent(_root);
                proxy.hideFlags = HideFlags.DontSave;
                _proxies[nodeId] = proxy;
            }

            proxy.transform.position = position;

            // Update color if it has a renderer (only for primitive fallback or simple materials)
            var renderer = proxy.GetComponentInChildren<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                // We shouldn't modify actual prefab materials permanently, so create an instance for the proxy
                if (proxy.GetComponent<MeshFilter>() != null) // It's the fallback capsule
                {
                    var mat = new Material(Shader.Find("Standard"));
                    mat.color = color;
                    renderer.material = mat;
                }
            }
        }

        public static void RemoveProxy(string nodeId)
        {
            if (_proxies.TryGetValue(nodeId, out GameObject proxy))
            {
                if (proxy != null)
                {
                    Object.DestroyImmediate(proxy);
                }
                _proxies.Remove(nodeId);
            }
        }
        
        public static void ClearAll()
        {
            foreach (var proxy in _proxies.Values)
            {
                if (proxy != null) Object.DestroyImmediate(proxy);
            }
            _proxies.Clear();
        }
    }
}
