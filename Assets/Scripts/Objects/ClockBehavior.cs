using UnityEngine;

public class ClockBehavior : BaseObjectBehavior
{

    protected override void OnTriggerEnter(Collider other)
    {
        playerInteractions.TimeBonusInteraction(transform.parent.parent.gameObject);
        Destroy(transform.parent.parent.gameObject);
    }
}
