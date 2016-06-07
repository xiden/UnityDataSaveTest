using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MsgPack;
using MsgPack.Serialization;

/// <summary>
/// MessagePackでシリアライズするクラスID一覧
/// </summary>
public enum ClassId {
	Unknown,

	/// <summary>
	/// ひよこデータクラスID
	/// </summary>
	HiyokoData,
}

/// <summary>
/// MessagePackでシリアライズする基本クラス
/// </summary>
public abstract class BaseClass {
	/// <summary>
	/// フィールド数、派生先クラスで上書きする
	/// </summary>
	protected const int FieldCount = 1;

	/// <summary>
	/// クラスID
	/// </summary>
	public ClassId ClassId;

	/// <summary>
	/// デフォルトコンストラクタ、MessagePackがこいつを必要としている
	/// </summary>
	BaseClass() {
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="id">派生先クラスID</param>
	public BaseClass(ClassId id) {
		this.ClassId = id;
	}

	/// <summary>
	/// パッキング
	/// </summary>
	/// <param name="packer">パッキング先</param>
	public abstract void PackToCore(Packer packer);

	/// <summary>
	/// アンパッキング
	/// </summary>
	/// <param name="unpacker">アンパッキング元</param>
	public abstract void UnpackFromCore(Unpacker unpacker);

	/// <summary>
	/// シリアライズする合計フィールド数の取得
	/// </summary>
	public abstract int GetFieldCount();
}

/// <summary>
/// ひよこ保存用データ
/// </summary>
public class HiyokoData : BaseClass {
	protected new const int FieldCount = BaseClass.FieldCount + 4; // 合計フィールド数をオーバーロード

	public int Kind;
	public float X, Y, Angle;

	public HiyokoData() : base(ClassId.HiyokoData) {
	}

	public override void PackToCore(Packer packer) {
		packer.Pack(this.Kind);
		packer.Pack(this.X);
		packer.Pack(this.Y);
		packer.Pack(this.Angle);
	}
	public override void UnpackFromCore(Unpacker unpacker) {
		unpacker.ReadInt32(out this.Kind);
		unpacker.ReadSingle(out this.X);
		unpacker.ReadSingle(out this.Y);
		unpacker.ReadSingle(out this.Angle);
	}
	public override int GetFieldCount() {
		return FieldCount;
	}
}

/// <summary>
/// 独自クラス処理オーバーライドさせるシリアライザ
/// </summary>
public class ClassSerializer : MessagePackSerializer<BaseClass> {
	static Dictionary<ClassId, Type> _Types; // シリアライズ可能型一覧

	/// <summary>
	/// 静的コンストラクタでシリアライズ可能な型一覧を初期化する
	/// </summary>
	static ClassSerializer() {
		_Types = new Dictionary<ClassId, Type>();
		_Types[ClassId.HiyokoData] = typeof(HiyokoData);
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	public ClassSerializer(SerializationContext ownerContext)
		: base(ownerContext) {
	}

	/// <summary>
	/// 独自クラス処理オーバーライドされたシリアライザを取得する
	/// </summary>
	/// <typeparam name="T">ルートクラスデータ型</typeparam>
	/// <returns>シリアライザ</returns>
	public static MessagePackSerializer<T> Get<T>() {
		var context = new SerializationContext();
		context.Serializers.RegisterOverride(new ClassSerializer(context));
		return MessagePackSerializer.Get<T>(context);
	}

	/// <summary>
	/// パッキング処理をオーバーライド、自作クラスをMsgPackの配列型としてパッキングする
	/// </summary>
	/// <param name="packer">パッキング先</param>
	/// <param name="objectTree">パッキングしたいオブジェクト</param>
	protected override void PackToCore(Packer packer, BaseClass objectTree) {
		packer.PackArrayHeader(objectTree.GetFieldCount()); // 配列の要素数セット
		packer.Pack((UInt32)objectTree.ClassId); // 先頭要素としてクラスIDセット
		objectTree.PackToCore(packer); // 後は派生先クラスに任せる
	}

	/// <summary>
	/// アンパッキング処理をオーバーライド、自作クラスをMsgPackの配列型としてアンパッキングする
	/// </summary>
	/// <param name="unpacker">アンパッキング元</param>
	/// <returns>生成したオブジェクト</returns>
	protected override BaseClass UnpackFromCore(Unpacker unpacker) {
		// 現在位置は配列の先頭でなければならない
		if (!unpacker.IsArrayHeader)
			throw SerializationExceptions.NewIsNotArrayHeader();

		// 既に読み込まれている配列要素数取得
		var length = unpacker.LastReadData.AsInt32();

		// 配列の先頭要素をクラスIDとして扱う
		UInt32 i;
		unpacker.ReadUInt32(out i);
		var id = (ClassId)i;

		// クラスIDに対応する型情報取得
		Type type;
		if (!_Types.TryGetValue(id, out type))
			throw new MsgPack.MessageTypeException(i.ToString() + " is not ClassId");

		// 型情報からオブジェクト生成
		BaseClass o = Activator.CreateInstance(type) as BaseClass;
		if (o.GetFieldCount() != length)
			throw new MsgPack.UnpackException(o.GetType() + " field count mismatch " + length);

		// 後は生成されたオブジェクトに任せる
		o.UnpackFromCore(unpacker);

		return o;
	}

	/// <summary>
	/// 指定されたファイルパスへ指定されたオブジェクトを保存する
	/// </summary>
	/// <typeparam name="T">オブジェクト型</typeparam>
	/// <param name="filePath">保存先ファイルパス名</param>
	/// <param name="obj">オブジェクト</param>
	public static void SaveObject<T>(string filePath, T obj) {
		using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
			var serializer = MessagePackSerializer.Get<T>();
			serializer.Pack(fs, obj);
		}
	}

	/// <summary>
	/// 指定されたファイルパスから指定されたオブジェクトを読み込む
	/// </summary>
	/// <typeparam name="T">オブジェクト型</typeparam>
	/// <param name="filePath">読み込み元ファイルパス名</param>
	/// <param name="defaultObj">ファイルが存在しない場合のデフォルト値</param>
	/// <returns>オブジェクト</returns>
	public static T LoadObject<T>(string filePath, T defaultObj) {
		if (!File.Exists(filePath))
			return defaultObj;

		using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
			var serializer = MessagePackSerializer.Get<T>();
			return serializer.Unpack(fs);
		}
	}

	/// <summary>
	/// クラスIDと型をバインドする
	/// </summary>
	/// <param name="id">クラスID</param>
	/// <param name="type">型</param>
	public static void BindType(ClassId id, Type type) {
		_Types[id] = type;
	}

	/// <summary>
	/// クラスIDと型をバインドする
	/// </summary>
	/// <typeparam name="T">クラス型</typeparam>
	/// <param name="id">クラスID</param>
	public static void BindType<T>(ClassId id) {
		BindType(id, typeof(T));
	}
}
