using UnityEngine;
using UnityEditor;
using GKBase;

public class GameAssetPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        var filename = string.Empty;
        try
        {
            foreach (var file in importedAssets)
            {
                filename = file;
                
                if (file.StartsWith("Assets/CSV/_AutoGen/GameData_"))
                {
                    Debug.Log(string.Format("OnPostprocessAllAssets {0}", file));
					GameDataImport.OnImportData(file);
                    continue;
                }

                // ...
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("OnPostprocessAllAssets Exception: " + filename + "\n" + e);
        }
    }

    //public void OnPreprocessModel() {
    //    var im = (ModelImporter) assetImporter;
    //    im.importNormals        = ModelImporterNormals.None;
    //    im.animationType		= ModelImporterAnimationType.Legacy;
    //    im.generateAnimations	= ModelImporterGenerateAnimations.GenerateAnimations;
    //}

    public void OnPreprocessTexture() 
	{
//		if(assetPath.Contains("Temp")) 
//		{
//			TextureImporter textureImporter = assetImporter as TextureImporter;
//		}

	//    var im = (TextureImporter) assetImporter;
    //    if (assetPath.StartsWith("Assets/Scene/GameLevels/"))
    //    {
    //        var filename = System.IO.Path.GetFileName( assetPath );
    //        if( filename.StartsWith("Temp_") ) {
    //            // using those setting to speed up the import, since we don't using those texture in the game anyway
    //            im.textureFormat = TextureImporterFormat.Alpha8;
    //            im.grayscaleToAlpha = true;
    //            im.mipmapEnabled = false;
    //        }
    //    }
    }

	public void OnPostprocessModel( GameObject root ) {
		OptimizeMesh( root );
	}

	static public void OptimizeMesh( GameObject root ) {
		if( ! root ) return;
		
		foreach( var mr in root.GetComponentsInChildren<SkinnedMeshRenderer>() ) {
			foreach( var mat in mr.sharedMaterials ) {
				_OptimizeMesh( mat, mr.sharedMesh );
			}
		}
		
		foreach( var mr in root.GetComponentsInChildren<MeshRenderer>() ) {
			foreach( var mat in mr.sharedMaterials ) {
				var mf = mr.GetComponent<MeshFilter>();			
				if( ! mf ) continue;
				_OptimizeMesh( mat, mf.sharedMesh );
			}
		}
	}
		
	static public void _OptimizeMesh( Material mat, Mesh mesh ) {
		if( ! mat ) return;
		if( ! mesh ) return;
		
		var shaderName = mat.shader.name;

		if( shaderName == "Bumped Diffuse" ) {
			mat.shader = GK.LoadMaterial("Materials/Diffuse").shader;
		}
	}
}




