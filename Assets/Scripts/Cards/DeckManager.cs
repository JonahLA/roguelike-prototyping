using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's deck, draw/discard piles, and hand for card-based gameplay.
/// Handles deck initialization, drawing, discarding, and shuffling.
/// </summary>
public class DeckManager : MonoBehaviour
{
    [Header("Deck Settings")]
    [Tooltip("Initial deck list (Card ScriptableObjects)")]
    public List<CardSO> startingDeck;

    [Header("Hand Settings")]
    [Tooltip("Number of cards to draw each hand")]
    public int handSize = 5;

    // Runtime state
    private List<CardInstance> _drawPile = new();
    private List<CardInstance> _discardPile = new();
    private List<CardInstance> _hand = new();

    /// <summary>
    /// Event invoked whenever the hand changes. Provides the new hand as a list.
    /// </summary>
    public static event Action<List<CardInstance>> HandChanged;

    /// <summary>
    /// Initializes the deck and draws the starting hand.
    /// </summary>
    private void Awake()
    {
        InitializeDeck();
    }

    /// <summary>
    /// Clears and rebuilds the deck from the starting deck, shuffles, and draws a new hand.
    /// </summary>
    public void InitializeDeck()
    {
        _drawPile.Clear();
        _discardPile.Clear();
        _hand.Clear();
        foreach (var card in startingDeck)
        {
            _drawPile.Add(new CardInstance(card));
        }
        Shuffle(_drawPile);
        DrawHand();
    }

    /// <summary>
    /// Draws a new hand of cards from the draw pile. Reshuffles discard pile if needed.
    /// </summary>
    public void DrawHand()
    {
        _hand.Clear();
        for (int i = 0; i < handSize; i++)
        {
            if (_drawPile.Count == 0)
            {
                ReshuffleDiscardIntoDraw();
                if (_drawPile.Count == 0) break;
            }
            var card = _drawPile[0];
            _drawPile.RemoveAt(0);
            _hand.Add(card);
        }
        HandChanged?.Invoke(new List<CardInstance>(_hand));
    }

    /// <summary>
    /// Discards a card from the hand to the discard pile and updates the hand event.
    /// </summary>
    /// <param name="card">The card instance to discard.</param>
    public void DiscardCard(CardInstance card)
    {
        if (_hand.Contains(card))
        {
            _hand.Remove(card);
            _discardPile.Add(card);
            HandChanged?.Invoke(new List<CardInstance>(_hand));
        }
    }

    /// <summary>
    /// Adds a new card to the draw pile and shuffles the pile.
    /// </summary>
    /// <param name="cardAsset">The card asset to add as a new instance.</param>
    public void AddCardToDeck(CardSO cardAsset)
    {
        var instance = new CardInstance(cardAsset);
        _drawPile.Add(instance);
        Shuffle(_drawPile);
    }

    /// <summary>
    /// Removes a card instance from all piles (draw, discard, hand).
    /// </summary>
    /// <param name="card">The card instance to remove.</param>
    public void RemoveCardFromDeck(CardInstance card)
    {
        _drawPile.Remove(card);
        _discardPile.Remove(card);
        _hand.Remove(card);
    }

    /// <summary>
    /// Moves all cards from the discard pile to the draw pile and shuffles.
    /// </summary>
    public void ReshuffleDiscardIntoDraw()
    {
        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        Shuffle(_drawPile);
    }

    /// <summary>
    /// Shuffles the provided list of card instances in place.
    /// </summary>
    /// <param name="list">The list to shuffle.</param>
    private void Shuffle(List<CardInstance> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // Serialization methods for saving/loading deck, hand, discard can be added here
    // ...

    /// <summary>
    /// Returns a copy of the current hand for external use (e.g., HandController).
    /// </summary>
    /// <returns>A new list containing the current hand's card instances.</returns>
    public List<CardInstance> GetHand() => new(_hand);
}
