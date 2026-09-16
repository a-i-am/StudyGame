using UnityEngine;
using StudyGame.Data;

namespace StudyGame.Managers
{
    public class ExplorationStageRunnerLauncher : MonoBehaviour
    {
        public StageScenarioData targetScenario;

        private void Start()
        {
            if (StageRunnerController.Instance != null && targetScenario != null)
            {
                StageRunnerController.Instance.LoadAndRunScenario(targetScenario);
            }
        }
    }
}
