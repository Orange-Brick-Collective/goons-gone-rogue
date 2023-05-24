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
            LeaderboardPanel a = new();
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

            Panel notice = new(this, "containerstatic");

            notice.AddChild(new Label() {Text = "You can scroll in this menu",
                Classes = "labelc"
            });

            ///////////

            Panel paragraphP = new(this, "container");
            
            paragraphP.AddChild(new Label() {Text = "Goons Gone Rogue is a sort of party-builder roguelike.\n" + 
                "You and your team are goons, all of which have their own set of stats to upgrade and progress.\n" + 
                "You simply go as far as you can, while earning the highest score at the same time.\n" + 
                "The controls are WASD to move, LMB to fire, Use (default E) to interact.\n\n" + 
                "You begin in the overworld, where the enemies you find are triggers that touching their tile will start the fight.",
                Classes = "labelb"
            });

            ///////////

            Panel statsPanel = new(this, "containerstatic");

            Image statsImage = new() {Classes = "bigimage"};
            statsImage.SetTexture("images/stats.png");
            statsPanel.AddChild(statsImage);

            statsPanel.AddChild(new Label() {Text = "" +
                "This is a goon's stats. " +
                "For each stat, the first number is the 'BASE' stat and the added number is the 'ADD' stat. " + 
                "This is important to know as Powerups (explained below) only effect 'ADD' stats. " + 
                "In Example, Glass Cannon increases Add damage, \nso if you had 10 + 4 Damage, \nyou get 10 + 8 damage. " + 
                "Stats cannot be worse under a functional level, so they cannot become unusable."
            }); 
            ///////////

            Panel startPanel = new(this, "container");

            Image startImage = new() {Classes = "smallimage"};
            startImage.SetTexture("images/start.png");
            startPanel.AddChild(startImage);

            startPanel.AddChild(new Label() {Text = "" +
                "This is the spawn platform for a level.\n" +
                "The end platform is on the opposite side of the level from it."
            });

            ///////////

            Panel endPanel = new(this, "container");

            Image endImage = new() {Classes = "smallimage"};
            endImage.SetTexture("images/end.png");
            endPanel.AddChild(endImage);

            endPanel.AddChild(new Label() {Text = "" +
                "This is the end platform for a level.\n" +
                "Stepping on it transitions to the next level.\n" + 
                "Every next level gets slightly more difficult in multiple ways."
            });

            ///////////

            Panel powerupPanel = new(this, "container");

            Image powerupImage = new() {Classes = "smallimage"};
            powerupImage.SetTexture("images/powerup.png");
            powerupPanel.AddChild(powerupImage);

            powerupPanel.AddChild(new Label() {Text = "" +
                "These are powerups. They are most commonly found in dead ends.\n" + 
                "Pressing use on them will let you give it's power to yourself or a teammate goon."
            });

            ///////////

            Panel shopPanel = new(this, "container");

            Image shopImage = new() {Classes = "smallimage"};
            shopImage.SetTexture("images/shop.png");
            shopPanel.AddChild(shopImage);

            shopPanel.AddChild(new Label() {Text = "" +
                "This is the store.\n" +
                "It sells extra powerups, any costing $1000 are double strength."
            });

            ///////////

            Panel statDescPanel = new(this, "containerstatic");
            
            statDescPanel.AddChild(new Label() {Text = "\n  Stats and their functional limits\n\n" +
                "Armor: Damage reduction\n" +
                "Limits: 0 to 150 (0% to 80% reduction)\n\n" +                  
                "Speed: How fast goon moves\n" +
                "Limits: 60 to 1000\n\n" +
                "Range: Unit distance goon's bullet reach\n" +
                "Limits: 100 to inf\n\n" +                 
                "Damage: Damage goon's bullets do\n" +
                "Limits: 1 to inf\n\n" +                 
                "Delay: Time between shooting\n" +
                "Limits: 0.05 to 1.5\n\n" +
                "Mag: Magazine size\n" +
                "Limits: 2 to inf\n\n" +
                "Spread: Degree inaccuracy\n" +
                "Limits: 0 to 8\n\n" +
                "Reload: Time for goon to reload\n" +
                "Limits: 0.1 to 6\n",
                Classes = "labelb"
            });

            ///////////

            Panel tipsPanel = new(this, "containerstatic");

            tipsPanel.AddChild(new Label() {Text = "  Tips\n\n" +
                " - Overworld battles in open streets can be avoided if your careful\n" + 
                " - Having high damage/range goons may be valuable\n" +
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

    public class LeaderboardPanel : Panel {
        public LeaderboardPanel() {
            Classes = "popup leaderboard";

            AddChild(new Label() {Classes = "text", Text = "Leaderboards"});

            string scores = "";
            foreach (var item in Leaderboard.Current.TopScores) {
                scores += $"{item.Key} --- {item.Value}\n";
            }

            AddChild(new Label() {Classes = "entries", Text = scores});
        }
    }
}