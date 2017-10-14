using UnityEngine;
using System.Collections;

public class XKPlayerGunRotCtrl : MonoBehaviour
{
	public PlayerEnum PlayerSt = PlayerEnum.PlayerOne;
	//坦克vr主炮炮管.
	public Transform TankPG;
	//坦克vr主炮炮台.
	public Transform TankPT;
	//坦克vr隐藏实体枪.
	public GameObject[] HiddenGunObj;
	public Transform PlayerGunTr;
	public Transform PlayerMainCamTr;
	float MaxPX = 0f;
	float MaxPY = 0f;
	float CurPX;
	float CurPY;
	Vector3 PlayerGunPos;
	float OffsetForward = 30f;
	static XKPlayerGunRotCtrl _InstanceOne;
	public static XKPlayerGunRotCtrl GetInstanceOne()
	{
		return _InstanceOne;
	}
	static XKPlayerGunRotCtrl _InstanceTwo;
	public static XKPlayerGunRotCtrl GetInstanceTwo()
	{
		return _InstanceTwo;
	}
	// Use this for initialization
	void Start()
	{
		switch (PlayerSt) {
		case PlayerEnum.PlayerOne:
			_InstanceOne = this;
			break;

		case PlayerEnum.PlayerTwo:
			_InstanceTwo = this;
			break;
		}
		MaxPX = Screen.width;
		MaxPY = Screen.height;

        if (HiddenGunObj != null && GameTypeCtrl.IsTankVRStatic) {
			for (int i = 0; i < HiddenGunObj.Length; i++) {
				HiddenGunObj[i].SetActive(false);
			}
        }
    }
    public void UpdatePlayerTankPaoTongTr()
    {
        UpdatePlayerGunRot();
        UpdateTankZhuPaoTr();
    }

	void UpdatePlayerMainCamera()
	{
		Vector3 forwardVal = PlayerMainCamTr.forward;
		forwardVal.y = 0f;
		if (forwardVal != Vector3.zero) {
			PlayerMainCamTr.forward = forwardVal;
		}
	}

    public GameObject CameraMainObj;
	void UpdatePlayerGunRot()
	{
		if (Camera.main == null) {
			return;
		}
        CameraMainObj = Camera.main.gameObject;

        Vector3 mousePosInput = pcvr.GetPlayerMousePos(PlayerEnum.PlayerTwo);
		CurPX = mousePosInput.x;
		CurPY = mousePosInput.y;
		CurPX = CurPX < 0f ? 0f : CurPX;
		CurPX = CurPX > MaxPX ? MaxPX : CurPX;

		CurPY = CurPY < 0f ? 0f : CurPY;
		CurPY = CurPY > MaxPY ? MaxPY : CurPY;
		mousePosInput.x = CurPX;
		mousePosInput.y = CurPY;
        
        PlayerGunPos = PlayerGunTr.position;
		Vector3 mousePos = mousePosInput + Vector3.forward * OffsetForward;
        Vector3 posTmp = Camera.main.ScreenToWorldPoint(mousePos);
		Vector3 gunForward = Vector3.Normalize(posTmp - PlayerGunPos);
		if (gunForward != Vector3.zero) {
			PlayerGunTr.forward = gunForward;
		}
        //if (XkPlayerCtrl.GetInstanceTanKe() != null)
        //{
        //    Vector3 dir = XkPlayerCtrl.GetInstanceTanKe().PlayerAutoFireScript.GunPosCalculate.CalculateGunForwardVec(PlayerSt);
        //    Quaternion rot = Quaternion.FromToRotation(Vector3.forward, dir);
        //    PlayerGunTr.localEulerAngles = rot.eulerAngles;
        //}
	}

	void UpdateTankZhuPaoTr()
	{
		if (!GameTypeCtrl.IsTankVRStatic) {
			return;
		}
		Vector3 gunEA = PlayerGunTr.localEulerAngles;
		Vector3 paoTaiEA = Vector3.zero;
		Vector3 paoGuanEA = Vector3.zero;
		paoTaiEA.y = gunEA.y;
		paoGuanEA.x = gunEA.x;
		TankPT.localEulerAngles = paoTaiEA;
		TankPG.localEulerAngles = paoGuanEA;
	}
}