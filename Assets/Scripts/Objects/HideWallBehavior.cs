using UnityEngine;

public class HideWallBehavior : MonoBehaviour
{
    public I_PlayerInteractions playerInteractions;

    private void OnTriggerEnter(Collider other)
    {
        playerInteractions.HideWallInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
