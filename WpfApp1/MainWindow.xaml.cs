using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Speech.Recognition;
using Microsoft.Speech.Synthesis;
using System.Globalization;
using System.ComponentModel;

namespace WpfApp1
{
  
    public partial class MainWindow : Window
    {

        static bool speechOn = true;
        static SpeechSynthesizer pTTS = new SpeechSynthesizer();
        static SpeechRecognitionEngine pSRE;

        int sizesN = 3;
        int thicknessN = 3;
        int addonsN = 5;
        string[] sizes = new string[] { "mała", "średnia", "duża", "małą", "średnią", "dużą" }; // pizza / pizzę
        string[] thickness = new string[] { "cienkie", "średnie", "grube", "cienkim", "średnim", "grubym" }; // cieście / ciasto
        string[] doubbler = new string[] { "podwójnym", "podwójnymi", "podwójną", "dwa razy" };
        string[] addons = new string[] { "ananas", "ser", "pieczarki", "kurczak", "cebula", 
                                         "ananasem", "serem", "pieczarkami", "kurczakiem","cebulą" };

        String oSize = "";
        String oThick = "";
        bool dA1, dA2, dA3, dA4, dA5 = false;
        String oAdd1 = "";
        String oAdd2 = "";
        String oAdd3 = "";
        String oAdd4 = "";
        String oAdd5 = "";

        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.RunWorkerAsync();

