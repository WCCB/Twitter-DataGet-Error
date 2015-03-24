using UnityEngine;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour{
	private static T instance;
	public static T Instance{
		get{
			if(instance == null){
				instance = (T)FindObjectOfType(typeof(T));
				if(instance == null){
					Debug.LogError (typeof(T) + "is nothing");
				}
			}			
			return instance;
		}
	}

	public void Awake(){
		//既にインスタンスが存在していたらコンポーネントを削除する
		if(this != Instance)	{Destroy(this);		return;}

		//シーンのロード時に壊されないようにする
		DontDestroyOnLoad(this.gameObject);

		//初期化
		initialize();
	}

	protected virtual void initialize(){}
}