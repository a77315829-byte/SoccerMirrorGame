using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class BlueAgentManager : NetworkBehaviour
{
    public static BlueAgentManager Instance;

    private readonly List<BlueAgentController> blueAgents = new();
    private int nextIndex = 0;

    void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        blueAgents.Clear();
        nextIndex = 0;

        // 서버에서 스폰된 BlueAgentController 전부 수집
        foreach (var a in FindObjectsByType<BlueAgentController>(FindObjectsSortMode.None))
        {
            blueAgents.Add(a);
        }

        Debug.Log($"[SERVER] BlueAgentManager collected blueAgents={blueAgents.Count}");
    }

    [Server]
    public BlueAgentController AssignAgent()
    {
        if (nextIndex >= blueAgents.Count)
        {
            Debug.LogError($"[SERVER] No more Blue Agents to assign! nextIndex={nextIndex}, count={blueAgents.Count}");
            return null;
        }

        var agent = blueAgents[nextIndex];
        nextIndex++;

        Debug.Log($"[SERVER] Assigned BlueAgent index={nextIndex - 1}, name={agent.name}");
        return agent;
    }
}
