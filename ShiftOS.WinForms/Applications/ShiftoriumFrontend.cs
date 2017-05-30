/*
 * MIT License
 * 
 * Copyright (c) 2017 Michael VanOverbeek and ShiftOS devs
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShiftOS.Engine;
using static ShiftOS.Engine.SkinEngine;
using backend = ShiftOS.Engine.Shiftorium;
namespace ShiftOS.WinForms.Applications
{
    [Launcher("Shiftorium", true, "al_shiftorium", "Utilities")]
    [RequiresUpgrade("shiftorium_gui")]
    [MultiplayerOnly]
    [WinOpen("shiftorium")]
    [DefaultTitle("Shiftorium")]
    [DefaultIcon("iconShiftorium")]
    public partial class ShiftoriumFrontend : UserControl, IShiftOSWindow
    {
        public int CategoryId = 0;
        public static System.Timers.Timer timer100;


        public ShiftoriumFrontend()
        {
            cp_update = new System.Windows.Forms.Timer();
            cp_update.Tick += (o, a) =>
            {
                lbcodepoints.Text = $"You have {SaveSystem.CurrentSave.Codepoints} Codepoints."; 
            };
            cp_update.Interval = 100;
            InitializeComponent();
            PopulateShiftorium();
            lbupgrades.SelectedIndexChanged += (o, a) =>
            {
                try
                {
                    lbupgrades.Refresh();
                    SelectUpgrade(lbupgrades.SelectedItem.ToString());
                }
                catch { }
            };
            this.pgupgradeprogress.Maximum = backend.GetDefaults().Count;
            this.pgupgradeprogress.Value = SaveSystem.CurrentSave.CountUpgrades();
            backend.Installed += () =>
            {
                this.pgupgradeprogress.Maximum = backend.GetDefaults().Count;
                this.pgupgradeprogress.Value = SaveSystem.CurrentSave.CountUpgrades();
            };

        }

        public void SelectUpgrade(string name)
        {
            btnbuy.Show();
            var upg = upgrades[name];
            lbupgradetitle.Text = Localization.Parse(upg.Name);
            lbupgradedesc.Text = Localization.Parse(upg.Description);
        }

        Dictionary<string, ShiftoriumUpgrade> upgrades = new Dictionary<string, ShiftoriumUpgrade>();

        public void PopulateShiftorium()
        {
            var t = new Thread(() =>
            {
                try
                {
                    Desktop.InvokeOnWorkerThread(() =>
                    {
                        lbnoupgrades.Hide();
                        lbupgrades.Items.Clear();
                        upgrades.Clear();
                        Timer();
                    });

                    foreach (var upg in backend.GetAvailable().Where(x => x.Category == backend.GetCategories()[CategoryId]))
                    {
                        string name = Localization.Parse(upg.Name) + " - " + upg.Cost.ToString() + "CP";
                        upgrades.Add(name, upg);
                        Desktop.InvokeOnWorkerThread(() =>
                        {
                            lbupgrades.Items.Add(name);
                        });
                    }

                    if (lbupgrades.Items.Count == 0)
                    {
                        Desktop.InvokeOnWorkerThread(() =>
                        {
                            lbnoupgrades.Show();
                            lbnoupgrades.Location = new Point(
                                (lbupgrades.Width - lbnoupgrades.Width) / 2,
                                lbupgrades.Top + (lbupgrades.Height - lbnoupgrades.Height) / 2
                                );
                        });
                    }
                    else
                    {
                        Desktop.InvokeOnWorkerThread(() =>
                        {
                            lbnoupgrades.Hide();
                        });
                    }

                    Desktop.InvokeOnWorkerThread(() =>
                    {
                        try
                        {
                            lblcategorytext.Text = Shiftorium.GetCategories()[CategoryId];
                            btncat_back.Visible = (CategoryId > 0);
                            btncat_forward.Visible = (CategoryId < backend.GetCategories().Length - 1);
                        }
                        catch
                        {

                        }
                    });
                }
                catch
                {
                    Desktop.InvokeOnWorkerThread(() =>
                    {
                        lbnoupgrades.Show();
                        lbnoupgrades.Location = new Point(
                            (lbupgrades.Width - lbnoupgrades.Width) / 2,
                            lbupgrades.Top + (lbupgrades.Height - lbnoupgrades.Height) / 2
                            );
                    });
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public static bool UpgradeInstalled(string upg)
        {
            return backend.UpgradeInstalled(upg);
        }

        public static bool UpgradeAttributesUnlocked(FieldInfo finf)
        {
            return backend.UpgradeAttributesUnlocked(finf);
        }

        public static bool UpgradeAttributesUnlocked(MethodInfo finf)
        {
            return backend.UpgradeAttributesUnlocked(finf);
        }

        public static bool UpgradeAttributesUnlocked(Type finf)
        {
            return backend.UpgradeAttributesUnlocked(finf);
        }

        public static bool UpgradeAttributesUnlocked(PropertyInfo finf)
        {
            return backend.UpgradeAttributesUnlocked(finf);
        }

        private void lbupgrades_DrawItem(object sender, DrawItemEventArgs e)
        {
            var foreground = new SolidBrush(LoadedSkin.ControlTextColor);
            var background = new SolidBrush(LoadedSkin.ControlColor);

            e.Graphics.FillRectangle(background, e.Bounds);
            try
            {
                if (lbupgrades.GetSelected(e.Index) == true)
                {
                    e.Graphics.FillRectangle(foreground, e.Bounds);
                    e.Graphics.DrawString(lbupgrades.Items[e.Index].ToString(), e.Font, background, e.Bounds.Location);
                }
                else
                {
                    e.Graphics.FillRectangle(background, e.Bounds);
                    e.Graphics.DrawString(lbupgrades.Items[e.Index].ToString(), e.Font, foreground, e.Bounds.Location);
                }
            }
            catch
            {
            }
        }

        private void btnbuy_Click(object sender, EventArgs e)
        {
            ulong cpCost = 0;
            backend.Silent = true;
            Dictionary<string, ulong> UpgradesToBuy = new Dictionary<string, ulong>(); 
            foreach (var itm in lbupgrades.SelectedItems)
            {
                cpCost += upgrades[itm.ToString()].Cost;
                UpgradesToBuy.Add(upgrades[itm.ToString()].ID, upgrades[itm.ToString()].Cost);
            }
            if (SaveSystem.CurrentSave.Codepoints < cpCost)
            {
                Infobox.Show("Insufficient Codepoints", $"You do not have enough Codepoints to perform this action. You need {cpCost - SaveSystem.CurrentSave.Codepoints} more.");
                
            }
            else
            {
                foreach(var upg in UpgradesToBuy)
                {
                    SaveSystem.CurrentSave.Codepoints -= upg.Value;
                    if (SaveSystem.CurrentSave.Upgrades.ContainsKey(upg.Key))
                    {
                        SaveSystem.CurrentSave.Upgrades[upg.Key] = true;
                    }
                    else
                    {
                        SaveSystem.CurrentSave.Upgrades.Add(upg.Key, true);
                    }
                    SaveSystem.SaveGame();
                    backend.InvokeUpgradeInstalled();
                }
            }

                backend.Silent = false;
            PopulateShiftorium();
            btnbuy.Hide();
        }

        private void Shiftorium_Load(object sender, EventArgs e) {

        }

        public void OnLoad()
        {
            cp_update.Start();
            lbnoupgrades.Hide();
        }

        public void OnSkinLoad()
        {

        }

        System.Windows.Forms.Timer cp_update = new System.Windows.Forms.Timer();

        public bool OnUnload()
        {
            cp_update.Stop();
            cp_update = null;
            return true;
        }

        public void OnUpgrade()
        {
            lbupgrades.SelectionMode = (UpgradeInstalled("shiftorium_gui_bulk_buy") == true) ? SelectionMode.MultiExtended : SelectionMode.One;
            lbcodepoints.Visible = Shiftorium.UpgradeInstalled("shiftorium_gui_codepoints_display");
        }

        private void lbcodepoints_Click(object sender, EventArgs e)
        {

        }

        void Timer()
        {
            timer100 = new System.Timers.Timer();
            timer100.Interval = 2000;
            //CLARIFICATION: What is this supposed to do? - Michael
            //timer100.Elapsed += ???;
            timer100.AutoReset = true;
            timer100.Enabled = true;
        }

        private void btncat_back_Click(object sender, EventArgs e)
        {
            if(CategoryId > 0)
            {
                CategoryId--;
                PopulateShiftorium();
            }
        }

        private void btncat_forward_Click(object sender, EventArgs e)
        {
            if(CategoryId < backend.GetCategories().Length - 1)
            {
                CategoryId++;
                PopulateShiftorium();
            }
        }

        private void lblcategorytext_Click(object sender, EventArgs e)
        {

        }
    }
}
