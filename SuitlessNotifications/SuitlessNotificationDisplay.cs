using HarmonyLib;
using UnityEngine;

namespace SuitlessNotifications;

// Modified version of SuitNotificationDisplay
public class SuitlessNotificationDisplay : NotificationDisplayTextLayout
{
	public override void Awake()
	{
		base.Awake();
		this._notificationTargetType = NotificationTarget.Player;
	}

	public override void Start()
	{
		base.Start();
		this._playerAudioController = Locator.GetPlayerAudioController();
	}

	public override void ExpandPool()
	{
		int num = this._textItemPool.Count * 2;
		while (this._textItemPool.Count < num)
		{
			GameObject gameObject = GameObject.Instantiate<GameObject>(this._textDisplayTemplate, this._textDisplayRoot);
			this._textItemPool.Add(gameObject);
			gameObject.SetActive(false);
		}
	}

	public override void PlayNotificationAudio()
	{
		this._playerAudioController.PlayNotificationTextScrolling();
	}

	public override void StopNotificationAudio()
	{
		if (this._playerAudioController != null)
		{
			this._playerAudioController.StopNotificationTextScrolling();
		}
	}

	private PlayerAudioController _playerAudioController;
}
