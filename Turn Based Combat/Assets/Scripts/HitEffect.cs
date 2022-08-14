using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HitEffect : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI hitText;
    [SerializeField] Transform criticalText;
    [SerializeField] Transform weaknessText;
    [SerializeField] Transform resistText;

    public void Hit(int damage, bool isCritical, bool isWeakness, bool isResistant)
    {
        GetComponent<Animator>().SetTrigger("Hit");
        hitText.text = damage.ToString();
        criticalText.gameObject.SetActive(isCritical);
        weaknessText.gameObject.SetActive(isWeakness);
        resistText.gameObject.SetActive(isResistant);
    }
}
