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
/// MessagePack処理の独自クラス処理オーバーライドするシリアライザ
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

	protected override void PackToCore(Packer packer, BaseClass objectTree) {
		packer.PackArrayHeader(objectTree.GetFieldCount());
		packer.Pack((UInt32)objectTree.ClassId);
		objectTree.PackToCore(packer);
	}

	protected override BaseClass UnpackFromCore(Unpacker unpacker) {
		if (!unpacker.IsArrayHeader)
			throw SerializationExceptions.NewIsNotArrayHeader();

		var length = unpacker.LastReadData.AsInt32();

		UInt32 i;
		unpacker.ReadUInt32(out i);
		var id = (ClassId)i;

		Type type;
		if (!_Types.TryGetValue(id, out type))
			throw new MsgPack.MessageTypeException(i.ToString() + " is not ClassId");

		BaseClass o = Activator.CreateInstance(type) as BaseClass;
		if (o.GetFieldCount() != length)
			throw new MsgPack.UnpackException(id + " field count mismatch");

		o.UnpackFromCore(unpacker);

		return o;
	}
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
	/// コンストラクタ、MessagePackからのみ呼び出される
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
	/// <param name="packer">MessagePackパッカー</param>
	public abstract void PackToCore(Packer packer);

	/// <summary>
	/// アンパッキング
	/// </summary>
	/// <param name="unpacker">MessagePackアンパッカー</param>
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
	/// <summary>
	/// 合計フィールド数を定数にしておく
	/// </summary>
	protected new const int FieldCount = BaseClass.FieldCount + 4;

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
