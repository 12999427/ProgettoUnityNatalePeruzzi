using UnityEngine;

public class ClockBehavior : MonoBehaviour
{
    public I_PlayerInteractions playerInteractions;

    private void OnTriggerEnter(Collider other)
    {
        playerInteractions.TimeBonusInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
