using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    public float floatAmplitude = 0.2f;   // Metà dell'altezza del movimento su/giù
    public float floatFrequency = 1f;     // Velocità dell'oscillazione su/giù

    public Vector3 rotationSpeed = new Vector3(0f, 30f, 0f); // Gradi al secondo

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        //Movimento verticale in base alla funzione seno
        float newY = startPosition.y + (1+Mathf.Sin(Time.time * floatFrequency)) * floatAmplitude;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);

        //Rotazione costante
        transform.Rotate(rotationSpeed * Time.deltaTime); //indipendente dal frame rate
    }
}
