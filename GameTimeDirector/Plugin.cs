using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using GameTimeDirector.Features;
using LabApi.Events;
using EventManager = GameTimeDirector.Events.EventManager;

namespace GameTimeDirector
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => nameof(GameTimeDirector);
        public override string Prefix => Name;
        public override string Author => "Morkamo";
        public override Version Version => new Version(1, 0, 0);
        public override Version RequiredExiledVersion => new Version(9, 12, 1);

        public static Plugin Instance { get; private set; }
        public GameHandler GameHandler { get; private set; }

        public override void OnEnabled()
        {
            Instance = this;
            GameHandler = new GameHandler();
            Exiled.Events.Handlers.Player.Verified += OnVerifiedPlayer;
            MorkamoEventsRegistrator.Plugin.AddRegistrator(GameHandler);
            DatabaseHandler.InitializeDatabase();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            DatabaseHandler.Shutdown();
            MorkamoEventsRegistrator.Plugin.RemoveRegistrator(GameHandler);
            Exiled.Events.Handlers.Player.Verified -= OnVerifiedPlayer;
            GameHandler = null;
            Instance = null;
            base.OnDisabled();
        }
        
        private void OnVerifiedPlayer(VerifiedEventArgs ev)
        {
            if (ev.Player.ReferenceHub.gameObject.GetComponent<GameTimeDirectorComponent>() != null)
                return;

            ev.Player.ReferenceHub.gameObject.AddComponent<GameTimeDirectorComponent>();
            
            EventManager.PlayerEvents.InvokePlayerFullConnected(ev.Player);
        }
    }
}