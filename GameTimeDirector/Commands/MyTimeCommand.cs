using System;
using CommandSystem;
using Exiled.API.Features;
using GameTimeDirector.Extensions;
using RemoteAdmin;

namespace GameTimeDirector.Commands;

[CommandHandler(typeof(ClientCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class MyTimeCommand : ICommand
{
    public string Command { get; } = "myTime";
    public string[] Aliases { get; } = ["mytime"];
    public string Description { get; } = "Показывает ваше наигранное время на сервере!";
    
    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        if (sender is not PlayerCommandSender)
        {
            response = "Команда только для игроков!";
            return false; 
        }

        double? time = DatabaseHandler.GetPlayerTime(sender.AsPlayer()?.UserId);

        if (time == null)
        {
            response = "<color=orange>Ошибка! Игрок не найден в базе данных. " +
                       "Возможно вы выполнили команду слишком рано, попробуйте снова.</color>";
            return false;
        }
        
        response = $"<color=green>Ваше наигранное время: {FormatedTime(time.Value)}</color>";
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