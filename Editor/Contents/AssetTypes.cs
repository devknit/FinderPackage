
using System.Collections.Generic;

namespace Finder
{
	internal enum AssetType
	{
		Unknown,
		Directory,
		Component,
		AnimationController,
		AnimationClip,
		AudioClip,
		AudioMixer,
		ComputeShader,
	//	Cubemap,
		Font,
		GUISkin,
		Material,
		Model,
		PhysicMaterial,
		Prefab,
		Scene,
		Script,
		ScriptableObject,
		Shader,
		TextAsset,
		Texture,
		VideoClip,
	}
	/* AssetDatabase.GetMainAssetTypeAtPath() 
	 * を使った型判断だと読み込みが発生してしまうため
	 * 暫定として拡張子から型を判断する
	 */
	internal sealed class AssetTypes
	{
		internal static readonly string[] kTypeNames = new string[]
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
		internal static readonly Dictionary<string, AssetType> kFilters = new( System.StringComparer.OrdinalIgnoreCase)
		{
			{ "t:AnimationController", AssetType.AnimationController },
			{ "t:Animator", AssetType.AnimationController },
			
			{ "t:AnimationClip", AssetType.AnimationClip },
			{ "t:Animation", AssetType.AnimationClip },
			
			{ "t:AudioClip", AssetType.AudioClip },
			{ "t:Audio", AssetType.AudioClip },
			
			{ "t:AudioMixer", AssetType.AudioMixer },
			{ "t:Mixer", AssetType.AudioMixer },
			
			{ "t:ComputeShader", AssetType.ComputeShader },
			
			{ "t:Font", AssetType.Font },
			
			{ "t:GUISkin", AssetType.GUISkin },
			
			{ "t:Material", AssetType.Material },
			
			{ "t:Model", AssetType.Model },
		//	{ "t:Mesh", AssetType.Model },
			
			{ "t:PhysicMaterial", AssetType.PhysicMaterial },
			
			{ "t:Prefab", AssetType.Prefab },
			
			{ "t:Scene", AssetType.Scene },
			
			{ "t:Script", AssetType.Script },
			
			{ "t:ScriptableObject", AssetType.ScriptableObject },
			{ "t:Asset", AssetType.ScriptableObject },
			
			{ "t:Shader", AssetType.Shader },
			
			{ "t:TextAsset", AssetType.TextAsset },
			{ "t:Text", AssetType.TextAsset },
			
			{ "t:Texture", AssetType.Texture },
			
			{ "t:VideoClip", AssetType.VideoClip },
			{ "t:Video", AssetType.VideoClip },
		};
		public static readonly Dictionary<string, AssetType> kExtensions =
			new Dictionary<string, AssetType>( System.StringComparer.OrdinalIgnoreCase)
		{
			/* kAnimationController */
			{ ".controller", AssetType.AnimationController },
			
			/* kAnimationClip */
			{ ".anim", AssetType.AnimationClip },
			
			/* kAudioClip */
			{ ".wav", AssetType.AudioClip },
			{ ".mp3", AssetType.AudioClip },
			{ ".ogg", AssetType.AudioClip },
			{ ".aif", AssetType.AudioClip },
			{ ".aiff", AssetType.AudioClip },
			{ ".xm", AssetType.AudioClip },
			{ ".mod", AssetType.AudioClip },
			{ ".it", AssetType.AudioClip },
			{ ".s3m", AssetType.AudioClip },
			
			/* kAudioMixer */
			{ ".mixer", AssetType.AudioMixer },
			
			/* kComputeShader */
			{ ".compute", AssetType.ComputeShader },
			
			/* kCubemap */
	//		{ ".hdr", AssetType.Cubemap },
	//		{ ".cubemap", AssetType.Cubemap },
			
			/* kFont */
			{ ".ttf", AssetType.Font },
			{ ".otf", AssetType.Font },
			{ ".dfont", AssetType.Font },
			
			/* kGUISkin */
			{ ".guiskin", AssetType.GUISkin },
			
			/* kMaterial */
			{ ".mat", AssetType.Material },
			{ ".material", AssetType.Material },
			
			/* kModel */
			{ ".3ds", AssetType.Model },
			{ ".blend", AssetType.Model },
			{ ".blender", AssetType.Model },
			{ ".c3d", AssetType.Model },
			{ ".c4d", AssetType.Model },
			{ ".dae", AssetType.Model },
			{ ".dfx", AssetType.Model },
			{ ".fbx", AssetType.Model },
			{ ".obj", AssetType.Model },
			{ ".ma", AssetType.Model },
			{ ".mb", AssetType.Model },
			{ ".max", AssetType.Model },
			{ ".lxo", AssetType.Model },
			{ ".lwo", AssetType.Model },
			{ ".jas", AssetType.Model },
			{ ".skp", AssetType.Model },
			
			/* kPhysicMaterial */
			{ ".physicMaterial", AssetType.PhysicMaterial },
			{ ".physicsMaterial2D", AssetType.PhysicMaterial },
			
			/* kPrefab */
			{ ".prefab", AssetType.Prefab },
			
			/* kScene */
			{ ".unity", AssetType.Scene },
			
			/* kScript */
			{ ".cs", AssetType.Script },
			{ ".js", AssetType.Script },
			
			/* kScriptableObject */
			{ ".asset", AssetType.ScriptableObject },
			
			/* kShader */
			{ ".shader", AssetType.Shader },
			
			/* kTextAsset */
			{ ".txt", AssetType.TextAsset },
			{ ".html", AssetType.TextAsset },
			{ ".htm", AssetType.TextAsset },
			{ ".xml", AssetType.TextAsset },
			{ ".bytes", AssetType.TextAsset },
			{ ".json", AssetType.TextAsset },
			{ ".csv", AssetType.TextAsset },
			{ ".yaml", AssetType.TextAsset },
			{ ".fnt", AssetType.TextAsset },
			
			/* kTexture */
			{ ".jpg", AssetType.Texture },
			{ ".jpeg", AssetType.Texture },
			{ ".tif", AssetType.Texture },
			{ ".tiff", AssetType.Texture },
			{ ".tga", AssetType.Texture },
			{ ".gif", AssetType.Texture },
			{ ".png", AssetType.Texture },
			{ ".psd", AssetType.Texture },
			{ ".bmp", AssetType.Texture },
			{ ".iff", AssetType.Texture },
			{ ".pict", AssetType.Texture },
			{ ".pic", AssetType.Texture },
			{ ".pct", AssetType.Texture },
			{ ".exr", AssetType.Texture },
			{ ".hdr", AssetType.Texture },
			{ ".cubemap", AssetType.Texture },
			
			/* kVideoClip */
			{ ".mov", AssetType.VideoClip },
			{ ".mpg", AssetType.VideoClip },
			{ ".mpeg", AssetType.VideoClip },
			{ ".mp4", AssetType.VideoClip },
			{ ".avi", AssetType.VideoClip },
			{ ".asf", AssetType.VideoClip },
		};
	}
}
