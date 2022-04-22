//using UnityEditor;
//using UnityEditor.Build.Reporting;
//using UnityEngine;

//namespace FTemplateNamespace
//{
//	public class DisableSplashScreen : UnityEditor.Build.IPreprocessBuildWithReport
//	{
//		public int callbackOrder { get { return 0; } }

//		[InitializeOnLoadMethod]
//		private static void Execute()
//		{
//			if( Application.HasProLicense() )
//			{
//				Debug.Log( "Disabling splash screen" );
//				PlayerSettings.SplashScreen.show = false;
//			}
//		}

//		public void OnPreprocessBuild( BuildReport report )
//		{
//			Execute();
//		}
//	}
//}