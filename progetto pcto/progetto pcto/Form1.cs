﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;



namespace progetto_pcto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private async void button1_Click(object sender, EventArgs e)
        {
            var response = await resthelper.Get(textBox1.Text);
            richTextBox1.Text=resthelper.BeautyJson(response);

        }

    }
}
