using Content.Client.UserInterface.Controls;
using Content.Shared.Popups;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using System.Numerics;
using Content.Shared._EstacaoPirata.Cards.Card;
using Content.Shared._EstacaoPirata.Cards.Stack;

namespace Content.Client._EstacaoPirata.Cards.Hand.UI;

[GenerateTypedNameReferences]
public sealed partial class CardHandMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private readonly SpriteSystem _spriteSystem;
    private readonly SharedPopupSystem _popup;

    public event Action<NetEntity>? CardHandDrawMessageAction;

    private EntityUid _owner;

    public CardHandMenu(EntityUid owner, CardHandMenuBoundUserInterface bui)
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);

        _spriteSystem = _entManager.System<SpriteSystem>();
        _popup = _entManager.System<SharedPopupSystem>();

        _owner = owner;

        // Find the main radial container
        var main = FindControl<RadialContainer>("Main");

        if (!_entManager.TryGetComponent<CardStackComponent>(owner, out var stack))
            return;

        foreach (var card in stack.Cards)
        {
            if (_playerManager.LocalSession == null)
                return;
            if (!_entManager.TryGetComponent<CardComponent>(card, out var cardComp))
                return;
            var button = new CardMenuButton()
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64f, 64f),
                ToolTip = cardComp.Name,
            };

            if (_entManager.TryGetComponent<SpriteComponent>(card, out var sprite))
            {
                if (sprite.Icon == null)
                    continue;

                var tex = new TextureRect()
                {
                    VerticalAlignment = VAlignment.Center,
                    HorizontalAlignment = HAlignment.Center,
                    Texture = sprite.Icon?.Default,
                    TextureScale = new Vector2(2f, 2f),
                };

                button.AddChild(tex);
            }

            main.AddChild(button);

            button.OnButtonUp += _ =>
            {
                CardHandDrawMessageAction?.Invoke(_entManager.GetNetEntity(card));
                Close();
            };
        }

        CardHandDrawMessageAction += bui.SendCardHandDrawMessage;
    }
}

public sealed class CardMenuButton : RadialMenuTextureButton
{
    public CardMenuButton()
    {

    }
}
