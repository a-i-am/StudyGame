using UnityEngine;
using TMPro;

public class TestScript : MonoBehaviour
{
    public TMP_Text myText;
    private string fullText = "첫번째 문장. 두번째 문장.";

    private void Start()
    {
        if (myText != null)
        {
            string[] sentences = fullText.Split('.');
            myText.text = sentences[0];
        }
    }

    public void OnButtonClick()
    {
        if (myText != null)
        {
            myText.color = Color.yellow;
        }
    }
}
