﻿using FlybyScript;
using Microsoft.Win32;
using System;
using System.Drawing;

namespace Settings.Ads
{
    public class PersonalizedAds : FeatureBase
    {
        public PersonalizedAds(Logger logger) : base(logger)
        {
        }

        private const string keyName = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo";
        private const string valueName = "Enabled";
        private const int desiredValue = 0;

        public override string ID() => "Disable Personalized Ads";

        public override string Info() => "This feature will disable personalized ads.";

        public override string GetRegistryKey()
        {
            return $"{keyName} | Value: {valueName} | Desired Value: {desiredValue}";
        }

        public override bool CheckFeature()
        {
            return Utils.IntEquals(keyName, valueName, 0);
        }

        public override bool DoFeature()
        {
            try
            {
                Registry.SetValue(keyName, valueName, 0, Microsoft.Win32.RegistryValueKind.DWord);

                return true;
            }
            catch (Exception ex)
            {
                logger.Log("Code red in " + ex.Message, Color.Red);
            }

            return false;
        }

        public override bool UndoFeature()
        {
            try
            {
                Registry.SetValue(keyName, valueName, 1, Microsoft.Win32.RegistryValueKind.DWord);

                return true;
            }
            catch (Exception ex)
            {
                logger.Log("Code red in " + ex.Message, Color.Red);
            }

            return false;
        }
    }
}