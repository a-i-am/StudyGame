using UnityEngine;
using StudyGame.Player;

public class AnomalyEncounter : MonoBehaviour
{
    public ScriptableObject anomalyData;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.SetMovementEnabled(false);
            }

            GetComponent<Collider>().enabled = false;

            if (BattleManager.Instance != null)
            {
                BattleManager.Instance.StartBattle(anomalyData, playerController);
            }
        }
    }
}