using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionShanda : RegionDataBase
{
    public RegionShanda(RegionDefine.Channel channel)
        : base(channel)
    {
        frontEndServerUrl = "";
        lastVersion = "Ver2018061300";
        version = "Ver2018061300";
        installationDownloadUrl = "www.baidu.com";
    }
}
