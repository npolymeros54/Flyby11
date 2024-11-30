using FlybyScript;
using Microsoft.Win32;
using System;
using System.Drawing;

namespace Settings.Privacy
{
    internal class FindMyDevice : FeatureBase
    {
        public FindMyDevice(Logger logger) : base(logger)
        {
        }

        private const string keyName = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location";
        private const string valueName = "Value";
        private const string desiredValue = @"Allow";

        public override string GetRegistryKey()
        {
            return $"{keyName} | Value: {valueName} | Desired Value: {desiredValue}";
        }

        public override string ID()
        {
            return "Disable Find my device";
        }

        public override string Info()
        {
            return "This feature will disable find my device.";
        }

        public override bool CheckFeature()
        {
            return !(
                  Utils.StringEquals(keyName, valueName, desiredValue)
            );
        }

        public override bool DoFeature()
        {
            try
            {
                Registry.SetValue(keyName, valueName, "Deny", RegistryValueKind.String);
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
                Registry.SetValue(keyName, valueName, desiredValue, RegistryValueKind.String);

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