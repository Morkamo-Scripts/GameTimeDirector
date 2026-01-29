using System;
using CommandSystem;
using Exiled.API.Features;
using GameTimeDirector.Extensions;
using RemoteAdmin;

namespace GameTimeDirector.Commands;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class PlayerTimeCommand : ICommand
{
    public string Command { get; } = "playerTime";
    public string[] Aliases { get; } = ["pltime"];
    public string Description { get; } = "Показывает наигранное время игрока на сервере!";
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (arguments.Count < 1)
        {
            response = "Формат ввода: pltime [id64]. (Пример: pltime 77777777777777777@steam)";
            return false; 
        }

        double? time = DatabaseHandler.GetPlayerTime(arguments.At(0));

        if (time == null)
        {
            response = "<color=orange>Игрок не найден в базе данных.</color>";
            return false;
        }

        response = $"Наигранное время игрока {arguments.At(0)}: {FormatedTime(time.Value)}";
        return true;
    }
    
    private static string FormatedTime(double totalMinutes)
    {
        if (totalMinutes <= 0)
            return "0 м.";

        var totalSeconds = totalMinutes * 60;
        var span = TimeSpan.FromSeconds(totalSeconds);

        var days = span.Days;
        var hours = span.Hours;
        var minutes = span.Minutes;

        if (days > 0)
            return $"{days} д. {hours} ч. {minutes} м.";
        if (hours > 0)
            return $"{hours} ч. {minutes} м.";
        return $"{minutes} м.";
    }
}