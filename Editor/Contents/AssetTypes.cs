
using System.Collections.Generic;

namespace Finder
{
	public enum AssetType
	{
		kUnknown,
		kDirectory,
		kComponent,
		kAnimationController,
		kAnimationClip,
		kAudioClip,
		kAudioMixer,
		kComputeShader,
	//	kCubemap,
		kFont,
		kGUISkin,
		kMaterial,
		kModel,
		kPhysicMaterial,
		kPrefab,
		kScene,
		kScript,
		kScriptableObject,
		kShader,
		kTextAsset,
		kTexture,
		kVideoClip,
	}
	/* AssetDatabase.GetMainAssetTypeAtPath() 
	 * を使った型判断だと読み込みが発生してしまうため
	 * 暫定として拡張子から型を判断する
	 */
	public sealed class AssetTypes
	{
		public static readonly string[] kTypeNames = new string[]
		{
			"AnimationController",
			"AnimationClip",
			"AudioClip",
			"AudioMixer",
			"ComputeShader",
			"Font",
			"GUISkin",
			"Material",
			"Model",
			"PhysicMaterial",
			"Prefab",
			"Scene",
			"Script",
			"ScriptableObject",
			"Shader",
			"TextAsset",
			"Texture",
			"VideoClip",
		};
		public static readonly Dictionary<string, AssetType> kFilters =
			new Dictionary<string, AssetType>( System.StringComparer.OrdinalIgnoreCase)
		{
			{ "t:AnimationController", AssetType.kAnimationController },
			{ "t:Animator", AssetType.kAnimationController },
			
			{ "t:AnimationClip", AssetType.kAnimationClip },
			{ "t:Animation", AssetType.kAnimationClip },
			
			{ "t:AudioClip", AssetType.kAudioClip },
			{ "t:Audio", AssetType.kAudioClip },
			
			{ "t:AudioMixer", AssetType.kAudioMixer },
			{ "t:Mixer", AssetType.kAudioMixer },
			
			{ "t:ComputeShader", AssetType.kComputeShader },
			
			{ "t:Font", AssetType.kFont },
			
			{ "t:GUISkin", AssetType.kGUISkin },
			
			{ "t:Material", AssetType.kMaterial },
			
			{ "t:Model", AssetType.kModel },
		//	{ "t:Mesh", AssetType.kModel },
			
			{ "t:PhysicMaterial", AssetType.kPhysicMaterial },
			
			{ "t:Prefab", AssetType.kPrefab },
			
			{ "t:Scene", AssetType.kScene },
			
			{ "t:Script", AssetType.kScript },
			
			{ "t:ScriptableObject", AssetType.kScriptableObject },
			{ "t:Asset", AssetType.kScriptableObject },
			
			{ "t:Shader", AssetType.kShader },
			
			{ "t:TextAsset", AssetType.kTextAsset },
			{ "t:Text", AssetType.kTextAsset },
			
			{ "t:Texture", AssetType.kTexture },
			
			{ "t:VideoClip", AssetType.kVideoClip },
			{ "t:Video", AssetType.kVideoClip },
		};
		public static readonly Dictionary<string, AssetType> kExtensions =
			new Dictionary<string, AssetType>( System.StringComparer.OrdinalIgnoreCase)
		{
			/* kAnimationController */
			{ ".controller", AssetType.kAnimationController },
			
			/* kAnimationClip */
			{ ".anim", AssetType.kAnimationClip },
			
			/* kAudioClip */
			{ ".wav", AssetType.kAudioClip },
			{ ".mp3", AssetType.kAudioClip },
			{ ".ogg", AssetType.kAudioClip },
			{ ".aif", AssetType.kAudioClip },
			{ ".aiff", AssetType.kAudioClip },
			{ ".xm", AssetType.kAudioClip },
			{ ".mod", AssetType.kAudioClip },
			{ ".it", AssetType.kAudioClip },
			{ ".s3m", AssetType.kAudioClip },
			
			/* kAudioMixer */
			{ ".mixer", AssetType.kAudioMixer },
			
			/* kComputeShader */
			{ ".compute", AssetType.kComputeShader },
			
			/* kCubemap */
	//		{ ".hdr", AssetType.kCubemap },
	//		{ ".cubemap", AssetType.kCubemap },
			
			/* kFont */
			{ ".ttf", AssetType.kFont },
			{ ".otf", AssetType.kFont },
			{ ".dfont", AssetType.kFont },
			
			/* kGUISkin */
			{ ".guiskin", AssetType.kGUISkin },
			
			/* kMaterial */
			{ ".mat", AssetType.kMaterial },
			{ ".material", AssetType.kMaterial },
			
			/* kModel */
			{ ".3ds", AssetType.kModel },
			{ ".blend", AssetType.kModel },
			{ ".blender", AssetType.kModel },
			{ ".c3d", AssetType.kModel },
			{ ".c4d", AssetType.kModel },
			{ ".dae", AssetType.kModel },
			{ ".dfx", AssetType.kModel },
			{ ".fbx", AssetType.kModel },
			{ ".obj", AssetType.kModel },
			{ ".ma", AssetType.kModel },
			{ ".mb", AssetType.kModel },
			{ ".max", AssetType.kModel },
			{ ".lxo", AssetType.kModel },
			{ ".lwo", AssetType.kModel },
			{ ".jas", AssetType.kModel },
			{ ".skp", AssetType.kModel },
			
			/* kPhysicMaterial */
			{ ".physicMaterial", AssetType.kPhysicMaterial },
			{ ".physicsMaterial2D", AssetType.kPhysicMaterial },
			
			/* kPrefab */
			{ ".prefab", AssetType.kPrefab },
			
			/* kScene */
			{ ".unity", AssetType.kScene },
			
			/* kScript */
			{ ".cs", AssetType.kScript },
			{ ".js", AssetType.kScript },
			
			/* kScriptableObject */
			{ ".asset", AssetType.kScriptableObject },
			
			/* kShader */
			{ ".shader", AssetType.kShader },
			
			/* kTextAsset */
			{ ".txt", AssetType.kTextAsset },
			{ ".html", AssetType.kTextAsset },
			{ ".htm", AssetType.kTextAsset },
			{ ".xml", AssetType.kTextAsset },
			{ ".bytes", AssetType.kTextAsset },
			{ ".json", AssetType.kTextAsset },
			{ ".csv", AssetType.kTextAsset },
			{ ".yaml", AssetType.kTextAsset },
			{ ".fnt", AssetType.kTextAsset },
			
			/* kTexture */
			{ ".jpg", AssetType.kTexture },
			{ ".jpeg", AssetType.kTexture },
			{ ".tif", AssetType.kTexture },
			{ ".tiff", AssetType.kTexture },
			{ ".tga", AssetType.kTexture },
			{ ".gif", AssetType.kTexture },
			{ ".png", AssetType.kTexture },
			{ ".psd", AssetType.kTexture },
			{ ".bmp", AssetType.kTexture },
			{ ".iff", AssetType.kTexture },
			{ ".pict", AssetType.kTexture },
			{ ".pic", AssetType.kTexture },
			{ ".pct", AssetType.kTexture },
			{ ".exr", AssetType.kTexture },
			{ ".hdr", AssetType.kTexture },
			{ ".cubemap", AssetType.kTexture },
			
			/* kVideoClip */
			{ ".mov", AssetType.kVideoClip },
			{ ".mpg", AssetType.kVideoClip },
			{ ".mpeg", AssetType.kVideoClip },
			{ ".mp4", AssetType.kVideoClip },
			{ ".avi", AssetType.kVideoClip },
			{ ".asf", AssetType.kVideoClip },
		};
	}
}
