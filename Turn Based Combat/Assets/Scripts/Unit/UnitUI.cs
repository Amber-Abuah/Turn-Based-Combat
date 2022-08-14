using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitUI : MonoBehaviour
{
    [SerializeField] Image hpBar;
    [SerializeField] Image mpBar;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI mpText;
    Vector3 originalSize;

    private void Start()
    {
        originalSize = transform.localScale;
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void SetHpBar(int currentHp, int maxHp)
    {
        hpBar.fillAmount = (float)currentHp/ maxHp;

        if(hpText != null)
            hpText.text = currentHp + "/" + maxHp;
    }

    public void SetMpBar(int currentMp, int maxMp)
    {
        if (mpBar != null)
        {
            mpBar.fillAmount = (float)currentMp / maxMp;
            mpText.text = currentMp + "/" + maxMp;
        }
    }

    public void Enlarge()
    {
        transform.localScale *= 1.2f;
    }

    public void ReturnToOriginalSize()
    {
        transform.localScale = originalSize;
    }
}
