using UnityEngine;
using System.Collections;

/// <summary>
/// ひよこちゃん
/// </summary>
public class Hiyoko : MonoBehaviour {
	/// <summary>
	/// ゲームオブジェクトから保存するデータを取得する
	/// </summary>
	public HiyokoData Store() {
		var t = this.transform;
		var p = t.position;
		HiyokoData hd = new HiyokoData();
		hd.Kind = t.tag == "Hiyoko" ? 0 : 1;
		hd.X = p.x;
		hd.Y = p.y;
		hd.Angle = t.localEulerAngles.z;
		return hd;
	}

	/// <summary>
	/// 保存してあるデータからゲームオブジェクトを復元する
	/// </summary>
	public static void Restore(HiyokoData hd) {
		GameObject.Instantiate(
			hd.Kind == 0 ? GameManager.Hiyo : GameManager.MatsuHiyo,
			new Vector3(hd.X, hd.Y, 0), Quaternion.Euler(0, 0, hd.Angle));
	}
}
