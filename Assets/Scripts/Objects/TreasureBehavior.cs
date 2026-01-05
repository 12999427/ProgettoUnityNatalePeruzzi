using UnityEngine;

public class TreasureBehavior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        I_PlayerInteractions playerInteractions = null;
        if ((playerInteractions = other.gameObject.GetComponent<I_PlayerInteractions>()) != null)
        {
            playerInteractions.CollectibleTreasureInteraction(other.transform, this.gameObject);
            Destroy(this.gameObject);
        }
    }
}
