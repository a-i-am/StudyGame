using UnityEditor;
using UnityEngine;
using StudyGame.Data;

namespace StudyGame.Editor.Director
{
    /// <summary>
    /// 디렉터 허브 에디터 윈도우 내에서 탭 간의 상태(Context)를 공유하기 위한 전역 상태 관리자.
    /// </summary>
    public static class DirectorStateManager
    {
        public static EpisodeNode ActiveEpisodeNode { get; private set; }
        public static DynamicProperty ActivePhaseProperty { get; private set; }
        public delegate void StateChangedHandler();
        public static event StateChangedHandler OnStateChanged;

        public static void SetActiveContext(EpisodeNode node, DynamicProperty phase = null)
        {
            ActiveEpisodeNode = node;
            ActivePhaseProperty = phase;
            OnStateChanged?.Invoke();
        }
    }
}
