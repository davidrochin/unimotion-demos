using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;

    void Awake() {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ConnectToMaster() {
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.NickName = "Player";
        PhotonNetwork.GameVersion = "v1";

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause) {
        /*base.OnDisconnected(cause);
        Debug.Log(cause);*/
    }

    public override void OnConnectedToMaster() {
        base.OnConnectedToMaster();
        /*Debug.Log("Connected to Master");
        PhotonNetwork.JoinRoom(SceneManager.GetActiveScene().name);*/
    }

    public override void OnCreatedRoom() {
        //Debug.Log("Create room with success");
    }

    #region Fails

    public override void OnJoinRandomFailed(short returnCode, string message) {
        /*base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Failed to join random room because: " + message + " - Code " + returnCode);
        PhotonNetwork.CreateRoom(SceneManager.GetActiveScene().name, new RoomOptions() {
            MaxPlayers = 20
        });*/
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        /*base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Failed to create room because: " + message + " - Code " + returnCode);*/
    }

    #endregion

    private void OnGUI() {
        if(GUILayout.Button("Connect to master")) {
            ConnectToMaster();
        }
    }
}
