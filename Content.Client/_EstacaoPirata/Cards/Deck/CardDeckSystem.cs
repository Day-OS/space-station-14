using System.Linq;
using System.Numerics;
using Content.Shared._EstacaoPirata.Cards.Deck;
using Content.Shared._EstacaoPirata.Stack.Cards;
using Robust.Client.GameObjects;

namespace Content.Client._EstacaoPirata.Cards.Deck;

/// <summary>
/// This handles...
/// </summary>
public sealed class CardDeckSystem : EntitySystem
{
    private List<Entity<CardDeckComponent>> NotInitialized = [];


    /// <inheritdoc/>
    public override void Initialize()
    {
        UpdatesOutsidePrediction = false;
        SubscribeLocalEvent<CardDeckComponent, ComponentStartup>(OnComponentStartupEvent);
        SubscribeLocalEvent<CardDeckComponent, CardStackComponent.CardStackUpdatedEvent>(OnStackUpdate);
        SubscribeNetworkEvent<CardStackComponent.CardStackInitiatedEvent>(OnStackStart);
        SubscribeLocalEvent<CardDeckComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Lazy way to make sure the sprite starts correctly
        foreach (var ent in NotInitialized.ToList())
        {
            if (!TryComp(ent.Owner, out CardStackComponent? stack))
                continue;

            // If the card was STILL not initialized, we skip it
            if (!TryGetCardLayer(stack.Cards.Last(), out var _))
                continue;

            // If cards were correctly initialized, we update the sprite
            UpdateSprite(ent.Owner, ent.Comp);
            NotInitialized.Remove(ent);
        }

    }

    // This is executed only if there are no available layers to work with
    private void SetupSpriteLayers(EntityUid uid, CardDeckComponent comp, SpriteComponent sprite, int layersQuantity)
    {
        if (!sprite.TryGetLayer(0, out var firstLayer))
            return;

        for (var i = layersQuantity; i < comp.CardLimit; i++)
        {
            sprite.AddLayer(firstLayer.State, i);
        }
    }

    private bool TryGetCardLayer(EntityUid card, out SpriteComponent.Layer? layer)
    {
        layer = null;
        if (!TryComp(card, out SpriteComponent? cardSprite))
            return false;

        if (!cardSprite.TryGetLayer(0, out var l))
            return false;

        layer = l;
        return true;
    }

    private void UpdateSprite(EntityUid uid, CardDeckComponent comp)
    {
        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        if (!TryComp(uid, out CardStackComponent? cardStack))
            return;

        // This sets up the layers if they are not initialized
        if (sprite.AllLayers.Count() < comp.CardLimit)
        {
            SetupSpriteLayers(uid, comp, sprite, sprite.AllLayers.Count());
        }


        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            sprite.LayerSetVisible(i, true);
        }

        var j = 0;
        // Shows the last 5 cards
        foreach (var card in cardStack.Cards.TakeLast(comp.CardLimit))
        {
            if (!TryGetCardLayer(card, out var layer) || layer == null)
            {
                // This means that the card was not initialized yet, so we add it to the following List:
                NotInitialized.Add((uid, comp));
                return;
            }

            sprite.LayerSetTexture(j, layer.Texture);
            sprite.LayerSetState(j, layer.State);
            sprite.LayerSetRotation(j, Angle.FromDegrees(90));
            sprite.LayerSetOffset(j, new Vector2(0, (comp.YOffset * j)));
            sprite.LayerSetScale(j, new Vector2(comp.Scale, comp.Scale));
            j++;
        }

        var cardsQuantity = cardStack.Cards.Count;
        var layersQuantity = sprite.AllLayers.ToList().Count;
        if (cardsQuantity < layersQuantity - 1)
        {
            for (int k = 0; k < (layersQuantity - cardsQuantity); k++)
            {
                sprite.LayerSetVisible(layersQuantity - k - 1, false);
            }
        }
    }

    private void OnComponentStartupEvent(EntityUid uid, CardDeckComponent comp, ComponentStartup args)
    {

        UpdateSprite(uid, comp);
    }
    private void OnStackUpdate(EntityUid uid, CardDeckComponent comp, CardStackComponent.CardStackUpdatedEvent args)
    {
        UpdateSprite(uid, comp);
    }

    private void OnAppearanceChanged(EntityUid uid, CardDeckComponent comp, AppearanceChangeEvent args)
    {
        UpdateSprite(uid, comp);
    }

    private void OnStackStart(CardStackComponent.CardStackInitiatedEvent args)
    {
        var entity = GetEntity(args.CardStack);
        if (!TryComp(entity, out CardDeckComponent? comp))
            return;

        UpdateSprite(entity, comp);
    }
}
