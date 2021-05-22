using System;
using System.Drawing;
using System.Windows.Forms;

namespace SaveConcert
{
    public partial class ListTileForSetListView : UserControl
    {
        static bool white;
        string name;

        public ListTileForSetListView(string name)
        {
            InitializeComponent();
            this.name = name;

            if(white)
            {
                this.BackColor = Color.LightGray;
                white = false;
            }
            else
            {
                white = true;
            }
        }

        private void ListTileForSetListView_Load(object sender, EventArgs e)
        {
            label1.Text = name;
        }
    }
}
