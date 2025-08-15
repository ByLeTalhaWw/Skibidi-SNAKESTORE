using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Exiled.API.Features;
using System.Xml;

namespace SnakeSystem
{
    public class PlayerScore
    {
        public string UserId { get; set; }
        public string LastKnownName { get; set; }
        public int TotalScore { get; set; }
        public DateTime LastPlayed { get; set; }
    }

    public static class PlayerScoreManager
    {
        private static readonly string ScorePath = Path.Combine(Paths.Configs, "snake_scores.json");
        private static Dictionary<string, PlayerScore> _playerScores = new Dictionary<string, PlayerScore>();

        static PlayerScoreManager()
        {
            LoadScores();
        }

        public static void LoadScores()
        {
            try
            {
                if (File.Exists(ScorePath))
                {
                    var json = File.ReadAllText(ScorePath);
                    _playerScores = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, PlayerScore>>(json) ?? new Dictionary<string, PlayerScore>();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading Snake scores: {ex.Message}");
                _playerScores = new Dictionary<string, PlayerScore>();
            }
        }

        public static void SaveScores()
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(_playerScores, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(ScorePath, json);
            }
            catch (Exception ex)
            {
                Log.Error($"Error saving Snake scores: {ex.Message}");
            }
        }

        public static int GetPlayerScore(string userId)
        {
            if (_playerScores.TryGetValue(userId, out PlayerScore score))
            {
                return score.TotalScore;
            }
            return 0;
        }

        public static void SavePlayerScore(string userId, int totalScore)
        {
            var player = Player.Get(userId);
            var playerName = player?.Nickname ?? "Unknown";

            if (_playerScores.ContainsKey(userId))
            {
                _playerScores[userId].TotalScore = totalScore;
                _playerScores[userId].LastKnownName = playerName;
                _playerScores[userId].LastPlayed = DateTime.Now;
            }
            else
            {
                _playerScores[userId] = new PlayerScore
                {
                    UserId = userId,
                    LastKnownName = playerName,
                    TotalScore = totalScore,
                    LastPlayed = DateTime.Now
                };
            }

            SaveScores();
        }

        public static List<PlayerScore> GetTopScores(int count = 10)
        {
            return _playerScores.Values
                .OrderByDescending(x => x.TotalScore)
                .Take(count)
                .ToList();
        }
    }
}