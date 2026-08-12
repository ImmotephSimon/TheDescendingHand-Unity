using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PileHUDController : MonoBehaviour
{
    [SerializeField] private Button drawButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private PileView pileView;

    private ICardPiles piles;
    private TMP_Text drawText;
    private TMP_Text discardText;
    private enum ActivePile { None, Draw, Discard }
    private ActivePile active = ActivePile.None;

    private void Start()
    {
        drawText = drawButton.GetComponentInChildren<TMP_Text>();
        discardText = discardButton.GetComponentInChildren<TMP_Text>();
        if (drawText == null || discardText == null)
            Debug.LogError($"Missing text: drawText: {drawText}, discardText: {discardText}");

        piles = ClientBridge.Instance.Player.CardController.CardPiles;
        if (piles == null)
            Debug.LogError($"Missing piles on player");

        Refresh();

        drawButton.onClick.AddListener(OnDrawClicked);
        discardButton.onClick.AddListener(OnDiscardClicked);
    }

    private void Refresh()
    {
        drawText.text = piles.DrawPile.Count.ToString();
        discardText.text = piles.DiscardPile.Count.ToString();
    }


    private void OnDrawClicked() => Toggle(ActivePile.Draw, piles.DrawPile);
    private void OnDiscardClicked() => Toggle(ActivePile.Discard, piles.DiscardPile);

    private void Toggle(ActivePile target, IReadOnlyList<CardDefinition> pile)
    {
        bool isSame = active == target;
        active = isSame ? ActivePile.None : target;

        if (isSame) pileView.Clear();
        else pileView.Show(pile);
    }
}