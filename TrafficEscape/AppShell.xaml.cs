namespace TrafficEscape
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(GamePage), typeof(GamePage));
            Routing.RegisterRoute(nameof(MainMenuPage), typeof(MainMenuPage));
        }

    }
}
