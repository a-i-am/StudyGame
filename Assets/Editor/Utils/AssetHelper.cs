using UnityEditor;
using UnityEngine;
using System.IO;

namespace StudyGame.Editor.Utils
{
    public static class AssetHelper
    {
        /// <summary>
        /// [Rule Validation] 기계적 검증: CreateAsset 호출 시 기존 에셋이 존재하면 덮어쓰지 못하고 
        /// 조용히 실패하는 유니티 API의 함정을 원천 차단하는 Safe Wrapper.
        /// </summary>
        public static void CreateOrOverwriteAsset(Object asset, string path)
        {
            Debug.Assert(asset != null, "[AssetHelper] Cannot create an asset from a null object.");
            Debug.Assert(!string.IsNullOrEmpty(path), "[AssetHelper] Cannot create an asset with an empty path.");
            Debug.Assert(path.StartsWith("Assets/"), "[AssetHelper] Asset path must start with 'Assets/'. Path: " + path);

            if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
