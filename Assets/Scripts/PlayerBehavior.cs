using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBehavior : MonoBehaviourPunCallbacks
{
    Rigidbody rb;

    [SerializeField, Range(0.0f, 10.0f)]
    float speed = 1.0f;
    [SerializeField]
    Material[] teamMat;

    Vector3 spawnPoint;
    Vector3 direction;

    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        GetComponent<MeshRenderer>().material = teamMat[PlayerPrefs.GetInt("player" + photonView.OwnerActorNr + "Team")];

        // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
        if (photonView.IsMine)
        {
            PlayerBehavior.LocalPlayerInstance = this.gameObject;
        }

        //Save spawn point. Used after for set a new round.
        spawnPoint = transform.position;

        // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.players.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        //Normalize the horizontal and vertical axis to maintain the same velocity in every direction
        direction = Vector3.Normalize(new Vector3(h, rb.velocity.y, v));
    }

    private void FixedUpdate()
    {
        rb.velocity = Vector3.zero;
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        if (GameManager.Instance != null) GameManager.Instance.players.Remove(this);
    }


    [PunRPC]
    public void ResetPosition()
    {
        transform.position = spawnPoint;
    }

    
}
