using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;

    void Start()
    {
        PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
    }

}
