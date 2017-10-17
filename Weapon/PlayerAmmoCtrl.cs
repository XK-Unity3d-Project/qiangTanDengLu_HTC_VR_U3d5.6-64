using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAmmoCtrl : MonoBehaviour {
	public PlayerAmmoType AmmoType = PlayerAmmoType.PuTongAmmo;
	public GameObject AmmoExplode;
	[Range(1f, 4000f)] public float MvSpeed = 50f;
	[Range(0.1f, 1000f)] public float AmmoDamageDis = 50f;
	[Range(0.01f, 10f)] public float AmmoSanSheDis = 0.5f;
	[Range(1f, 100f)] public float LiveTime = 4f;
	public GameObject MetalParticle;		//金属.
	public GameObject ConcreteParticle;		//混凝土.
	public GameObject DirtParticle;			//土地.
	public GameObject WoodParticle;			//树木.
	public GameObject WaterParticle;		//水.
	public GameObject SandParticle;			//沙滩.
	public GameObject GlassParticle;		//玻璃.
	float SpawnAmmoTime;
	GameObject ObjAmmo;
	Transform AmmoTran;
	PlayerEnum PlayerState = PlayerEnum.Null;
	public static LayerMask PlayerAmmoHitLayer;
	Vector3 AmmoStartPos;
	Vector3 AmmoEndPos;
	bool IsHandleRpc;
	GameObject HitNpcObj;
	bool IsDestroyAmmo;
	bool IsDonotHurtNpc;
	TrailRenderer TrailScript;
	float TrailTime = 3f;
    XKSpawnParticle SpawnParticleCom;
    void Awake()
    {
        SpawnParticleCom = gameObject.AddComponent<XKSpawnParticle>();
        if (TrailScript == null) {
			TrailScript = GetComponentInChildren<TrailRenderer>();
			if (TrailScript != null) {
				TrailScript.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				TrailScript.receiveShadows = false;
				TrailTime = TrailScript.time;
			}
		}
		AmmoTran = transform;
		ObjAmmo = gameObject;
		AmmoTran.parent = XkGameCtrl.PlayerAmmoArray;
	}
	
	void Update()
	{
		if (Time.frameCount % 200 != 0) {
			return;
		}

		if (!IsHandleRpc) {
			return;
		}
		
		if (IsDestroyAmmo) {
			return;
		}
		
		if (Time.realtimeSinceStartup - SpawnAmmoTime > LiveTime) {
			MoveAmmoOnCompelteITween();
			return;
		}
	}

	public void StartMoveAmmo(Vector3 firePos, PlayerEnum playerSt,
	                          NpcPathCtrl ammoMovePath = null, GameObject hitObjNpc = null)
	{
		HitNpcObj = hitObjNpc;
		ObjAmmo = gameObject;
		if (!ObjAmmo.activeSelf) {
			ObjAmmo.SetActive(true);
			IsDestroyAmmo = false;
        }
        SpawnAmmoTime = Time.realtimeSinceStartup;
        AmmoTran = transform;
		PlayerState = playerSt;
		MoveAmmoByItween(firePos, ammoMovePath);
		IsHandleRpc = true;
	}

	void ResetTrailScriptInfo()
	{
		gameObject.SetActive(false);
		if (TrailScript == null) {
			return;
		}
		TrailScript.time = TrailTime;
	}
	
	void MoveAmmoByItween(Vector3 firePos, NpcPathCtrl ammoMovePath)
	{
		Vector3[] posArray = new Vector3[2];
		if (ammoMovePath == null) {
			posArray[0] = AmmoTran.position;
			posArray[1] = firePos;
			AmmoStartPos = AmmoTran.position;
			iTween.MoveTo(ObjAmmo, iTween.Hash("path", posArray,
			                                   "speed", MvSpeed,
			                                   "orienttopath", true,
			                                   "easeType", iTween.EaseType.linear,
			                                   "oncomplete", "MoveAmmoOnCompelteITween"));
		}
		else {
			int countMark = ammoMovePath.transform.childCount;
			Transform[] tranArray = ammoMovePath.transform.GetComponentsInChildren<Transform>();
			List<Transform> nodesTran = new List<Transform>(tranArray){};
			nodesTran.Remove(ammoMovePath.transform);
			transform.position = nodesTran[0].position;
			transform.rotation = nodesTran[0].rotation;
			firePos = nodesTran[countMark-1].position;
			AmmoStartPos = nodesTran[countMark-2].position;
			iTween.MoveTo(ObjAmmo, iTween.Hash("path", nodesTran.ToArray(),
			                                   "speed", MvSpeed,
			                                   "orienttopath", true,
			                                   "easeType", iTween.EaseType.linear,
			                                   "oncomplete", "MoveAmmoOnCompelteITween"));
		}
		AmmoEndPos = firePos;
	}

	void SpawnAmmoParticleObj()
	{
		GameObject objParticle = null;
		GameObject obj = null;
		Transform tran = null;
		Vector3 hitPos = transform.position;

		RaycastHit hit;
		if (!IsHandleRpc) {
			AmmoEndPos = transform.position;
			AmmoStartPos = transform.position - transform.forward * 3f;
			Physics.Raycast(AmmoStartPos, transform.forward, out hit, 1000f, PlayerAmmoHitLayer);
			if (hit.collider != null) {
				AmmoEndPos = hit.point;
			}
		}

		Vector3 forwardVal = Vector3.Normalize(AmmoEndPos - AmmoStartPos);
		if (AmmoType == PlayerAmmoType.PuTongAmmo) {
//			float disVal = Vector3.Distance(AmmoEndPos, AmmoStartPos) + 10f;
//			Physics.Raycast(AmmoStartPos, forwardVal, out hit, disVal, PlayerAmmoHitLayer);

			Vector3 startPos =  Vector3.zero;
			Vector3 backVal = -10f * transform.forward;
			if (GameTypeCtrl.IsTankVRStatic) {
				startPos = transform.position + backVal;
			}
			else {
				float disAmmoOffset = AmmoSanSheDis;
				float randKay  = Random.Range(0, 100) % 2 == 0 ? -1 : 1;
				Vector3 upVal = randKay * Random.Range(0f, disAmmoOffset) * transform.up;
				randKay  = Random.Range(0, 100) % 2 == 0 ? -1 : 1;
				Vector3 rightVal = randKay * Random.Range(0f, disAmmoOffset) * transform.right;
				startPos = transform.position + upVal + rightVal + backVal;
			}

			Physics.Raycast(startPos, transform.forward, out hit, 20f, PlayerAmmoHitLayer);
			if (hit.collider != null) {
				hitPos = hit.point;
				XKAmmoParticleCtrl ammoParticleScript = hit.collider.GetComponent<XKAmmoParticleCtrl>();
				if (ammoParticleScript != null && ammoParticleScript.PuTongAmmoLZ != null) {
					objParticle = ammoParticleScript.PuTongAmmoLZ;
				}
				else {
					string tagHitObj = hit.collider.tag;
					switch (tagHitObj) {
					case "metal":
						if (MetalParticle != null) {
							objParticle = MetalParticle;
						}
						break;
						
					case "concrete":
						if (ConcreteParticle != null) {
							objParticle = ConcreteParticle;
						}
						break;
						
					case "dirt":
						if (DirtParticle != null) {
							objParticle = DirtParticle;
						}
						break;
						
					case "wood":
						if (WoodParticle != null) {
							objParticle = WoodParticle;
						}
						break;
						
					case "water":
						if (WaterParticle != null) {
							objParticle = WaterParticle;
						}
						break;
						
					case "sand":
						if (SandParticle != null) {
							objParticle = SandParticle;
						}
						break;
						
					case "glass":
						if (GlassParticle != null) {
							objParticle = GlassParticle;
						}
						break;
					}
					
					if (objParticle == null) {
						objParticle = AmmoExplode;
					}
				}

				if (IsHandleRpc
				    && !IsDonotHurtNpc
				    && Network.peerType != NetworkPeerType.Server
				    && AmmoType == PlayerAmmoType.PuTongAmmo) {
					XKNpcHealthCtrl healthScript = hit.collider.GetComponent<XKNpcHealthCtrl>();
					if (healthScript != null) {
						if (HitNpcObj == null || HitNpcObj != hit.collider.gameObject) {
							//Debug.Log("playerAmmo hit npc, npc is "+hit.collider.name);
							healthScript.OnDamageNpc(AmmoType, PlayerState);
						}
					}
				}
			}
			else {
				objParticle = AmmoExplode;
			}
		}
		else {
			float disVal = Vector3.Distance(AmmoEndPos, AmmoStartPos) + 10f;
			Physics.Raycast(AmmoStartPos, forwardVal, out hit, disVal, PlayerAmmoHitLayer);
			if (hit.collider != null) {
//				if (AmmoType == PlayerAmmoType.GaoBaoAmmo) {
//					Debug.Log("hit.collider "+hit.collider.name);
//				}
				hitPos = hit.point;
				string tagHitObj = hit.collider.tag;
				switch (tagHitObj) {
				case "dirt":
					if (DirtParticle != null) {
						objParticle = DirtParticle;
					}
					break;

				case "water":
					if (WaterParticle != null) {
						objParticle = WaterParticle;
					}
					break;
				}
				
				if (objParticle == null) {
					objParticle = AmmoExplode;
				}
			}
			else {
				objParticle = AmmoExplode;
			}
		}
		
		if (objParticle == null) {
			return;
		}
		
		if (AmmoType == PlayerAmmoType.DaoDanAmmo) {
			Vector3 AmmoPos = transform.position - (transform.forward * 3f);
			Physics.Raycast(AmmoPos, forwardVal, out hit, 13f, XkGameCtrl.GetInstance().LandLayer);
			if (hit.collider != null) {
				hitPos = hit.point;
				Vector3 normalVal = hit.normal;
				Quaternion rotVal = Quaternion.LookRotation(normalVal);
				obj = SpawnParticleCom.SpawnParticleObject(objParticle, hitPos, rotVal);
				obj.transform.up = normalVal;
			}
			else {
				obj = SpawnParticleCom.SpawnParticleObject(objParticle, hitPos, transform.rotation);
			}
		}
		else {
			obj = SpawnParticleCom.SpawnParticleObject(objParticle, hitPos, transform.rotation);
		}
		tran = obj.transform;
		tran.parent = XkGameCtrl.PlayerAmmoArray;
		XkGameCtrl.CheckObjDestroyThisTimed(obj);

		XkAmmoTieHuaCtrl tieHuaScript = obj.GetComponent<XkAmmoTieHuaCtrl>();
		if (tieHuaScript != null && tieHuaScript.TieHuaTran != null) {
			Transform tieHuaTran = tieHuaScript.TieHuaTran;
			Vector3 AmmoPos = transform.position - (transform.forward * 3f);
			Physics.Raycast(AmmoPos, forwardVal, out hit, 13f, XkGameCtrl.GetInstance().PlayerAmmoHitLayer);
			if (hit.collider != null) {
				tieHuaTran.up = hit.normal;
			}
		}
	}
	
	void MoveAmmoOnCompelteITween()
	{
		if (IsDestroyAmmo) {
			return;
		}
		IsDestroyAmmo = true;
		//Debug.Log("MoveAmmoOnCompelteITween...");
		SpawnAmmoParticleObj();
		
		iTween itweenScript = GetComponent<iTween>();
		if (itweenScript != null) {
			itweenScript.isRunning = false;
			itweenScript.isPaused = true;
			itweenScript.enabled = false;
		}

		if (!IsDonotHurtNpc) {
			CheckAmmoDamageNpc();
		}

		if (AmmoType == PlayerAmmoType.PuTongAmmo || AmmoType == PlayerAmmoType.GaoBaoAmmo) {
			//Destroy(ObjAmmo, 0.1f); //test
			if (!IsInvoking("DaleyHiddenPlayerAmmo")) {
				Invoke("DaleyHiddenPlayerAmmo", 0.01f);
			}
		}
		else {
			Destroy(ObjAmmo, 0.1f);
		}
	}

	void DaleyHiddenPlayerAmmo()
	{
		if (TrailScript != null) {
			TrailScript.time = 0f;
			Invoke("ResetTrailScriptInfo", 0.01f);
		}
		else {
			gameObject.SetActive(false);
		}
	}

	void CheckAmmoDamageNpc()
	{
		if (AmmoType == PlayerAmmoType.Null) {
			return;
		}
		
		XKNpcHealthCtrl healthScript = null;
		Transform[] npcArray = XkGameCtrl.GetInstance().GetNpcTranList().ToArray();
		int max = npcArray.Length;
		Vector3 posA = AmmoTran.position;
		Vector3 posB = Vector3.zero;
		for (int i = 0; i < max; i++) {
			if (npcArray[i] == null) {
				continue;
			}
			
			posB = npcArray[i].position;
			//Debug.Log("disTest "+disTest+", posA "+posA+", posB "+posB+", AmmoDamageDis "+AmmoDamageDis);
			if (Vector3.Distance(posA, posB) <= AmmoDamageDis) {
				healthScript = npcArray[i].GetComponentInChildren<XKNpcHealthCtrl>();
				if (healthScript != null)
                {
                        //Add Damage Npc num to PlayerInfo.
                        healthScript.OnDamageNpc(AmmoType, PlayerState);
				}
			}
		}
	}
	
	public void SetIsDonotHurtNpc(bool isDonotHurt)
	{
		IsDonotHurtNpc = isDonotHurt;
	}
}