using HarmonyLib;
using OWML.Common;
using OWML.ModHelper;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuitlessNotifications;

[HarmonyPatch]
public class SuitlessNotifications : ModBehaviour
{


	private void Start()
	{
		LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
		{
			if (loadScene != OWScene.SolarSystem) return;
			OnLoadSolarSystem();
		};
	}

	private void OnLoadSolarSystem()
	{
		var suitedDisplayUICanvas = GameObject.FindObjectOfType<SuitNotificationDisplay>().transform.parent.gameObject;

		// Instantiate inactive
		suitedDisplayUICanvas.SetActive(false);
		var suitlessDisplayUICanvas = GameObject.Instantiate(suitedDisplayUICanvas);
		suitedDisplayUICanvas.SetActive(true);

		suitlessDisplayUICanvas.name = nameof(SuitlessNotificationDisplay);

		// Remove all children but SuitNotificationDisplay
		foreach (Transform child in suitlessDisplayUICanvas.transform)
		{
			if (child.GetComponent<SuitNotificationDisplay>() == null)
			{
				GameObject.Destroy(child.gameObject);
			}
		}

		// Original is on HelmetOnUI
		suitlessDisplayUICanvas.transform.parent = GameObject.Find("PlayerHUD/HelmetOffUI").transform;
		suitlessDisplayUICanvas.transform.localPosition = suitedDisplayUICanvas.transform.localPosition;
		suitlessDisplayUICanvas.transform.localRotation = Quaternion.identity;

		var copiedSuitDisplay = suitlessDisplayUICanvas.GetComponentInChildren<SuitNotificationDisplay>();
		var suitlessDisplay = copiedSuitDisplay.gameObject.AddComponent<SuitlessNotificationDisplay>();

		// Copy serialized fields
		// NotificationDisplayTextLayout
		suitlessDisplay._textDisplayTemplate = copiedSuitDisplay._textDisplayTemplate;
		suitlessDisplay._backgroundImage = copiedSuitDisplay._backgroundImage;
		suitlessDisplay._displaySpace = copiedSuitDisplay._displaySpace;

		// NotificationDisplay
		suitlessDisplay._displayText = copiedSuitDisplay._displayText;
		suitlessDisplay._displayCanvas = copiedSuitDisplay._displayCanvas;

		GameObject.Destroy(copiedSuitDisplay);

		SetLayersRecursively(suitlessDisplayUICanvas, LayerMask.NameToLayer("UI"));

		// Create suitless hud camera
		var HUDCamera = GameObject.FindObjectOfType<HUDCamera>();

		HUDCamera.gameObject.SetActive(false);
		var suitlessHUDCameraObj = GameObject.Instantiate(HUDCamera.gameObject);
		HUDCamera.gameObject.SetActive(true);

		suitlessHUDCameraObj.name = nameof(SuitlessHUDCamera);

		suitlessHUDCameraObj.transform.parent = suitlessDisplayUICanvas.transform.parent;
		suitlessHUDCameraObj.transform.localPosition = HUDCamera.transform.localPosition;
		suitlessHUDCameraObj.transform.localRotation = HUDCamera.transform.localRotation;

		//Component.Destroy(suitlessHUDCameraObj.GetComponent<HUDCamera>());
		//suitlessHUDCameraObj.gameObject.AddComponent<SuitlessHUDCamera>();
		var suitlessHUDCamera = suitlessHUDCameraObj.GetComponent<Camera>();

		ModHelper.Events.Unity.FireOnNextUpdate(() =>
		{
			suitlessHUDCameraObj.SetActive(true);
			suitlessDisplayUICanvas.SetActive(true);
			foreach (var canvas in suitlessDisplayUICanvas.GetComponentsInChildren<Canvas>(true).Append(suitlessDisplayUICanvas.GetComponent<Canvas>()))
			{
				canvas.worldCamera = suitlessHUDCamera;
			}
		});
	}

	private void SetLayersRecursively(GameObject obj, LayerMask uiLayer)
	{
		foreach (Transform child in obj.GetComponentInChildren<Transform>(true))
		{
			child.gameObject.layer = uiLayer;
		}
		obj.layer = uiLayer;
	}

#if DEBUG
	public void Update()
	{
		if (Keyboard.current[Key.Numpad7].wasReleasedThisFrame)
		{
			NotificationManager.SharedInstance.PostNotification(new NotificationData(NotificationTarget.All, "TEST", 7f, true), false);
		}
	}
}
#endif
