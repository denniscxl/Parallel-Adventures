using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDKInterface : SDKInterfaceBase<SDKInterface>
{
	#region PublicMethod
	public SDKInterface()
	{
		sdkData = new SDKDataBase ();
		sdkData.isInited = true;
		sdkData.isLogined = true;
	}

	public void SetSDKData(SDKDataBase data)
	{
		sdkData = data;
	}
	#endregion

	#region PrivateMethod
	#endregion
}
