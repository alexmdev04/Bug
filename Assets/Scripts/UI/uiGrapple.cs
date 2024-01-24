using TMPro;
using UnityEngine;

public class uiGrapple : MonoBehaviour
{
    [SerializeField] GameObject
        uiGrappleAmmoInfinite,
        uiGrappleAmmoDivider;
    [SerializeField] TextMeshProUGUI
        uiGrappleAmmo;
    //public void Refresh(Grapple.ammoStateEnum ammoState, float ammoStateCurrent = 0, float ammoStateMax = 0)
    //{
    //    uiGrappleAmmoInfinite.SetActive(false);
    //    uiGrappleAmmo.text = "";
    //    uiGrappleAmmoDivider.SetActive(false);
    //    switch (ammoState)
    //    {
    //        case Grapple.ammoStateEnum.infinite:
    //            {
    //                uiGrappleAmmoInfinite.SetActive(true);
    //                break;
    //            }
    //        case Grapple.ammoStateEnum.distanceLimited:
    //            {
    //                uiGrappleAmmo.text = ammoStateCurrent + "m\n" + ammoStateMax + "m";
    //                uiGrappleAmmoDivider.SetActive(true);
    //                break;
    //            }
    //        case Grapple.ammoStateEnum.usesLimited:
    //            {
    //                uiGrappleAmmo.text = ammoStateCurrent + "/" + ammoStateMax;
    //                break;
    //            }
    //    }
    //}
}