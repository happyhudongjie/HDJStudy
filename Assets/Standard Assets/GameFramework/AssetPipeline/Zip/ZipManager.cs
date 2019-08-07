using System;
using System.Collections;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using UnityEngine;

public class ZipManager  {

	public static void CompressFile(string inputFile,string outputFile,int compressLevel = Deflater.BEST_COMPRESSION,string password = ""){
		FileInfo fileInfo = new FileInfo (inputFile);
		string dir = Path.GetDirectoryName (outputFile);
		Directory.CreateDirectory (dir);
		FileStream fsOut = File.Create (outputFile);

		ZipOutputStream zipStream = new ZipOutputStream (fsOut);

		zipStream.SetLevel (compressLevel);		//0-9,0 being the highest level of compression
		zipStream.Password = password;			//optional。 NUll is the same as not setting.Required if using AES.

		string entryName = Path.GetFileName (inputFile);
		ZipEntry newEntry = new ZipEntry (entryName);
		newEntry.DateTime = fileInfo.LastWriteTime;
		newEntry.Size = fileInfo.Length;
		zipStream.PutNextEntry (newEntry);

		//Zip the file in buffered chunks
		//the "using" will close the stream even if an exception occurs
		byte[] buffer = new byte[4096];
		using (FileStream streamReader = File.OpenRead (inputFile)) {
			StreamUtils.Copy (streamReader, zipStream, buffer);
		}
		zipStream.CloseEntry ();

		//Makes the Close alse Close the underlying stream
		zipStream.IsStreamOwner = true;
		zipStream.Close ();
	}
}
