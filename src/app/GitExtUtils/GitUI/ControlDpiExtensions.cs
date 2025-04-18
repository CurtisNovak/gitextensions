using System.ComponentModel;
using System.Windows.Forms;
using GitExtUtils.GitUI;

namespace GitUI
{
    public static class ControlDpiExtensions
    {
        public static void AdjustForDpiScaling(this Control control)
        {
            ArgumentNullException.ThrowIfNull(control);

            bool isDpiScaled = DpiUtil.IsNonStandard;

            // If we are in design mode, don't scale anything as the designer may
            // write scaled values back to InitializeComponent.
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            foreach (Control descendant in control.FindDescendants())
            {
                // NOTE we can't automatically scale TreeView or ListView here as
                // adjustment must be done before images are added to the
                // ImageList otherwise they're all removed.
                switch (descendant)
                {
                    case ButtonBase button:
                    {
                        if (isDpiScaled && button.Image is not null)
                        {
                            button.Image = DpiUtil.Scale(button.Image);
                            button.Padding = DpiUtil.Scale(new Padding(4, 0, 4, 0));
                        }

                        break;
                    }

                    case PictureBox pictureBox:
                    {
                        if (isDpiScaled && pictureBox.Image is not null)
                        {
                            pictureBox.Image = DpiUtil.Scale(pictureBox.Image);
                        }

                        if (isDpiScaled && pictureBox.BackgroundImage is not null)
                        {
                            pictureBox.BackgroundImage = DpiUtil.Scale(pictureBox.BackgroundImage);
                        }

                        break;
                    }

                    case TabControl tabControl:
                    {
                        if (!isDpiScaled)
                        {
                            tabControl.Padding = new Point(8, 6);
                        }
                        else if (tabControl.Tag as string != "__DPI_SCALED__")
                        {
                            tabControl.Tag = "__DPI_SCALED__";
                            tabControl.Padding = DpiUtil.Scale(new Point(8, 6));
                        }

                        break;
                    }

                    case SplitContainer splitContainer:
                    {
                        const int splitterWidth = 6;

                        if (!isDpiScaled)
                        {
                            splitContainer.SplitterWidth = splitterWidth;
                        }
                        else if (splitContainer.Tag as string != "__DPI_SCALED__")
                        {
                            splitContainer.Tag = "__DPI_SCALED__";
                            splitContainer.SplitterWidth = DpiUtil.Scale(splitterWidth);
                        }

                        splitContainer.BackColor = Color.Transparent;
                        break;
                    }

                    case TextBoxBase textBox when textBox.Margin == new Padding(12):
                    {
                        // Work around a bug in WinForms where the control's margin gets scaled beyond expectations
                        // see https://github.com/gitextensions/gitextensions/issues/5098
                        textBox.Margin = DpiUtil.Scale(new Padding(3));
                        break;
                    }

                    case UpDownBase upDown when upDown.Margin == new Padding(96):
                    {
                        // Work around a bug in WinForms where the control's margin gets scaled beyond expectations
                        // see https://github.com/gitextensions/gitextensions/issues/5098
                        upDown.Margin = DpiUtil.Scale(new Padding(3));
                        break;
                    }

                    case ToolStrip toolStrip:
                    {
                        foreach (ToolStripItem toolStripItem in toolStrip.FindToolStripItems())
                        {
                            if (toolStripItem is ToolStripComboBox toolStripComboBox)
                            {
                                // ToolStrip items are adjusting vertically, but not horizontally. Furthermore, in some cases vertical adjustments
                                // done here come out overblown so we only adjust the width.
                                toolStripComboBox.Size = new Size(DpiUtil.Scale(toolStripComboBox.Size.Width), toolStripComboBox.Size.Height);
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}
