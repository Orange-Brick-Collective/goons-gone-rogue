using Sandbox;

namespace GGame;

public partial class GGame {
	[ConCmd.Server("ggr_setmoney")]
	public static void ServerSetMoneyCMD(int money) {
		GGame.Current.Money = money;
	}

	[ConCmd.Server("ggr_setdepth")]
	public static void ServerSetDepthCMD(int depth) {
		GGame.Current.currentWorld.depth = depth;
		GGame.Current.CurrentDepth = depth;
	}

    [ConCmd.Server("ggr_generate")]
	public static async void ServerSetTypeCMD(int wallType) {
        await WorldGen.Current.GenerateWorld(12, 8, 4, wallType);
		Player.Current.InMenu = false;
		Player.Current.Transform = Current.currentWorld.startPos;
	}
}