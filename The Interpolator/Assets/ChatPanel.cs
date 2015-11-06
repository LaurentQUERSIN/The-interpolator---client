using UnityEngine;
using UnityEngine.UI;

public class ChatPanel : MonoBehaviour
{

    public Text chat;
    public InputField playerText;
    public Button sendButton;

    public bool _connected = false;
    private bool _isWrtting = false;

    void update()
    {
        //if (Input.GetKeyDown("enter") && _connected == true)
        //{
        //    if (_isWrtting == false)
        //    {
        //        _isWrtting = true;
        //        playerText.Select();
        //    }
        //    else
        //    {
        //        _isWrtting = false;
        //        sendButton.onClick.Invoke();
        //    }
        //}
    }
    
}
