using System;
using Exiled.API.Features;
using Exiled.API.Interfaces;

namespace SnakeSystem
{
    public class SnakePlugin : Plugin<Config>
    {
        public override string Name => "Snake Market System";
        public override string Author => "ByLeTalhaWw";
        public override Version Version => new Version(1, 0, 0);

        public static SnakePlugin Instance;

        public override void OnEnabled()
        {
            Instance = this;

            PlayerScoreManager.LoadScores();
            SnakeEventManager.Initialize();
            SnakeGameMonitor.StartMonitoring();

            Log.Debug(Config.Language.SystemEnabled);
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            SnakeGameMonitor.StopMonitoring();
            SnakeEventManager.Cleanup();
            PlayerScoreManager.SaveScores();

            Instance = null;
            base.OnDisabled();
        }
    }
}