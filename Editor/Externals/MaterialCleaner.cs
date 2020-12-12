
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

internal sealed class MaterialCleaner
{
	[MenuItem( kMenuItemName, false)]
	static void MaterialCleanerMenu()
	{
		Clean( Selection.assetGUIDs);
	}
	[MenuItem( kMenuItemName, true)]
	static bool IsEnabledMaterialCleanerMenu()
	{
		string[] assetGuids = Selection.assetGUIDs;
		if( assetGuids != null)
		{
			for( int i0 = 0; i0 < assetGuids.Length; ++i0)
			{
				string path = AssetDatabase.GUIDToAssetPath( assetGuids[ i0]);
				if( AssetDatabase.LoadMainAssetAtPath( path) is Material)
				{
					return true;
				}
			}
		}
		return false;
	}
	public static void Clean( IEnumerable<string> assetGuids)
	{
		if( assetGuids != null)
		{
			if( EditorUtility.DisplayDialog( kProgressCaptionName, 
				"Change the Material and save the Asset.\nDo you want to start processing?", "Yes", "No") != false)
			{
				float assetCount = assetGuids.Count();
				int processCount = 0;
				int i0 = 0;
				
				try
				{
					foreach( var assetGuid in assetGuids)
					{
						string path = AssetDatabase.GUIDToAssetPath( assetGuid);
						
						if( AssetDatabase.LoadMainAssetAtPath( path) is Material material)
						{
							EditorUtility.DisplayProgressBar( kProgressCaptionName, 
								string.Format( "{0}", material.name), i0 / assetCount);
							DeleteUnusedProperties( material, path);
							++processCount;
						}
						++i0;
					}
					EditorUtility.DisplayProgressBar( kProgressCaptionName, "Refresh AssetDatabase...", 1);
				}
				catch( System.Exception e)
				{
					Debug.LogError( e);
				}
				AssetDatabase.Refresh();
				EditorUtility.ClearProgressBar();
				
				if( processCount > 0)
				{
					if( EditorUtility.DisplayDialog( kProgressCaptionName,
						"Writes all unsaved asset changes to disk.", "Yes", "No") != false)
					{
						AssetDatabase.SaveAssets();
					}
				}
			}
		}
	}
	static void DeleteUnusedProperties( Material material, string path)
	{
		var newMaterial = new Material( material.shader);

		newMaterial.name = material.name;
		newMaterial.renderQueue = ( material.shader.renderQueue == material.renderQueue) ? -1 : material.renderQueue;
		newMaterial.enableInstancing = material.enableInstancing;
		newMaterial.doubleSidedGI = material.doubleSidedGI;
		newMaterial.globalIlluminationFlags = material.globalIlluminationFlags;
		newMaterial.hideFlags = material.hideFlags;
		newMaterial.shaderKeywords = material.shaderKeywords;

		var properties = MaterialEditor.GetMaterialProperties( new Material[] { material });
		for( int i0 = 0; i0 < properties.Length; ++i0)
		{
			SetPropertyToMaterial( newMaterial, properties[ i0], null);
		}
		
		string tempPath = AssetDatabase.GenerateUniqueAssetPath( path);
		AssetDatabase.CreateAsset( newMaterial, tempPath);
		FileUtil.ReplaceFile( tempPath, path);
		AssetDatabase.DeleteAsset( tempPath);
	}
	static void CopyPropertyToMaterial( Material material, MaterialProperty[] properties, string srcPropertyName, string dstPropertyName)
	{
		var src = properties.FirstOrDefault( p => p.name.Equals( srcPropertyName));
		var dst = properties.FirstOrDefault( p => p.name.Equals( dstPropertyName));
		if( src != null && dst != null)
		{
			SetPropertyToMaterial( material, src, dst.name);
		}
	}
	static void SetPropertyToMaterial( Material material, MaterialProperty property, string propertyName)
	{
		if( propertyName == null)
		{
			propertyName = property.name;
		}
		switch( property.type)
		{
			case MaterialProperty.PropType.Color:
			{
				material.SetColor( propertyName, property.colorValue);
				break;
			}
			case MaterialProperty.PropType.Float:
			case MaterialProperty.PropType.Range:
			{
				material.SetFloat( propertyName, property.floatValue);
				break;
			}
			case MaterialProperty.PropType.Texture:
			{
				Texture texture = null;
				
				if( (property.flags & MaterialProperty.PropFlags.PerRendererData) != MaterialProperty.PropFlags.PerRendererData)
				{
					texture = property.textureValue;
				}
				material.SetTexture( propertyName, texture);
				
				if( (property.flags & MaterialProperty.PropFlags.NoScaleOffset) != MaterialProperty.PropFlags.NoScaleOffset)
				{
					material.SetTextureScale( propertyName, new Vector2( property.textureScaleAndOffset.x, property.textureScaleAndOffset.y));
					material.SetTextureOffset( propertyName, new Vector2( property.textureScaleAndOffset.z, property.textureScaleAndOffset.w));
				}
				break;
			}
			case MaterialProperty.PropType.Vector:
			{
				material.SetVector( propertyName, property.vectorValue);
				break;
			}
		}
	}
	const string kProgressCaptionName = "Material Cleaner";
	const string kMenuItemName = "Assets/Material Cleaner";
}
