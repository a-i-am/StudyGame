using UnityEngine;
using UnityEditor;
using StudyGame.Testing;

namespace StudyGame.Editor
{
    public static class GameViewInputSimulatorEditor
    {
        [MenuItem("StudyGame/Run Game View Input Test")]
        public static void RunInputTestInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[GameViewInputSimulator] Play Mode가 아닙니다. Play Mode 진입 후 시뮬레이션을 실행합니다.");
                EditorApplication.isPlaying = true;
                return;
            }

            var simulator = Object.FindFirstObjectByType<GameViewInputSimulator>();
            if (simulator == null)
            {
                var go = new GameObject("GameViewInputSimulator");
                simulator = go.AddComponent<GameViewInputSimulator>();
            }

            simulator.RunTestSequence();
        }
    }
}
