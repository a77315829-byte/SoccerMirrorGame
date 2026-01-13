using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [Header("Spawn Prefabs")]
    public GameObject blueStrikerPrefab;
    public GameObject purpleStrikerPrefab;
    public GameObject ballPrefab;

    [Header("Spawn Points")]
    public Transform[] blueSpawnPoints;   // size 2
    public Transform[] purpleSpawnPoints; // size 2
    public Transform ballSpawnPoint;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // 1) Ball
        var ball = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation);
        NetworkServer.Spawn(ball);

        // 2) Blue x2
        for (int i = 0; i < blueSpawnPoints.Length; i++)
        {
            var a = Instantiate(blueStrikerPrefab, blueSpawnPoints[i].position, blueSpawnPoints[i].rotation);
            NetworkServer.Spawn(a);
        }

        // 3) Purple x2
        for (int i = 0; i < purpleSpawnPoints.Length; i++)
        {
            var a = Instantiate(purpleStrikerPrefab, purpleSpawnPoints[i].position, purpleSpawnPoints[i].rotation);
            NetworkServer.Spawn(a);
        }
    }
}
