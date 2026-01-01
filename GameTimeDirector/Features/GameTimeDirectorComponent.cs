using Exiled.API.Features;
using GameTimeDirector.Features.Components;
using UnityEngine;

namespace GameTimeDirector.Features;

public sealed class GameTimeDirectorComponent() : MonoBehaviour
{
    private void Awake()
    {
        Player = Player.Get(gameObject);
        PlayerProps = new PlayerProperties(this);
    }
    
    public Player Player { get; private set; }
    public PlayerProperties PlayerProps { get; private set; }
}