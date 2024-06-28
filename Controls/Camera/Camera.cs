using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Illumine.LPR
{
    public partial class Camera : UserControl
    {
        public Action OpenDoor;

        string buttontooltip = "ToolTip Message Here";

        public Camera()
        {
            InitializeComponent();

            button1.Width = button2.Width = this.Width / 10;
            button1.Height = button2.Height = this.Height / 10;
            button2.Left = button1.Left = this.Width - button1.Width;
            button2.Top = 0;
            button1.Top = this.Height - button1.Height;

            toolTip1.OwnerDraw = true;
            toolTip1.Draw += new DrawToolTipEventHandler(toolTip1_Draw);
            toolTip1.Popup += new PopupEventHandler(toolTip1_Popup);
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
            // on popip set the size of tool tip
            e.ToolTipSize = TextRenderer.MeasureText(buttontooltip, new Font("微軟正黑體", 20.0f)) + new Size(4, 8);
        }

        private void toolTip1_Draw(object sender, DrawToolTipEventArgs e)
        {
            Font f = new Font("微軟正黑體", 20.0f);
            e.DrawBackground();
            e.DrawBorder();
            buttontooltip = e.ToolTipText;
            e.Graphics.DrawString(e.ToolTipText, f, Brushes.Black, new PointF(2, 2));
        }

        public bool ValidETag
        {
            get => this.lblEtagIndicator.Visible;
            set
            {
                this.lblEtagIndicator.Visible = value;
            }
        }

        public bool _ETagConnecting;

        public bool ETagConnecting
        {
            get => _ETagConnecting;
            set
            {
                _ETagConnecting = value;
                this.lblEtagIndicator.BackColor = value ? Color.Lime : Color.Red;
                SetToolTip(lblEtagIndicator, value ? "Connecting" : "Disconnected");
            }
        }

        private string _ETag;
        public string ETag
        {
            get => _ETag;
            set
            {
                _ETag = value;
                this.Reading(value);
            }
        }

        public string ModeText
        {
            get => this.label1.Text;
            set
            {
                this.label1.Visible = !string.IsNullOrEmpty(value);
                this.label1.Text = value;
            }
        }

        public string EntryText
        {
            get => this.label3.Text;
            set
            {
                this.label3.Visible = !string.IsNullOrEmpty(value);
                this.label3.Text = value;
            }
        }

        public string TimeText
        {
            get => this.label2.Text;
            set
            {
                this.label2.Visible = !string.IsNullOrEmpty(value);
                this.label2.Text = value;
            }
        }

        public Image SnapshotImage
        {
            get => this.pictureBox1.Image;
            set
            {
                if (value == null)
                {
                    this.pictureBox1.Visible = false;
                }
                else
                {
                    switch (this.PresentMode)
                    {
                        case PresentMode.ShowSnapshotForSeconds:
                            Task.Run((Action)(() => this.ShowSnapShot(value)));
                            break;
                        case PresentMode.OnlySnapshot:
                            this.pictureBox1.Visible = value != null;
                            this.pictureBox1.Image = value;
                            break;
                    }
                }
            }
        }

        public PresentMode PresentMode { get; set; } = PresentMode.OnlySnapshot;

        public IntPtr panelHandle => this.panel1.Handle;

        private async void ShowSnapShot(Image image)
        {
            Camera camera = this;
            if (this.IsHandleCreated)
                camera.Invoke(new Action(() =>
                {
                    this.label1.Visible = true;
                    this.label2.Visible = true;
                    this.pictureBox1.Visible = true;
                    this.pictureBox1.Image = image;
                }));
            await Task.Delay(3000);
            if (this.IsHandleCreated)
                camera.Invoke(new Action(() =>
                {
                    this.pictureBox1.Visible = false;
                    this.label1.Visible = false;
                    this.label2.Visible = false;
                }));
        }

        private void Reading(string eTag)
        {
            Camera camera = this;
            if (this.IsHandleCreated)
                if (eTag != "")

                    camera.Invoke(new Action(() =>
                    {
                        this.lblEtagIndicator.BackColor = Color.Aqua;
                        SetToolTip(lblEtagIndicator, eTag);
                    }));
                else
                    camera.Invoke(new Action(() =>
                    {
                        this.lblEtagIndicator.BackColor = _ETagConnecting ? Color.Lime : Color.Red;
                        SetToolTip(lblEtagIndicator, _ETagConnecting ? "Connecting" : "Disconnected");
                    }));
        }

        private void SetToolTip(Control control, string text)
        {
            buttontooltip = text;
            toolTip1.SetToolTip(control, text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.label1.Visible = !this.pictureBox1.Visible;
            this.label2.Visible = !this.pictureBox1.Visible;
            this.pictureBox1.Visible = !this.pictureBox1.Visible;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Action openDoor = this.OpenDoor;
            if (openDoor == null)
                return;
            openDoor();
        }

        private void lblEtagIndicator_MouseLeave(object sender, EventArgs e)
        {
            toolTip1.Hide(this);
        }

        private void lblEtagIndicator_MouseEnter(object sender, EventArgs e)
        {
            toolTip1.Show(buttontooltip, this);
        }

        public Image Snapshot()
        {
            return this.pictureBox1.Image;
        }
    }
}
