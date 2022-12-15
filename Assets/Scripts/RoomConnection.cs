using Mirror;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class RoomConnection : MonoBehaviour
{
    [SerializeField] Button enterRoomButton;
    [SerializeField] InputField roomNameInput;
    private string roomName;

    public string NowRoomName
    {
        get => roomName;
        private set => roomName = value;
    }

    [DllImport("__Internal")]
    public static extern void EnterRoom(string roomID);

    // Start is called before the first frame update
    void Start()
    {
        enterRoomButton.onClick.AddListener(() =>
        {
            roomName = roomNameInput.text;
            if (roomName == "") return;
            EnterRoom(roomName);
        });
    }

    private void Update()
    {
        enterRoomButton.gameObject.SetActive(!NetworkManager.singleton.isNetworkActive);
        roomNameInput.gameObject.SetActive(!NetworkManager.singleton.isNetworkActive);
    }
}