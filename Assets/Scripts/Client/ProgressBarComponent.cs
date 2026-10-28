using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarComponent : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text levelText;

    public void SetValue(int level, float progress)
    {
        progressBar.fillAmount = progress;
    }
}