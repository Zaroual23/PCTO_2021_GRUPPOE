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
            if (!taskRunning)
            {
                Task.Run(() =>
                {
                    taskRunning = true;

                    //Set up query
                    Setlist query = new Setlist();
                    query.Artist = new Artist("Metallica");
                    Setlists result = sam.Search(query);
                    
                    //Send data to GlobeViewer
                    IList<(string, string, string, string)> locations = new List<(string MarkerName, string Name, string X, string Y)>();
                    foreach (Setlist i in result)
                    {
                        locations.Add((i.Id, i.Venue.City.State + " " + i.Venue.City.Name + " " + i.Venue.Name, i.Venue.City.Coords.Longitude.ToString(), i.Venue.City.Coords.Latitude.ToString()));
                    }
                    try
                    {
                        gv.LoadMarkers(locations, geocodeAlways: true, skipUngeocodableLocations: true);
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
