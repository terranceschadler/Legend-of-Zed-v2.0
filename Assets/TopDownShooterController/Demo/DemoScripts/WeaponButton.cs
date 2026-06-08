using System;
using System.Collections;
using System.Collections.Generic;
using TopDownShooter;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TopDownShooter
{
    public class WeaponButton : MonoBehaviour
    {
        public ShowPlayerWeapons ShowPlayerWeaponsComponent;
        public ShooterController ShooterControllerComponent;
        public Text InputNumberText;
        public Text WeaponName;
        public Text WeaponCurrentBullets;
        public Image WeaponIcon;
        public int CurrentButtonWeaponIndex;
        public int InputNumber;

        private void Update()
        {
            if (IsInputNumberPressedThisFrame())
            {
                ChangeWeaponToThis();
            }
        }

        private void FixedUpdate()
        {
            if (ShooterControllerComponent.CurrentDbWeaponIndex == CurrentButtonWeaponIndex &&
                ShooterControllerComponent.PlayerIsReloading())
            {
                WeaponCurrentBullets.text = "Reload";
            }
            else
            {
                WeaponCurrentBullets.text = ShooterControllerComponent.WeaponsBullets
                    .Find(weapon => weapon.WeaponLoadIndex == CurrentButtonWeaponIndex).WeaponCurrentBullets.ToString();
            }
        }

        public void SetButton(string weaponName, int weaponIndex, int inputNumber,
            ShowPlayerWeapons showPlayerWeaponComponent, ShooterController shooterControllerComponent)
        {
            InputNumberText.text = inputNumber.ToString();
            WeaponName.text = weaponName;
            CurrentButtonWeaponIndex = weaponIndex;
            InputNumber = inputNumber;
            ShowPlayerWeaponsComponent = showPlayerWeaponComponent;
            ShooterControllerComponent = shooterControllerComponent;
            WeaponIcon.sprite = ShooterControllerComponent.WeaponData.Weapons[weaponIndex].WeaponImage;

            WeaponCurrentBullets.gameObject.SetActive(
                ShooterControllerComponent.WeaponData.Weapons[weaponIndex].WeaponClass != Weapon.WeaponType.Melee);
        }

        public void ChangeWeaponToThis()
        {
            ShowPlayerWeaponsComponent.EquipWeapon(CurrentButtonWeaponIndex);
        }

        private bool IsInputNumberPressedThisFrame()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            switch (InputNumber)
            {
                case 0:
                    return keyboard.digit0Key.wasPressedThisFrame || keyboard.numpad0Key.wasPressedThisFrame;
                case 1:
                    return keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame;
                case 2:
                    return keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame;
                case 3:
                    return keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame;
                case 4:
                    return keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame;
                case 5:
                    return keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame;
                case 6:
                    return keyboard.digit6Key.wasPressedThisFrame || keyboard.numpad6Key.wasPressedThisFrame;
                case 7:
                    return keyboard.digit7Key.wasPressedThisFrame || keyboard.numpad7Key.wasPressedThisFrame;
                case 8:
                    return keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame;
                case 9:
                    return keyboard.digit9Key.wasPressedThisFrame || keyboard.numpad9Key.wasPressedThisFrame;
                default:
                    return false;
            }
        }
    }
}