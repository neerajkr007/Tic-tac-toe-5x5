using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public Transform AlertCanvas;
    public GameManager gameManager;
    PhotonView pv;
    string RoomId;
    [SerializeField]
    bool inRoom = false;

    private void Start()
    {
        pv = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if(inRoom)
        {
            if(PhotonNetwork.CurrentRoom.PlayerCount != 2)
            {
                AlertCanvas.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "finding opponent, please wait...";
            }
            else
            {
                AlertCanvas.GetChild(0).GetComponent<TMPro.TMP_Text>().text = "starting match...";
                StartCoroutine(startMatch());
                inRoom = false;
            }
        }
    }

    IEnumerator startMatch()
    {
        yield return new WaitForSeconds(1f);
        AlertCanvas.gameObject.SetActive(false);
        gameManager.startgame(false);
    }
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            AlertCanvas.gameObject.SetActive(true);
            PhotonNetwork.GameVersion = "v0.1";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        print("connected to server. ");
        AlertCanvas.gameObject.SetActive(false);
        joinOrCreate();
    }

    public void joinOrCreate()
    {
        RoomOptions options = new RoomOptions();
        options.IsVisible = true;
        options.MaxPlayers = (byte)2;
        PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: options);
    }

    public override void OnCreatedRoom()
    {
        print("Room created successfully, Id - " + RoomId);
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        print("room creation failed coz " + message);
    }
    public override void OnJoinedRoom()
    {
        print("joined the room " + PhotonNetwork.CurrentRoom.Name);
        inRoom = true;
        AlertCanvas.gameObject.SetActive(true);
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("join room failed coz " + message);
    }
}
