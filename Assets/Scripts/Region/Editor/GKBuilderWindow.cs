using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System;
using GKBase;

public class GKBuilderWindow : EditorWindow {

	#region PublicField
	#endregion

	#region PrivateField
	private int selected = 0;
	private bool bLarger = false;
	private bool bObfuscation = false;
	#endregion

	#region PublicMethod
	public static void MenuItem_Window() {
		var w = EditorWindow.GetWindow<GKBuilderWindow>("Game builder");
		w.autoRepaintOnSceneChange = true;
		// Window size is not allowed.
		w.minSize = new Vector2( 800, 500 );
		w.maxSize = new Vector2( 800, 500 );
		w.Show();	
	}
	#endregion

	#region PrivateMethod
	void OnGUI() {
		EditorGUILayout.BeginVertical ();
		{
			GUILayout.Label ("Select the channel.");

			string [] channels = GK.EnumNames<RegionDefine.Channel> ();
			selected = EditorGUILayout.Popup (selected, channels);

			RegionDefine.Channel [] channelEnum = GK.EnumValues<RegionDefine.Channel> ();
			RegionDefine.Channel curRegion = RegionDefine.GetRegionType(channelEnum[selected]);
			RegionDefine.currentChannel = curRegion;	// Set region for output path.
			string region = curRegion.ToString();
			GUILayout.Label (string.Format("Region name: [ {0} ]", region));

			bLarger = GUILayout.Toggle (bLarger, "Full resource installation package.");

			bObfuscation = GUILayout.Toggle (bObfuscation, "Code Obfuscation.");

			if( GUILayout.Button("Build") ) 
			{				
				GKBuilder.Building (channelEnum [selected], bLarger, bObfuscation);
			}
		}
		EditorGUILayout.EndVertical ();
	}
	#endregion
}
