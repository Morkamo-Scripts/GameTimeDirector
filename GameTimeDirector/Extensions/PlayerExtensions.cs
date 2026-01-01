using CommandSystem;
using Exiled.API.Features;
using GameTimeDirector.Features;

namespace GameTimeDirector.Extensions;

public static class PlayerExtensions
{
    public static Player AsPlayer(this ICommandSender sender)
        => Player.Get(sender);

    public static GameTimeDirectorComponent GameDirectorComponents(this Player player)
        => player.ReferenceHub.GetComponent<GameTimeDirectorComponent>();
}