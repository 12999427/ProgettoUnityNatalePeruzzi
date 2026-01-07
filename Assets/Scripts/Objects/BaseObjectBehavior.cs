using UnityEngine;

public abstract class BaseObjectBehavior : MonoBehaviour
{
    protected abstract void OnTriggerEnter(Collider other);
    public I_PlayerInteractions playerInteractions;

}
