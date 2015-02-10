﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Office = Microsoft.Office.Core;
using PPT = Microsoft.Office.Interop.PowerPoint;

// TODO:  Follow these steps to enable the Ribbon (XML) item:

// 1: Copy the following code block into the ThisAddin, ThisWorkbook, or ThisDocument class.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new SlideTrackerRibbon();
//  }

// 2. Create callback methods in the "Ribbon Callbacks" region of this class to handle user
//    actions, such as clicking a button. Note: if you have exported this Ribbon from the Ribbon designer,
//    move your code from the event handlers to the callback methods and modify the code to work with the
//    Ribbon extensibility (RibbonX) programming model.

// 3. Assign attributes to the control tags in the Ribbon XML file to identify the appropriate callback methods in your code.  

// For more information, see the Ribbon XML documentation in the Visual Studio Tools for Office Help.


namespace SlideTracker
{
    [ComVisible(true)]
    public class SlideTrackerRibbon : Office.IRibbonExtensibility
    {
        bool startup = false; // starts as false. after initializing will be true. for setting default options
        bool displayStopButton = false; //should we display the stop button (true) or broadcast button (false)
        bool displayOptionsGroup = false; //is the options group displayed
        private bool showRibbon = true; //should the ribbon be shown at all
        private Office.IRibbonUI ribbon; //the ribbon object
        internal static Office.IRibbonUI ribbon1; //for access from other functions
        private System.Windows.Forms.Form successForm; //form to notify success
        public SlideTrackerRibbon()
        {
        }

        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("SlideTracker.SlideTrackerRibbon.xml");
        }

