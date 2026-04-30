using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LostInAForgottenCity.Views
{
    public partial class TutorialView : UserControl
    {
        // ── Fields ──────────────────────────────
        private int _currentScene = 0;
        private DispatcherTimer _typewriterTimer = new();
        private int _typewriterPosition = 0;
        private string _targetText = "";
        private TextBlock? _targetBlock;
        private Queue<(TextBlock Block, string Text, int SpeedMs)> _typewriterQueue = new();

        // ── Constructor ─────────────────────────
        public TutorialView()
        {
            InitializeComponent();
            ShowScene(0);
        }

        // ── Scene logic ─────────────────────────
        private void ShowScene(int scene)
        {
            _currentScene = scene;
            switch (scene)
            {
                case 0:
                    TypeWrite(FortuneTellerText, "\" Another one arrives in search of knowledge, and I have what you seek.\n" +
                                                "The only question I have is — do you have any idea of what I will show you,\n" +
                                                "or do you need hand holding? \"", 60);
                    NarrativeText.Text = "";
                    HintText.Text = "";
                    ActionButton.Content = "  I HAVE AN IDEA  ";
                    break;

                case 1:
                    TypeWrite(FortuneTellerText, "\" I see. Now get closer to the Crystal Ball and witness\n" +
                                                "the final moments of these unfortunate souls. \"", 60);
                    NarrativeText.Text = "";
                    HintText.Text = "";
                    ActionButton.Content = "  PROCEED  ";
                    break;

                case 2:
                    TypeWrite(FortuneTellerText, "\" This one lasted a bit longer than the others.\n" +
                                                "Let's take a closer look. \"", 60);
                    TypeWrite(NarrativeText, "A hiker. Supplies running thin, legs tired from the mountain trail.\n" +
                                            "Through the trees, a silhouette — buildings, rooftops, civilization.\n" +
                                            "He adjusts his pack and walks toward it.", 30);
                    HintText.Text = "";
                    ActionButton.Content = "  CONTINUE  ";
                    break;

                case 3:
                    FortuneTellerText.Text = "";
                    TypeWrite(NarrativeText, "\"Finally I reached the city. Hmm... the fog is quite thick.\n" +
                                            "I should stick to landmarks or I might get lost.\"", 30);
                    HintText.Text = "[ In this city you cannot travel freely.\n" +
                                   "Movement is limited to the landmarks within each location. ]";
                    ActionButton.Content = "  GO TO THE NEARBY HOUSE  ";
                    break;

                case 4:
                    FortuneTellerText.Text = "";
                    TypeWrite(NarrativeText, "The house is silent. Dust on every surface.\n" +
                                            "Frozen in time, like someone simply walked out one day\n" +
                                            "and never came back.", 30);
                    HintText.Text = "[ When reaching a landmark, an automatic scan reveals\n" +
                                   "what surrounds you and what actions are available. ]";
                    ActionButton.Content = "  LOOK AROUND  ";
                    break;

                case 5:
                    FortuneTellerText.Text = "";
                    TypeWrite(NarrativeText, "\"This table has some things on it...\n" +
                                            "I don't know if it's right to touch other people's stuff.\"", 30);
                    HintText.Text = "[ When interacting with an object you can perform different actions:\n" +
                                   "Examine — look closely.  Search — look for items.  Exit — step away. ]";
                    ActionButton.Content = "  APPROACH THE TABLE  ";
                    break;

                case 6:
                    FortuneTellerText.Text = "";
                    TypeWrite(NarrativeText, "*On the table are some papers and an empty bottle.*\n\n" +
                                            "\"I think nobody will miss this bottle.\n" +
                                            "After all, who needs garbage?\"", 30);
                    HintText.Text = "[ When finding an item you can take it or leave it.\n" +
                                   "Remember — inventory space is limited. Choose wisely. ]";
                    ActionButton.Content = "  PICK UP THE BOTTLE  ";
                    break;

                case 7:
                    FortuneTellerText.Text = "";
                    TypeWrite(NarrativeText, "\"This bottle may be useful. I should keep it.\"\n\n" +
                                            "*He steps outside. The fog has thickened.*\n" +
                                            "*Something moves in the distance. A shape. Dark. Still.*", 30);
                    HintText.Text = "[ The city is filled with items, each with a different purpose.\n" +
                                   "Some usable immediately. Others only in specific situations. ]";
                    ActionButton.Content = "  EXIT THE HOUSE  ";
                    break;

                case 8:
                    FortuneTellerText.Text = "";
                    TypeWrite(NarrativeText, "\"What is that... am I finally starting to lose my vision,\n" +
                                            "or is there something there?\"", 30);
                    HintText.Text = "[ The city is not empty. Your mind is not made of steel.\n" +
                                   "If you don't stop what is draining your sanity — bad things will happen. ]";
                    ActionButton.Content = "  LOOK AWAY  ";
                    break;

                case 9:
                    FortuneTellerText.Text = "";
                    TypeWrite(NarrativeText, "*Whatever you did, it grabbed that thing's attention.*\n\n" +
                                            "\"Shit... that thing is getting closer. I need to hide!\"", 30);
                    HintText.Text = "[ When you are in danger an encounter begins.\n" +
                                   "All normal actions lock. Only encounter actions are available. ]";
                    ActionButton.Content = "  HIDE  ";
                    break;

                case 10:
                    FortuneTellerText.Text = "";
                    TypeWrite(NarrativeText, "*The shape draws closer. A low hum cuts through the fog.*\n\n" +
                                            "\"That thing is getting closer. I must do something!\"", 30);
                    HintText.Text = "[ Enemies cannot teleport — they move through locations.\n" +
                                   "Always be aware of their proximity and plan accordingly. ]";
                    ActionButton.Content = "  DISTRACT IT WITH THE BOTTLE  ";
                    break;

                case 11:
                    TypeWrite(FortuneTellerText, "\" He lasted longer than most.\n" +
                                                "But the city is patient. It always gets what it wants. \"", 60);
                    TypeWrite(NarrativeText, "*You run in the other direction.*\n" +
                                            "*Around the corner — another one.*\n" +
                                            "*You don't have time to react. You lock eyes.*\n\n" +
                                            "*You fall unconscious.*\n\n" +
                                            "Your journey ends here.", 30);
                    HintText.Text = "";
                    ActionButton.Content = "  RETURN TO MENU  ";
                    break;
            }
        }

        // ── Typewriter ──────────────────────────
        private void TypeWrite(TextBlock block, string text, int speedMs)
        {
            if (_typewriterTimer.IsEnabled)
            {
                _typewriterQueue.Enqueue((block, text, speedMs));
                return;
            }
            StartTypewriter(block, text, speedMs);
        }

        private void StartTypewriter(TextBlock block, string text, int speedMs)
        {
            _targetText = text;
            _targetBlock = block;
            _typewriterPosition = 0;
            block.Text = string.Empty;
            _typewriterTimer.Interval = TimeSpan.FromMilliseconds(speedMs);
            _typewriterTimer.Tick -= TypewriterTimer_Tick;
            _typewriterTimer.Tick += TypewriterTimer_Tick;
            _typewriterTimer.Start();
        }

        private void TypewriterTimer_Tick(object? sender, EventArgs e)
        {
            _typewriterPosition++;
            if (_targetBlock != null)
                _targetBlock.Text = _targetText.Substring(0, _typewriterPosition);
            if (_typewriterPosition >= _targetText.Length)
            {
                _typewriterTimer.Stop();
                if (_typewriterQueue.Count > 0)
                {
                    var next = _typewriterQueue.Dequeue();
                    StartTypewriter(next.Block, next.Text, next.SpeedMs);
                }
            }
        }

        // ── Button events ────────────────────────
        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentScene == 11)
            {
                MainWindow.Instance?.NavigateTo(new MenuView());
            }
            else
            {
                ShowScene(_currentScene + 1);
            }
        }
    }
}