using Unity.VisualScripting;
using UnityEngine;

public class EndLevelBehavior : BaseObjectBehavior
{
    protected override void OnTriggerEnter(Collider other)
    {
        playerInteractions.EndLevelInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
