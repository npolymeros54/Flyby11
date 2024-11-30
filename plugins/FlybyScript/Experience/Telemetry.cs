using FlybyScript;
using Microsoft.Win32;
using System;
using System.Drawing;

namespace Settings.Privacy
{
    public class Telemetry : FeatureBase
    {
        public Telemetry(Logger logger) : base(logger)
        {
        }

        private const string dataCollection = @"HKEY_LOCAL_MACHINE\Software\Policies\Microsoft\Windows\DataCollection";
        private const string diagTrack = @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\Services\DiagTrack";

        public override string ID() => "Turn off Telemetry data collection";

        public override string Info() => "This feature will turn off telemetry data collection and prevent the data from being sent to Microsoft.";

        public override string GetRegistryKey()
        {
            return $"{dataCollection} | {diagTrack}";
        }

        public override bool CheckFeature()
        {
            return (
               Utils.IntEquals(dataCollection, "AllowTelemetry", 0) &&
                Utils.IntEquals(diagTrack, "Start", 2)

           );
        }

        public override bool DoFeature()
        {
            try
            {
                Registry.SetValue(dataCollection, "AllowTelemetry", 0, RegistryValueKind.DWord);
                Registry.SetValue(diagTrack, "Start", 2, RegistryValueKind.DWord);

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
                Registry.SetValue(dataCollection, "AllowTelemetry", 1, RegistryValueKind.DWord);
                Registry.SetValue(diagTrack, "Start", 4, RegistryValueKind.DWord);

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