using Sandbox;
using Sandbox.UI;

namespace GGame;

public class Menu : Panel {
    public Panel openMenu;

    public Menu() {
        StyleSheet.Load("ui/Menu.scss");

        AddChild(new Label() {Text = "Goons Gone Rogue", Classes = "title"});
        Panel buttons = new(this, "buttons");

        buttons.AddChild(new Button("Start","", () => {
            TeamUI.Current.Add(Player.Current);
            ServerGameStart("dpiol");
        }) {Classes = "buttone"});

        buttons.AddChild(new Button("Information","", () => {
            openMenu?.Delete();
            Help a = new();
            openMenu = a;
            AddChild(a);
        }) {Classes = "buttone"});

        buttons.AddChild(new Button("Leaderboard","", () => {
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

        buttons.AddChild(new Button("Toggle music","", () => {
            ServerToggleMusic();
        }) {Classes = "buttone"});
    }

    [ConCmd.Server]
    public static void ServerGameStart(string password) {
        if (password != "dpiol") return;
        GGame.Current.GameStart();
    }

    [ConCmd.Server]
    public static void ServerToggleMusic() {
        GGame.Current.IsMusicEnabled = !GGame.Current.IsMusicEnabled;
        GGame.Current.OnIsMusicEnabledChanged();
    }

    public class Help : Panel {
        public Help() {
            Classes = "popup help";

            Panel notice = new(this, "containerb");

            notice.AddChild(new Label() {Text = "You can scroll in this menu",
                Classes = "labelc"
            });

            ///////////

            Panel a1 = new(this, "container");
            
            a1.AddChild(new Label() {Text = "Goons Gone Rogue is a sort of party-builder roguelike.\n" +
                "You and your team are goons, all of which have their own set of stats to upgrade and progress.\n" + 
                "You simply go as far as you can, while earning the highest score at the same time.\n" + 
                "The controls are WASD to move, LMB to fire, E to interact.",
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
                "In Example, Glass Cannon increases Add damage, \nso if you had 10 + 4 Damage, \nyou get 10 + 8 damage. " + 
                "Stats cannot be worse under a functional level, so they cannot become unusable."
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
                "Pressing e on them will let you give it's power to yourself or a teammate goon."
            });

            ///////////

            Panel i5 = new(this, "containerb");

            i5.AddChild(new Label() {Text = "\n  Stats and their functional limits\n\n" +
                "Armor: Damage reduction\n" +
                "Limits: 0 to 150 (0% to 80% reduction)\n\n" +                  
                "Speed: How fast goon moves\n" +
                "Limits: 50 to inf\n\n" +
                "Range: Unit distance goon's bullet reach\n" +
                "Limits: 100 to inf\n\n" +                 
                "Damage: Damage goon's bullets do\n" +
                "Limits: 1 to inf\n\n" +                 
                "Delay: Time between shooting\n" +
                "Limits: 0.05 to 1.5\n\n" +
                "Mag: Magazine size\n" +
                "Limits: 2 to inf\n\n" +
                "Spread: Degree inaccuracy of goon\n" +
                "Limits: 25 to inf\n\n" +
                "Reload: Time for goon to reload\n" +
                "Limits: 0.1 to 4\n",
                Classes = "labelb"
            });

            ///////////

            Panel i6 = new(this, "containerb");

            i6.AddChild(new Label() {Text = "  Tips\n\n" +
                " - Battles in open spaces can be avoided if your careful\n" + 
                " - Having high damage/range goons is useful\n" +
                " - You start stronger than your goon team\n",
                Classes = "labelb"
            });
        }
    }

    public class About : Panel {
        public About() {
            Classes = "popup about";

            AddChild(new Label() {Text = "Made for the Three Thieves spring 2023 gamejam\n\n" +
                "It was absolutely difficult, being our first Game Jam.\n" + 
                "We learned a ton, and will make better games in the future because of it. " +
                "It was worth it, a wonderful opportunity. Thank you Three Thieves for running this event." +
                "\n\n" +
                "-- Made By OBC --\n" + 
                "Kodi022 - conception, programming, ui, sound, music\n" + 
                "Andy - theme, art, modelling, sound"
            });
        }
    }

    public class Leaderboard : Panel {
        public Leaderboard() {
            Classes = "popup leaderboard";

            AddChild(new Label() {Classes = "text", Text = "Leaderboards"});

            string scores = "";
            foreach (var item in Leaderboards.Current.TopScores) {
                scores += $"{item.Key} --- {item.Value}\n";
            }

            AddChild(new Label() {Classes = "entries", Text = scores});
        }
    }
}