using System;
using System.Collections;
using System.Collections.Generic;
using Exiled.API.Features;
using GameTimeDirector.Events;
using GameTimeDirector.Features;
using LabApi.Events.Arguments.PlayerEvents;
using MorkamoEventsRegistrator.Components;
using SerpentHands.Events.EventArgs.Player;
using UnityEngine;

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
        if (ev.Player == null || ev.Player.IsNPC)
            return;
        
        if (_activeTimeTrackers.ContainsKey(ev.Player.UserId))
            DestroyTimeTracker(ev.Player.UserId);
        
        _activeTimeTrackers.Add(ev.Player.UserId, CoroutineRunner.Run(TimeTracker(ev.Player)));
    }

    private void OnPlayerLeft(PlayerLeftEventArgs ev)
    {
        try
        {
            DestroyTimeTracker(ev.Player?.UserId);
        }
        catch { /*ignored*/ }
    }
    
    private IEnumerator TimeTracker(Player player)
    {
        Log.Debug($"Player {player.UserId} connected! Tracker initialized!");
        
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
    }
    
    private void DestroyTimeTracker(string playerUserId)
    {
        var coroutine = _activeTimeTrackers[playerUserId];
        if (coroutine == null)
            return;
        
        CoroutineRunner.Stop(_activeTimeTrackers[playerUserId]);
        _activeTimeTrackers.Remove(playerUserId);
        
        Log.Debug($"Player {playerUserId} disconnected! Tracker destroyed...");
    }
}