using UnityEngine;
using TMPro;

public class Alert : MonoBehaviour
{
    private bool isAlerting = false;
    private TMP_Text AlertText;

    [SerializeField]
    private MicAudioSource micAS = null;

    void Start()
    {
        AlertText = GetComponent<TMP_Text>();
        if (AlertText == null)
        {
            Debug.LogError("TextMeshPro component is missing on this GameObject.");
        }

        if (micAS == null)
        {
            Debug.LogError("MicAudioSource is not assigned.");
        }
    }

    void Update()
    {
        Debug.Log(micAS.now_dB);
        if (micAS != null && micAS.now_dB > -20.0f && !isAlerting)
        {
            Debug.Log("Alert!");
            if (AlertText != null)
            {
                AlertText.text = "注意！";
                Invoke("ClearAlert", 3.0f);
            }
            isAlerting = true;
        }
    }

    void ClearAlert()
    {
        if (AlertText != null)
        {
            AlertText.text = "";
            isAlerting = false;
        }
    }
}