            initButtons();
        }
        void initButtons()
        {
            s1.Content = sizes[0];
            s2.Content = sizes[1];
            s3.Content = sizes[2];

            t1.Content = thickness[0];
            t2.Content = thickness[1];
            t3.Content = thickness[2];

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
                CultureInfo ci = new CultureInfo("pl-PL");
                pSRE = new SpeechRecognitionEngine(ci);
                pSRE.SetInputToDefaultAudioDevice();
                // Przypisanie obsługi zdarzenia realizowanego po rozpoznaniu wypowiedzi zgodnej z gramatyką:
                pSRE.SpeechRecognized += PSRE_SpeechRecognized; // TAB   PSRE_SpeechRecognized;
                // -------------------------------------------------------------------------
                // Budowa gramatyki numer 1 - POLECENIA SYSTEMOWE
                // Budowa gramatyki numer 1 - określenie komend:
                Choices stopChoice = new Choices();
                stopChoice.Add("Stop");
                stopChoice.Add("Pomoc");
                stopChoice.Add("Dziękuję");
                // Budowa gramatyki numer 1 - definiowanie składni gramatyki:
                GrammarBuilder buildGrammarSystem = new GrammarBuilder();
                buildGrammarSystem.Append(stopChoice);
                // Budowa gramatyki numer 1 - utworzenie gramatyki:
                Grammar grammarSystem = new Grammar(buildGrammarSystem);

               
                Choices chSizes = new Choices();
                    chSizes.Add(sizes);
                Choices chThickness = new Choices();
                    chThickness.Add(thickness);
                Choices chDouble = new Choices();
                    chDouble.Add(doubbler);
                Choices chAddons = new Choices();
                    chAddons.Add(addons);


                GrammarBuilder grammarPizza = new GrammarBuilder();
                //grammarPizza.Append("Poproszę");
                grammarPizza.Append("Poproszę", 0, 1);
                grammarPizza.Append(new SemanticResultKey("size", chSizes), 0, 1);
                grammarPizza.Append("pizzę", 0, 1);

                grammarPizza.Append("na", 0, 1);
                grammarPizza.Append(new SemanticResultKey ("thickness", chThickness), 0, 1);
                grammarPizza.Append("cieście", 0, 1);

                grammarPizza.Append("z", 0, 1);

                grammarPizza.Append(new SemanticResultKey("dA1", chDouble), 0, 1);
                grammarPizza.Append(new SemanticResultKey("add1", chAddons), 0, 1);
                grammarPizza.Append("i", 0, 1);
                    grammarPizza.Append(new SemanticResultKey("dA2", chDouble), 0, 1);
                    grammarPizza.Append(new SemanticResultKey("add2", chAddons), 0, 1);
                grammarPizza.Append("i", 0, 1);
                    grammarPizza.Append(new SemanticResultKey("dA3", chDouble), 0, 1);
                    grammarPizza.Append(new SemanticResultKey("add3", chAddons), 0, 1);
                grammarPizza.Append("i", 0, 1);
                    grammarPizza.Append(new SemanticResultKey("dA4", chDouble), 0, 1);
                    grammarPizza.Append(new SemanticResultKey("add4", chAddons), 0, 1);
                grammarPizza.Append("i", 0, 1);
                    grammarPizza.Append(new SemanticResultKey("dA5", chDouble), 0, 1);
                    grammarPizza.Append(new SemanticResultKey("add5", chAddons), 0, 1);


                Grammar g_Pizza = new Grammar(grammarPizza); 

                pSRE.LoadGrammarAsync(g_Pizza);
                               
                pSRE.LoadGrammarAsync(grammarSystem);
                // Ustaw rozpoznawanie przy wykorzystaniu wielu gramatyk:
                pSRE.RecognizeAsync(RecognizeMode.Multiple);
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
            if (confidence > 0.30)
            {
                if (txt.IndexOf("Stop") >= 0) {
                    speechOn = false;
                }
                else if (txt.IndexOf("Pomoc") >= 0) {
                    pTTS.SpeakAsync("Składnia polecenia: Poproszę... rozmiar pizzy, grubość ciasta, dodatki");
                }
                else if ((txt.IndexOf("Dziękuję") >= 0) && speechOn == true)
                {
                    Console.WriteLine("oSize: " + oSize + " oThick: " + oThick + " Add: " + (dA1 ? " 2x" : " ") + oAdd1 + (dA2 ? " 2x" : " ") + oAdd2 +
                                        (dA3 ? " 2x" : " ") + oAdd3 + (dA4 ? " 2x" : " ") + oAdd4 + (dA5 ? " 2x" : " ") + oAdd5); 
                    pTTS.SpeakAsync("zamówiono " + oSize + " pizze " + oThick + " ciasto z " + (dA1 ? " 2x" : " ") + oAdd1 + (dA2 ? " 2x" : " ") + oAdd2 +
                                        (dA3 ? " 2x" : " ") + oAdd3 + (dA4 ? " 2x" : " ") + oAdd4 + (dA5 ? " 2x" : " ") + oAdd5);

                    this.Dispatcher.BeginInvoke(new Action(() => {
                        confirm_Click(null,null);
                        clearForm();
                    }));

                    oSize = oThick = oAdd1 = oAdd2 = oAdd3 = oAdd4 = oAdd5 = "";
                    dA1 = dA2 = dA3 = dA4 = dA5 = false;
                }
                else if ((txt.IndexOf("Poproszę") >= 0) && speechOn == true)
                {
                    try
                    {
                        oSize = (String)e.Result.Semantics["size"].Value;
                        if (oSize != "")
                            this.Dispatcher.BeginInvoke(new Action(() => {
                                setSize(oSize);
                            }));
                    }
                    catch (Exception ex) { }

                    try {
                        oThick = (String)e.Result.Semantics["thickness"].Value;
                        if (oThick != "")
                            this.Dispatcher.BeginInvoke(new Action(() => {
                                setThickness(oThick);
                            }));
                    }
                    catch (Exception ex) { }

                    try {
                        oAdd1 = (String)e.Result.Semantics["add1"].Value;
                        oAdd2 = (String)e.Result.Semantics["add2"].Value;
                        oAdd3 = (String)e.Result.Semantics["add3"].Value;
                        oAdd4 = (String)e.Result.Semantics["add4"].Value;
                        oAdd5 = (String)e.Result.Semantics["add5"].Value;

                        if (e.Result.Semantics["dA1"].Value != null) dA1 = true;
                        if (e.Result.Semantics["dA2"].Value != null) dA2 = true;
                        if (e.Result.Semantics["dA3"].Value != null) dA3 = true;
                        if (e.Result.Semantics["dA4"].Value != null) dA4 = true;
                        if (e.Result.Semantics["dA5"].Value != null) dA5 = true;

                        //TODO
                        if (oAdd1 != "") this.Dispatcher.BeginInvoke(new Action(() => {
                            setAdd(oAdd1, dA1);
                        }));
                        if (oAdd2 != "") this.Dispatcher.BeginInvoke(new Action(() => {
                            setAdd(oAdd1, dA2);
                        }));
                        if (oAdd3 != "") this.Dispatcher.BeginInvoke(new Action(() => {
                            setAdd(oAdd1, dA3);
                        }));
                        if (oAdd4 != "") this.Dispatcher.BeginInvoke(new Action(() => {
                            setAdd(oAdd1, dA4);
                        }));
                        if (oAdd5 != "") this.Dispatcher.BeginInvoke(new Action(() => {
                            setAdd(oAdd1, dA5);
                        }));

                    }
                    catch (Exception ex) { }
                   

                    //this.Dispatcher.BeginInvoke(new Action(() => {
                    //    TB1.Text = ("zamówiono " + oSize + " pizze " + oThick + " ciasto z: " + oAdd1 + ", " + oAdd2 + ", " + oAdd3 + ", " + oAdd4 + ", " + oAdd4);
                    //}));
                    
                    Console.WriteLine("oSize: " + oSize + " oThick: " + oThick + " Add: " + (dA1?" 2x":" ") + oAdd1 + (dA2?" 2x" : " ") + oAdd2 +
                                        (dA3?" 2x" : " ") + oAdd3 + (dA4?" 2x" : " ") + oAdd4 + (dA5?" 2x" : " ") + oAdd5 );

                    if (oSize.Equals(""))
                    {
                        pTTS.SpeakAsync("jaki rozmiar pizzy? ");
                    }
                    else if (oThick.Equals(""))
                    {
                        pTTS.SpeakAsync("jaka grubość ciasta? ");
                    }

                }
                else if ( speechOn == true ) // TODO
                {
                    if (oSize.Equals(""))
                    {
                        try {
                            oSize = (String) e.Result.Semantics["size"].Value;
                            if (oSize != "")
                                this.Dispatcher.BeginInvoke(new Action(() => {
                                    setSize(oSize);
                                }));
                        }
                        catch (Exception ex) { }
                    }
                    else
                        if (oThick.Equals(""))
                        {
                            try { 
                                oThick = (String) e.Result.Semantics["thickness"].Value;
                            if (oThick != "")
                                this.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    setThickness(oThick);
                                }));
                            }
                            catch (Exception ex) { }
                    }

