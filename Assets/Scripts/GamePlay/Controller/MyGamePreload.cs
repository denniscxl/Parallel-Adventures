using System.Collections;
using GKBase;
using GKController;

public class MyGamePreload : GamePreload {

    public static MyGamePreload Instance = null;

	public override IEnumerator Preload()
	{
		yield return null;
        MyGame.Instance.Init ();
        GK.SetParent(MyGame.Instance.gameObject, gameObject, false);
	}
}
