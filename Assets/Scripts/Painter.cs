using UnityEngine;
using System.Collections;

/// <summary>
/// ペイントする様にゲームオブジェクトを生成する
/// </summary>
public class Painter : MonoBehaviour {
	static float _angle;

	void FixedUpdate() {
		var mpos = Input.mousePosition;
		var position = Camera.main.ScreenToWorldPoint(mpos);
		position.z = 0;

		// マウス左ボタン押したらひよこ生成
		if (Input.GetMouseButton(0)) {
			Instantiate(GameManager.Hiyo, position, Quaternion.Euler(0, 0, _angle));
			_angle += 2;
		}

		// マウス右ボタン押したら祭りひよこ生成
		if (Input.GetMouseButton(1)) {
			Instantiate(GameManager.MatsuHiyo, position, Quaternion.Euler(0, 0, _angle));
			_angle += 2;
		}

		// マウス中ボタン押されたら全ひよこ削除
		if (Input.GetMouseButtonDown(2)) {
			foreach (var obj in GameObject.FindObjectsOfType<GameObject>()) {
				var h = obj.GetComponent<Hiyoko>();
				if (h != null) {
					GameObject.Destroy(obj);
				}
			}
		}
	}
}
