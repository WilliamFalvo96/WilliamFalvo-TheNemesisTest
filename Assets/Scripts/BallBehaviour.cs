using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

[RequireComponent(typeof(Rigidbody))]
public class BallBehaviour : MonoBehaviourPunCallbacks
{
    Rigidbody rb;

    //Wall interaction properties
    [SerializeField]
    int wallLayer;
    [SerializeField]
    float wallBounce;

    //Player interaction properties
    [Space(8)]
    [SerializeField]
    int playerLayer;
    [SerializeField]
    float playerBounce;

    int teamScored;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Check if ball hitted a Player or Wall
        if(PhotonNetwork.IsMasterClient)
        {
            if(collision.gameObject.layer == wallLayer)
            {
                rb.AddForce(collision.contacts[0].normal * wallBounce);
            }
            if(collision.gameObject.layer == playerLayer)
            {
                rb.AddForce(collision.contacts[0].normal * playerBounce);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GetComponent<SphereCollider>().enabled = false;

        //Check wich portal's trigger ball entered
        if(other.tag == "Team1")
        {
            teamScored = 0;
        }
        else
        {
            teamScored = 1;
        }

        Invoke("CallNewRound", 1f);
    }    

    //Call a RPC function by GameManager to set a new Round and pass wich team scored
    void CallNewRound()
    {
        Debug.Log("New round");
        GameManager.Instance.NewRound(teamScored);
    }
}
