using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 永続化されるゲームデータ
/// </summary>
public class GameData {
	static string _filePath = Application.persistentDataPath + "/data.dat";

	/// <summary>
	/// ゲームデータ保存先パス名
	/// </summary>
	public static string FilePath {
		get {
			return _filePath;
		}
	}

	/// <summary>
	/// ひよこデータ一覧　保存＆読み込み時のみ使用する
	/// </summary>
	public List<HiyokoData> Hiyokos;

	/// <summary>
	/// ゲームオブジェクトのデータを内部に取り込む
	/// </summary>
	public void StoreGameObjects() {
		this.Hiyokos = new List<HiyokoData>();
		foreach (var obj in GameObject.FindObjectsOfType<GameObject>()) {
			var h = obj.GetComponent<Hiyoko>();
			if (h != null) {
				this.Hiyokos.Add(h.Store());
			}
		}
	}

	/// <summary>
	/// ゲームオブジェクトを復元する
	/// </summary>
	public void RestoreGameObjects() {
		foreach (var hd in this.Hiyokos) {
			Hiyoko.Restore(hd);
		}
		this.Hiyokos = null;
	}
}
