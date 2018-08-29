using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionDev : RegionDataBase {

	public RegionDev (RegionDefine.Channel channel) 
		: base (channel)
	{
		frontEndServerUrl = "";
        lastVersion = "20180418";
        version = "20180419";
		installationDownloadUrl = "www.baidu.com";
	}
}
