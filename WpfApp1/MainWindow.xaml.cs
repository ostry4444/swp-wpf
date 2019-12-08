using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Globalization;
using System.ComponentModel;
using System.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    /// 
   

    public partial class MainWindow : Window
    {

        static bool speechOn = true;
        static SpeechSynthesizer pTTS = new SpeechSynthesizer();
        static SpeechRecognitionEngine pSRE;

        string[] sizes = new string[] { "mała", "średnia", "duża", "małą", "średnią", "dużą" }; // pizza / pizzę
        string[] thickness = new string[] { "cienkim", "średnim", "grubym", "cienkie", "średnie", "grube" }; // cieście / ciasto
        string[] addons = new string[] { "ananas", "ser", "pieczarki", "kurczak", "cebula", 
                                         "ananasem", "serem", "pieczarkami", "kurczakiem","cebulą" };

        String oSize = "";
        String oThick = "";
        String oAdd = "";
        String oAdd2 = "";
        String oAdd3 = "";

        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            //InitializeBackgroundWorker();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            //backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerAsync();

            initButtons();
        }
        void initButtons()
        {
            a1.Content = addons[0];
            a2.Content = addons[1];
            a3.Content = addons[2];
            a4.Content = addons[3];
            a5.Content = addons[4];

            a11.Content = "2x " + addons[0];
            a22.Content = "2x " + addons[1];
            a33.Content = "2x " + addons[2];
            a44.Content = "2x " + addons[3];
            a55.Content = "2x " + addons[4];
        }

        
        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                pTTS.SetOutputToDefaultAudioDevice();
                pTTS.Speak("Witam w pizzerii");
                // Ustawienie języka rozpoznawania:
                CultureInfo ci = new CultureInfo("pl-PL");
                // Utworzenie "silnika" rozpoznawania:
                pSRE = new SpeechRecognitionEngine(ci);
                // Ustawienie domyślnego urządzenia wejściowego:
                pSRE.SetInputToDefaultAudioDevice();
                // Przypisanie obsługi zdarzenia realizowanego po rozpoznaniu wypowiedzi zgodnej z gramatyką:
                pSRE.SpeechRecognized += PSRE_SpeechRecognized; // TAB   PSRE_SpeechRecognized;
                // -------------------------------------------------------------------------
                // Budowa gramatyki numer 1 - POLECENIA SYSTEMOWE
                // Budowa gramatyki numer 1 - określenie komend:
                Choices stopChoice = new Choices();
                stopChoice.Add("Stop");
                stopChoice.Add("Pomoc");
                // Budowa gramatyki numer 1 - definiowanie składni gramatyki:
                GrammarBuilder buildGrammarSystem = new GrammarBuilder();
                buildGrammarSystem.Append(stopChoice);
                // Budowa gramatyki numer 1 - utworzenie gramatyki:
                Grammar grammarSystem = new Grammar(buildGrammarSystem); //
                                                                         // -------------------------------------------------------------------------
                                                                         // Budowa gramatyki numer 2 - POLECENIA DLA PROGRAMU
                                                                         // Budowa gramatyki numer 2 - określenie komend:
                
                // poproszę / chce zamówić /.. pizzę
                // rozmiar (mała/ duża /średnia)
                // na (cienkim/ średnim/ grubym) cieście
                // * dodatki ...

                Choices chSizes = new Choices();
                chSizes.Add(sizes);

                Choices chThickness = new Choices();
                chThickness.Add(thickness);

                Choices chAddons = new Choices();
                chAddons.Add(addons);


                GrammarBuilder grammarPizza = new GrammarBuilder();
                grammarPizza.Append("Poproszę");
               // grammarPizza.Append("Poproszę", 0,1);
                grammarPizza.Append(new SemanticResultKey("sizes", chSizes), 0, 1);
                grammarPizza.Append("pizzę", 0, 1);

                grammarPizza.Append("na", 0, 1);
                grammarPizza.Append(new SemanticResultKey ("thickness", chThickness), 0, 1);
                grammarPizza.Append("cieście", 0, 1);

                grammarPizza.Append("z", 0, 1);

                grammarPizza.Append(new SemanticResultKey("addons", chAddons), 0, 1);
                grammarPizza.Append("i", 0, 1);
                grammarPizza.Append(new SemanticResultKey("addons2", chAddons), 0, 3);
                grammarPizza.Append("i", 0, 1);
                grammarPizza.Append(new SemanticResultKey("addons3", chAddons), 0, 3);


                Grammar g_Pizza = new Grammar(grammarPizza); 

                pSRE.LoadGrammarAsync(g_Pizza);



                pSRE.LoadGrammarAsync(grammarSystem);
                // Ustaw rozpoznawanie przy wykorzystaniu wielu gramatyk:
                pSRE.RecognizeAsync(RecognizeMode.Multiple);
                // -------------------------------------------------------------------------
                Console.WriteLine("\nAby zakonczyć działanie programu powiedz 'dziękuję'\n");
                while (speechOn == true) {; } //pętla w celu uniknięcia zamknięcia programu
                Console.WriteLine("\tWCIŚNIJ <ENTER> aby wyjść z programu\n");
                Console.ReadLine();


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        private void PSRE_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string txt = e.Result.Text;
            string comments;
            float confidence = e.Result.Confidence;
            comments = String.Format("ROZPOZNANO (wiarygodność: {0:0.000}): '{1}'",
            e.Result.Confidence, txt);
            Console.WriteLine(comments);
            if (confidence > 0.20)
            {
                if (txt.IndexOf("Stop") >= 0)
                {
                    speechOn = false;
                }
                else if (txt.IndexOf("Pomoc") >= 0)
                {
                    pTTS.SpeakAsync("Składnia polecenia: ...");

                }
                else if ((txt.IndexOf("Dziękuję") >= 0) && speechOn == true)
                {
                    oSize = oThick = oAdd = oAdd2 = oAdd3 = "";
                    this.Dispatcher.BeginInvoke(new Action(() => {
                       // TODO: click() buttons according to order
                    }));
                    Console.WriteLine("zamówiono ");
                    //confirm_Click(null,null);
                }
                else if ((txt.IndexOf("Poproszę") >= 0) && speechOn == true)
                {
                    try
                    {
                        oSize = (String)e.Result.Semantics["size"].Value;
                    }
                    catch (Exception ex) { }

                    try
                    {
                        oThick = (String)e.Result.Semantics["thickness"].Value;
                    }
                    catch (Exception ex) { }

                    try
                    {
                        oAdd = (String)e.Result.Semantics["addons"]?.Value;
                        oAdd2 = (String)e.Result.Semantics["addons2"].Value;
                        oAdd3 = (String)e.Result.Semantics["addons3"].Value;
                    }
                    catch (Exception ex) { }
                    /*
                    if (oThick.Length == 0){ //ask 
                        pTTS.SpeakAsync("jaka grubość?");
                        
                    }*/

                    this.Dispatcher.BeginInvoke(new Action(() => {
                        
                        // TODO: click() buttons according to order
                    }));


                    Console.WriteLine("zamówiono " + oSize + " pizze " + oThick + " ciasto " + oAdd + " i " + oAdd2);
                    pTTS.SpeakAsync("zamówiono " + oSize + " pizze " + oThick + " ciasto " + oAdd + " i " + oAdd2);
                }
                else if (true) // ask
                {/*
                    if (oSize == "")
                    {
                        oSize = e.Result.Semantics["addons"]?.Value;
                    }*/
                }


            }
            else
            {
                comments = String.Format("\tNISKI WSPÓŁCZYNNIK WIARYGODNOŚCI - powtórz polecenie");
                Console.WriteLine(comments);
                pTTS.SpeakAsync("Proszę powtórzyć");
            }

        }


        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            pTTS.SpeakAsync("confirm");
            //order confirmation
        }

        private void newOrder_Click(object sender, RoutedEventArgs e)
        {
            pTTS.SpeakAsync("newOrder");
            oSize = oThick = oAdd = oAdd2 = oAdd3= "";

            s1.IsEnabled = s2.IsEnabled = s3.IsEnabled = true;
            t1.IsEnabled = t2.IsEnabled = t3.IsEnabled = true;
            a1.IsEnabled = a2.IsEnabled = a3.IsEnabled = a4.IsEnabled = a5.IsEnabled = true;
            a11.IsEnabled = a22.IsEnabled = a33.IsEnabled = a44.IsEnabled = a55.IsEnabled = true;
            
            s1.Background = s2.Background = s3.Background = Brushes.LightGray;
            t1.Background = t2.Background = t3.Background = Brushes.LightGray;
            a1.Background = a2.Background = a3.Background = a4.Background = a5.Background = Brushes.LightGray;
            a11.Background = a22.Background = a33.Background = a44.Background = a55.Background = Brushes.LightGray;
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void s1_Click(object sender, RoutedEventArgs e)
        {
            s1.Background = Brushes.LightGreen;
            s2.IsEnabled = false;
            s3.IsEnabled = false;
        }

        private void s2_Click(object sender, RoutedEventArgs e)
        {
            s1.IsEnabled = false;
            s2.Background = Brushes.LightGreen; 
            s3.IsEnabled = false;
        }

        private void s3_Click(object sender, RoutedEventArgs e)
        {
            s1.IsEnabled = false;
            s2.IsEnabled = false;
            s3.Background = Brushes.LightGreen;
        }

        private void t1_Click(object sender, RoutedEventArgs e)
        {
            t1.Background = Brushes.LightGreen;
            t2.IsEnabled = false;
            t3.IsEnabled = false;
        }

        private void t2_Click(object sender, RoutedEventArgs e)
        {
            t1.IsEnabled = false;
            t2.Background = Brushes.LightGreen; 
            t3.IsEnabled = false;
        }

        private void t3_Click(object sender, RoutedEventArgs e)
        {
            t1.IsEnabled = false;
            t2.IsEnabled = false;
            t3.Background = Brushes.LightGreen;
        }

        private void a1_Click(object sender, RoutedEventArgs e)
        {
            a1.Background = Brushes.LightGreen;
            a11.IsEnabled = false;
        }
        private void a11_Click(object sender, RoutedEventArgs e)
        {
            a11.Background = Brushes.LightGreen;
            a1.IsEnabled = false;
        }

        private void a2_Click(object sender, RoutedEventArgs e)
        {
            a2.Background = Brushes.LightGreen;
            a22.IsEnabled = false;
        }
        private void a22_Click(object sender, RoutedEventArgs e)
        {
            a22.Background = Brushes.LightGreen;
            a2.IsEnabled = false;
        }

        private void a3_Click(object sender, RoutedEventArgs e)
        {
            a3.Background = Brushes.LightGreen;
            a33.IsEnabled = false;
        }
        private void a33_Click(object sender, RoutedEventArgs e)
        {
            a33.Background = Brushes.LightGreen;
            a3.IsEnabled = false;
        }

        private void a4_Click(object sender, RoutedEventArgs e)
        {
            a4.Background = Brushes.LightGreen;
            a44.IsEnabled = false;
        }
        private void a44_Click(object sender, RoutedEventArgs e)
        {
            a44.Background = Brushes.LightGreen;
            a4.IsEnabled = false;
        }

        private void a5_Click(object sender, RoutedEventArgs e)
        {
            a5.Background = Brushes.LightGreen;
            a55.IsEnabled = false;
        }
        private void a55_Click(object sender, RoutedEventArgs e)
        {
            a55.Background = Brushes.LightGreen;
            a5.IsEnabled = false;
        }

    }
}
