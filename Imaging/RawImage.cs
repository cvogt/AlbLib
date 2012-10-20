﻿using System;
using System.Drawing;
using System.IO;

namespace AlbLib.Imaging
{
	/// <summary>
	/// Image that consists only of pixels with no other data.
	/// </summary>
	[Serializable]
	public sealed class RawImage : ImageBase
	{
		/// <summary>
		/// Image width is not stored within image data.
		/// </summary>
		public int Width{get; set;}
		
		/// <summary>
		/// Image height is not stored within image data.
		/// </summary>
		public int Height{get; set;}
		
		/// <returns>Width</returns>
		public override int GetWidth()
		{
			return Width;
		}
		/// <returns>Height</returns>
		public override int GetHeight()
		{
			return Height;
		}
		
		/// <summary>
		/// Converts entire image to format-influenced byte array.
		/// </summary>
		/// <returns>
		/// Byte array containing image.
		/// </returns>
		public override byte[] ToRawData()
		{
			return ImageData;
		}
		
		/// <summary>
		/// Initializes new instance.
		/// </summary>
		public RawImage(byte[] rawdata)
		{
			if(rawdata.Length==0)return;
			ImageData = rawdata;
		}
		
		/// <summary>
		/// Initializes new instance.
		/// </summary>
		public RawImage(Stream stream, int length)
		{
			ImageData = new byte[length];
			stream.Read(ImageData, 0, length);
		}
		
		/// <summary>
		/// Initializes new instance.
		/// </summary>
		public RawImage(Stream stream, int width, int height)
		{
			ImageData = new byte[width*height];
			stream.Read(ImageData, 0, width*height);
			Width = width;
			Height = height;
		}
		
		/// <summary>
		/// Initializes new instance.
		/// </summary>
		public RawImage(byte[] data, int width)
		{
			Width = width;
			Height = (data.Length+width-1)/width;
			ImageData = data;
		}
		
		/// <summary>
		/// Initializes new instance.
		/// </summary>
		public RawImage(byte[] data, int width, int height)
		{
			Width = width;
			Height = height;
			ImageData = data;
		}
		
		/// <summary>
		/// Creates new instance.
		/// </summary>
		public static RawImage FromRawData(byte[] data)
		{
			if(data.Length==0)return null;
			return new RawImage(data);
		}
		
		/// <summary>
		/// Creates new instance.
		/// </summary>
		public static RawImage FromStream(Stream stream, int length)
		{
			return new RawImage(stream, length);
		}
		
		/// <summary>
		/// Creates new instance.
		/// </summary>
		public static RawImage FromBitmap(Bitmap bmp, ImagePalette palette)
		{
			return new RawImage(Drawing.LoadBitmap(bmp, palette), bmp.Width, bmp.Height);
		}
	}
}