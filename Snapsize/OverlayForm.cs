using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snapsize
{
    public partial class OverlayForm : Form
    {
        public OverlayForm()
        {
            InitializeComponent();
        }

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawString(
                string.Format("({0}, {1}) -> ({2}, {3})", this.Left, this.Top, this.Right, this.Bottom),
                this.Font, Brushes.Blue, 20, 20);

            e.Graphics.DrawString(
                string.Format("{0} x {1}", this.Width, this.Height),
                this.Font, Brushes.Blue, 20, 120);
        }

        private void OverlayForm_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
