using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AlertImage : MonoBehaviour
{
    private bool isAlerting = false;
    private Image AlertImage_;

    [SerializeField]
    private MicAudioSource micAS = null;

    void Start()
    {
        AlertImage_ = GetComponent<Image>();
        AlertImage_.enabled = false;
    }

    void Update()
    {
        Debug.Log(micAS.now_dB);
        if (micAS != null && micAS.now_dB > -20.0f && !isAlerting)
        {
            // Debug.Log("Alert!");
            
            AlertImage_.enabled = true;
            Invoke("ClearAlert", 3.0f);
            
            isAlerting = true;
        }
    }

    void ClearAlert()
    {
        if (AlertImage_ != null)
        {
            AlertImage_.enabled = false;
            isAlerting = false;
        }
    }
}
