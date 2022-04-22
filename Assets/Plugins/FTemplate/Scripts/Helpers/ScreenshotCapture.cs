using System;
using System.IO;
using UnityEngine;

public static class ScreenshotCapture
{
	// Saves the screenshot to desktop
	public static string Capture()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		string saveDirectory = Environment.GetFolderPath( Environment.SpecialFolder.DesktopDirectory );
#else
		string saveDirectory = Application.persistentDataPath;
#endif
		int fileIndex = 0;
		string path;
		do
		{
			path = Path.Combine( saveDirectory, string.Format( "Screenshot {0}.png", ++fileIndex ) );
		} while( File.Exists( path ) );

		Capture( path );
		return path;
	}

	public static void Capture( string path )
	{
		Camera camera = Camera.main;
		RenderTexture temp = RenderTexture.active;
		RenderTexture temp2 = camera.targetTexture;

		RenderTexture renderTex = RenderTexture.GetTemporary( Screen.width, Screen.height, 24 );
		Texture2D screenshot = null;

		try
		{
			RenderTexture.active = renderTex;

			camera.targetTexture = renderTex;
			camera.Render();

			screenshot = new Texture2D( renderTex.width, renderTex.height, TextureFormat.RGB24, false );
			screenshot.ReadPixels( new Rect( 0, 0, renderTex.width, renderTex.height ), 0, 0, false );
			screenshot.Apply( false, false );

			File.WriteAllBytes( path, screenshot.EncodeToPNG() );
		}
		finally
		{
			camera.targetTexture = temp2;
			RenderTexture.active = temp;
			RenderTexture.ReleaseTemporary( renderTex );

			if( screenshot != null )
				UnityEngine.Object.DestroyImmediate( screenshot );
		}
	}
}