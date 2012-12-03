using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using Windows.ApplicationModel;
using Windows.Storage;
using System.Net;

namespace Fair_Air_Abq
{
    class airDB
    {
        public int count { get; set; }
        public string date { get; set; }
        public string ozone { get; set; }
        public string ozoneAQI { get; set; }
        public string pm10 { get; set; }
        public string pm10AQI { get; set; }
        public string pm25 { get; set; }
        public string pm25AQI { get; set; }
        public string no2 { get; set; }
        public string no2AQI { get; set; }
        public string so2 { get; set; }
        public string so2AQI { get; set; }

        private static string filename = "airDB.xml";

        public static async Task<XDocument> getAQI()
        {
            XDocument xd = null;

            string url = "http://data.cabq.gov/airquality/aqindex/daily/";
            string day = Convert.ToString(System.DateTime.Now.Day);
            string year = Convert.ToString(System.DateTime.Now.Year);
            string month = Convert.ToString(System.DateTime.Now.Month);
            string hour = Convert.ToString(System.DateTime.Now.Hour);
            year = year.Remove(0, 2);

            string urlEnd = year + month + day;
            //tx1.Text = hour;
            string result = null;
            bool except = false;
            string furl = null;
            string date = null;
            //gets data with furl as the target url
            for (int i = System.DateTime.Now.Hour; i >= 0; i--)
            {
                if (i < 10)
                {
                    furl = url + urlEnd + "0" + Convert.ToString(i) + ".NM2";
                    date = urlEnd;
                }
                else
                {
                    furl = url + urlEnd + Convert.ToString(i) + ".NM2";
                    date = urlEnd;
                }
                try
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(furl);
                    req.Method = "GET";
                    WebResponse res = await req.GetResponseAsync();
                    StreamReader sr = new StreamReader(res.GetResponseStream(), System.Text.Encoding.UTF8);
                    result = sr.ReadToEnd();
                    sr.Dispose();
                    res.Dispose();
                }
                catch (Exception ex)
                {
                    // handle error
                    except = true;
                }
                if (except == false)
                    break;
            }
            if (except == true)// nothing was recieved, return old file or create one if it doesnt exist
            {
                StorageFolder sfProject = Package.Current.InstalledLocation;
                StorageFolder sfLocal = ApplicationData.Current.LocalFolder;
                Stream sr, sw;

                bool bFileExists;

                try
                {
                    sw = await sfLocal.OpenStreamForWriteAsync(filename, CreationCollisionOption.FailIfExists);
                    sw.Dispose();
                    bFileExists = false;

                }
                catch
                {
                    bFileExists = true;
                }
                if (bFileExists == false)
                {
                    sr = await sfProject.OpenStreamForReadAsync(filename);
                    sw = await sfLocal.OpenStreamForWriteAsync(filename, CreationCollisionOption.ReplaceExisting);
                    await sr.CopyToAsync(sw);
                    sr.Dispose();
                    sw.Dispose();
                }

                sr = await sfLocal.OpenStreamForReadAsync(filename);
                xd = XDocument.Load(sr);
                sr.Dispose();
                return xd;
            }
            string[] param = result.Split('\n');

            int ind = 0;
            int ine = 0;
            List<string[]> data = new List<string[]>();
            while (ind != -1)
            {
                ind = Array.IndexOf(param, "BEGIN_GROUP");
                ine = Array.IndexOf(param, "END_GROUP");
                data.Add(param.Skip(ind).Take(ine - ind + 1).ToArray());
                param = param.Skip(ine + 1).Take(param.Length).ToArray();
            }
            data.RemoveAt(data.Count - 1);
            string[] final = new string[19];
            int offset = 0;

            for (int i = 0; i < data.Count; i++)// goes through all types, ozone, co2,ect..
            {
                string[] tempArray = data.ElementAt(i);
                string type = tempArray[1].Remove(0, 9);
                string[] tempVal = new string[50];
                string[] tempVal2 = new string[50];

                int count = 0;
                double totalSum = 0;
                for (int h = 14; h < tempArray.Length; h = h + 2)//goes through all locations, odd check ignores good/bad location values
                {
                    if (tempArray[h].Equals("END_DATA"))
                        break;

                    tempVal = tempArray[h].Split(',').Skip(2).ToArray();
                    tempVal2 = tempArray[h + 1].Split(',').Skip(2).ToArray();

                    double sum = 0;
                    double cur;
                    int goodCount = 0;
                    for (int f = 0; f < tempVal.Length; f++)
                    {
                        if (tempVal2[f].Trim().Equals("G"))
                        {
                            if (double.TryParse(tempVal[f], out cur))
                            {
                                sum = sum + cur;
                                goodCount++;
                            }
                        }
                    }
                    totalSum = totalSum + (sum / goodCount);
                    count++;


                }
                totalSum = totalSum / count;
                final[i + offset] = type;
                final[i + 1 + offset] = Convert.ToString(totalSum);
                final[i + 2 + offset] = null;
                offset = offset + 2;

            }
            final[18] = date;
            xd = await update(final);
            return xd;
        }

