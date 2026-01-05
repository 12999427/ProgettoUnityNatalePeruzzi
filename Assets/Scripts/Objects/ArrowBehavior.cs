using UnityEngine;

public class ArrowBehavior : MonoBehaviour
{
    public I_PlayerInteractions playerInteractions;

    private void OnTriggerEnter(Collider other)
    {
        playerInteractions.ArrowBonusInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
