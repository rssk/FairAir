using Fair_Air_Abq.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using System.Xml.Linq;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Fair_Air_Abq
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ItemsPage : Fair_Air_Abq.Common.LayoutAwarePage
    {
        public ItemsPage()
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

        private void TextBlock_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            MessageDialog mBox = new MessageDialog("Index values are calculated using the Air Quality Index equations used by AirNow.\nHowever these indexes are not guaranteed to be accurate nor are endorsed by the EPA.\nColor Meanings:\nGreen: Good\nYellow: Moderate\nOrange: Unhealth for sensitive persons\nRed: Unhealthy\nPurple: Very Unhealthy\nMaroon: Hazardous", "Index Values");
            mBox.ShowAsync();
        }

        private async void Button_Tapped_4(object sender, TappedRoutedEventArgs e)
        {
            XDocument xd = await airDB.getAQI();
            if (xd != null)
                chartDB(xd);
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        private void chartDB(XDocument xd)
        {
            bool oneRecord = false;
            string cur = xd.Root.Element("current").Value;
            double curD = Convert.ToDouble(cur);

            if (curD == 0)// there is no data in the file, return
                return;

            string elem = "d" + cur;
            double count = sl1.Value;
            string[] values = new string[30 * 11];

            for (int i = 0; i < count; i++)
            {
                XElement mod = xd.Root.Element("data").Element(elem);

                values[i * 11] = mod.Element("date").Value;
                if (String.Equals(values[i * 11], ""))//deals with blank records
                {
                    if (i == 1)
                    {
                        oneRecord = true;
                        break;
                    }
                    else
                    {
                        count = i;
                        break;
                    }
                }

                values[i * 11 + 1] = mod.Element("ozone").Value;
                values[i * 11 + 2] = mod.Element("ozoneAQI").Value;
                values[i * 11 + 3] = mod.Element("pm10").Value;
                values[i * 11 + 4] = mod.Element("pm10AQI").Value;
                values[i * 11 + 5] = mod.Element("pm25").Value;
                values[i * 11 + 6] = mod.Element("pm25AQI").Value;
                values[i * 11 + 7] = mod.Element("no2").Value;
                values[i * 11 + 8] = mod.Element("noAQI").Value;
                values[i * 11 + 9] = mod.Element("so2").Value;
                values[i * 11 + 10] = mod.Element("so2AQI").Value;

                if (curD == 1)
                {
                    curD = 30;
                    elem = "d" + Convert.ToString(curD);
                }
                else
                {
                    curD--;
                    elem = "d" + Convert.ToString(curD);

                }
            }

            Dictionary<string, double> dailyVal =
        new Dictionary<string, double>();

            double v;
            if (Double.TryParse(values[2], out v))
                dailyVal.Add("Ozone", v);
            else
                dailyVal.Add("Ozone", -9999);
            if (Double.TryParse(values[4], out v))
                dailyVal.Add("PM10", v);
            else
                dailyVal.Add("PM10", -9999);
            if (Double.TryParse(values[6], out v))
                dailyVal.Add("PM25", v);
            else
                dailyVal.Add("PM25", -9999);
            if (Double.TryParse(values[8], out v))
                dailyVal.Add("NO2", v);
            else
                dailyVal.Add("NO2", -9999);
            if (Double.TryParse(values[10], out v))
                dailyVal.Add("SO2", v);
            else
                dailyVal.Add("SO2", -9999);

            var sortList = from x in dailyVal
                           orderby x.Value descending
                           select x;

            //sets color
            if (sortList.ElementAt(0).Value <= 50 && sortList.ElementAt(0).Value >= 0)
            {
                s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }
            else if (sortList.ElementAt(0).Value > 50 && sortList.ElementAt(0).Value <= 100)
            {
                s1.Fill = new SolidColorBrush(Colors.Yellow);
            }
            else if (sortList.ElementAt(0).Value > 100 && sortList.ElementAt(0).Value <= 150)
            {
                s1.Fill = new SolidColorBrush(Colors.Orange);
            }
            else if (sortList.ElementAt(0).Value > 150 && sortList.ElementAt(0).Value <= 200)
            {
                s1.Fill = new SolidColorBrush(Colors.Red);
            }
            else if (sortList.ElementAt(0).Value > 200 && sortList.ElementAt(0).Value <= 300)
            {
                s1.Fill = new SolidColorBrush(Colors.Purple);
            }
            else if (sortList.ElementAt(0).Value > 300 && sortList.ElementAt(0).Value <= 500)
            {
                s1.Fill = new SolidColorBrush(Colors.Maroon);
            }
            else
            {
                s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }

            if (sortList.ElementAt(1).Value <= 50 && sortList.ElementAt(1).Value >= 0)
            {
                s2.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }
            else if (sortList.ElementAt(1).Value > 50 && sortList.ElementAt(1).Value <= 100)
            {
                s2.Fill = new SolidColorBrush(Colors.Yellow);
            }
            else if (sortList.ElementAt(1).Value > 100 && sortList.ElementAt(1).Value <= 150)
            {
                s2.Fill = new SolidColorBrush(Colors.Orange);
            }
            else if (sortList.ElementAt(1).Value > 150 && sortList.ElementAt(1).Value <= 200)
            {
                s2.Fill = new SolidColorBrush(Colors.Red);
            }
            else if (sortList.ElementAt(1).Value > 200 && sortList.ElementAt(1).Value <= 300)
            {
                s2.Fill = new SolidColorBrush(Colors.Purple);
            }
            else if (sortList.ElementAt(1).Value > 300 && sortList.ElementAt(1).Value <= 500)
            {
                s2.Fill = new SolidColorBrush(Colors.Maroon);
            }
            else
            {
                s2.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }

            if (sortList.ElementAt(2).Value <= 50 && sortList.ElementAt(2).Value >= 0)
            {
                s3.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }
            else if (sortList.ElementAt(2).Value > 50 && sortList.ElementAt(2).Value <= 100)
            {
                s3.Fill = new SolidColorBrush(Colors.Yellow);
            }
            else if (sortList.ElementAt(2).Value > 100 && sortList.ElementAt(2).Value <= 150)
            {
                s3.Fill = new SolidColorBrush(Colors.Orange);
            }
            else if (sortList.ElementAt(2).Value > 150 && sortList.ElementAt(2).Value <= 200)
            {
                s3.Fill = new SolidColorBrush(Colors.Red);
            }
            else if (sortList.ElementAt(2).Value > 200 && sortList.ElementAt(2).Value <= 300)
            {
                s3.Fill = new SolidColorBrush(Colors.Purple);
            }
            else if (sortList.ElementAt(2).Value > 300 && sortList.ElementAt(2).Value <= 500)
            {
                s3.Fill = new SolidColorBrush(Colors.Maroon);
            }
            else
            {
                s3.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }

            if (sortList.ElementAt(3).Value <= 50 && sortList.ElementAt(3).Value >= 0)
            {
                s4.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }
            else if (sortList.ElementAt(3).Value > 50 && sortList.ElementAt(3).Value <= 100)
            {
                s4.Fill = new SolidColorBrush(Colors.Yellow);
            }
            else if (sortList.ElementAt(3).Value > 100 && sortList.ElementAt(3).Value <= 150)
            {
                s4.Fill = new SolidColorBrush(Colors.Orange);
            }
            else if (sortList.ElementAt(3).Value > 150 && sortList.ElementAt(3).Value <= 200)
            {
                s4.Fill = new SolidColorBrush(Colors.Red);
            }
            else if (sortList.ElementAt(3).Value > 200 && sortList.ElementAt(3).Value <= 300)
            {
                s4.Fill = new SolidColorBrush(Colors.Purple);
            }
            else if (sortList.ElementAt(3).Value > 300 && sortList.ElementAt(3).Value <= 500)
            {
                s4.Fill = new SolidColorBrush(Colors.Maroon);
            }
            else
            {
                s4.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }

            if (sortList.ElementAt(4).Value <= 50 && sortList.ElementAt(4).Value >= 0)
            {
                s5.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }
            else if (sortList.ElementAt(4).Value > 50 && sortList.ElementAt(4).Value <= 100)
            {
                s5.Fill = new SolidColorBrush(Colors.Yellow);
            }
            else if (sortList.ElementAt(4).Value > 100 && sortList.ElementAt(4).Value <= 150)
            {
                s5.Fill = new SolidColorBrush(Colors.Orange);
            }
            else if (sortList.ElementAt(4).Value > 150 && sortList.ElementAt(4).Value <= 200)
            {
                s5.Fill = new SolidColorBrush(Colors.Red);
            }
            else if (sortList.ElementAt(4).Value > 200 && sortList.ElementAt(4).Value <= 300)
            {
                s5.Fill = new SolidColorBrush(Colors.Purple);
            }
            else if (sortList.ElementAt(4).Value > 300 && sortList.ElementAt(4).Value <= 500)
            {
                s5.Fill = new SolidColorBrush(Colors.Maroon);
            }
            else
            {
                s5.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
            }

            p1.Text = sortList.ElementAt(0).Key;
            if (sortList.ElementAt(0).Value != -9999)
                n1.Text = Convert.ToString(sortList.ElementAt(0).Value);
            else
                n1.Text = "N/A";

            p2.Text = sortList.ElementAt(1).Key;
            if (sortList.ElementAt(1).Value != -9999)
                n2.Text = Convert.ToString(sortList.ElementAt(1).Value);
            else
                n2.Text = "N/A";

            p3.Text = sortList.ElementAt(2).Key;
            if (sortList.ElementAt(2).Value != -9999)
                n3.Text = Convert.ToString(sortList.ElementAt(2).Value);
            else
                n3.Text = "N/A";

            p4.Text = sortList.ElementAt(3).Key;
            if (sortList.ElementAt(3).Value != -9999)
                n4.Text = Convert.ToString(sortList.ElementAt(3).Value);
            else
                n4.Text = "N/A";

            p5.Text = sortList.ElementAt(4).Key;
            if (sortList.ElementAt(4).Value != -9999)
                n5.Text = Convert.ToString(sortList.ElementAt(4).Value);
            else
                n5.Text = "N/A";

            if (oneRecord)
                return;

            double temp = 0;
            double max = 0;
            double min = 10000000;// way bigger than anything in the array

            //proably a better way to do this, but couldnt get linq quite working
            for (int i = 0; i < (count * 11); i++)
            {
                if (Double.TryParse(values[i], out temp))
                {
                    if (temp > max && temp < 10000)//second check because of dates
                        max = temp;
                    if (temp < min)
                        min = temp;
                }
            }

            double llxcanv = 0.0;
            double llycanv = cv.Height - 1;
            double urxcanv = cv.Width - 1;
            double urycanv = 0.0;

            double xscale = (urxcanv - llxcanv) / (count - 1);
            double yscale = (urycanv - llycanv) / (max - min);
            double parse;

            //Ozone
            Polyline ploz = new Polyline();
            ploz.StrokeThickness = 6;
            ploz.Stroke = new SolidColorBrush(Colors.Blue);

            for (int i = 0; i < count; i++)
            {
                Point newp;
                newp.X = (i + 1 - 1) * xscale + llxcanv;
                if (!Double.TryParse(values[i * 11 + 2], out parse))
                    parse = 0;
                newp.Y = (parse - min) * yscale + llycanv;
                ploz.Points.Add(newp);

            }

            //PM10
            Polyline plpm10 = new Polyline();
            plpm10.StrokeDashArray = new DoubleCollection() { 4 };
            plpm10.StrokeThickness = 6;
            plpm10.Stroke = new SolidColorBrush(Colors.DarkOrange);

            for (int i = 0; i < count; i++)
            {
                Point newp;
                newp.X = (i + 1 - 1) * xscale + llxcanv;
                if (!Double.TryParse(values[i * 11 + 4], out parse))
                    parse = 0;
                newp.Y = (parse - min) * yscale + llycanv;
                plpm10.Points.Add(newp);

            }

            //PM25
            Polyline plpm25 = new Polyline();
            plpm25.StrokeDashArray = new DoubleCollection() { 3 };
            plpm25.StrokeThickness = 6;
            plpm25.Stroke = new SolidColorBrush(Colors.Gray);

            for (int i = 0; i < count; i++)
            {
                Point newp;
                newp.X = (i + 1 - 1) * xscale + llxcanv;
                if (!Double.TryParse(values[i * 11 + 6], out parse))
                    parse = 0;
                newp.Y = (parse - min) * yscale + llycanv;
                plpm25.Points.Add(newp);

            }

            //no2
            Polyline plno2 = new Polyline();
            plno2.StrokeDashArray = new DoubleCollection() { 1 };
            plno2.StrokeThickness = 6;
            plno2.Stroke = new SolidColorBrush(Colors.Green);

            for (int i = 0; i < count; i++)
            {
                Point newp;
                newp.X = (i + 1 - 1) * xscale + llxcanv;
                if (!Double.TryParse(values[i * 11 + 8], out parse))
                    parse = 0;
                newp.Y = (parse - min) * yscale + llycanv;
                plno2.Points.Add(newp);

            }

            //so2
            Polyline plso2 = new Polyline();
            plso2.StrokeDashArray = new DoubleCollection() { 2 }; ;
            plso2.StrokeThickness = 6;
            plso2.Stroke = new SolidColorBrush(Colors.SkyBlue);

            for (int i = 0; i < count; i++)
            {
                Point newp;
                newp.X = (i + 1 - 1) * xscale + llxcanv;
                if (!Double.TryParse(values[i * 11 + 10], out parse))
                    parse = 0;
                newp.Y = (parse - min) * yscale + llycanv;
                plso2.Points.Add(newp);

            }

            cv.Children.Clear();
            cv.Children.Add(ploz);
            cv.Children.Add(plpm10);
            cv.Children.Add(plpm25);
            cv.Children.Add(plno2);
            cv.Children.Add(plso2);
        }

        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            if (pageState != null)
            {
                p1.Text = pageState["p1"].ToString();
                n1.Text = pageState["n1"].ToString();

                double d;
                if (Double.TryParse(n1.Text, out d))
                {
                    if (d <= 50 && d >= 0)
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                    else if (d > 50 && d <= 100)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (d > 100 && d <= 150)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Orange);
                    }
                    else if (d > 150 && d <= 200)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else if (d > 200 && d <= 300)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Purple);
                    }
                    else if (d > 300 && d <= 500)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Maroon);
                    }
                    else
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                }
                else
                {
                    s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                }

                p2.Text = pageState["p2"].ToString();
                n2.Text = pageState["n2"].ToString();
                if (Double.TryParse(n1.Text, out d))
                {
                    if (d <= 50 && d >= 0)
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                    else if (d > 50 && d <= 100)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (d > 100 && d <= 150)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Orange);
                    }
                    else if (d > 150 && d <= 200)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else if (d > 200 && d <= 300)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Purple);
                    }
                    else if (d > 300 && d <= 500)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Maroon);
                    }
                    else
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                }
                else
                {
                    s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                }


                p3.Text = pageState["p3"].ToString();
                n3.Text = pageState["n3"].ToString();
                if (Double.TryParse(n1.Text, out d))
                {
                    if (d <= 50 && d >= 0)
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                    else if (d > 50 && d <= 100)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (d > 100 && d <= 150)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Orange);
                    }
                    else if (d > 150 && d <= 200)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else if (d > 200 && d <= 300)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Purple);
                    }
                    else if (d > 300 && d <= 500)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Maroon);
                    }
                    else
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                }
                else
                {
                    s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                }


                p4.Text = pageState["p4"].ToString();
                n4.Text = pageState["n4"].ToString();
                if (Double.TryParse(n1.Text, out d))
                {
                    if (d <= 50 && d >= 0)
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                    else if (d > 50 && d <= 100)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (d > 100 && d <= 150)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Orange);
                    }
                    else if (d > 150 && d <= 200)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else if (d > 200 && d <= 300)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Purple);
                    }
                    else if (d > 300 && d <= 500)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Maroon);
                    }
                    else
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                }
                else
                {
                    s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                }


                p5.Text = pageState["p5"].ToString();
                n5.Text = pageState["n5"].ToString();
                if (Double.TryParse(n1.Text, out d))
                {
                    if (d <= 50 && d >= 0)
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                    else if (d > 50 && d <= 100)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (d > 100 && d <= 150)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Orange);
                    }
                    else if (d > 150 && d <= 200)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Red);
                    }
                    else if (d > 200 && d <= 300)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Purple);
                    }
                    else if (d > 300 && d <= 500)
                    {
                        s1.Fill = new SolidColorBrush(Colors.Maroon);
                    }
                    else
                    {
                        s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                    }
                }
                else
                {
                    s1.Fill = new SolidColorBrush(Color.FromArgb(255, 102, 232, 74));
                }



            }
        }

        protected override void SaveState(Dictionary<String, Object> pageState)
        {

            pageState["p1"] = p1.Text;
            pageState["n1"] = n1.Text;

            pageState["p2"] = p2.Text;
            pageState["n2"] = n2.Text;

            pageState["p3"] = p3.Text;
            pageState["n3"] = n3.Text;

            pageState["p4"] = p4.Text;
            pageState["n4"] = n4.Text;

            pageState["p5"] = p5.Text;
            pageState["n5"] = n5.Text;
        }

        private void button1_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(SettingsPage));
        }

        private void button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(DBViewPage));
        }

        private void button_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			RecBoardStop.Stop();
			RecBoard.Begin();
        }

        private void button_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			RecBoard.Stop();
			RecBoardStop.Begin();
        }

        private void button1_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			SetBoardStop.Stop();
			SetBoard.Begin();
        }

        private void button1_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			SetBoard.Stop();
			SetBoardStop.Begin();
        }

    }
}
