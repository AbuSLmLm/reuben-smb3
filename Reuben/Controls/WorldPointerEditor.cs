﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Daiz.Library;
using Daiz.NES.Reuben.ProjectManagement;

namespace Daiz.NES.Reuben
{
    public partial class WorldPointerEditor : UserControl
    {
        public WorldPointerEditor()
        {
            InitializeComponent();
        }

        private WorldPointer _CurrentPointer;
        public WorldPointer CurrentPointer
        {
            get
            {
                return _CurrentPointer;
            }
            set
            {
                if (value == null)
                {
                    this.Enabled = false;
                }
                else
                {
                    this.Enabled = true;
                    _CurrentPointer = value;
                    if (value.LevelGuid != Guid.Empty)
                    {
                        LevelInfo li = ProjectController.LevelManager.GetLevelInfo(value.LevelGuid);
                        LblPointsToWorld.Text = "World: " + ProjectController.WorldManager.GetWorldInfo(li.WorldGuid).Name;
                        LblPointsToLevel.Text = "Level: " + li.Name;
                    }
                    else
                    {
                        LblPointsToLevel.Text = "No level set.";
                    }

                    LblXEnter.Text = "X: 0";
                    LblYEnter.Text = "Y: 0";
                    UpdatePosition();
                }
            }
        }


        private void BtnChange_Click(object sender, EventArgs e)
        {
            LevelSelect lSelect = new LevelSelect();
            DialogResult dr = lSelect.ShowDialog();
            if (dr == DialogResult.OK)
            {
                if (lSelect.SelectedLevel != null)
                {
                    _CurrentPointer.LevelGuid = lSelect.SelectedLevel.LevelGuid;
                    LblPointsToWorld.Text = "World: " + ProjectController.WorldManager.GetWorldInfo(lSelect.SelectedLevel.WorldGuid).Name;
                    LblPointsToLevel.Text = " Level: " + lSelect.SelectedLevel.Name;
                }
            }
        }


        public void UpdatePosition()
        {
            LblXEnter.Text = "X: " + CurrentPointer.X.ToHexString();
            LblYEnter.Text = "Y: " + CurrentPointer.Y.ToHexString();
        }

        private void BtnOpenLevel_Click(object sender, EventArgs e)
        {
            LevelInfo li = ProjectController.LevelManager.GetLevelInfo(CurrentPointer.LevelGuid);
            if (li == null)
            {
                MessageBox.Show("The level could no longer be found in the project");
            }
            else
            {
                ReubenController.EditLevel(li);
            }
        }
    }
}