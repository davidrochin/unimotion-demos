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

        if (Application.isEditor) {
            PhotonNetwork.PhotonServerSettings.StartInOfflineMode = true;
        } else {
            PhotonNetwork.PhotonServerSettings.StartInOfflineMode = false;
        }

        PhotonNetwork.NickName = "Player";
        PhotonNetwork.GameVersion = "v1";

        PhotonNetwork.ConnectUsingSettings();
    }
}
