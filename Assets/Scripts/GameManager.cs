using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    private const int maxScore = 3;

    #region Public Fields

    public static GameManager Instance;

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    [Tooltip("The prefab to use for representing the ball")]
    public GameObject ballPrefab;

    public List<PlayerBehavior> players = new List<PlayerBehavior>();
    GameObject ball;

    #endregion

    #region Private Fields


    [SerializeField]
    Transform[] spawnPoints;
    [SerializeField]
    Vector3[] portalSpawnPoints;
    [SerializeField]
    Transform[] portals;
    [SerializeField]
    TextMeshProUGUI[] scoreTexts;
    [SerializeField]
    GameObject[] winTexts;
    int[] scores = {0,0};

    int localPlayerTeam;
    bool isGameCompleted = false;



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
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        if(!isGameCompleted) OpenWinPanel(localPlayerTeam);
    }


    #endregion


    #region MonoBehaviour CallBacks


    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    /// 

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PlayerBehavior.LocalPlayerInstance == null)
        {
            Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            localPlayerTeam = PlayerPrefs.GetInt("player" + (PhotonNetwork.LocalPlayer.ActorNumber) + "Team");
            PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoints[localPlayerTeam].position, Quaternion.identity, 0);
        }
        else
        {
            Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
        }

        if(PhotonNetwork.IsMasterClient)
        {
            ball = PhotonNetwork.Instantiate(this.ballPrefab.name, Vector3.up, Quaternion.identity, 0);
        }
    }


    #endregion


    #region Public Methods


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    /// <summary>
    /// Called after one of players set a goal point. Reset player and ball at start position, choose random point for portals and add point to the team scored.
    /// </summary>
    /// <param name="teamScored"></param>
    public void NewRound(int teamScored)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            foreach(PlayerBehavior p in players)
            {
                p.gameObject.GetPhotonView().RPC("ResetPosition", RpcTarget.All);
            }

            PhotonNetwork.Destroy(ball.GetPhotonView());
            ball = PhotonNetwork.Instantiate(this.ballPrefab.name, Vector3.up, Quaternion.identity, 0);

            List<Vector3> spawnedPoints = new List<Vector3>();
            for (int i = 0; i < portals.Length; i++)
            {
                Vector3 spawnPoint = new Vector3();
                do
                {
                    spawnPoint = portalSpawnPoints[Random.Range(0, portalSpawnPoints.Length)];
                } while (spawnedPoints.Contains(spawnPoint));

                spawnedPoints.Add(spawnPoint);
                portals[i].position = spawnPoint;
            }

            photonView.RPC("AddScore", RpcTarget.All, teamScored);
        }
    }

    /// <summary>
    /// Update score text and add point to the team scored. Check if there are the conditions to end the game.
    /// </summary>
    /// <param name="teamScored"></param>
    [PunRPC]
    public void AddScore(int teamScored)
    {
        scores[teamScored]++;
        scoreTexts[teamScored].text = scores[teamScored].ToString();

        if(scores[teamScored] == maxScore)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(ball.GetPhotonView());
            }

            isGameCompleted = true;
            OpenWinPanel(teamScored);
        }
    }

    /// <summary>
    /// Open end game panel and show who win the game.
    /// </summary>
    /// <param name="teamIndex"></param>
    void OpenWinPanel(int teamIndex)
    {
        winTexts[teamIndex].SetActive(true);
        winTexts[0].transform.parent.gameObject.SetActive(true);
    }


    #endregion



}
