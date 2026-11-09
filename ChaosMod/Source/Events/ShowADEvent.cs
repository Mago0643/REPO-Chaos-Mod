using Lego.ChaosMod.Backend;

namespace Lego.ChaosMod.Events;
public class ShowADEvent: ChaosEvent // this is also unused because videos are taking long to load (also it EATS TOO MANY RAM)
{
    public ShowADEvent(): base("광고 시청", "개발자도 먹고 살아야죠...")
    {
        
    }

    public override float GetTime()
        => ChaosMod.Instance.adViewer!.GetLength();

    public override void Start()
    {
        ChaosMod.Instance.adViewer!.SetVideo();
        ChaosMod.Instance.adViewer.Show();

        base.Start();
    }

    public override void OnFinished()
    {
        base.OnFinished();

        ChaosMod.Instance.adViewer!.Hide();
    }
}