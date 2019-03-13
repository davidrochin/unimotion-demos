using Photon.Pun;
using UnityEngine;

namespace Unimotion {
    public class Player : MonoBehaviourPun {

        void Awake() {
            DontDestroyOnLoad(this);
        }

        private void Start() {
            if(!photonView.IsMine && PhotonNetwork.IsConnected && GetComponent<CharacterMotor>() != null) {
                Destroy(GetComponent<CharacterInput>());
                Destroy(GetComponent<CharacterMotor>());
            }
        }

        public static void RefreshInstance(ref Player player, GameObject prefab) {

            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            if (player != null) {
                position = player.transform.position;
                rotation = player.transform.rotation;
                PhotonNetwork.Destroy(player.gameObject);
            }

            player = PhotonNetwork.Instantiate(prefab.gameObject.name, position, rotation).GetComponent<Player>();
        }

    }
}

