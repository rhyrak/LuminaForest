using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;

public class SpawnPlayers : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;
    [SerializeField]
    private Vector3 SpawnPoint = new(0, 0, 0);
    [SerializeField]
    private CinemachineVirtualCamera Camera = null;

    void Start()
    {
        var player = PhotonNetwork.Instantiate(PlayerPrefab.name, SpawnPoint, Quaternion.identity);
        if (Camera != null)
        {
            Camera.Follow = player.transform;
        }
    }

}
