using GameTimeDirector.Features.Components.Interfaces;

namespace GameTimeDirector.Features.Components;

public class PlayerProperties(GameTimeDirectorComponent gameTimeDirectorComponent) : IPropertyModule
{
    public GameTimeDirectorComponent GameTimeDirectorComponent { get; } = gameTimeDirectorComponent;
    // ВРЕМЕННО НЕ ФУНКЦИОНИРУЕТ
}