using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Views;

namespace FlybyScript
{
    public partial class MainForm : Form
    {
        private Dictionary<TreeNode, bool> pendingChanges = new Dictionary<TreeNode, bool>(); // Store pending changes
        private PSPatcher PSPatcher;

        private Logger logger;

        public MainForm()
        {
            InitializeComponent();

            // Initialize the logger
            logger = new Logger(rtbDescription);

            // Set the default view to the main panel
            SwitchView.DefaultView = panelMain;

            // Subscribe to the AfterSelect event for importing plugins
            treeSettings.AfterSelect += TreeSettings_AfterSelect;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // Add NativePatcher parent node
            TreeNode nativePatcherNode = new TreeNode("Native (Recommended)");
            treeSettings.Nodes.Add(nativePatcherNode);

            // Add Method 1 as a child of NativePatcher
            TreeNode method1Node = new TreeNode("(Method 1) Inplace Upgrade via Server Setup")
            {
                Tag = "Method1",
                BackColor = Color.LightBlue
            };
            nativePatcherNode.Nodes.Add(method1Node);

            // Load plugins
            await ScriptPatcher.LoadPlugins("upgraider", treeSettings.Nodes, logger);

            // Load PowerShell plugins
            PSPatcher = new PSPatcher();
            PSPatcher.LoadPowerShellPlugins(treeSettings);

            // Add the Import node at the end
            TreeNode importNode = new TreeNode("Import [...]")
            {
                NodeFont = new Font(treeSettings.Font, FontStyle.Italic),
                Checked = false,
                ForeColor = Color.Black
            };
            treeSettings.Nodes.Add(importNode);

            // Expand all nodes
            ExpandAllNodes(treeSettings.Nodes);
        }

