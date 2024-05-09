using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class TutorialInputLayout : MonoBehaviour
{
    public CanvasGroup cvg;
    public GameObject g_button_ability, g_text_ability;

    private void OnEnable()
    {
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        var ability_info = AbilityController.Instance.GetPrimaryAbility().Info;
        g_button_ability.SetActive(ability_info.hasAbilityInput);
        g_text_ability.SetActive(ability_info.hasAbilityInput);
    }

    public void Hide()
    {
        cvg.alpha = 0;
    }

    public void AnimateIntroTutorial()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(1.0f);

            yield return LerpEnumerator.Value(1f, f =>
            {
                cvg.alpha = Mathf.Lerp(0f, 1f, f);
            });

            yield return new WaitForSeconds(8f);

            yield return LerpEnumerator.Value(1f, f =>
            {
                cvg.alpha = Mathf.Lerp(1f, 0f, f);
            });
        }
    }
}