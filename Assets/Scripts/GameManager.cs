using UnityEngine;
using System.Collections;
using MsgPack.Serialization;

/// <summary>
/// ゲーム全体の管理を行う
/// </summary>
public class GameManager : MonoBehaviour {
	static GameObject _hiyo, _matsuhiyo;

	/// <summary>
	/// ひよこプレファブ取得
	/// </summary>
	public static GameObject Hiyo {
		get {
			return _hiyo = _hiyo ?? (GameObject)Resources.Load("Prefabs/Hiyoko");
		}
	}

	/// <summary>
	/// 祭りひよこプレファブ取得
	/// </summary>
	public static GameObject MatsuHiyo {
		get {
			return _matsuhiyo = _matsuhiyo ?? (GameObject)Resources.Load("Prefabs/MatsuHiyo");
		}
	}

	/// <summary>
	/// 初期化時に保存してあるゲームデータから復元
	/// </summary>
	void Awake() {
		Debug.Log("Start load game data");
		var gd = ClassSerializer.LoadObject<GameData>(GameData.FilePath, new GameData());
		gd.RestoreGameObjects();
		Debug.Log("End load game data");
	}

	/// <summary>
	/// アプリ終了時に保存
	/// </summary>
	void OnApplicationQuit() {
		Debug.Log("Start save game data");
		var gd = new GameData();
		gd.StoreGameObjects();
		ClassSerializer.SaveObject<GameData>(GameData.FilePath, gd);
		Debug.Log("End save game data");
	}
}
