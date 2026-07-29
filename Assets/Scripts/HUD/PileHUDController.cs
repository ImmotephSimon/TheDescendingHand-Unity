using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class PileHUDController : MonoBehaviour
{
    [SerializeField] private Button drawButton;
    [SerializeField] private Button discardButton;

    private ICardPiles piles;
    private TMP_Text drawText;
    private TMP_Text discardText;
    private PileView pileView;

    private void Start()
    {
        drawText = drawButton.GetComponentInChildren<TMP_Text>();
        discardText = discardButton.GetComponentInChildren<TMP_Text>();
        if (drawText == null || discardText == null)
            Debug.LogError($"Missing text: drawText: {drawText}, discardText: {discardText}");

        piles = ClientBridge.Instance.Player.CardPiles;
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

    private void OnDrawClicked()
    {
        pileView.Show(piles.DrawPile);
    }

    private void OnDiscardClicked()
    {
        pileView.Show(piles.DiscardPile);
    }
}