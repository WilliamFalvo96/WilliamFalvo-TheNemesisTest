using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerTeam : MonoBehaviourPunCallbacks, IPunObservable
{
    public int teamIndex = 1;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;

    [SerializeField]
    bool canChoose = true;

    /// <summary>
    /// Photon function to synchronize variables.
    /// Used to synchronize team selection panel over all clients
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(teamIndex);
        }
        else
        {
            int newIndex = (int)stream.ReceiveNext();
            if (newIndex != teamIndex)
            {
                teamIndex = newIndex;
                TeamSelectionBehaviour.Instance.ChangeTeam(photonView.OwnerActorNr, teamIndex);
            }
        }
    }

    private void Awake()
    {
        if (photonView.IsMine)
        {
            PlayerTeam.LocalPlayerInstance = this.gameObject;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        if (!canChoose) return;

        if ((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) && teamIndex > 0)
        {
            if(TeamSelectionBehaviour.Instance.ChangeTeam(photonView.OwnerActorNr, teamIndex-1)) teamIndex--;
        }
        if((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && teamIndex < 2)
        {
            if(TeamSelectionBehaviour.Instance.ChangeTeam(photonView.OwnerActorNr, teamIndex+1)) teamIndex++;
        }
    }

    /// <summary>
    /// Disable player to change again team and save to playerprefs the current choose.
    /// </summary>
    [PunRPC]
    public void RPC_DisableChoose()
    {
        canChoose = false;
        PlayerPrefs.SetInt("player"+ photonView.OwnerActorNr +"Team", (int)Mathf.Clamp01(teamIndex));
    }
}
