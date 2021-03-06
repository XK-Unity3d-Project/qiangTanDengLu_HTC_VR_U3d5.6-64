﻿using UnityEngine;
using System.Collections;

public class XKTriggerStopMovePlayer : MonoBehaviour {
	[Range(0f, 100f)]public float DistanceVal = 1f;
	public static bool IsActiveTrigger;
	Transform PlayerCameraTr;
	public static Transform KaQiuShaAmmoTr;
	static float AmmoSpeedVal;
	public AiPathCtrl TestPlayerPath;
	void Start()
	{
		XkGameCtrl.GetInstance().ChangeBoxColliderSize(transform);
		CheckIsHiddenObj();
	}

	void CheckIsHiddenObj()
	{
		bool isHiddenObj = false;
		if (Network.peerType == NetworkPeerType.Disconnected) {
			if (XkGameCtrl.GameModeVal != GameMode.LianJi
			    && XkGameCtrl.GameJiTaiSt == GameJiTaiType.TanKeJiTai) {
				isHiddenObj = true;
			}
		}
		else {
			if (Network.peerType == NetworkPeerType.Server
			    || XkGameCtrl.GameJiTaiSt == GameJiTaiType.TanKeJiTai) {
				isHiddenObj = true;
			}
		}
		
		if (isHiddenObj) {
			gameObject.SetActive(false);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (XKTriggerKaQiuShaFire.IsCloseKaQiuShaTest) {
			return; //test;
		}

		if (JiFenJieMianCtrl.GetInstance() != null && JiFenJieMianCtrl.GetInstance().GetIsShowFinishTask()) {
			return;
		}

		if (Network.peerType == NetworkPeerType.Server) {
			return;
		}

		XkPlayerCtrl script = other.GetComponent<XkPlayerCtrl>();
		if (script == null || !script.GetIsHandleRpc()) {
			return;
		}
		PlayerCameraTr = script.GetPlayerCameraScript().transform;
		IsActiveTrigger = true;
	}

	void FixedUpdate()
	{
		if (!IsActiveTrigger) {
			return;
		}

		if (PlayerCameraTr == null || KaQiuShaAmmoTr == null) {
			return;
		}

		Vector3 posA = PlayerCameraTr.position;
		Vector3 posB = KaQiuShaAmmoTr.position;
		float disAB = Vector3.Distance(posA, posB);
		DistanceVal = 10f;
		float minDisVal = DistanceVal + (AmmoSpeedVal * Time.deltaTime * Time.timeScale);
		if (disAB <= minDisVal) {
			Debug.Log("XKTriggerStopMovePlayer -> disAB "+disAB+", DistanceVal "+minDisVal);
			IsActiveTrigger = false; //打开主角UI,恢复世界时间.
			XKTriggerCameraFieldOfView.Instance.ResetWorldTimeVal();
			gameObject.SetActive(false);
		}
	}

	public static void SetKaQiuShaAmmoTrInfo(NpcAmmoCtrl ammoScript)
	{
		if (ammoScript == null) {
			return;
		}
		KaQiuShaAmmoTr = ammoScript.transform;
		AmmoSpeedVal = ammoScript.MvSpeed / 3.6f;
	}
	
	void OnDrawGizmosSelected()
	{
		if (!XkGameCtrl.IsDrawGizmosObj) {
			return;
		}

		if (!enabled) {
			return;
		}

		if (TestPlayerPath != null) {
			TestPlayerPath.DrawPath();
		}
	}
}