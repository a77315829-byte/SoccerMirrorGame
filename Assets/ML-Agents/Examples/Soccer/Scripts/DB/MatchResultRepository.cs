using System;
using System.IO;
using UnityEngine;
using SQLite4Unity3d;

public class MatchResultRepository : MonoBehaviour
{
    SQLiteConnection _conn;
    string _dbPath;

    void Awake()
    {
        // Windows에서도 안전한 저장 위치
        _dbPath = Path.Combine(Application.persistentDataPath, "soccer_stats.db");
        Debug.Log($"[DB] Path = {_dbPath}");

        _conn = new SQLiteConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        _conn.CreateTable<MatchResultRow>();
    }

    void OnDestroy()
    {
        _conn?.Close();
        _conn = null;
    }

    public void InsertMatch(DateTime startedAt, DateTime endedAt, int playerGoals, int targetGoals, bool cleared)
    {
        if (_conn == null) return;

        var row = new MatchResultRow
        {
            StartedAt = startedAt.ToString("o"), // ISO 8601
            EndedAt = endedAt.ToString("o"),
            PlayerGoals = playerGoals,
            TargetGoals = targetGoals,
            Cleared = cleared ? 1 : 0
        };

        _conn.Insert(row);
        Debug.Log($"[DB] Inserted match id={row.Id}, goals={playerGoals}, cleared={cleared}");
    }

    public int GetTotalClears()
    {
        if (_conn == null) return 0;
        return _conn.Table<MatchResultRow>().Count(x => x.Cleared == 1);
    }
}
