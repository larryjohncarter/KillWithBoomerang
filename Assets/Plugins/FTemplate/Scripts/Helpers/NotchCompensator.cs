using UnityEngine;

public class NotchCompensator : MonoBehaviour
{
#pragma warning disable 0649
	[SerializeField]
	private float referenceResolution = 1080f;

	[SerializeField]
	private RectTransform[] topPanels;

	[SerializeField]
	private RectTransform[] bottomPanels;
#pragma warning restore 0649

	private void Awake()
	{
		// Remove destroyed RectTransforms from arrays
		int topPanelsSize = 0;
		for( int i = 0; i < topPanels.Length; i++ )
		{
			if( topPanels[i] )
				topPanels[topPanelsSize++] = topPanels[i];
		}

		int bottomPanelsSize = 0;
		for( int i = 0; i < bottomPanels.Length; i++ )
		{
			if( bottomPanels[i] )
				bottomPanels[bottomPanelsSize++] = bottomPanels[i];
		}

		if( topPanelsSize != topPanels.Length )
			System.Array.Resize( ref topPanels, topPanelsSize );

		if( bottomPanelsSize != bottomPanels.Length )
			System.Array.Resize( ref bottomPanels, bottomPanelsSize );
	}

	private void Start()
	{
		if( FTemplate.Ads )
			FTemplate.Ads.OnBannerAdVisibilityChanged += OnBannerAdVisibilityChanged;

		Refresh();
	}

	private void OnDisable()
	{
		if( FTemplate.Ads )
			FTemplate.Ads.OnBannerAdVisibilityChanged -= OnBannerAdVisibilityChanged;
	}

	public void Refresh()
	{
		if( FTemplate.Ads )
			OnBannerAdVisibilityChanged( FTemplate.Ads.BannerAdVisible );
		else
			OnBannerAdVisibilityChanged( false );
	}

	private void OnBannerAdVisibilityChanged( bool isVisible )
	{
		if( !isVisible )
		{
			RefreshTop( 0f );
			RefreshBottom( 0f );
		}
		else
		{
			if( FTemplate.Ads.BannerAdAtBottom )
			{
				RefreshTop( 0f );
				RefreshBottom( FTemplate.Ads.BannerAdHeightInPixels );
			}
			else
			{
				RefreshTop( FTemplate.Ads.BannerAdHeightInPixels );
				RefreshBottom( 0f );
			}
		}
	}

	private void RefreshTop( float additionalObscuredHeight )
	{
		if( topPanels.Length == 0 )
			return;

		float safeAreaOffset = referenceResolution * ( Screen.height - Screen.safeArea.yMax + additionalObscuredHeight ) / Screen.height;
		for( int i = 0; i < topPanels.Length; i++ )
		{
			if( !topPanels[i] )
				Debug.LogWarning( "Unassigned top panel in Notch Compensator!", this );
			else if( topPanels[i].anchoredPosition.y > -safeAreaOffset )
				topPanels[i].anchoredPosition = new Vector2( topPanels[i].anchoredPosition.x, -safeAreaOffset );
		}
	}

	private void RefreshBottom( float additionalObscuredHeight )
	{
		if( bottomPanels.Length == 0 )
			return;

		float safeAreaOffset = referenceResolution * ( Screen.safeArea.yMin + additionalObscuredHeight ) / Screen.height;
		for( int i = 0; i < bottomPanels.Length; i++ )
		{
			if( !bottomPanels[i] )
				Debug.LogWarning( "Unassigned bottom panel in Notch Compensator!", this );
			else if( bottomPanels[i].anchoredPosition.y < safeAreaOffset )
				bottomPanels[i].anchoredPosition = new Vector2( bottomPanels[i].anchoredPosition.x, safeAreaOffset );
		}
	}
}