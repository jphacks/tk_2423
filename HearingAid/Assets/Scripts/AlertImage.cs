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
        this.gameObject.SetActive(false);
    }

    void Update()
    {
        //Debug.Log(micAS.now_dB);
        if (micAS != null && micAS.now_dB > -20.0f && !isAlerting)
        {
            // Debug.Log("Alert!");
            this.gameObject.SetActive(true);
            AlertImage_.enabled = true;
            Invoke("ClearAlert", 3.0f);
            
            isAlerting = true;
        }
    }

    void ClearAlert()
    {
        if (AlertImage_ != null)
        {
            isAlerting = false;
            this.gameObject.SetActive(false);
        }
    }
}