        private void TreeSettings_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Text == "Import [...]")
            {
                OpenPluginsForm();
            }
        }

        private void OpenPluginsForm()
        {
            var pluginsForm = new PluginsForm(this);
            pluginsForm.ShowDialog();
        }

        // Refresh the tree view
        public void RefreshTreeView()
        {
            treeSettings.Nodes.Clear();

            // Reload
            MainForm_Load(this, EventArgs.Empty);
        }

        // Expand all nodes
        private void ExpandAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Expand();
                ExpandAllNodes(node.Nodes);
            }
        }

        private async void btnTogglePatch_Click(object sender, EventArgs e)
        {
            btnTogglePatch.Enabled = false;
            foreach (var entry in pendingChanges)
            {
                var node = entry.Key; // The TreeNode
                bool shouldApply = entry.Value; // Whether this patch should be applied (true) or undone (false)

                // Check if the entry is Method2 and if the patch is enabled
                if (node.Tag?.ToString() == "Method1" && shouldApply)
                {
                    // Start Method2 (Setup Server variant)
                    logger.Log("(Method 1) Automating Windows 11 Inplace Upgrade...", Color.Blue);
                    NativePatcher automation = new NativePatcher(logger);
                    await automation.AutomateWindowsInstallation();

                    node.BackColor = Color.LightGreen;
                    logger.Log("Method 1 Started: Windows 11 Installation Automated", Color.Black);
                }
                else if (node.Tag is ScriptPatcher plugin)
                {
                    if (shouldApply)
                    {
                        // Apply the patch
                        logger.Log($"Applying patch: {plugin.PlugID}", Color.Blue);
                        plugin.PlugDoFeature();
                        node.BackColor = Color.LightGreen;
                        logger.Log($"Activated patch: {plugin.PlugID}", Color.Black);
                    }
                    else
                    {
                        // Undo the patch
                        logger.Log($"Undoing patch: {plugin.PlugID}", Color.Blue);
                        plugin.PlugUndoFeature();
                        node.BackColor = Color.LightGray;
                        logger.Log($"Deactivated patch: {plugin.PlugID}", Color.Crimson);
                    }
                }
                // Handle PowerShell scripts
                else if (node.Tag is string psScriptPath)
                {
                    if (shouldApply)
                    {
                        // Log and execute the PowerShell script
                        logger.Log($"Executing PowerShell script: {Path.GetFileName(psScriptPath)}", Color.Blue);

                        var PSPatcher = new PSPatcher();
                        await PSPatcher.ExecutePlugin(psScriptPath, logger);

                        node.BackColor = Color.LightGreen;
                        logger.Log($"Executed PowerShell script: {Path.GetFileName(psScriptPath)}", Color.Black);
                    }
                    else
                    {
                        // Log for deactivation (not reversible for PS scripts)
                        logger.Log($"PowerShell script cannot be undone: {Path.GetFileName(psScriptPath)}", Color.Crimson);
                        node.BackColor = Color.LightGray;
                    }
                }
            }

            // Clear pending changes after applying
            pendingChanges.Clear();
            btnTogglePatch.Enabled = true;
        }

        private void treeSettings_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Check if the node's checked status has changed
            if (e.Node.Checked)
            {
                // Add the node and set its status to true (enabled)
                pendingChanges[e.Node] = true;
                e.Node.BackColor = Color.LightGreen; // Mark the node as active

                // Check the type of node (either JSONPluginLoader/Patch or PowerShell script)
                if (e.Node.Tag is ScriptPatcher jsonPlugin)
                {
                    logger.Log($"Patch activated: {jsonPlugin.PlugID}", Color.Black);
                }
                else if (e.Node.Tag is string psScriptPath)
                {
                    logger.Log($"PowerShell script activated: {Path.GetFileName(psScriptPath)}", Color.Black);
                }
            }
            else
            {
                // Add the node and set its status to false (disabled)
                pendingChanges[e.Node] = false;
                e.Node.BackColor = Color.LightGray; // Mark the node as inactive

                // Check the type of node again (either JSONPluginLoader or PowerShell script)
                if (e.Node.Tag is ScriptPatcher jsonPlugin)
                {
                    logger.Log($"Patch deactivated: {jsonPlugin.PlugID}", Color.Crimson);
                }
                else if (e.Node.Tag is string psScriptPath)
                {
                    logger.Log($"PowerShell script deactivated: {Path.GetFileName(psScriptPath)}", Color.Crimson);
                }
            }

            // Synchronize parent-child node states, so when a child node is checked or unchecked, its parent node automatically reflects this change
            if (e.Node.Parent != null)
            {
                e.Node.Parent.Checked = e.Node.Checked;
            }
        }

        private void treeSettings_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode node = treeSettings.GetNodeAt(e.Location);

            if (node != null)
            {
                // Check if the node's Tag is the native "Method1"
                if (node.Tag?.ToString() == "Method1")
                {
                    rtbDescription.Text = "(Current release: Windows 11 2024 Update l Version 24H2)\nThis method leverages the Windows Server variant of the Windows setup, " +
                        "which skips most hardware compatibility checks. It allows Windows 11 to be installed on unsupported PCs, " +
                        "bypassing the usual system requirements. Importantly, while the setup runs in server mode, " +
                        "it installs the standard Windows 11 (not the server version).\n\nHow it works:\n1. " +
                        "The app automatically downloads the Windows 11 version 24H2 ISO using the 'Fido' dependency script.\n2. " +
                        "The ISO image is mounted automatically.\n3. Simply follow the on-screen instructions to complete the upgrade.\n\n" +
                        "**Technical Note:** The POPCNT requirement cannot be bypassed; it is essential for running Windows 11 (24H2), as the operating system requires this feature to be supported by the CPU. " +
                        "POPCNT has been included in CPUs since around 2010. However, the patch is expected to work for most users with compatible hardware." +
                        "Please do not blame me; I am working within the constraints of what is technically possible."
;
                }
                else if (node.Tag is ScriptPatcher plugin)                     // Handle JSON plugin description
                {
                    string pluginInfo = $"ID: {plugin.PlugID}\nHow-to:\n{plugin.PlugInfo}";
                    rtbDescription.Text = pluginInfo;
                }
                else if (node.Tag is string psScriptPath)                // Handle PowerShell script description
                {
                    rtbDescription.Clear();
                    string scriptInfo = $"PowerShell Script: {Path.GetFileName(psScriptPath)}\nPath: {psScriptPath}";
                    rtbDescription.Text = scriptInfo;
                }
                else                                                 // Clear description if no node is hovered
                {
                    rtbDescription.Clear();
                }
            }
        }

        private void linkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/builtbybel/Flyby11");
        }

        private async void btnMountRun_Click(object sender, EventArgs e)
        {
            NativePatcher patcher = new NativePatcher(logger);
            await patcher.RunSetupFromMountedISO();
        }

        private void linkChangeExperience_Paint(object sender, PaintEventArgs e)
        {
            // Linkoption gains focus, so trigger a vivid focus rectangle
            ControlPaint.DrawFocusRectangle(e.Graphics, linkChangeExperience.ClientRectangle);
        }

        private void linkChangeExperience_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ExperienceView experienceView = new ExperienceView();
            SwitchView.SetView(experienceView, panelForm);
        }

        private void checkInstallationMedia_CheckedChanged(object sender, EventArgs e)
        {
            checkInstallationMedia.Checked = false;
            MediaView installMediaView = new MediaView();
            SwitchView.SetView(installMediaView, panelForm);
        }
    }
}