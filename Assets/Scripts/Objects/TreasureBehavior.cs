using UnityEngine;

public class TreasureBehavior : BaseObjectBehavior
{
    protected override void OnTriggerEnter(Collider other)
    {
        playerInteractions.CollectibleTreasureInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
