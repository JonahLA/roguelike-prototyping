using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Settings")]
    [Tooltip("Initial deck list (Card ScriptableObjects)")]
    public List<Card> startingDeck;

    [Header("Hand Settings")]
    [Tooltip("Number of cards to draw each hand")] 
    public int handSize = 5;

    // Runtime state
    private List<CardInstance> _drawPile = new();
    private List<CardInstance> _discardPile = new();
    private List<CardInstance> _hand = new();

    // Events (static, HealthManager pattern)
    public static event Action<List<CardInstance>> HandChanged;

    private void Awake()
    {
        InitializeDeck();
    }

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

    public void DiscardCard(CardInstance card)
    {
        if (_hand.Contains(card))
        {
            _hand.Remove(card);
            _discardPile.Add(card);
            HandChanged?.Invoke(new List<CardInstance>(_hand));
        }
    }

    public void AddCardToDeck(Card cardAsset)
    {
        var instance = new CardInstance(cardAsset);
        _drawPile.Add(instance);
        Shuffle(_drawPile);
    }

    public void RemoveCardFromDeck(CardInstance card)
    {
        _drawPile.Remove(card);
        _discardPile.Remove(card);
        _hand.Remove(card);
    }

    public void ReshuffleDiscardIntoDraw()
    {
        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        Shuffle(_drawPile);
    }

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

    // Expose hand for HandController
    public List<CardInstance> GetHand() => new List<CardInstance>(_hand);
}
