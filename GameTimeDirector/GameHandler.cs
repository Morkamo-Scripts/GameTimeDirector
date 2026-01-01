using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Exiled.API.Features;
using GameTimeDirector.Events;
using GameTimeDirector.Features;
using GameTimeDirector.Features.Components;
using LabApi.Events.Arguments.PlayerEvents;
using MorkamoEventsRegistrator.Components;
using SerpentHands.Events.EventArgs.Player;
using UnityEngine;
using events = Exiled.Events.Handlers;

namespace GameTimeDirector;

public class GameHandler : IEventsRegistrator
{
    public void RegisterEvents()
    {
        EventManager.PlayerEvents.PlayerFullConnected += OnPlayerFullConnected;
        LabApi.Events.Handlers.PlayerEvents.Left += OnPlayerLeft;
    }
    
    public void UnregisterEvents() 
    {
        EventManager.PlayerEvents.PlayerFullConnected -= OnPlayerFullConnected;
        LabApi.Events.Handlers.PlayerEvents.Left -= OnPlayerLeft;
    }
    
    private readonly Dictionary<string, Coroutine> _activeTimeTrackers = new();

    private void OnPlayerFullConnected(PlayerFullConnectedEventArgs ev)
    {
        if (_activeTimeTrackers.ContainsKey(ev.Player.UserId))
            DestroyTimeTracker(ev.Player.UserId);
        
        _activeTimeTrackers.Add(ev.Player.UserId, CoroutineRunner.Run(TimeTracker(ev.Player)));
    }

    private void OnPlayerLeft(PlayerLeftEventArgs ev) => DestroyTimeTracker(ev.Player.UserId);
    
    private IEnumerator TimeTracker(Player player)
    {
        Log.Info("Player connected! Tracker initialized!");
        
        while (player.IsConnected)
        {
            if (!Round.IsStarted)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            
            yield return new WaitForSeconds(60f);
            
            DatabaseHandler.CheckPlayerInDatabase(player.UserId, true);
            DatabaseHandler.UpdatePlayerTime(player.UserId, 1);
        }
        
        Log.Info("Player disconnected! Tracker destroyed...");
    }
    
    private void DestroyTimeTracker(string playerUserId)
    {
        var coroutine = _activeTimeTrackers[playerUserId];
        if (coroutine == null)
            return;
        
        CoroutineRunner.Stop(_activeTimeTrackers[playerUserId]);
        _activeTimeTrackers.Remove(playerUserId);

        return;
    }
}