                    if (oSize=="")                    
                        pTTS.SpeakAsync("jaki rozmiar pizzy? ");                    
                    else if (oThick=="")                    
                        pTTS.SpeakAsync("jaka grubość ciasta? ");
                    
                } 
            }
            else
            {
                comments = String.Format("\tNISKI WSPÓŁCZYNNIK WIARYGODNOŚCI - powtórz polecenie");
                Console.WriteLine(comments);
                pTTS.SpeakAsync("Proszę powtórzyć");
            }

        }

        private void setSize(String si)
        {
            for (int i = 0; i < sizes.Length; i++)
            {
                if (si.Equals(sizes[i])) 
                { 
                    switch (i % sizesN)
                    {
                        case 0: s1_Click(null, null); break;
                        case 1: s2_Click(null, null); break;
                        case 2: s3_Click(null, null); break;
                    }
                break;
                }
            }
        }
        private void setThickness(String th)
        {
            for (int i = 0; i < thickness.Length; i++)
            {
                if (th.Equals(thickness[i]))
                { 
                    switch (i % thicknessN)
                    {
                        case 0: t1_Click(null, null); break;
                        case 1: t2_Click(null, null); break;
                        case 2: t3_Click(null, null); break;
                    }
                break;
                }
            }
        }
        private void setAdd(String add, bool x2)
        {
            for (int i = 0; i < addons.Length; i++)
            {
                if (add.Equals(addons[i]))
                { 
                    switch (i % addonsN)
                    {
                        case 0: if (x2) a11_Click(null, null); else a1_Click(null, null); break;
                        case 1: if (x2) a22_Click(null, null); else a2_Click(null, null); break;
                        case 2: if (x2) a33_Click(null, null); else a3_Click(null, null); break;
                        case 3: if (x2) a44_Click(null, null); else a4_Click(null, null); break;
                        case 4: if (x2) a55_Click(null, null); else a5_Click(null, null); break;
                    }
                break;
                }
            }
        }

        private void confirm_Click(object sender, RoutedEventArgs e)
        {
            //pTTS.SpeakAsync("confirm");
            SolidColorBrush bg = Brushes.LightGreen;
            
            String size = (s1.Background.Equals(bg) ? sizes[0] : "") + (s2.Background.Equals(bg) ? sizes[1] : "") + (s3.Background.Equals(bg) ? sizes[2] : "");
            String thicknes = (t1.Background.Equals(bg) ? thickness[0] : "") + (t2.Background.Equals(bg) ? thickness[1] : "") + (t3.Background.Equals(bg) ? thickness[2]: "");
            String add = (a1.Background.Equals(bg) ? addons[0] : "") + (a2.Background.Equals(bg) ? ","+addons[1] : "") + (a3.Background.Equals(bg) ? "," + addons[2] : "") + (a4.Background.Equals(bg) ? "," + addons[3] : "") + (a5.Background.Equals(bg) ? "," + addons[4] : "")+
                        (a11.Background.Equals(bg) ? ",2x "+addons[0] : "") + (a22.Background.Equals(bg) ? ",2x "+addons[1] : "") + (a33.Background.Equals(bg) ? ",2x "+addons[2] : "") + (a44.Background.Equals(bg) ? ",2x "+addons[3] : "") + (a55.Background.Equals(bg) ? ",2x "+addons[4] : "");

            clearForm(); 
            TB1.Text = "Zamówiono: "+ size+" pizza, "+thicknes+" ciasto"+", dodatki: "+add;
        }

        private void clearForm()
        {
            s1.IsEnabled = s2.IsEnabled = s3.IsEnabled = true;
            t1.IsEnabled = t2.IsEnabled = t3.IsEnabled = true;
            a1.IsEnabled = a2.IsEnabled = a3.IsEnabled = a4.IsEnabled = a5.IsEnabled = true;
            a11.IsEnabled = a22.IsEnabled = a33.IsEnabled = a44.IsEnabled = a55.IsEnabled = true;

            s1.Background = s2.Background = s3.Background = Brushes.LightGray;
            t1.Background = t2.Background = t3.Background = Brushes.LightGray;
            a1.Background = a2.Background = a3.Background = a4.Background = a5.Background = Brushes.LightGray;
            a11.Background = a22.Background = a33.Background = a44.Background = a55.Background = Brushes.LightGray;
        }

        private void newOrder_Click(object sender, RoutedEventArgs e)
        {
            //pTTS.SpeakAsync("newOrder");
            oSize = oThick = oAdd1 = oAdd2 = oAdd3 = oAdd4 = oAdd5 = "";
            dA1 = dA2 = dA3 = dA4 = dA5 =false;
            TB1.Text = "";
            clearForm();
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
