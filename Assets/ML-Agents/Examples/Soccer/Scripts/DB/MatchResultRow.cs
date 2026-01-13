using System;
using SQLite4Unity3d; // 패키지 네임스페이스가 다르면 SQLite로 바꿔주세요.

[Table("match_results")]
public class MatchResultRow
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string StartedAt { get; set; }
    public string EndedAt { get; set; }

    public int PlayerGoals { get; set; }
    public int TargetGoals { get; set; }

    public int Cleared { get; set; } // 0/1
}
