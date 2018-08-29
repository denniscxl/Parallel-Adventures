using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionMainland : RegionDataBase {

	public RegionMainland (RegionDefine.Channel channel) 
		: base (channel)
	{
		frontEndServerUrl = "";
        lastVersion = "20180418";
        version = "20180418";
		installationDownloadUrl = "www.baidu.com";
	}
}
