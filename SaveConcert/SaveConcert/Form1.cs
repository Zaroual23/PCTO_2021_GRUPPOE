using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GlobeViewer.Classes;
using GlobeViewer.Interfaces;

namespace progetto_pcto
{
    public partial class Form1 : Form
    {
        public IGlobeViewer gv = default(IGlobeViewer);
        public Form1()
        {
            InitializeComponent();
            gv = new GlobeViewer.Classes.GlobeViewer(panel1);
            gv.BindMarkerClickedEvent(delegate (object sender, string location)
            {
                MessageBox.Show(location);
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] placesName = textBox1.Text.Split(' ');
            IList<(string, string, string)> locations = new List<(string Name, string X, string Y)>();

            foreach (string i in placesName)
            {
                //locations.Add((i, "12.646361", "42.504154"));
                locations.Add(("Compaq Center", "0", "0"));
            }

            gv.LoadMarkers(locations);
        }
    }
}
