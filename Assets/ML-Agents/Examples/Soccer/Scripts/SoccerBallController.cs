using Mirror;
using UnityEngine;

public class SoccerBallController : NetworkBehaviour
{
    [Header("Goal Tags")]
    public string purpleGoalTag = "purpleGoal";
    public string blueGoalTag = "blueGoal";

    SoccerEnvController env;

    public override void OnStartServer()
    {
        // ✅ Find 금지: 서버 싱글톤 사용
        env = SoccerEnvController.ServerInstance;

        if (env == null)
        {
            Debug.LogError("[BALL] SoccerEnvController.ServerInstance is NULL (server)");
        }
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null) return;

        if (other.CompareTag(purpleGoalTag))
        {
            Debug.Log("[BALL] Enter PurpleGoal -> Blue scored");
            
            env.ServerGoalTouched(Team.Blue);

        }
        else if (other.CompareTag(blueGoalTag))
        {
            Debug.Log("[BALL] Enter BlueGoal -> Purple scored");
            
            env.ServerGoalTouched(Team.Purple);
        }
    }

}
