using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using UnityEngine;

public class ZipLibUtils {

	public static byte[] Compress(byte[] input){
		if (input == null || input.Length == 0) {
			Debug.LogError ("Compress error inputBytes len = 0");
			return input;
		}
		//Create the compressor wtih highest level of compression
		Deflater compressor = GetDeflater();
		compressor.SetLevel (Deflater.BEST_COMPRESSION);

		//Give the compressor with highest level of compression
		compressor.SetInput(input);
		compressor.Finish ();

		MemoryStream result = new MemoryStream (input.Length);
		byte[] buffer = new byte[1024];
		while (!compressor.IsFinished) {
			int count = compressor.Deflate (buffer);
			result.Write (buffer, 0, count);
		}

		return result.ToArray ();
	}

	private static Deflater _delfater ;
	private static Deflater GetDeflater(){
		if (_delfater == null)
			_delfater = new Deflater ();
		else
			_delfater.Reset ();
		return _delfater;
	}

	public static byte[] Uncompress(byte[] input){
		if (input == null || input.Length == 0) {
			Debug.LogError ("Uncompress error imputBytes len = 0");
			return input;
		}
		Inflater decompressor = GetInflater ();
		decompressor.SetInput (input);

		MemoryStream result = new MemoryStream (input.Length);

		byte[] buffer = new byte[4096];
		while (!decompressor.IsFinished) {
			int count = decompressor.Inflate (buffer);
			result.Write (buffer, 0, count);
		}
		return result.ToArray ();
	}

	private static Inflater _inflater;
	public static Inflater GetInflater(){
		if (_inflater == null)
			_inflater = new Inflater ();
		else
			_inflater.Reset ();
		return _inflater;
	}
}
