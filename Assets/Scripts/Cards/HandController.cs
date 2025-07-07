using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandController : MonoBehaviour
{
    [Header("Hand Settings")]
    [Tooltip("Reference to DeckManager on the player")] 
    public DeckManager deckManager;

    [Tooltip("Discard mode: true = discard manually, false = discard on play")] 
    public bool discardManually = false;

    private List<CardInstance> _hand = new();
    private int _selectedCardIndex = 0;

    private void OnEnable()
    {
        DeckManager.HandChanged += OnHandChanged;
    }
    private void OnDisable()
    {
        DeckManager.HandChanged -= OnHandChanged;
    }

    private void Start()
    {
        if (deckManager != null)
        {
            _hand = deckManager.GetHand();
        }
    }

    /// <summary>
    /// Called by PlayerInput (Invoke Unity Events) for highlighting the card to the left.
    /// </summary>
    public void OnHighlightLeft(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed && _hand.Count > 0)
        {
            _selectedCardIndex = Mathf.Max(0, _selectedCardIndex - 1);
            // Optionally notify UI of highlight change
        }
    }

    /// <summary>
    /// Called by PlayerInput (Invoke Unity Events) for highlighting the card to the right.
    /// </summary>
    public void OnHighlightRight(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed && _hand.Count > 0)
        {
            _selectedCardIndex = Mathf.Min(_hand.Count - 1, _selectedCardIndex + 1);
            // Optionally notify UI of highlight change
        }
    }

    /// <summary>
    /// Called by PlayerInput (Invoke Unity Events) for playing the currently highlighted card.
    /// </summary>
    public void OnPlayHighlightedCard(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed && _hand.Count > 0 && _selectedCardIndex >= 0 && _selectedCardIndex < _hand.Count)
        {
            var card = _hand[_selectedCardIndex];
            // Play card logic (cost, effect, etc.) would go here
            if (!discardManually)
            {
                deckManager.DiscardCard(card);
            }
            // If discardManually, wait for explicit discard input (not implemented here)
        }
    }

    /// <summary>
    /// Called by PlayerInput (Invoke Unity Events) for deselecting/un-highlighting the current card.
    /// </summary>
    public void OnDeselectCard(InputAction.CallbackContext ctx)
    {
        if (ctx.phase == InputActionPhase.Performed)
        {
            _selectedCardIndex = -1;
            // Optionally notify UI of deselection
        }
    }

    private void OnHandChanged(List<CardInstance> newHand)
    {
        _hand = newHand;
        _selectedCardIndex = 0;
        // Notify UI here if needed
    }

    // Additional methods for discarding, UI feedback, etc. can be added here
}