        private static double calc(double AQIhigh, double AQIlow, double Conchigh, double Conclow, double Conc)
        {
            double linear;
            double a;
            a = ((Conc - Conclow) / (Conchigh - Conclow)) * (AQIhigh - AQIlow) + AQIlow;
            linear = Math.Round(a);
            return linear;
        }

        /*Calculates the appropriate AQI(air quality index) values for the respective values
         * 
         */
        private static async Task<XDocument> update(string[] values)
        {


            //ozone function
            double c;
            c = (Math.Floor(Convert.ToDouble(values[1]))) / 1000;
            if (c >= 0 && c < .060)
            {
                values[2] = Convert.ToString(calc(50, 0, 0.059, 0, c));
            }
            else if (c >= .060 && c < .076)
            {
                values[2] = Convert.ToString(calc(100, 51, .075, .060, c));
            }
            else if (c >= .076 && c < .096)
            {
                values[2] = Convert.ToString(calc(150, 101, .095, .076, c));
            }
            else if (c >= .096 && c < .116)
            {
                values[2] = Convert.ToString(calc(200, 151, .115, .096, c));
            }
            else if (c >= .116 && c < .375)
            {
                values[2] = Convert.ToString(calc(300, 201, .374, .116, c));
            }
            else
            {
                values[2] = "N/A";
            }


            //Particulate matter 10 microns
            c = Math.Floor(Convert.ToDouble(values[4]));
            if (c >= 0 && c < 55)
            {
                values[5] = Convert.ToString(calc(50, 0, 54, 0, c));
            }
            else if (c >= 55 && c < 155)
            {
                values[5] = Convert.ToString(calc(100, 51, 154, 55, c));
            }
            else if (c >= 155 && c < 255)
            {
                values[5] = Convert.ToString(calc(150, 101, 254, 155, c));
            }
            else if (c >= 255 && c < 355)
            {
                values[5] = Convert.ToString(calc(200, 151, 354, 255, c));
            }
            else if (c >= 355 && c < 425)
            {
                values[5] = Convert.ToString(calc(300, 201, 424, 355, c));
            }
            else if (c >= 425 && c < 505)
            {
                values[5] = Convert.ToString(calc(400, 301, 504, 425, c));
            }
            else if (c >= 505 && c < 605)
            {
                values[5] = Convert.ToString(calc(500, 401, 604, 505, c));
            }
            else
            {
                values[5] = "N/A";
            }

            //Particulate matter 2.5 microns
            c = (Math.Floor(10 * Convert.ToDouble(values[7]))) / 10;
            if (c >= 0 && c < 15.5)
            {
                values[8] = Convert.ToString(calc(50, 0, 15.4, 0, c));
            }
            else if (c >= 15.5 && c < 35.5)
            {
                values[8] = Convert.ToString(calc(100, 51, 35.4, 15.5, c));
            }
            else if (c >= 35.5 && c < 65.5)
            {
                values[8] = Convert.ToString(calc(150, 101, 65.4, 35.5, c));
            }
            else if (c >= 65.5 && c < 150.5)
            {
                values[8] = Convert.ToString(calc(200, 151, 150.4, 65.5, c));
            }
            else if (c >= 150.5 && c < 250.5)
            {
                values[8] = Convert.ToString(calc(300, 201, 250.4, 150.5, c));
            }
            else if (c >= 250.5 && c < 350.5)
            {
                values[8] = Convert.ToString(calc(400, 301, 350.4, 250.5, c));
            }
            else if (c >= 350.5 && c < 500.5)
            {
                values[8] = Convert.ToString(calc(500, 401, 500.4, 350.5, c));
            }
            else
            {
                values[8] = "N/A";
            }

            //NO2 calcuation
            c = (Math.Floor(Convert.ToDouble(values[13]))) / 1000;
            if (c >= 0 && c < .054)
            {
                values[14] = Convert.ToString(calc(50, 0, .053, 0, c));
            }
            else if (c >= .054 && c < .101)
            {
                values[14] = Convert.ToString(calc(100, 51, .100, .054, c));
            }
            else if (c >= .101 && c < .361)
            {
                values[14] = Convert.ToString(calc(150, 101, .360, .101, c));
            }
            else if (c >= .361 && c < .650)
            {
                values[14] = Convert.ToString(calc(200, 151, .649, .361, c));
            }
            else if (c >= .650 && c < 1.250)
            {
                values[14] = Convert.ToString(calc(300, 201, 1.249, .650, c));
            }
            else if (c >= 1.250 && c < 1.650)
            {
                values[14] = Convert.ToString(calc(400, 301, 1.649, 1.250, c));
            }
            else if (c >= 1.650 && c <= 2.049)
            {
                values[14] = Convert.ToString(calc(500, 401, 2.049, 1.650, c));
            }
            else
            {
                values[14] = "N/A";
            }

            //SO2 calculation
            c = Math.Floor(Convert.ToDouble(values[16]));
            if (c >= 0 && c < 36)
            {
                values[17] = Convert.ToString(calc(50, 0, 35, 0, c));
            }
            else if (c >= 36 && c < 76)
            {
                values[17] = Convert.ToString(calc(100, 51, 75, 36, c));
            }
            else if (c >= 76 && c < 186)
            {
                values[17] = Convert.ToString(calc(150, 101, 185, 76, c));
            }
            else if (c >= 186 && c <= 304)
            {
                values[17] = Convert.ToString(calc(200, 151, 304, 186, c));
            }
            else
            {
                values[17] = "N/A";
            }

            //Write out to xml
            StorageFolder sfProject = Package.Current.InstalledLocation;
            StorageFolder sfLocal = ApplicationData.Current.LocalFolder;
            Stream sr, sw;
            bool bFileExists;

            try
            {
                sw = await sfLocal.OpenStreamForWriteAsync(filename, CreationCollisionOption.FailIfExists);
                sw.Dispose();
                bFileExists = false;

            }
            catch
            {
                bFileExists = true;
            }
            if (bFileExists == false)
            {
                sr = await sfProject.OpenStreamForReadAsync(filename);
                sw = await sfLocal.OpenStreamForWriteAsync(filename, CreationCollisionOption.ReplaceExisting);
                await sr.CopyToAsync(sw);
                sr.Dispose();
                sw.Dispose();
            }

            sr = await sfLocal.OpenStreamForReadAsync(filename);
            XDocument xd = XDocument.Load(sr);
            sr.Dispose();
            string num = xd.Root.Element("current").Value;
            string elem = null;

            //Same data check, just overwrites with updated values
            if (!String.Equals(xd.Root.Element("data").Element("d" + num).Element("date").Value, values[18]))
            {
                xd.Root.Element("current").Value = num;

                int numI = Convert.ToInt32(num);
                if (numI == 0)
                {
                    num = "1";
                    elem = "d" + num;
                }
                else if (numI < 30)
                {
                    num = Convert.ToString(1 + numI);
                    elem = "d" + num;
                }
                else if (numI == 30)
                {
                    elem = "d1";
                    num = "0";
                }
            }
            else
            {
                elem = "d" + num;
            }

            XElement mod = xd.Root.Element("data").Element(elem);

            mod.Element("date").Value = values[18];
            mod.Element("ozone").Value = values[1];
            mod.Element("ozoneAQI").Value = values[2];
            mod.Element("pm10").Value = values[4];
            mod.Element("pm10AQI").Value = values[5];
            mod.Element("pm25").Value = values[7];
            mod.Element("pm25AQI").Value = values[8];
            mod.Element("no2").Value = values[13];
            mod.Element("noAQI").Value = values[14];
            mod.Element("so2").Value = values[16];
            mod.Element("so2AQI").Value = values[17];


            Stream sr2 = await sfLocal.OpenStreamForWriteAsync(filename, CreationCollisionOption.ReplaceExisting);
            xd.Save(sr2);
            sr2.Dispose();

            return xd;
        }

        public static async Task<bool> clear()
        {
            StorageFolder sfProject = Package.Current.InstalledLocation;
            StorageFolder sfLocal = ApplicationData.Current.LocalFolder;
            Stream sr, sw;
            bool replaced = true;

            try
            {
                sr = await sfProject.OpenStreamForReadAsync(filename);
                sw = await sfLocal.OpenStreamForWriteAsync(filename, CreationCollisionOption.ReplaceExisting);
                await sr.CopyToAsync(sw);
                sr.Dispose();
                sw.Dispose();
            }
            catch (Exception e)
            {
                replaced = false;
            }

            return replaced;

        }

    }
}
