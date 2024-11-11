using UnityEngine;
using TMPro;


public class SignLanguage : MonoBehaviour
{
    private TMP_Text SignText;

    public bool red = false;
    public bool me = false;
    public bool name = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SignText = GetComponent<TMP_Text>();
    }

    public void SetRed()
    {
        red = true;
    }
    public void ResetRed()
    {
        red = false;
    }
    public void SetMe()
    {
        me = true;
    }
    public void ResetMe()
    {
        me = false;
    }
    public void SetName()
    {
        name = true;
    }
    public void ResetName()
    {
        name = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (red)
        {
            SignText.text = "赤";
        }
        else if (me)
        {
            SignText.text = "私";
        }
        else if (name)
        {
            SignText.text = "名前";
        }
        else
        {
            SignText.text = "";
        }
    }
}
