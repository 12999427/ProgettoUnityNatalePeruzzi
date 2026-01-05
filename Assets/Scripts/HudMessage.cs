using UnityEngine;
using TMPro;
using System.Collections;

public class HUDMessage : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] TextMeshProUGUI messageText_time;

    Coroutine hideCoroutine;

    void Awake()
    {
        panel.SetActive(false);
    }

    public void ShowMessage(string s)
    {
        // mostra subito
        panel.SetActive(true);
        messageText.text = s;

        //resetta il timer
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay(5f));
    }

    public void UpdateTimeMessage(string s)
    {
        messageText_time.text = s;
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
        hideCoroutine = null;
    }
}
