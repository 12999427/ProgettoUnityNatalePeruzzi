using UnityEngine;

public class ArrowBehavior : BaseObjectBehavior
{
    protected override void OnTriggerEnter(Collider other)
    {
        playerInteractions.ArrowBonusInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
