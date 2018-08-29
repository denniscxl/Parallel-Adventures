using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDKDataBase 
{
	public bool isInited { get; set;}
	public bool isLogined { get; set;}
	public string userID { get; set;}
	public string userName { get; set;}
	public bool isAdult { get; set;}	// Anti addiction system;

	public SDKDataBase()
	{
		isInited = false;
		isLogined = false;
		userID = string.Empty;
		userName = string.Empty;
		isAdult = false;
	}
}
