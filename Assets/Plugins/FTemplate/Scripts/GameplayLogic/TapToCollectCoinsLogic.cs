using FTemplateNamespace;
using MoreMountains.NiceVibrations;
using System.Collections;
using UnityEngine;

public class TapToCollectCoinsLogic
{
	private readonly Transform coinSource;
	private readonly Camera camera;
	private int gainedCoins;
	private readonly int coinsPerClick;
	private readonly float coinScaleMultiplier;
	private readonly float coinsSpreadAmount;
	private float clickTutorialShowTime;
	private readonly System.Action onClick;

	public TapToCollectCoinsLogic( Transform coinSource, int gainedCoins, int requiredClicks, float coinScaleMultiplier, float coinsSpreadAmount, System.Action onClick = null )
	{
		this.coinSource = coinSource;
		this.camera = Camera.main;
		this.gainedCoins = gainedCoins;
		this.coinScaleMultiplier = coinScaleMultiplier;
		this.coinsSpreadAmount = coinsSpreadAmount;
		this.onClick = onClick;
		coinsPerClick = Mathf.Max( 1, Mathf.CeilToInt( (float) gainedCoins / requiredClicks ) );
		clickTutorialShowTime = 0f;
	}

	public IEnumerator Run()
	{
		if( gainedCoins <= 0 )
			yield break;

		FTemplate.UI.Show( UIElementType.TotalCoinsText );
		FTemplate.UI.TapToDoStuffTutorialLabel = "Tap to Collect Coins";

		while( gainedCoins > 0 )
		{
			if( Time.time >= clickTutorialShowTime )
			{
				clickTutorialShowTime = float.PositiveInfinity;
				FTemplate.UI.Show( UIElementType.TapToDoStuffTutorial );
			}

			if( AnyPointerPressedThisFrame() )
			{
				int coinsToSpawn = Mathf.Min( coinsPerClick, gainedCoins );
				gainedCoins -= coinsToSpawn;

				FTemplate.UI.SpawnCollectedCoins( camera.WorldToScreenPoint( coinSource.position ), coinsToSpawn, coinsToSpawn, coinScaleMultiplier, coinsSpreadAmount );
				MMVibrationManager.Haptic( HapticTypes.MediumImpact );

				clickTutorialShowTime = Time.time + 1.5f;
				FTemplate.UI.Hide( UIElementType.TapToDoStuffTutorial );

				if( onClick != null )
					onClick();
			}

			yield return null;
		}

		yield return BetterWaitForSeconds.Wait( 1f );
	}

	private bool AnyPointerPressedThisFrame()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		return Input.GetMouseButtonDown( 0 );
#else
		for( int i = 0; i < Input.touchCount; i++ )
		{
			if( Input.GetTouch( i ).phase == TouchPhase.Began )
				return true;
		}

		return false;
#endif
	}
}