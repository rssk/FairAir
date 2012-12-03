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
using Windows.UI.Popups;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace Fair_Air_Abq
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class SettingsPage : Fair_Air_Abq.Common.LayoutAwarePage
    {
        public SettingsPage()
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Assign a bindable collection of items to this.DefaultViewModel["Items"]
        }

        private async void Button_Tapped_1(object sender, TappedRoutedEventArgs e)
        {

            bool cleared = false;
            MessageDialog mBox = new MessageDialog("Selecting yes will clear all the air quality data stored by the app.", "Clear User Data");
            var cmd = new UICommandInvokedHandler((command) =>
            {
                var temp = command.Label;
            });

            mBox.Commands.Add(new UICommand(
                      "Yes", cmd));
            mBox.Commands.Add(new UICommand(
                                  "No", cmd));

            var returnCmd = await mBox.ShowAsync();

            if (String.Equals(returnCmd.Label, "Yes"))
                cleared = await airDB.clear();
            else
                return;

            if (cleared)
            {
                MessageDialog mBox2 = new MessageDialog("User Data Cleared", "User Data");
                mBox2.ShowAsync();
            }
            else
            {
                MessageDialog mBox2 = new MessageDialog("Failed to Clear User Data (there may be none to clear)", "User Data");
                mBox2.ShowAsync();
            }
        }

        private void Button_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            MessageDialog mBox2 = new MessageDialog("This app uses the pollutant data that is recorded by the EPA air stations in Albuquerque,\nand made available online by Cabq.gov, specifically at http://data.cabq.gov/airquality/ \nThis data is then used to calculate index values using the Air Quality index equations put out by the EPA, found here: http://www.epa.gov/airnow/aqi_tech_assistance.pdf \n" +
            "Those are the values you see on the front page, and are intended to be a supplement to, but not replace, the AQI values put out by the EPA", "Fair Air Abq");
            mBox2.ShowAsync();
        }
    }
}
