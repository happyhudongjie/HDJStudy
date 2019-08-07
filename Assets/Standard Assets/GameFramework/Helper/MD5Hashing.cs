using System;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

public static class MD5Hashing  {
	private static MD5 _md5;
	private static MD5 Md5{
		get{ 
			if (_md5 == null)
				_md5 = MD5.Create ();
			return _md5;
		}
	}

	public static string HashFile(string path){
		try{
			byte[] fileBytes = File.ReadAllBytes(path);
			return HashBytes(fileBytes);
		}catch(System.Exception e){
			Debug.LogError (e.Message);
			return "";
		}
	}

	public static string HashBytes(byte[] bytes){
		byte[] hashBytes = Md5.ComputeHash (bytes);
		string resule = System.BitConverter.ToString (hashBytes);
		resule = resule.Replace ("-", "");
		return resule;
	}

}
