using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unimotion;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks {

    public NetworkManager networkManager;

    public GameObject playerPrefab;
    public static Unimotion.Player localPlayer;

    public static string targetScene = "apartment_01";
    public static string targetMarkerId = "1";

    public string startingSceneName;

    void Awake() {
        DontDestroyOnLoad(this);
        DontDestroyOnLoad(Camera.main);
    }

    void Start () {
        Debug.Log("Connecting to master...");
        networkManager.ConnectToMaster();
    }

    public static void GoToScene(string sceneName, string markerId) {

        targetScene = sceneName;
        targetMarkerId = markerId;

        Debug.Log("Leaving room...");
        if(PhotonNetwork.CurrentRoom != null) {
            PhotonNetwork.LeaveRoom();
        }
        
    }

    public override void OnConnectedToMaster() {
        //GameManager.GoToScene("apartment_01", "1");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() {
        Debug.Log("Creating or joining to room " + targetScene + "...");
        PhotonNetwork.JoinOrCreateRoom(targetScene, new RoomOptions() { MaxPlayers = 20 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom() {
        Debug.Log("Joined room");
        localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, Vector3.zero, Quaternion.identity).GetComponent<Unimotion.Player>();

        PlayerCamera camera = Camera.main.GetComponent<PlayerCamera>();
        camera.SetTarget(localPlayer.GetComponent<CharacterMotor>());

        UnityEngine.Events.UnityAction<Scene, LoadSceneMode> tmpDelegate = null;
        tmpDelegate = delegate (Scene scene, LoadSceneMode mode) {
            Marker[] markers = FindObjectsOfType<Marker>();
            foreach (Marker m in markers) {
                if (targetMarkerId.Equals(m.id)) {
                    localPlayer.transform.position = m.transform.position + Vector3.up * 0.01f;
                    localPlayer.transform.rotation = Quaternion.LookRotation(m.transform.forward, -Physics.gravity.normalized);
                    break;
                }
            }
            SceneManager.sceneLoaded -= tmpDelegate;
        };

        SceneManager.sceneLoaded += tmpDelegate;
        SceneManager.LoadScene(targetScene);
    }

}
