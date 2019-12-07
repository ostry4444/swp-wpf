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
        string[] addons = new string[] { "ananas", "ser", "pieczarki", "ananasem", "serem", "pieczarkami" };  // / z ...

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
                    oSize = oThick = oAdd = oAdd2 = oAdd3 ="";
                    this.Dispatcher.BeginInvoke(new Action(() => {
                        tb1.Text = "";
                        tb2.Text = "";
                        tb3.Text = "";;
                    }));
                    Console.WriteLine("zamówiono ");
                }
                else if ((txt.IndexOf("Poproszę") >= 0) && speechOn == true) 
                {
                    try
                    {
                        oSize = (String)e.Result.Semantics["size"].Value;
                    } catch (Exception ex){ }

                    try { 
                        oThick = (String) e.Result.Semantics["thickness"].Value;
                    }
                    catch (Exception ex) { }

                    try {
                        oAdd = (String) e.Result.Semantics["addons"]?.Value; 
                        oAdd2 = (String) e.Result.Semantics["addons2"].Value;
                        oAdd3 = (String) e.Result.Semantics["addons3"].Value;
                    }
                    catch (Exception ex) { }
                    /*
                    if (oThick.Length == 0){ //ask 
                        pTTS.SpeakAsync("jaka grubość?");
                        
                    }*/

                    this.Dispatcher.BeginInvoke(new Action(() => {
                        tb1.Text = oSize;
                        tb2.Text = oThick;
                        tb3.Text = oAdd;
                        // Ulgowy.Visibility = Visibility.Hidden;
                        // Normalny.Visibility = Visibility.Visible;
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
            else {
                comments = String.Format("\tNISKI WSPÓŁCZYNNIK WIARYGODNOŚCI - powtórz polecenie");
                Console.WriteLine(comments);
                pTTS.SpeakAsync("Proszę powtórzyć");
            }

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            pTTS.SpeakAsync("baton");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //pTTS.SpeakAsync("do usłyszenia");
            System.Windows.Application.Current.Shutdown();
        }
    }
}
