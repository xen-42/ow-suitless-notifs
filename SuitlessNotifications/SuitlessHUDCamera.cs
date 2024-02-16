using UnityEngine;

namespace SuitlessNotifications;

// Modified version of HUDCamera
public class SuitlessHUDCamera : MonoBehaviour
{
	private Camera _camera;
	private Material _hudMaterial;
	private float _cameraInitFieldOfView;
	private RenderTexture _hudRenderTex;
	private bool _activated;
	private bool _suspended;
	private bool _keepDeallocated;

	public void Awake()
	{
		this._camera = base.GetComponent<Camera>();

		// Serialized field, copy it from the original
		var hudCamera = GameObject.FindObjectOfType<HUDCamera>();
		_hudMaterial = new Material(hudCamera._hudMaterial);
		_camera.clearFlags = hudCamera._camera.clearFlags;
		_camera.backgroundColor = hudCamera._camera.backgroundColor;
		_camera.allowHDR = hudCamera._camera.allowHDR;
		_camera.allowMSAA = hudCamera._camera.allowMSAA;
		_camera.cullingMask = hudCamera._camera.cullingMask;
		_camera.farClipPlane = hudCamera._camera.farClipPlane;
		_camera.nearClipPlane = hudCamera._camera.nearClipPlane;

		this._cameraInitFieldOfView = this._camera.fieldOfView;
		this._camera.enabled = false;
		Vector2 vector = new Vector2(1920f, 1080f);
		float num = (float)Screen.width / (float)Screen.height;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			vector.x *= num / 1.7777778f;
			this._camera.fieldOfView = this._cameraInitFieldOfView * (1.7777778f / num);
		}
		this._hudRenderTex = new RenderTexture(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y), 16, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		this._hudRenderTex.name = "SuitlessHelmetHUD";
		this._hudRenderTex.Create();
		this._camera.targetTexture = this._hudRenderTex;
		this._hudMaterial.SetTexture("_MainTex", this._hudRenderTex);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = this._camera.targetTexture;
		GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
		RenderTexture.active = active;

		// These are reversed
		GlobalMessenger.AddListener("HelmetHUDActivated", new Callback(this.DeactivateHUD));
		GlobalMessenger.AddListener("RemoveHelmet", new Callback(this.ActivateHUD));

		// Remove dreamworld callbacks

		GlobalMessenger<OWCamera>.AddListener("SwitchActiveCamera", new Callback<OWCamera>(this.OnSwitchActiveCamera));
		GlobalMessenger<GraphicSettings>.AddListener("GraphicSettingsUpdated", new Callback<GraphicSettings>(this.OnGraphicSettingsUpdated));
	}

	public void OnDestroy()
	{
		this._hudRenderTex.Release();
		GameObject.Destroy(this._hudRenderTex);
		this._hudRenderTex = null;
		GlobalMessenger.RemoveListener("HelmetHUDActivated", new Callback(this.DeactivateHUD));
		GlobalMessenger.RemoveListener("RemoveHelmet", new Callback(this.ActivateHUD));

		GlobalMessenger<OWCamera>.RemoveListener("SwitchActiveCamera", new Callback<OWCamera>(this.OnSwitchActiveCamera));
		GlobalMessenger<GraphicSettings>.RemoveListener("GraphicSettingsUpdated", new Callback<GraphicSettings>(this.OnGraphicSettingsUpdated));
	}

	public void OnPreCull()
	{
		QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
	}

	public void OnPostRender()
	{
		QualitySettings.shadows = UnityEngine.ShadowQuality.All;
	}

	private void OnSwitchActiveCamera(OWCamera camera)
	{
		if (camera.CompareTag("MainCamera"))
		{
			this.ResumeHUDRendering();
			return;
		}
		this.SuspendHUDRendering();
	}

	private void OnGraphicSettingsUpdated(GraphicSettings graphicSettings)
	{
		Vector2 vector = new Vector2(1920f, 1080f);
		float num = (float)graphicSettings.displayResWidth / (float)graphicSettings.displayResHeight;
		if (!OWMath.ApproxEquals(num, 1.777f, 0.01f))
		{
			vector.x *= num / 1.7777778f;
			this._camera.fieldOfView = this._cameraInitFieldOfView * (1.7777778f / num);
		}
		else
		{
			this._camera.fieldOfView = this._cameraInitFieldOfView;
		}
		int num2 = Mathf.RoundToInt(vector.x);
		int num3 = Mathf.RoundToInt(vector.y);
		if (this._hudRenderTex.width != num2 || this._hudRenderTex.height != num3)
		{
			this._hudRenderTex.Release();
			this._hudRenderTex.width = num2;
			this._hudRenderTex.height = num3;
			if (!this._keepDeallocated)
			{
				this._hudRenderTex.Create();
			}
		}
		this._camera.ResetAspect();
	}

	private void ActivateHUD()
	{
		this._activated = true;
		this._camera.enabled = (this._activated && !this._suspended);
	}

	private void DeactivateHUD()
	{
		this._activated = false;
		this._camera.enabled = (this._activated && !this._suspended);
	}

	private void SuspendHUDRendering()
	{
		this._suspended = true;
		this._camera.enabled = (this._activated && !this._suspended);
	}

	private void ResumeHUDRendering()
	{
		this._suspended = false;
		this._camera.enabled = (this._activated && !this._suspended);
	}
}
