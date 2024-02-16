using OWML.Common;
using OWML.ModHelper;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SuitlessNotifications
{
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
			/*
			var suitlessHUDCameraObj = new GameObject("SuitlessHUDCamera");
			suitlessHUDCameraObj.transform.parent = suitlessDisplayUICanvas.transform.parent;
			suitlessHUDCameraObj.transform.localPosition = Vector3.zero;
			suitlessHUDCameraObj.transform.localRotation = Quaternion.identity;
			var suitlessHUDCamera = suitlessHUDCameraObj.AddComponent<Camera>();
			suitlessHUDCameraObj.AddComponent<SuitlessHUDCamera>();
			*/

			foreach (var canvas in suitlessDisplayUICanvas.GetComponentsInChildren<Canvas>(true).Append(suitlessDisplayUICanvas.GetComponent<Canvas>()))
			{
				ModHelper.Events.Unity.FireOnNextUpdate(() => canvas.worldCamera = Locator.GetPlayerCamera().mainCamera);
				//canvas.worldCamera = suitlessHUDCamera;
			}

			suitlessDisplayUICanvas.SetActive(true);
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
}
