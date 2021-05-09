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
using SetlistNet;
using SetlistNet.Models;

namespace SaveConcert
{
    public partial class Form1 : Form
    {
        public IGlobeViewer gv = default(IGlobeViewer);
        public ISetlistAPIManager sam = default(ISetlistAPIManager);
        private bool taskRunning;
        public Form1()
        {
            InitializeComponent();

            gv = new GlobeViewer.Classes.GlobeViewer(panel1);
            gv.BindMarkerClickedEvent(delegate (object sender, string location)
            {
                MessageBox.Show(location);
            });

            sam = new SetlistAPIManager("TDpswW5K3jP46t26H1XXtPzZRv1xwgX2nGxo");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*string[] placesName = textBox1.Text.Split(' ');
            IList<(string, string, string)> locations = new List<(string Name, string X, string Y)>();*/

            /*foreach (string i in placesName)
            {
                //locations.Add((i, "12.646361", "42.504154"));
                locations.Add((i, null, null));
            }*/
            /*locations.Add(("Italy", null, null));
            locations.Add(("Germany", null, null));
            locations.Add(("France", null, null));
            locations.Add(("Austria", null, null));
            locations.Add(("Poland", null, null));
            locations.Add(("Finland", null, null));
            locations.Add(("Norway", null, null));
            locations.Add(("Sweden", null, null));

            try
            {
                gv.LoadMarkers(locations);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }*/
            if (!taskRunning)
            {
                Task.Run(() =>
                {
                    taskRunning = true;
                    var a = new Setlist();
                    a.Artist = new Artist("Metallica");
                    var b = sam.Search(a);
                    MessageBox.Show(b.Total.ToString() + " " + b.Count.ToString());

                    string[] placesName = textBox1.Text.Split(' ');
                    IList<(string, string, string, string)> locations = new List<(string MarkerName, string Name, string X, string Y)>();
                    foreach (Setlist i in b)
                    {
                        locations.Add((i.Id, i.Venue.City.State + " " + i.Venue.City.Name + " " + i.Venue.Name, i.Venue.City.Coords.Longitude.ToString(), i.Venue.City.Coords.Latitude.ToString()));
                    }

                    try
                    {
                        gv.LoadMarkers(locations, geocodeAlways: true);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    taskRunning = false;
                });
            }
        }
    }
}
