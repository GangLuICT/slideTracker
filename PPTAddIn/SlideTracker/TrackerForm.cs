﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SlideTracker
{
    public partial class TrackerForm : Form
    {
        public bool done = false;
        public bool cancelledForm = false;
        public TrackerForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.linkLabel1.Visible = false;
            this.label1.MaximumSize = new Size(250, 0);
            this.label1.AutoSize = true;
        }

        public void SetOKVisible(bool state)
        {
            this.OK_button.Visible = state;
            this.FormRefresh();
        }

        public void SetCancelVisible(bool state)
        {
            this.cancel_button.Visible = state;
            this.FormRefresh();
        }

        private void OK_button_Click(object sender, EventArgs e)
        {
            if (this.done) { this.Close(); }
        }

        private void cancel_button_Click(object sender, EventArgs e)
        {
            Globals.ThisAddIn.bw.CancelAsync();
            this.cancelledForm = true;
            this.Focus();
        }

        public void ChangeLabelText(string text)
        {
            this.label1.Text = text;
            this.FormRefresh();
        }

        public void InitProgressBar(int nSlides)
        {
            this.progressBar.Maximum = nSlides;
            this.progressBar.Step = 1;
        }

        public void UpdateProgressBar(int val)
        { 
            if (val <= this.progressBar.Maximum)
            {
                this.progressBar.Value = val;
            }
            else
            {
                this.progressBar.Value = val;
            }
            this.ChangeLabelText("Uploaded " + val + " of " + this.progressBar.Maximum + " slides");
            this.FormRefresh();
        }

        public void FormRefresh()
        {
            //this.Focus();
            //this.TopMost = true;
            //this.Refresh();
        }

        public void DisplayLinkLabel(string text)
        {
            this.linkLabel1.Text = text;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(LinkClicked);
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.Blue;
            this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabel1.Visible = true;
        }

        private void LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e) //callback for clicking link
        {
            System.Diagnostics.Process.Start(Globals.ThisAddIn.GetLinkURL());
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void TrackerForm_Load(object sender, EventArgs e)
        {

        }
    }
}
