using Unity.VisualScripting;
using UnityEngine;

public class EndLevelBehavior : MonoBehaviour
{
    public I_PlayerInteractions playerInteractions;

    private void OnTriggerEnter(Collider other)
    {
        playerInteractions.EndLevelInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
