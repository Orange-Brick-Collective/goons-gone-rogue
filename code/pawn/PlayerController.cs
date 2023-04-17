using Sandbox;

namespace GGame;

public class PlayerController {
    public virtual void BuildInput() {}
}

public class PlayerMenuController : PlayerController {
    public Player player;

    public PlayerMenuController(Player player) {
        this.player = player;
    }

    public override void BuildInput() {
        player.ViewAngles = new Angles(20, Time.Tick * 0.16f, 0);
    }
}

public class PlayerPlayingController : PlayerController {
    public Player player;

    public PlayerPlayingController(Player player) {
        this.player = player;
    }

    public override void BuildInput() {
        Angles viewAngles = player.ViewAngles;
        viewAngles += Input.AnalogLook;
        viewAngles.pitch = viewAngles.pitch.Clamp(-72, 68);
        player.ViewAngles = viewAngles.Normal;

        if (player.IsPlaying) player.InputDirection = Input.AnalogMove;
    }
}