using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tensori.FPSHandsHorrorPack.Demo
{
    public class DemoAmmoHandlingExample : MonoBehaviour
    {
        [Header("Animation Event Messages")]
        [SerializeField] private string consumeAmmoEvent = "CheckForDamage";
        [SerializeField] private string reloadClipFullEvent = "ReloadAll";
        [SerializeField] private string reloadSingleEvent = "ReloadOne";
        [SerializeField] private string reloadLoopEndEvent = "ReloadLoopEnd";

        [Header("Object References")]
        [SerializeField] private FPSHandsController handsController = null;
        [SerializeField] private FPSItemSelector itemSelector = null;
        [SerializeField] private UnityEngine.UI.Text ammoText = null;

        private FPSItem heldItem = null;
        private Dictionary<FPSItem, int> remainingClipAmmoCollection = new Dictionary<FPSItem, int>();

        private void Awake()
        {
            if (handsController != null)
                handsController.OnAnimationEvent.AddListener(this.OnAnimationEvent);

            if (itemSelector != null)
                itemSelector.OnItemSelected += this.OnItemSelected;
        }

        private void OnItemSelected(FPSItemSelector.InputItemOption option)
        {
            heldItem = option.ItemAsset;

            if (remainingClipAmmoCollection.ContainsKey(heldItem) == false)
                remainingClipAmmoCollection.Add(heldItem, option.ItemAsset.AmmunitionClipSize);

            UpdateHandsControllerParameters();
            UpdateAmmoText();
        }

        private void OnAnimationEvent(string animationEvent)
        {
            bool ammoUpdated = false;

            if (animationEvent == consumeAmmoEvent)
            {
                remainingClipAmmoCollection[heldItem]--;
                ammoUpdated = true;
            }
            else if (animationEvent == reloadClipFullEvent)
            {
                remainingClipAmmoCollection[heldItem] = heldItem.AmmunitionClipSize;
                ammoUpdated = true;
            }
            else if (animationEvent == reloadSingleEvent)
            {
                remainingClipAmmoCollection[heldItem]++;
                ammoUpdated = true;

                bool isClipFull = remainingClipAmmoCollection[heldItem] >= heldItem.AmmunitionClipSize;

                if (isClipFull)
                    handsController.JumpToLastAnimationEventInPose(reloadLoopEndEvent);
                else
                    handsController.JumpToFirstAnimationEventInPose(reloadSingleEvent);
            }

            if (ammoUpdated)
            {
                UpdateHandsControllerParameters();
                UpdateAmmoText();
            }
        }

        private void UpdateHandsControllerParameters()
        {
            if (heldItem == null)
            {
                handsController.CanReload = false;
                handsController.CanShoot = false;
                return;
            }

            int remainingAmmo = remainingClipAmmoCollection[heldItem];

            handsController.CanReload = remainingAmmo < heldItem.AmmunitionClipSize;
            handsController.CanShoot = remainingAmmo > 0;
        }

        private void UpdateAmmoText()
        {
            if (ammoText == null)
                return;

            int remainingAmmo = remainingClipAmmoCollection[heldItem];
            ammoText.text = "Clip ammo: " + remainingAmmo + "/" + heldItem.AmmunitionClipSize;
            ammoText.enabled = heldItem.EnableAmmunitionUI;
        }
    }
}