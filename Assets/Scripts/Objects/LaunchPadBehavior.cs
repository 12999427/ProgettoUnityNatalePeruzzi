using UnityEngine;

public class LaunchPadBehavior : MonoBehaviour
{
    public float jumpForce = 3f; //Forza (N) del salto

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            //Azzera velocità verticale
            Vector3 v = rb.linearVelocity;
            v.y = 0f;
            rb.linearVelocity = v;

            // Forza verso l'alto di jumpForce
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
