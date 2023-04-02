using Sandbox.UI;

namespace GGame;

public class Menu : Panel {
    public Panel openMenu;


    public Menu() {
        StyleSheet.Load("ui/Menu.scss");

        AddChild(new Label() {Text = "Goons Gone Rogue", Classes = "title"});
        Panel buttons = new(this, "buttons");

        buttons.AddChild(new Button("Start","", () => {
            GGame.TransitionStart("dpiol");
        }) {Classes = "buttone"});

        buttons.AddChild(new Button("Beginner Info","", () => {
            openMenu?.Delete();
            Help a = new();
            openMenu = a;
            AddChild(a);
        }) {Classes = "buttone"});

        buttons.AddChild(new Button("...","", () => {
            openMenu?.Delete();
            Leaderboard a = new();
            openMenu = a;
            AddChild(a);
        }) {Classes = "buttone"});

        buttons.AddChild(new Button("About","", () => {
            openMenu?.Delete();
            About a = new();
            openMenu = a;
            AddChild(a);
        }) {Classes = "buttone"});
    }

    public class Help : Panel {
        public Help() {
            Classes = "popup help";

            Panel a1 = new(this, "container");

            Image img1 = new();
            img1.SetTexture("images/start.png");
            a1.AddChild(img1);

            a1.AddChild(new Label() {Text = @"
                This is the spawn platform for a level.
            "});

            ///////////

            Panel a2 = new(this, "container");

            Image img2 = new();
            img2.SetTexture("images/end.png");
            a2.AddChild(img2);

            a2.AddChild(new Label() {Text = ""});

            ///////////

            Panel a3 = new(this, "container");

            Image img3 = new();
            img3.SetTexture("images/powerup.png");
            a3.AddChild(img3);

            a3.AddChild(new Label() {Text = ""});
        }
    }

    public class About : Panel {
        public About() {
            Classes = "popup about";

            AddChild(new Label() {Text = @"
                Made for the Three Thieves spring 2023 gamejam

                -- Made By OBC --
                Kodi022 - conception, programming, ui
                Andy - theme, art, modelling
            "});
        }
    }

    public class Leaderboard : Panel {
        public Leaderboard() {
            Classes = "popup leaderboard";



            AddChild(new Label() {Text = ""});
        }
    }
}