
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Finder
{
	public sealed class Search
	{
		public static void TracePrecedents( 
			IEnumerable<string> traceGuids, bool recursive,
			out Dictionary<string, ElementSource> traces,
			out Dictionary<string, ElementSource> founds)
		{
			var traceAssets = new Dictionary<string, string>();
			string[] assetPaths;
			string targetGuid;
			string targetPath;
			string tracePath;
			
			founds = new Dictionary<string, ElementSource>();
			traces = new Dictionary<string, ElementSource>();
			
			if( OnProgress( "Trace Precedents", 0) != false)
			{
				OnFinish();
				return;
			}
			foreach( string traceGuid in traceGuids)
			{
				tracePath = AssetDatabase.GUIDToAssetPath( traceGuid);
				if( string.IsNullOrEmpty( tracePath) == false
				&&	AssetDatabase.IsValidFolder( tracePath) == false)
				{
					traceAssets.Add( tracePath, traceGuid);
					traces.Add( tracePath, new ElementSource( tracePath, 0));
				}
			}
			if( traceAssets.Count > 0)
			{
				string[] targetGuids = AssetDatabase.FindAssets( 
					"t:Scene t:Prefab t:Material t:AnimatorController t:ScriptableObject");
				string traceGuid;
				
				for( int i0 = 0; i0 < targetGuids.Length; ++i0)
				{
					targetGuid = targetGuids[ i0];
					targetPath = AssetDatabase.GUIDToAssetPath( targetGuid);
					
					if( i0 % 3 == 2)
					{
						if( OnProgress( "Trace Precedents", targetPath, (float)i0 / (float)targetGuids.Length) != false)
						{
							OnFinish();
							return;
						}
					}
					if( founds.ContainsKey( targetPath) == false)
					{
						assetPaths = AssetDatabase.GetDependencies( targetPath, recursive);
						
						for( int i1 = 0; i1 < assetPaths.Length; ++i1)
						{
							tracePath = assetPaths[ i1];
							
							if( traceAssets.TryGetValue( tracePath, out traceGuid) != false)
							{
								if( traceGuid != targetGuid)
								{
									if( founds.ContainsKey( targetPath) == false)
									{
										founds.Add( targetPath, new ElementSource( targetPath));
									}
									traces[ tracePath].Reference++;
								}
							}
						}
					}
				}
			}
			OnProgress( "Trace Precedents", 1);
			OnFinish();
		}
		public static void TraceDependents( 
			IEnumerable<string> traceGuids, bool recursive,
			out Dictionary<string, ElementSource> traces,
			out Dictionary<string, ElementSource> founds)
		{
			int traceGuidCount = traceGuids.Count();
			float unitProgress = 1.0f / (float)traceGuidCount;
			float progress;
			string[] assetPaths;
			string targetPath;
			string targetGuid;
			string tracePath;
			int i0 = 0, i1;
			
			founds = new Dictionary<string, ElementSource>();
			traces = new Dictionary<string, ElementSource>();
			
			if( OnProgress( "Trace Dependents", 0) != false)
			{
				OnFinish();
				return;
			}
			foreach( string traceGuid in traceGuids)
			{
				tracePath = AssetDatabase.GUIDToAssetPath( traceGuid);
				
				if( string.IsNullOrEmpty( tracePath) == false
				&&	AssetDatabase.IsValidFolder( tracePath) == false)
				{
					if( traces.ContainsKey( tracePath) == false)
					{
						assetPaths = AssetDatabase.GetDependencies( tracePath, recursive);
						
						for( i1 = 0; i1 < assetPaths.Length; ++i1)
						{
							progress = i1 / (float)assetPaths.Length;
							progress /= (float)traceGuidCount;
							progress += i0 * unitProgress;
							targetPath = assetPaths[ i1];
							
							if( OnProgress( "Trace Dependents", targetPath, progress) != false)
							{
								OnFinish();
								return;
							}
							if( recursive != false)
							{
								if( founds.ContainsKey( targetPath) == false)
								{
									targetGuid = AssetDatabase.AssetPathToGUID( targetPath);
									if( traceGuid != targetGuid)
									{
										founds.Add( targetPath, new ElementSource( targetPath));
									}
								}
							}
							else
							{
								targetGuid = AssetDatabase.AssetPathToGUID( targetPath);
								
								if( traceGuid != targetGuid)
								{
									if( typeof( GameObject).Equals( 
										AssetDatabase.GetMainAssetTypeAtPath( targetPath)) == false)
									{
										if( founds.ContainsKey( targetPath) == false)
										{
											founds.Add( targetPath, new ElementSource( targetPath));
										}
									}
									else
									{
										if( founds.ContainsKey( targetPath) == false)
										{
											founds.Add( targetPath, new ElementSource( targetPath));
										}
									}
								}
							}
						}
						traces.Add( tracePath, new ElementSource( tracePath, assetPaths.Length));
					}
				}
				++i0;
			}
			OnProgress( "Trace Dependents", 1);
			OnFinish();
		}
	#if false
		static void Test( string targetPath)
		{
			var builder = new System.Text.StringBuilder();
			try
			{
				var assets = AssetDatabase.LoadAllAssetsAtPath( targetPath);
				
				foreach( var asset in assets)
				{
					LogSerializedObject( asset, builder, "  ");
				}
			}
			catch( System.Exception e)
			{
				Debug.LogError( e);
			}
			Debug.Log( builder.ToString());
		}
	#endif
	#if false
		static void Test( string targetPath)
		{
			IEnumerable<UnityEngine.Object> assets;
			SerializedObject serializedObject;
			SerializedProperty property;
			
			assets = AssetDatabase.LoadAllAssetsAtPath( targetPath);
			
			foreach( Object asset in assets)
			{
				serializedObject = new SerializedObject( asset);
				property = serializedObject.GetIterator();
			}
		}
	#endif
		public static void CheckMissing(
			IEnumerable<string> traceGuids,
			out Dictionary<string, ElementSource> traces,
			out Dictionary<string, ElementSource> founds)
		{
			IEnumerable<UnityEngine.Object> assets;
			SerializedObject serializedObject;
			SerializedProperty fileIdProperty;
			SerializedProperty property;
			System.Type assetType;
			string tracePath;
			int traceGuidCount = traceGuids.Count();
			int traceAssetCount;
			float unitProgress = 1.0f / (float)traceGuidCount;
			float progress;
			int i0 = 0, i1;
			
			founds = new Dictionary<string, ElementSource>();
			traces = new Dictionary<string, ElementSource>();
			
			if( OnProgress( "Check Missing", 0) != false)
			{
				OnFinish();
				return;
			}
			foreach( string traceGuid in traceGuids)
			{
				tracePath = AssetDatabase.GUIDToAssetPath( traceGuid);
				
				if( string.IsNullOrEmpty( tracePath) == false
				&&	AssetDatabase.IsValidFolder( tracePath) == false)
				{
					if( traces.ContainsKey( tracePath) == false)
					{
						assetType = AssetDatabase.GetMainAssetTypeAtPath( tracePath);
						if( typeof( SceneAsset).Equals( assetType) != false)
						{
						//	Debug.LogWarning( string.Format( $"Unimplemented.\n{tracePath}<{assetType}>"));
						}
						/* SceneAsset 以外をチェック */
						else if( typeof( Material).Equals( assetType) != false
						||		 typeof( GameObject).Equals( assetType) != false
						||		 typeof( AnimationClip).Equals( assetType) != false
						||		 typeof( LightingDataAsset).Equals( assetType) != false
						||		 typeof( ScriptableObject).IsAssignableFrom( assetType) != false
						)
						{
							assets = AssetDatabase.LoadAllAssetsAtPath( tracePath);
							traces.Add( tracePath, new ElementSource( tracePath, 0));
							
							traceAssetCount = assets.Count();
							i1 = 0;
							
							foreach( Object asset in assets)
							{
								progress = i1 / (float)traceAssetCount;
								progress /= (float)traceGuidCount;
								progress += i0 * unitProgress;
								
								if( OnProgress( "Check Missing", tracePath, progress) != false)
								{
									OnFinish();
									return;
								}
								if( asset == null)
								{
									continue;
								}
								serializedObject = new SerializedObject( asset);
								property = serializedObject.GetIterator();
								
								while( property.Next( true))
								{
									if( property.propertyType == SerializedPropertyType.ObjectReference
									&&	property.objectReferenceValue == null)
									{
										bool bMissing = false;
										
										if( property.objectReferenceInstanceIDValue != 0)
										{
											bMissing = true;
										}
										else
										{
											if( property.hasChildren != false)
											{
												fileIdProperty = property.FindPropertyRelative( "m_FileID");
												if( fileIdProperty != null && fileIdProperty.intValue != 0)
												{
													bMissing = true;
												}
											}
										}
										if( bMissing != false)
										{
											SerializedProperty gameObjectProperty;
											GameObject gameObject = null;
											string hierarchyPath = string.Empty;
											string componentPath;
											string foundKeyPath;
											string assetGuid = string.Empty;
											long assetLocalId = 0;
											int intstanceId = 0;
											int tryCount = 0;
											
											if( asset is Component component)
											{
												var transform = component.transform as Transform;
												
												if( transform != null)
												{
													intstanceId = transform.GetInstanceID();
												}
												while( transform != null)
												{
													hierarchyPath = (hierarchyPath.Length == 0)? 
														transform.name : (transform.name + "/" + hierarchyPath);
													transform = transform.parent;
												}
											}
											componentPath = string.Format( 
												$"{tracePath}/{hierarchyPath.Replace( "/" , "|")}:{property.propertyPath.Replace( "/" , "#")}");
											
											if( property.type == "UnityEngine.GameObject")
											{
												gameObjectProperty = property;
											}
											else
											{
												gameObjectProperty = serializedObject.FindProperty( "m_GameObject");
											}
											if( gameObjectProperty != null)
											{
												gameObject = gameObjectProperty.objectReferenceValue as GameObject;
												if( gameObject != null)
												{
													AssetDatabase.TryGetGUIDAndLocalFileIdentifier( 
														gameObject.transform, out assetGuid, out assetLocalId);
												}
											}
											do
											{
												foundKeyPath = string.Format( $"{componentPath}<{assetLocalId}>#{tryCount}");
													
												if( founds.ContainsKey( foundKeyPath) == false)
												{
													string name = string.Format( $"{hierarchyPath}<{asset.GetType()}> ${property.propertyPath}");
													founds.Add( foundKeyPath, new ElementComponentSource( 
														name, asset.GetType(), hierarchyPath, assetLocalId, foundKeyPath, 0));
													break;
												}
												++tryCount;
											}
											while( true);
											
											traces[ tracePath].Reference++;
										}
									}
								}
								++i1;
							}
						}
						else
						{
						//	Debug.LogWarning( string.Format( $"Treated as exempt.\n{tracePath}<{assetType}>"));
						}
					}
				}
				++i0;
			}
			OnProgress( "Check Missing", 1);
			OnFinish();
		}
	#if false
		static GetProperty( Object targetAsset, string propertyName)
		{
			var serializedObject = new SerializedObject( targetAsset);
			SerializedProperty property = serializedObject.GetIterator();
			
			while( property.Next( true))
			{
				if( property.name == propertyName)
				{
					break;
				}
			}
			
		}
	#endif
		static bool OnProgress( string caption, float progress)
		{
			return EditorUtility.DisplayCancelableProgressBar( caption, string.Empty, progress);
		}
		static bool OnProgress( string caption, string message, float progress)
		{
			return EditorUtility.DisplayCancelableProgressBar( caption, message, progress);
		}
		static void OnFinish()
		{
			EditorUtility.ClearProgressBar();
		}
	#if false
		[MenuItem("Tools/Log Serialized Assets")]
		private static void LogSerializedAssets2()
		{
			var builder = new System.Text.StringBuilder();
			try
			{
				LogSerializedObject( Selection.activeObject, builder, "  ");
				
				if( Selection.activeObject is GameObject gameObject)
				{
					LogSerializedObject( gameObject.transform, builder, "  ");
				}
			}
			catch( System.Exception e)
			{
				Debug.LogError( e);
			}
			Debug.Log( builder.ToString());
		}
		[MenuItem("Assets/Log Serialized Assets")]
		private static void LogSerializedAssets()
		{
			var builder = new System.Text.StringBuilder();
			try
			{
				builder.AppendLine( "Show Serialized");
				
				var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
				var assets = AssetDatabase.LoadAllAssetsAtPath( assetPath);
				
				foreach( var asset in assets)
				{
					LogSerializedObject( asset, builder, "  ");
				}
			}
			catch( System.Exception e)
			{
				Debug.LogError( e);
			}
			Debug.Log( builder.ToString());
		}
		static void LogSerializedObject( Object asset, System.Text.StringBuilder builder, string indent)
		{
			if( asset != null)
			{
				string guid = string.Empty;
				long localId = 0, localIdInFile = 0;
				
				SerializedObject serializedObject = new SerializedObject( asset);
				PropertyInfo cachedInspectorModeInfo = typeof( SerializedObject).GetProperty( "inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
				cachedInspectorModeInfo.SetValue( serializedObject, InspectorMode.Debug, null);
				SerializedProperty serializedProperty = serializedObject.FindProperty( "m_LocalIdentfierInFile");
				if( serializedProperty != null)
				{
					localIdInFile = serializedProperty.longValue;
				}
				SerializedProperty property = serializedObject.GetIterator();
				
				if( AssetDatabase.TryGetGUIDAndLocalFileIdentifier( asset, out guid, out localId) == false)
				{
					builder.AppendFormat( $"{asset.name}<{asset.GetType()}> localIdInFile={localIdInFile}\n");
				}
				else
				{
					builder.AppendFormat( $"{asset.name}<{asset.GetType()}> guid={guid}, localId={localId}, localIdInFile={localIdInFile}\n");
				}
				while( property.Next( true) != false)
				{
					LogSerializedProperty( property, builder, indent);
				}
				builder.AppendLine( string.Empty);
			}
		}
		static void LogSerializedProperty( SerializedProperty property, System.Text.StringBuilder builder, string indent)
		{
			builder.AppendFormat( $"{indent}{property.propertyType} {property.name} = ");
			
			switch( property.propertyType)
			{
				case SerializedPropertyType.Generic:
				{
					builder.AppendLine( string.Empty);
					
					if( property.isArray == false)
					{
						var child = property.Copy();
						var end = property.GetEndProperty( true);
						if( child.Next( true) != false)
						{
							while( SerializedProperty.EqualContents( child, end) == false)
							{
								LogSerializedProperty( child, builder, indent + "  ");
								if( child.Next( true) == false)
								{
									break;
								}
							}
						}
					}
					else
					{
						for( int i0 = 0; i0 < property.arraySize; ++i0)
						{
							LogSerializedProperty( property.GetArrayElementAtIndex( i0), builder, indent + "    ");
						}
					}
					break;
				}
				case SerializedPropertyType.ObjectReference:
				{
					if( property.objectReferenceValue != null)
					{
						Object asset = property.objectReferenceValue;
						string guid = string.Empty;
						long localId = 0, localIdInFile = 0;
						
						SerializedObject serializedObject = new SerializedObject( property.objectReferenceValue);
						PropertyInfo cachedInspectorModeInfo = typeof( SerializedObject).GetProperty( "inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
						cachedInspectorModeInfo.SetValue( serializedObject, InspectorMode.Debug, null);
						SerializedProperty serializedProperty = serializedObject.FindProperty( "m_LocalIdentfierInFile");
						if( serializedProperty != null)
						{
							localIdInFile = serializedProperty.longValue;
						}
						if( AssetDatabase.TryGetGUIDAndLocalFileIdentifier( asset, out guid, out localId) == false)
						{
							builder.AppendFormat( $"{asset.name}<{asset.GetType()}> localIdInFile={localIdInFile}\n");
						}
						else
						{
							builder.AppendFormat( $"{asset.name}<{asset.GetType()}> guid={guid}, localId={localId}, localIdInFile={localIdInFile}\n");
						}
					}
					else
					{
						builder.AppendLine();
					}
					var child = property.Copy();
					var end = property.GetEndProperty( true);
					if( child.Next( true) != false)
					{
						while( SerializedProperty.EqualContents( child, end) == false)
						{
							LogSerializedProperty( child, builder, indent + "  ");
							if( child.Next( true) == false)
							{
								break;
							}
						}
					}
					break;
				}
				case SerializedPropertyType.Integer:
				case SerializedPropertyType.LayerMask:
				case SerializedPropertyType.ArraySize:
				{
					builder.AppendFormat( $"{property.intValue}\n");
					break;
				}
				case SerializedPropertyType.Boolean:
				{
					builder.AppendFormat( $"{property.boolValue}\n");
					break;
				}
				case SerializedPropertyType.Float:
				{
					builder.AppendFormat( $"{property.floatValue}\n");
					break;
				}
				case SerializedPropertyType.String:
				{
					builder.AppendFormat( $"{property.stringValue}\n");
					break;
				}
				case SerializedPropertyType.Color:
				{
					builder.AppendFormat( $"{property.colorValue}\n");
					break;
				}
				case SerializedPropertyType.Enum:
				{
					string enumValueName = string.Empty;
					
					if( property.enumNames != null)
					{
						if( property.enumValueIndex >= 0 && property.enumValueIndex < property.enumNames.Length)
						{
							enumValueName = property.enumNames[ property.enumValueIndex];
						}
					}
					builder.AppendFormat( $"{enumValueName}\n");
					break;
				}
				case SerializedPropertyType.Vector2:
				{
					builder.AppendFormat( $"{property.vector2Value}\n");
					break;
				}
				case SerializedPropertyType.Vector3:
				{
					builder.AppendFormat( $"{property.vector3Value}\n");
					break;
				}
				case SerializedPropertyType.Vector4:
				{
					builder.AppendFormat( $"{property.vector4Value}\n");
					break;
				}
				case SerializedPropertyType.Rect:
				{
					builder.AppendFormat( $"{property.rectValue}\n");
					break;
				}
				case SerializedPropertyType.Character:
				case SerializedPropertyType.AnimationCurve:
				case SerializedPropertyType.Gradient:
				{
					builder.AppendFormat( $"<Not compatible>\n");
					break;
				}
				case SerializedPropertyType.Bounds:
				{
					builder.AppendFormat( $"{property.boundsValue}\n");
					break;
				}
				case SerializedPropertyType.Quaternion:
				{
					builder.AppendFormat( $"{property.quaternionValue}\n");
					break;
				}
				default:
				{
					builder.AppendFormat( $"<Unexpected>\n");
					break;
				}
			}
		}
	#endif
	}
}
