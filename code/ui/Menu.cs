using Sandbox.UI;

namespace GGame;

public class Menu : Panel {
    public Panel openMenu;


    public Menu() {
        StyleSheet.Load("ui/Menu.scss");

        AddChild(new Label() {Text = "Goons Gone Rogue", Classes = "title"});
        Panel buttons = new(this, "buttons");

        buttons.AddChild(new Button("Start","", () => {
            GGame.TransitionGameStart("dpiol");
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

            ///////////

            Panel a1 = new(this, "container");
            
            a1.AddChild(new Label() {Text = "Goons Gone Rogue is a sort of party-builder roguelike.\n" +
                "You and your team are goons, all of which have their own set of stats to upgrade and progress.\n" + 
                "You simply go as far as you can, while earning the highest score at the same time.\n" + 
                "The controls are WASD to move, LMB to fire, E to interact",
                Classes = "labelb"
            });

            ///////////

            Panel i1 = new(this, "containerb");

            Image img1 = new();
            img1.SetTexture("images/stats.png");
            i1.AddChild(img1);

            i1.AddChild(new Label() {Text = "" +
                "This is a goon's stats. " +
                "For each stat, the first number is the 'BASE' stat and the added number is the 'ADD' stat. " + 
                "This is important to know as Powerups (explained below) only effect 'ADD' stats. " + 
                "In Example, Glass Cannon doubles Add damage, \nso if you had 10 + 4 Damage, \nyou get 10 + 8 damage."
            });
            ///////////

            Panel i2 = new(this, "container");

            Image img2 = new();
            img2.SetTexture("images/start.png");
            i2.AddChild(img2);

            i2.AddChild(new Label() {Text = "" +
                "This is the spawn platform for a level.\n" +
                "The end platform is on the opposite side of the level from it."
            });

            ///////////

            Panel i3 = new(this, "container");

            Image img3 = new();
            img3.SetTexture("images/end.png");
            i3.AddChild(img3);

            i3.AddChild(new Label() {Text = "" +
                "This is the end platform for a level.\n" +
                "Stepping on it transitions to the next level.\n" + 
                "Every next level gets slightly larger and more difficult."
            });

            ///////////

            Panel i4 = new(this, "container");

            Image img4 = new();
            img4.SetTexture("images/powerup.png");
            i4.AddChild(img4);

            i4.AddChild(new Label() {Text = "" +
                "These are powerups. They are most commonly found in dead ends.\n" + 
                "Pressing e on them will let you give it's power to yourself or a teammate goon"
            });
        }
    }

    public class About : Panel {
        public About() {
            Classes = "popup about";

            AddChild(new Label() {Text = "Made for the Three Thieves spring 2023 gamejam\n\n" +
                "-- Made By OBC --\n" + 
                "Kodi022 - conception, programming, ui\n" + 
                "Andy - theme, art, modelling"
            });
        }
    }

    public class Leaderboard : Panel {
        public Leaderboard() {
            Classes = "popup leaderboard";



            AddChild(new Label() {Text = ""});
        }
    }
}