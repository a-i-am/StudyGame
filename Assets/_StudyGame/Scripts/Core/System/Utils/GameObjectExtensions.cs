using UnityEngine;

namespace StudyGame.Utils
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// [Rule Validation] 기계적 검증: 컴포넌트 중복 추가(RequireComponent 충돌 등)로 인한 
        /// NullReferenceException을 원천 차단하는 Safe Wrapper.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }
    }
}
