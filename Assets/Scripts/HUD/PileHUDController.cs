using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

public class PileHUDController : MonoBehaviour
{
    [SerializeField] private Button drawButton;
    [SerializeField] private Button discardButton;
    [SerializeField] private PileView pileView;

    private TMP_Text drawText;
    private TMP_Text discardText;
    private CardController _cardController;

    private enum ActivePile { None, Draw, Discard }
    private ActivePile active = ActivePile.None;

    private void Start()
    {
        drawText = drawButton.GetComponentInChildren<TMP_Text>();
        discardText = discardButton.GetComponentInChildren<TMP_Text>();

        if (drawText == null || discardText == null)
            Debug.LogError($"Missing text: drawText: {drawText}, discardText: {discardText}");

        _cardController = ClientBridge.Instance.ClientPlayer.GetComponent<CardController>();
        _cardController.OnPileReceived += OnPileReceived;

        drawButton.onClick.AddListener(OnDrawClicked);
        discardButton.onClick.AddListener(OnDiscardClicked);
    }

    private void OnDrawClicked()
    {
        Toggle(ActivePile.Draw);
    }

    private void OnDiscardClicked()
    {
        Toggle(ActivePile.Discard);
    }

    private void Toggle(ActivePile target)
    {
        bool isSame = active == target;
        active = isSame ? ActivePile.None : target;

        if (isSame)
        {
            pileView.Clear();
            return;
        }

        _cardController.RequestPile(target == ActivePile.Draw);
    }

    private void OnPileReceived(
        bool isDrawPile,
        IReadOnlyList<CardDefinition> cards)
    {
        bool isActive =
            (isDrawPile && active == ActivePile.Draw) ||
            (!isDrawPile && active == ActivePile.Discard);

        if (!isActive)
            return;

        pileView.Show(cards);

        if (isDrawPile)
            drawText.text = cards.Count.ToString();
        else
            discardText.text = cards.Count.ToString();
    }

    private void OnDestroy()
    {
        if (_cardController != null)
            _cardController.OnPileReceived -= OnPileReceived;
    }
}