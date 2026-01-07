using UnityEngine;

public class HideWallBehavior : BaseObjectBehavior
{
    protected override void OnTriggerEnter(Collider other)
    {
        playerInteractions.HideWallInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
