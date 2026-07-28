using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text levelText;

    public void SetValue(int level, float progress)
    {
        levelText.text = level.ToString();
        progressBar.fillAmount = progress;
    }
}