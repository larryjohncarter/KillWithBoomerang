using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class SingleColorTextureDetector : EditorWindow, IHasCustomMenu
{
	[Serializable]
	private class SaveData
	{
		public List<string> paths = new List<string>();
	}

	private const string LOW_RES_DUMMY_TEXTURE_PATH = "Assets/low_dummyy_texturee.png";
	private const string HIGH_RES_DUMMY_TEXTURE_PATH = "Assets/high_dummyy_texturee.png";
	private const string SAVE_FILE_PATH = "Library/SingleColorAssetDetector.json";
	private const float BUTTON_DRAG_THRESHOLD_SQR = 600f;

	private readonly MethodInfo instanceIDFromGUID = typeof( AssetDatabase ).GetMethod( "GetInstanceIDFromGUID", BindingFlags.NonPublic | BindingFlags.Static );

	private List<string> results = new List<string>(); // This is not readonly so that it can be serialized

	private double lastClickTime;
	private string lastClickedPath;

	private readonly GUIContent buttonGUIContent = new GUIContent();
	private Vector2 buttonPressPosition;
	private Vector2 scrollPos;

	[MenuItem( "Window/Single Color Texture Detector" )]
	private static void Init()
	{
		SingleColorTextureDetector window = GetWindow<SingleColorTextureDetector>();
		window.titleContent = new GUIContent( "Solid Textures" );
		window.minSize = new Vector2( 200f, 150f );
		window.Show();
	}

	private void Awake()
	{
		LoadSession( null );
	}

	// Show additional options in the window's context menu
	public void AddItemsToMenu( GenericMenu menu )
	{
		if( results.Count > 0 )
			menu.AddItem( new GUIContent( "Save To Clipboard" ), false, () => GUIUtility.systemCopyBuffer = JsonUtility.ToJson( new SaveData() { paths = results }, true ) );
		else
			menu.AddDisabledItem( new GUIContent( "Save To Clipboard" ) );

		if( string.IsNullOrEmpty( GUIUtility.systemCopyBuffer ) )
			menu.AddDisabledItem( new GUIContent( "Load From Clipboard" ) );
		else
		{
			menu.AddItem( new GUIContent( "Load From Clipboard" ), false, () =>
			{
				string json = GUIUtility.systemCopyBuffer;
				LoadSession( json );
				SaveSession( json ); // If load succeeds, overwrite the saved session
			} );
		}
	}

	private void OnGUI()
	{
		Event ev = Event.current;
		scrollPos = GUILayout.BeginScrollView( scrollPos );

		// Calculate single color Textures
		if( GUILayout.Button( "Refresh" ) )
		{
			try
			{
				double startTime = EditorApplication.timeSinceStartup;

				CalculateSingleColorTextures();
				SaveSession( null );

				Debug.Log( "Refreshed in " + ( EditorApplication.timeSinceStartup - startTime ) + " seconds." );
			}
			catch( Exception e )
			{
				Debug.LogException( e );
			}
			finally
			{
				EditorUtility.ClearProgressBar();

				if( File.Exists( LOW_RES_DUMMY_TEXTURE_PATH ) )
					AssetDatabase.DeleteAsset( LOW_RES_DUMMY_TEXTURE_PATH );
				if( File.Exists( HIGH_RES_DUMMY_TEXTURE_PATH ) )
					AssetDatabase.DeleteAsset( HIGH_RES_DUMMY_TEXTURE_PATH );
			}

			GUIUtility.ExitGUI();
		}

		// Draw found results
		if( results.Count > 0 )
		{
			EditorGUILayout.HelpBox( "- Double click a path to select the Texture asset\n- Right/middle click a path to hide it from the list", MessageType.Info );

			for( int i = 0; i < results.Count; i++ )
			{
				Rect rect = EditorGUILayout.GetControlRect( false, EditorGUIUtility.singleLineHeight + 2f );
				rect.xMin += 3f;
				rect.xMax -= 3f;

				Rect iconRect = new Rect( rect.x, rect.y, rect.height, rect.height );
				rect.xMin += iconRect.width + 2f;

				Texture icon = AssetDatabase.GetCachedIcon( results[i] );
				if( icon )
					EditorGUI.DrawTextureTransparent( iconRect, icon );

				// Buttons must support 1) click and 2) drag & drop. The most reliable way is to simulate GUI.Button from scratch
				buttonGUIContent.text = results[i];
				int buttonControlID = GUIUtility.GetControlID( FocusType.Passive );
				switch( ev.GetTypeForControl( buttonControlID ) )
				{
					case EventType.MouseDown:
						if( rect.Contains( ev.mousePosition ) )
						{
							GUIUtility.hotControl = buttonControlID;
							buttonPressPosition = ev.mousePosition;
						}

						break;
					case EventType.MouseDrag:
						if( GUIUtility.hotControl == buttonControlID && ev.button == 0 && ( ev.mousePosition - buttonPressPosition ).sqrMagnitude >= BUTTON_DRAG_THRESHOLD_SQR )
						{
							GUIUtility.hotControl = 0;

							Object asset = AssetDatabase.LoadMainAssetAtPath( results[i] );
							if( asset )
							{
								// Credit: https://forum.unity.com/threads/editor-draganddrop-bug-system-needs-to-be-initialized-by-unity.219342/#post-1464056
								DragAndDrop.PrepareStartDrag();
								DragAndDrop.objectReferences = new Object[] { asset };
								DragAndDrop.StartDrag( "DuplicateAssetDetector" );
							}

							ev.Use();
						}

						break;
					case EventType.MouseUp:
						if( GUIUtility.hotControl == buttonControlID )
						{
							GUIUtility.hotControl = 0;

							if( rect.Contains( ev.mousePosition ) )
							{
								if( ev.button == 0 && File.Exists( results[i] ) )
								{
									// Ping clicked Texture
									double clickTime = EditorApplication.timeSinceStartup;
									if( clickTime - lastClickTime < 0.5f && lastClickedPath == results[i] )
									{
										if( !ev.control && !ev.shift )
											Selection.objects = new Object[] { AssetDatabase.LoadMainAssetAtPath( results[i] ) };
										else
										{
											// While holding CTRL, either add clicked asset to current selection or remove it from current selection
											Object asset = AssetDatabase.LoadMainAssetAtPath( results[i] );
											List<Object> selection = new List<Object>( Selection.objects );
											if( !selection.Remove( asset ) )
												selection.Add( asset );

											Selection.objects = selection.ToArray();
										}
									}
									else if( instanceIDFromGUID != null )
										EditorGUIUtility.PingObject( (int) instanceIDFromGUID.Invoke( null, new object[] { AssetDatabase.AssetPathToGUID( results[i] ) } ) );
									else
										EditorGUIUtility.PingObject( AssetDatabase.LoadMainAssetAtPath( results[i] ) );

									lastClickTime = clickTime;
									lastClickedPath = results[i];
								}
								else if( ev.button == 1 )
								{
									// Show an option to hide that Texture from the list
									int _i = i;
									GenericMenu menu = new GenericMenu();
									menu.AddItem( new GUIContent( "Hide" ), false, () => HideTexture( _i ) );
									menu.ShowAsContext();
								}
								else if( ev.button == 2 )
									HideTexture( i );
							}
						}
						break;
					case EventType.Repaint:
						EditorStyles.textField.Draw( rect, buttonGUIContent, buttonControlID );
						break;
				}

				if( ev.isMouse && GUIUtility.hotControl == buttonControlID )
					ev.Use();
			}

			GUILayout.Space( 1f );
		}

		GUILayout.EndScrollView();
	}

	private void CalculateSingleColorTextures()
	{
		// Dummy Texture is used to read Textures' pixels
		CreateDummyTexture( LOW_RES_DUMMY_TEXTURE_PATH, 32 );
		CreateDummyTexture( HIGH_RES_DUMMY_TEXTURE_PATH, 1024 );

		results.Clear();

		string[] textureGUIDs = AssetDatabase.FindAssets( "t:Texture" );
		if( textureGUIDs.Length == 0 )
			return;

		string pathsLengthStr = "/" + textureGUIDs.Length.ToString();
		float progressMultiplier = 1f / textureGUIDs.Length;

		for( int i = 0; i < textureGUIDs.Length; i++ )
		{
			if( i % 15 == 0 && EditorUtility.DisplayCancelableProgressBar( "Please wait...", string.Concat( "Searching: ", ( i + 1 ).ToString(), pathsLengthStr ), ( i + 1 ) * progressMultiplier ) )
				throw new Exception( "Search aborted" );

			if( string.IsNullOrEmpty( textureGUIDs[i] ) )
				continue;

			string path = AssetDatabase.GUIDToAssetPath( textureGUIDs[i] );
			if( string.IsNullOrEmpty( path ) || !path.StartsWith( "Assets/" ) || path == LOW_RES_DUMMY_TEXTURE_PATH || path == HIGH_RES_DUMMY_TEXTURE_PATH )
				continue;

			// Happens for Font Assets' Textures, for example
			if( !typeof( Texture ).IsAssignableFrom( AssetDatabase.GetMainAssetTypeAtPath( path ) ) )
				continue;

			// First downscale the Texture to 32 pixels for performance reasons, then downscale it to 1024 pixels to verify the result
			if( CheckTextureAtPath( path, LOW_RES_DUMMY_TEXTURE_PATH ) && CheckTextureAtPath( path, HIGH_RES_DUMMY_TEXTURE_PATH ) )
				results.Add( path );
		}
	}

	// Creates dummy Texture asset that will be used to read Textures' pixels
	private void CreateDummyTexture( string path, int maxSize )
	{
		if( !File.Exists( path ) )
		{
			File.WriteAllBytes( path, new Texture2D( 2, 2 ).EncodeToPNG() );
			AssetDatabase.ImportAsset( path, ImportAssetOptions.ForceUpdate );
		}

		TextureImporter textureImporter = AssetImporter.GetAtPath( path ) as TextureImporter;
		textureImporter.maxTextureSize = maxSize;
		textureImporter.isReadable = true;
		textureImporter.filterMode = FilterMode.Point;
		textureImporter.mipmapEnabled = false;
		textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
		textureImporter.alphaIsTransparency = true;
		textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
		textureImporter.SaveAndReimport();
	}

	// Checks if downsized Texture's pixels are all same
	private bool CheckTextureAtPath( string texturePath, string dummyTexturePath )
	{
		File.Copy( texturePath, dummyTexturePath, true );
		AssetDatabase.ImportAsset( dummyTexturePath, ImportAssetOptions.ForceUpdate );

		Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>( dummyTexturePath );
		if( !texture ) // RenderTextures, for example, are also Textures but not Texture2Ds
			return false;

		Color32[] colors = texture.GetPixels32();
		Color32 color = colors[0];
		for( int i = 1; i < colors.Length; i++ )
		{
			Color32 color2 = colors[i];
			if( color2.r != color.r || color2.g != color.g || color2.b != color.b || color2.a != color.a )
				return false;
		}

		return true;
	}

	// Hides the Texture at the specified index from the results
	private void HideTexture( int textureIndex )
	{
		results.RemoveAt( textureIndex );

		SaveSession( null );
		Repaint();
	}

	// Saves current session to file
	private void SaveSession( string json )
	{
		if( string.IsNullOrEmpty( json ) )
			json = JsonUtility.ToJson( new SaveData() { paths = results }, false );

		File.WriteAllText( SAVE_FILE_PATH, json );
	}

	// Restores previous session
	private void LoadSession( string json )
	{
		if( string.IsNullOrEmpty( json ) )
		{
			if( !File.Exists( SAVE_FILE_PATH ) )
				return;

			json = File.ReadAllText( SAVE_FILE_PATH );
		}

		SaveData saveData = JsonUtility.FromJson<SaveData>( json );

		// Remove non-existent paths
		for( int i = saveData.paths.Count - 1; i >= 0; i-- )
		{
			if( !File.Exists( saveData.paths[i] ) )
				saveData.paths.RemoveAt( i );
		}

		results = saveData.paths;
		Repaint();
	}
}