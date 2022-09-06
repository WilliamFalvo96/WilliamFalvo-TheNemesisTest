using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class TeamSelectionBehaviour : MonoBehaviourPunCallbacks
{

    #region Public Fields

    public static TeamSelectionBehaviour Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    public Button readyBtn;

    [SerializeField]
    Image[] player1TeamImages;
    [SerializeField]
    Image[] player2TeamImages;
    [SerializeField]
    int player1TeamIndex = 1;
    [SerializeField]
    int player2TeamIndex = 1;


    #endregion


    #region Photon Callbacks


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }


    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom
        }
    }

    /// <summary>
    /// If one of the players leave the pre-game scene, return to main scene.
    /// </summary>
    /// <param name="other"></param>
    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        LeaveRoom();
    }


    #endregion


    #region MonoBehaviour CallBacks

    private void Awake()
    {
        Instance = this;
    }


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        if (PlayerTeam.LocalPlayerInstance == null)
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 2f, 0f), Quaternion.identity, 0);
        }
        else
        {
            Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
        }

        if(!PhotonNetwork.IsMasterClient)
        {
            readyBtn.gameObject.SetActive(false);
        }
    }


    #endregion


    #region Public Methods


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Change the player's current team selection. Check if there are the conditions to start the match (only for master).
    /// </summary>
    /// <param name="playerIndex"></param>
    /// <param name="teamIndex"></param>
    /// <returns></returns>
    public bool ChangeTeam(int playerIndex, int teamIndex)
    {
        if(playerIndex == 1)
        {
            player1TeamImages[player1TeamIndex].enabled = false;
            player1TeamIndex = teamIndex;
            player1TeamImages[player1TeamIndex].enabled = true;
        }
        else
        {
            player2TeamImages[player2TeamIndex].enabled = false;
            player2TeamIndex = teamIndex;
            player2TeamImages[player2TeamIndex].enabled = true;
        }

        if(PhotonNetwork.IsMasterClient)
        {
            if(player1TeamIndex != 1 && player2TeamIndex != 1 && player1TeamIndex != player2TeamIndex)
            {
                readyBtn.interactable = true;
            }
            else
            {
                readyBtn.interactable = false;
            }
        }

        return true;
    }

    /// <summary>
    /// Send to players the event to save current selection. After, load match scene.
    /// </summary>
    public void ConfirmTeam()
    {
        foreach (PhotonView player in PhotonNetwork.PhotonViewCollection)
        {
            player.RPC("RPC_DisableChoose",RpcTarget.All);
        }
        readyBtn.interactable = false;

        LoadArena();
    }


    #endregion


    #region Private Methods

    /// <summary>
    /// Load match scene, and after close the room, so other players cannot enter from matchmaking scene.
    /// </summary>
    void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(2);
    }


    #endregion



}
