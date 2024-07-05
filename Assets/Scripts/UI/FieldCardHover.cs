using UnityEngine.EventSystems;
using UnityEngine;

public class FieldCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public FieldCard card;
    public float hoverDelay = 0.4f;
    private bool isHovering = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Player.localPlayer.isTargeting && Player.gameManager.isOurTurn && card.casterType == Target.FRIENDLIES && card.CanAttack())
        {
            card.SpawnTargetingArrow(card.card);
            HideCardInfo();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        Invoke(nameof(ShowCardInfo), hoverDelay);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        card.cardHover.gameObject.SetActive(false);

        Player.gameManager.CmdOnFieldCardHover(this.gameObject, false, false);

        Player.gameManager.isHoveringField = false;
    }

    public void ShowCardInfo()
    {
        if (isHovering)
        {
            if (!Player.localPlayer.isTargeting) card.cardHover.gameObject.SetActive(true);

            if (Player.gameManager.isOurTurn)
            {
                Player.gameManager.isHoveringField = true;
                Player.gameManager.CmdOnFieldCardHover(this.gameObject, true, Player.localPlayer.isTargeting);
            }
        }
    }

    public void HideCardInfo()
    {
        card.cardHover.gameObject.SetActive(false);
    }
}
