
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

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
							progress /= traceGuidCount;
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
						
						if( CheckMissing( tracePath, traces, i0 * unitProgress) == false)
						{
							OnFinish();
							return;
						}
					}
				}
				++i0;
			}
			OnProgress( "Trace Dependents", 1);
			OnFinish();
		}
		static bool CheckMissing( string tracePath, Dictionary<string, ElementSource> traces, float progress)
		{
			if( string.IsNullOrEmpty( tracePath) == false && AssetDatabase.IsValidFolder( tracePath) == false)
			{
				System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath( tracePath);
				
				if( typeof( Material).Equals( assetType) != false
				||	typeof( GameObject).Equals( assetType) != false
				||	typeof( AnimationClip).Equals( assetType) != false
				||	typeof( LightingDataAsset).Equals( assetType) != false
				||	typeof( ScriptableObject).IsAssignableFrom( assetType) != false)
				{
					Object[] assets = AssetDatabase.LoadAllAssetsAtPath( tracePath);
					
					for( int i0 = 0; i0 < assets.Length; ++i0)
					{
						if( OnProgress( "Check Missing", tracePath, progress) != false)
						{
							OnFinish();
							return false;
						}
						Object asset = assets[ i0];
						
						if( asset == null)
						{
							continue;
						}
						var serializedObject = new SerializedObject( asset);
						SerializedProperty property = serializedObject.GetIterator();
						
						var displayDirectory = new Stack<string>();
						string currentDisplayName = string.Empty;
						
						while( property.Next( true) != false)
						{
							if( property.propertyPath.EndsWith( ".Array") == false
							&&	property.propertyPath.EndsWith( ".Array.size") == false)
							{
								if( displayDirectory.Count < property.depth)
								{
									displayDirectory.Push( currentDisplayName);
									currentDisplayName = property.displayName;
								}
								else if( displayDirectory.Count > property.depth)
								{
									while( displayDirectory.Count > property.depth)
									{
										displayDirectory.Pop();
										currentDisplayName = property.displayName;
									}
								}
								else
								{
									currentDisplayName = property.displayName;
								}
							}
							if( property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue == null)
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
										SerializedProperty fileIdProperty = property.FindPropertyRelative( "m_FileID");
										
										if( fileIdProperty != null && fileIdProperty.intValue != 0)
										{
											bMissing = true;
										}
									}
								}
								if( bMissing != false)
								{
									string hierarchyPath = string.Empty;
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
									string componentPath = string.Format( 
										$"{tracePath}/{asset.GetType()} {property.propertyPath.Replace( "/" , "#")}");
									SerializedProperty gameObjectProperty = (property.type == "UnityEngine.GameObject")? 
										property : serializedObject.FindProperty( "m_GameObject");
									
									if( gameObjectProperty?.objectReferenceValue is GameObject gameObject)
									{
										AssetDatabase.TryGetGUIDAndLocalFileIdentifier( 
											gameObject.transform, out string assetGuid, out assetLocalId);
									}
									do
									{
										string foundKeyPath = string.Format( $"{componentPath}<{assetLocalId}>#{tryCount}");
											
										if( traces.ContainsKey( foundKeyPath) == false)
										{
											string displayPath = string.Join( '/', displayDirectory.Reverse());
											displayPath = Path.Combine( displayPath, currentDisplayName).Replace( @"\", "/");
											string name = string.Format( $"<{asset.GetType().Name}> {displayPath}");
											traces.Add( foundKeyPath, new ElementComponentSource( 
												name, asset.GetType(), hierarchyPath, assetLocalId, foundKeyPath, -1));
											break;
										}
										++tryCount;
									}
									while( true);
									
									traces[ tracePath].Missing++;
								}
							}
						}
					}
				}
			}
			return true;
		}
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
