using System;
using FCSCommon.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
	public bool isOn;

	public Color onColorBg = Color.green;
	public Color offColorBg = Color.red;

	public Image toggleBgImage;
	public RectTransform toggle;

	public GameObject handle;
	private RectTransform handleTransform;

	private float handleSize;
	private float onPosX;
	private float offPosX;

	public float handleOffset = 4;

	public GameObject onIcon;
	public GameObject offIcon;


	public float speed = 1;
	static float t;

	private bool switching;
    private bool _isInitialize;


    public void Initialize()
    {
		if(_isInitialize) return;
        toggleBgImage = gameObject.FindChild("ToggleBg").GetComponent<Image>();
		toggle = gameObject.GetComponent<RectTransform>();
		handle = gameObject.FindChild("Handle");
		onIcon = gameObject.FindChild("ON");
		offIcon = gameObject.FindChild("OFF");
        var button = gameObject.GetComponentInChildren<Button>();
		button.onClick.AddListener(Switching);

		handleTransform = handle.GetComponent<RectTransform>();
		RectTransform handleRect = handle.GetComponent<RectTransform>();
		handleSize = handleRect.sizeDelta.x;
		float toggleSizeX = toggle.sizeDelta.x;
		onPosX = (toggleSizeX / 2) - (handleSize / 2) - handleOffset;
		offPosX = onPosX * -1;

        if (isOn)
        {
			toggleBgImage.color = onColorBg;
			handleTransform.localPosition = new Vector3(onPosX, 0f, 0f);
			onIcon.gameObject.SetActive(true);
			offIcon.gameObject.SetActive(false);
		}
        else
        {
			toggleBgImage.color = offColorBg;
			handleTransform.localPosition = new Vector3(offPosX, 0f, 0f);
			onIcon.gameObject.SetActive(false);
			offIcon.gameObject.SetActive(true);
		}


		_isInitialize = true;

    }


	void Start()
	{
		
	}

	void Update()
	{
		if(!_isInitialize) return;
		if (switching)
		{
			Toggle(isOn);
		}
	}

	public void DoYourStaff()
    {
        OnToggleSwitched?.Invoke(isOn);
    }

    public Action<bool> OnToggleSwitched { get; set; }

    public void Switching()
	{
		switching = true;
	}



	public void Toggle(bool toggleStatus)
	{
		if (!onIcon.active || !offIcon.active)
		{
			onIcon.SetActive(true);
			offIcon.SetActive(true);
		}

		if (toggleStatus)
		{
			toggleBgImage.color = SmoothColor(onColorBg, offColorBg);
			Transparency(onIcon, 1f, 0f);
			Transparency(offIcon, 0f, 1f);
			handleTransform.localPosition = SmoothMove(handle, onPosX, offPosX);
		}
		else
		{
			toggleBgImage.color = SmoothColor(offColorBg, onColorBg);
			Transparency(onIcon, 0f, 1f);
			Transparency(offIcon, 1f, 0f);
			handleTransform.localPosition = SmoothMove(handle, offPosX, onPosX);
		}

	}


	Vector3 SmoothMove(GameObject toggleHandle, float startPosX, float endPosX)
	{

		Vector3 position = new Vector3(Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f, 0f);
		StopSwitching();
		return position;
	}

	Color SmoothColor(Color startCol, Color endCol)
	{
		Color resultCol;
		resultCol = Color.Lerp(startCol, endCol, t += speed * Time.deltaTime);
		return resultCol;
	}

	CanvasGroup Transparency(GameObject alphaObj, float startAlpha, float endAlpha)
	{
		CanvasGroup alphaVal;
		alphaVal = alphaObj.gameObject.GetComponent<CanvasGroup>();
		alphaVal.alpha = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.deltaTime);
		return alphaVal;
	}

	void StopSwitching()
	{
		if (t > 1.0f)
		{
			switching = false;

			t = 0.0f;
			switch (isOn)
			{
				case true:
					isOn = false;
					DoYourStaff();
					break;

				case false:
					isOn = true;
					DoYourStaff();
					break;
			}

		}
	}

}