        #endregion

        
        //Create callback methods here. For more information about adding callback methods, visit http://go.microsoft.com/fwlink/?LinkID=271226

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            if (!Globals.ThisAddIn.CheckVersion())
            {
                System.Windows.Forms.MessageBox.Show("You slideTracker version, " +
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                    " is out of date on no longer compatible. Please visit " + Globals.ThisAddIn.GetLinkURL() +
                    " for the latest version. ","slideTracker Error");
                showRibbon = false;

            }
            this.ribbon = ribbonUI;
            ribbon1 = ribbonUI; // to expose this to globals.ribbons
        }

        #region visibility helpers

        public bool DisplayRibbon(Office.IRibbonControl control) // show/hide the whole ribbon
        {
            return showRibbon;
        }

        public bool IsStopButtonVisible(Office.IRibbonControl control) // show/Hide StopBroadcast button
        {
            return displayStopButton;
        }

        public bool IsExportButtonVisible(Office.IRibbonControl control) // show/hide export button, opposite of stop button
        {
            return !displayStopButton;
        }

        public bool DisplayOptionsGroup(Office.IRibbonControl control) // show/hide options group
        {
            return displayOptionsGroup;
        }

        /*public bool OptioinsNotVisible(Office.IRibbonControl control)
        {
            return displayOptionsGroup;
        }*/

        public void ToggleDisplay(Office.IRibbonControl control)
        {
            displayOptionsGroup = !displayOptionsGroup;
            this.ribbon.InvalidateControl("OptionsGroup");
            GetToggleDisplayLabel(control);
        }

        public string GetToggleDisplayLabel(Office.IRibbonControl control) 
            // text for button to display/hide the options
        {
            string ret;
            if (displayOptionsGroup)
            {
                ret = "Hide Options";
            }
            else
            {
                ret = "Show Options";
            }
            this.ribbon.InvalidateControl("HideOptionsButton");
            return ret;
        }
        #endregion
        #region Ribbon Callbacks

        public void OnExportButton(Office.IRibbonControl control) //export to png, make remote pres, upload it. 
        {
            Globals.ThisAddIn.uploadSuccess = true;
            int pad = 10;
            System.Windows.Forms.Form progressForm = new System.Windows.Forms.Form();
            System.Windows.Forms.Label lab = new System.Windows.Forms.Label();
            progressForm.Size = new System.Drawing.Size(350, 100);
            progressForm.Text = "Upload Progress";
            lab.Width = progressForm.Width - 4 * pad;
            lab.Top = pad;
            lab.Left = pad;
            lab.Text = "Exporting files to " + Globals.ThisAddIn.fmt;
            lab.Font = new System.Drawing.Font("Arial", 12);
            progressForm.Controls.Add(lab);
            progressForm.Show();
            progressForm.Update();
            Globals.ThisAddIn.MakeLUT();
            Globals.ThisAddIn.Application.ActivePresentation.Export(Globals.ThisAddIn.SlideDir, Globals.ThisAddIn.fmt);
            Globals.ThisAddIn.DeleteHiddenSlides();
            if (Globals.ThisAddIn.allowDownload)
            {
                Globals.ThisAddIn.Application.ActivePresentation.ExportAsFixedFormat(
                    Globals.ThisAddIn.SlideDir + "/presentation.pdf", PPT.PpFixedFormatType.ppFixedFormatTypePDF);
            }
            try
            {
                lab.Text = "Contacting server... This may take a moment.";
                if (!Globals.ThisAddIn.CheckFileRequirements())
                {
                    Globals.ThisAddIn.uploadSuccess = false;
                    System.Windows.Forms.MessageBox.Show("Sorry, total file size too big for slideTracker.");
                    return;
                }

                string resp = Globals.ThisAddIn.CreateRemotePresentation();
                lab.Text = "uploading remote presentation...";
                progressForm.Update();
                string resp2 = Globals.ThisAddIn.UploadRemotePresentation();
                progressForm.Close();

                DisplaySuccessform();

                displayStopButton = true;
                UpdateDisplay();
            }
            catch
            {
                Globals.ThisAddIn.uploadSuccess = false;
                System.Windows.Forms.MessageBox.Show("Problem communicating with server. Check internet connection and try again");
                //progressForm.Close();
            }
            finally
            {
                if (!progressForm.IsDisposed) { progressForm.Close(); }
            }

        }

        private void DisplaySuccessform() // format and display the form indicating upload success
        {
            System.Windows.Forms.Form successForm = new System.Windows.Forms.Form();
            this.successForm = successForm;
            successForm.Size = new System.Drawing.Size(450, 180);
            successForm.Icon = new System.Drawing.Icon(System.Drawing.SystemIcons.Information, 60, 60);

            int pad = 10;
            System.Drawing.Font myFont = new System.Drawing.Font("Arial", 11);
            
            System.Windows.Forms.Label textLabel = new System.Windows.Forms.Label();
            textLabel.Text = "ALL DONE!" + System.Environment.NewLine +
                "Just start presenting as usual. The audience will see the tracking ID on your slides.";
            textLabel.AutoSize = false;
            textLabel.Width = successForm.Width - 4*pad;
            textLabel.Left =(successForm.Width - textLabel.Width) / 2;
            textLabel.Top = pad;
            textLabel.Height = 60;
            textLabel.Font = myFont;

            System.Windows.Forms.LinkLabel linkLabel = new System.Windows.Forms.LinkLabel();
            linkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(LinkClicked);
            linkLabel.Text = Globals.ThisAddIn.GetLinkURL();
            linkLabel.VisitedLinkColor = System.Drawing.Color.Blue;
            linkLabel.LinkColor = System.Drawing.Color.Navy;
            linkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            linkLabel.Font = myFont;
            linkLabel.Size = new System.Drawing.Size(340, 80);
            linkLabel.Top = textLabel.Height + pad;
            linkLabel.Left = (successForm.Width - linkLabel.Width) / 2;
            linkLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Size = new System.Drawing.Size(90, 40);
            okButton.Text = "OK";
            okButton.Font = myFont;
            okButton.Left = (successForm.Width - okButton.Width) / 2;
            okButton.Top = -pad + 3 * (successForm.Height - okButton.Height) / 4;
            okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            okButton.Click += new EventHandler(CloseSuccessForm);

            successForm.Controls.Add(textLabel);
            successForm.Controls.Add(okButton);
            successForm.Controls.Add(linkLabel);
            Globals.ThisAddIn.broadcastPresentationName = Globals.ThisAddIn.Application.ActivePresentation.Name;
            successForm.Text = "Success: " + Globals.ThisAddIn.broadcastPresentationName;

            successForm.Show();
        }

        private void CloseSuccessForm(object sender, EventArgs e) { this.successForm.Close(); }

        public void OnStopBroadcast(Office.IRibbonControl control)
        {
            //gets called when the StopBroadcast button is pressed
            //delete remote pres, delete all slide files in slideDir, update button
            Globals.ThisAddIn.DeleteRemotePresentation();
            DirectoryInfo dirInfo = new DirectoryInfo(Globals.ThisAddIn.SlideDir);
            foreach(FileInfo fi in dirInfo.GetFiles("*." + Globals.ThisAddIn.fmt)) //dont delete log file
            {
                fi.Delete();
            }
            //now delete the pdf file (if exists)
            foreach (FileInfo fi in dirInfo.GetFiles("*.pdf"))
            {
                fi.Delete();
            }
            displayStopButton = false;
            Globals.ThisAddIn.broadcastPresentationName = null;
            UpdateDisplay(); // go back to start broadcast button, remove pres_ID, etc. 
            Globals.ThisAddIn.uploadSuccess = false;
            Globals.ThisAddIn.maxClients = 0;
        }

        private void UpdateDisplay() //update the controls that may get changed
        {
            this.ribbon.InvalidateControl("BroadcastButton"); //updates the display for this control
            this.ribbon.InvalidateControl("StopBroadcast"); //update display
            this.ribbon.InvalidateControl("PresID");
            this.ribbon.InvalidateControl("PresIDLink");
            this.ribbon.InvalidateControl("PresIDGroup");
            this.ribbon.InvalidateControl("NumViewers");
            this.ribbon.InvalidateControl("AllowDownload");
        }

        public void OnAllowDownload(Office.IRibbonControl control, bool isClicked)
        {
            //gets called when the AllowDownload button is checked/unchecked
            Globals.ThisAddIn.allowDownload = isClicked;
        }

        public bool EnableAllowDownload(Office.IRibbonControl control) //callback for clicking "allow Downloads"
        {
            return !Globals.ThisAddIn.uploadSuccess;
        }

        public void OnDropDownShowIP(Office.IRibbonControl control, string selectedId, int selectedIndex)
        // callback for selecting which slides to display tracking ID 
        {
            Globals.ThisAddIn.showOnAll = ("all" == selectedId);
        }

        public string GetSelectedShowIP(Office.IRibbonControl control) //return list item for which slides to show tracking ID
        {
            //set default dropdown menu to "all"
            //this is a hack and will break if we change the order of things
            //relies on the fact that this one loads before the next dropdown menu
            if (startup)
            {
                return control.Id;
            }
            else
            {
                return "all";
            }
        }

        public void OnBannerLocation(Office.IRibbonControl control, string selectedID, int selectedIndex)
        // callback for banner location dropdown
        {
            float width = Globals.ThisAddIn.Application.ActivePresentation.PageSetup.SlideWidth - (float)Globals.ThisAddIn.width;
            float height = Globals.ThisAddIn.Application.ActivePresentation.PageSetup.SlideHeight - (float)Globals.ThisAddIn.height;
            float offset = 8;
            switch (selectedIndex)
            {
                case 0: // BL
                    Globals.ThisAddIn.left = offset;
                    Globals.ThisAddIn.top = height - offset;
                    break;
                case 1: //BR
                    Globals.ThisAddIn.left = width - offset;
                    Globals.ThisAddIn.top = height - offset;
                    break;
                case 2: //TL
                    Globals.ThisAddIn.left = offset;
                    Globals.ThisAddIn.top = offset;
                    break;
                case 3: //TR
                    Globals.ThisAddIn.left = width - offset;
                    Globals.ThisAddIn.top = offset;
                    break;
            }
        }

        public string GetSelectedShowBanner(Office.IRibbonControl control) //returns list item for where on slide to show tracking ID
        {
            //this is a hack. relies on the fact that this downdown loads second
            if (startup)
            {
                return control.Id;
            }
            else
            {
                startup = true;
                //terrible hack to hard code the corret values at startup
                float width = Globals.ThisAddIn.Application.ActivePresentation.PageSetup.SlideWidth - (float)Globals.ThisAddIn.width;
                float height = Globals.ThisAddIn.Application.ActivePresentation.PageSetup.SlideHeight - (float)Globals.ThisAddIn.height;
                float offset = 8;
                Globals.ThisAddIn.left = width - offset;
                Globals.ThisAddIn.top = offset;

                return "TR";
            }
        }

        public string GetPresLink(Office.IRibbonControl control) //returns the link text for presentation
        {
            if (Globals.ThisAddIn.uploadSuccess)
            {
                return System.Environment.NewLine + Globals.ThisAddIn.GetLinkURL();
            }
            else
            {
                return "";
            }
        }

        public void FollowPresLink(Office.IRibbonControl control) //executed when link pressed in ribbon
        {
            if (Globals.ThisAddIn.uploadSuccess)
            {
                System.Diagnostics.Process.Start(Globals.ThisAddIn.GetLinkURL());
            }
        }

        public string GetPresID(Office.IRibbonControl control) //return the text for pres_ID to ribbon
        {
            if (Globals.ThisAddIn.uploadSuccess)
            {
                return "Presentation ID:  " + Globals.ThisAddIn.pres_ID + 
                    System.Environment.NewLine + "   Presentation: " + Globals.ThisAddIn.broadcastPresentationName;
            }
            else
            {
                return "";
            }
        }

        public string GetNumViewers(Office.IRibbonControl control) //return the text for the max num viewers for ribbon
        {
            if (Globals.ThisAddIn.maxClients > 0 && Globals.ThisAddIn.uploadSuccess)
            {
                return "Maximum viewers: " + Globals.ThisAddIn.maxClients;
            }
            else
            {
                return "";
            }
        }

        private void LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e) //callback for clicking link
        {
            System.Diagnostics.Process.Start(Globals.ThisAddIn.GetLinkURL());
        }



        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
