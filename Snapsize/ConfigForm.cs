﻿using System;
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
    public partial class ConfigForm : Form
    {
        public ConfigForm()
        {
            InitializeComponent();
        }

        public string Areas
        {
            get
            {
                return areasTextBox.Text;
            }
            set
            {
                areasTextBox.Text = value;
            }
        }
    }
}
