﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel.Web;
using System.Web;
using Newtonsoft.Json;

namespace SproutStudyGuide
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WpfCsSample.CodeSampleControls.TrackingHandler.HorizontalWindow horizontalWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //horizontalWindow = new WpfCsSample.CodeSampleControls.TrackingHandler.HorizontalWindow();
            //horizontalWindow.Show();
        }

        void Window_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Handled = true;
        }

        void Window_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {

            // Get the Rectangle and its RenderTransform matrix.
            Image rectToMove = e.OriginalSource as Image;
            Matrix rectsMatrix = ((MatrixTransform)rectToMove.RenderTransform).Matrix;

            // Rotate the Rectangle.
            rectsMatrix.RotateAt(e.DeltaManipulation.Rotation,
                                 e.ManipulationOrigin.X,
                                 e.ManipulationOrigin.Y);

            // Resize the Rectangle.  Keep it square 
            // so use only the X value of Scale.
            rectsMatrix.ScaleAt(e.DeltaManipulation.Scale.X,
                                e.DeltaManipulation.Scale.X,
                                e.ManipulationOrigin.X,
                                e.ManipulationOrigin.Y);

            // Move the Rectangle.
            rectsMatrix.Translate(e.DeltaManipulation.Translation.X,
                                  e.DeltaManipulation.Translation.Y);

            // Apply the changes to the Rectangle.
            rectToMove.RenderTransform = new MatrixTransform(rectsMatrix);

            Rect containingRect =
                new Rect(((FrameworkElement)e.ManipulationContainer).RenderSize);

            Rect shapeBounds =
                rectToMove.RenderTransform.TransformBounds(
                    new Rect(rectToMove.RenderSize));

            // Check if the rectangle is completely in the window.
            // If it is not and intertia is occuring, stop the manipulation.
            if (e.IsInertial && !containingRect.Contains(shapeBounds))
            {
                e.Complete();
            }


            e.Handled = true;
        }

        void Window_InertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {

            // Decrease the velocity of the Rectangle's movement by 
            // 10 inches per second every second.
            // (10 inches * 96 pixels per inch / 1000ms^2)
            e.TranslationBehavior.DesiredDeceleration = 10.0 * 96.0 / (1000.0 * 1000.0);

            // Decrease the velocity of the Rectangle's resizing by 
            // 0.1 inches per second every second.
            // (0.1 inches * 96 pixels per inch / (1000ms^2)
            e.ExpansionBehavior.DesiredDeceleration = 0.1 * 96 / (1000.0 * 1000.0);

            // Decrease the velocity of the Rectangle's rotation rate by 
            // 2 rotations per second every second.
            // (2 * 360 degrees / (1000ms^2)
            e.RotationBehavior.DesiredDeceleration = 720 / (1000.0 * 1000.0);

            e.Handled = true;
        }

        private void PerformTrackingCapture_Click(object sender, RoutedEventArgs e)
        {
            WpfCsSample.CodeSampleControls.TrackingHandler.TrackingHandlerMatControl thmControl = new WpfCsSample.CodeSampleControls.TrackingHandler.TrackingHandlerMatControl();
            thmControl.mainWindow = this;
            thmControl.PerformCapture_Click(sender, e);
        }

        private void PerformOCRCapture_Click(object sender, RoutedEventArgs e)
        {
            WpfCsSample.CodeSampleControls.OCR.OCRMatControl ocrControl = new WpfCsSample.CodeSampleControls.OCR.OCRMatControl();
            ocrControl.DebugTextBox = this.DebugTextBox;
            ocrControl.mainWindow = this;
            ocrControl.PerformCapture_Click(sender, e);
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            var response = HttpGet(
                "https://api.idolondemand.com/1/api/sync/querytextindex/v1?text=Dog&database_match=wiki_eng&apikey=7353dd9f-f651-4025-8713-572be5771178");

            JsonTextReader reader = new JsonTextReader(new StringReader(response));
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    var tempStr = String.Format("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
                    Console.WriteLine(tempStr);
                    //DebugTextBox.Text += tempStr + '\n';
                }
                else
                {
                    var tempStr = String.Format("Token: {0}", reader.TokenType);
                    Console.WriteLine(tempStr);
                    //DebugTextBox.Text += tempStr + '\n';
                }
            }
            DebugTextBox.Text = response;
        }

        static string HttpGet(string url)
        {
            HttpWebRequest req = WebRequest.Create(url)
                                 as HttpWebRequest;
            string result = null;
            using (HttpWebResponse resp = req.GetResponse()
                                          as HttpWebResponse)
            {
                StreamReader reader =
                    new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
}
