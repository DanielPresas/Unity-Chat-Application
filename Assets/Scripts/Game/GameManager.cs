using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager get { get; private set; }
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;

    private void Awake() {
        if(get != null) {
            Logger.Error("Game manager", "Instance already exists, destroying new object!");
            Destroy(this);
            return;
        }

        get = this;
    }

    public void SpawnPlayer(int playerId, string playerName, Vector3 position, Quaternion rotation) {
        GameObject player = (playerId == Client.get.id) ? Instantiate(localPlayerPrefab, position, rotation) : Instantiate(playerPrefab, position, rotation);
        
        var playerManager = player.GetComponent<PlayerManager>();
        playerManager.id = playerId;
        playerManager.username = playerName;
        players.Add(playerId, playerManager);


    }
}
