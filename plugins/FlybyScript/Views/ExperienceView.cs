using FlybyScript;
using Settings.Ads;
using Settings.Privacy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Views
{
    public partial class ExperienceView : UserControl
    {
        private Logger logger;
        private List<FeatureBase> features;

        public ExperienceView()
        {
            InitializeComponent();
            SetStyle();
            InitializeFeatures();
            LoadFeatures();
        }

        private void SetStyle()
        {
            // Segoe MDL2 Assets assets
            btnBack.Text = "\uE72B";
        }

        private void InitializeFeatures()
        {
            // Initialize logger
            logger = new Logger(rtbDescription);

            // Initialize the list of features
            features = new List<FeatureBase>
            {
                new PrivacyExperience(logger),
                new ActivityHistory(logger),
                new LocationTracking(logger),
                new Telemetry(logger),
                new FileExplorerAds(logger),
                new FinishSetupAds(logger),
                new LockScreenAds(logger),
                new PersonalizedAds(logger),
                new SettingsAds(logger),
                new StartmenuAds(logger),
                new TailoredExperiences(logger),
                new TipsAndSuggestions(logger),
                new WelcomeExperienceAds(logger)
            };
        }

        private void LoadFeatures()
        {
            // Ensure featurePanel is not null
            if (featurePanel == null)
            {
                MessageBox.Show("FeaturePanel is not initialized. Please check the designer.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Clear any existing controls in the featurePanel
            featurePanel.Controls.Clear();

            // Populate featurePanel with FeaturePanels
            foreach (var feature in features)
            {
                var featurePanelInstance = new FeaturePanel(feature, logger)
                {
                    Width = featurePanel.Width - 50, // Set the width to match the parent featurePanel
                    Height = 160,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.FromArgb(245, 245, 245),
                    Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, featurePanel.Width, 160, 20, 20)) // Rounded corners
                };

                featurePanel.Controls.Add(featurePanelInstance);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            SwitchView.GoBack(this.Parent as Panel);
        }

        // Import rounded corners
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void btnApply_Click(object sender, EventArgs e)
        {
            foreach (var feature in features)
            {
                if (!feature.CheckFeature()) // If feature is not enabled
                {
                    feature.DoFeature(); // Apply feature
                    logger.Log($"{feature.ID()} enabled.", Color.Green);
                }
            }
            LoadFeatures();
            MessageBox.Show("All features have been applied.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnRevert_Click(object sender, EventArgs e)
        {
            foreach (var feature in features)
            {
                if (feature.CheckFeature())
                {
                    feature.UndoFeature(); // Revert feature
                    logger.Log($"{feature.ID()} reverted.", Color.Red);
                }
            }
            LoadFeatures();
            MessageBox.Show("All features have been reverted.", "Revert Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}