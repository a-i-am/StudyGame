using UnityEngine;

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
                playerController.enabled = false;
            }

            GetComponent<Collider>().enabled = false;

            BattleManager.Instance.StartBattle(anomalyData, playerController);
        }
    }

}