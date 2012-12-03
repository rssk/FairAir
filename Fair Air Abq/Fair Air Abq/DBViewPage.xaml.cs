using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Xml.Linq;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Fair_Air_Abq
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class DBViewPage : Fair_Air_Abq.Common.LayoutAwarePage
    {
        public DBViewPage()
        {
            this.InitializeComponent();
        }
        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        /// 
        private async void bt_Tapped(object sender, TappedRoutedEventArgs e)
        {
            XDocument xd;
            xd = await airDB.getAQI();
            List<airDB> aList = new List<airDB>();

            //Couldn't get linq working for tags that weren't the same (ie: d1, d2,...), this is messy but works...
            for (int i = 1; i < 31; i++)
            {
                airDB air = new airDB();
                air.date = "Date: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("date").Value;
                if (String.Equals(air.date, "Date: "))
                {
                    air.date = "Empty Record";
                    break;
                }
                air.ozone = "Ozone Raw: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("ozone").Value;
                air.ozoneAQI = "Ozone Index: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("ozoneAQI").Value;
                air.pm10 = "PM10 Raw: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("pm10").Value;
                air.pm10AQI = "PM10 Index: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("pm10AQI").Value;
                air.pm25 = "PM25 Raw: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("pm25").Value;
                air.pm25AQI = "PM25 Index: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("pm25AQI").Value;
                air.no2 = "NO2 Raw: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("no2").Value;
                air.no2AQI = "NO2 Index: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("noAQI").Value;
                air.so2 = "SO2 Raw: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("so2").Value;
                air.so2AQI = "SO2 Index: " + xd.Root.Element("data").Element("d" + Convert.ToString(i)).Element("so2AQI").Value;

                aList.Add(air);
            }
            gv.ItemsSource = aList;
        }

    }
}
