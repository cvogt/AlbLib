﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.IO;
using AlbLib.XLD;

namespace AlbLib.Imaging
{
	/// <summary>
	/// Color palette used when drawing images.
	/// </summary>
	public abstract class ImagePalette : IList<Color>
	{
		/// <summary>
		/// Returns Color at index in palette.
		/// </summary>
		public abstract Color this[int index]
		{
			get;
		}
		
		Color IList<Color>.this[int index]
		{
			get{
				return this[index];
			}
			set{
				throw new NotSupportedException();
			}
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		/// <summary>
		/// Enumerates through all colors in a palette.
		/// </summary>
		/// <returns>Enumerator object.</returns>
		public virtual IEnumerator<Color> GetEnumerator()
		{
			for(int i = 0; i < this.Length; i++)
			{
				yield return this[i];
			}
		}
		
		/// <summary>
		/// Palettes are always read-only.
		/// </summary>
		public bool IsReadOnly{
			get{
				return true;
			}
		}
		
		int ICollection<Color>.Count{
			get{
				return this.Length;
			}
		}
		
		/// <summary>
		/// Gets count of all colors.
		/// </summary>
		public abstract int Length{
			get;
		}
		
		bool ICollection<Color>.Remove(Color c)
		{
			throw new NotSupportedException();
		}
		
		/// <summary>
		/// Copies colors to another array.
		/// </summary>
		/// <param name="array">Output array.</param>
		/// <param name="index">Start index.</param>
		public virtual void CopyTo(Color[] array, int index)
		{
			for(int i = 0; i < this.Length; i++)
			{
				array[index+i] = this[i];
			}
		}
		
		/// <summary>
		/// Checks if palette contains given color.
		/// </summary>
		/// <param name="item">Color to check.</param>
		/// <returns>True if <paramref name="item"/> is in palette, otherwise false.</returns>
		public virtual bool Contains(Color item)
		{
			for(int i = 0; i < this.Length; i++)
			{
				if(this[i] == item)return true;
			}
			return false;
		}
		
		void ICollection<Color>.Clear()
		{
			throw new NotSupportedException();
		}
		
		void ICollection<Color>.Add(Color item)
		{
			throw new NotSupportedException();
		}
		
		void IList<Color>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
		
		void IList<Color>.Insert(int index, Color item)
		{
			throw new NotSupportedException();
		}
		
		/// <summary>
		/// Returns index of color in a palette.
		/// </summary>
		/// <param name="item">Color to find.</param>
		/// <returns>Zero-based index of <paramref name="item"/>.</returns>
		public virtual int IndexOf(Color item)
		{
			for(int i = 0; i < this.Length; i++)
			{
				if(this[i] == item)return i;
			}
			return -1;
		}
		
		/// <summary>
		/// Returns index of the nearest color to <paramref name="original"/>.
		/// </summary>
		/// <param name="original">Original color.</param>
		/// <returns>Nearest color index.</returns>
		public virtual int GetNearestColorIndex(Color original)
		{
			double input_red = original.R;
			double input_green = original.G;
			double input_blue = original.B;
			double distance = 500.0;
			int nearest_color_index = -1;
			for(int i = 0; i < this.Length; i++)
			{
				Color c = this[i];
				double test_red = Math.Pow(c.R - input_red, 2.0);
				double test_green = Math.Pow(c.G - input_green, 2.0);
				double test_blue = Math.Pow(c.B - input_blue, 2.0);
				double temp = Math.Sqrt(test_blue + test_green + test_red);
				if(temp == 0.0)
				{
					return i;
				}
				else if (temp < distance)
				{
					distance = temp;
					nearest_color_index = i;
				}
			}
			return nearest_color_index;
		}
		
		/// <summary>
		/// Converts palette to color array.
		/// </summary>
		/// <returns>Array containing palette colors.</returns>
		public Color[] ToArray()
		{
			Color[] arr = new Color[this.Length];
			this.CopyTo(arr, 0);
			return arr;
		}
		
		private static readonly ImagePalette[] Palettes = new ImagePalette[Byte.MaxValue];
		private static ImagePalette GlobalPalette = null;
		
		/// <summary>
		/// Default transparent color index. Default is -1.
		/// </summary>
		public static int TransparentIndex = -1;
		
		/// <summary>
		/// Loads all palettes.
		/// </summary>
		public static void LoadPalettes()
		{
			int id = 0;
			foreach(string path in Paths.PaletteN.EnumerateList())
			{
				using(FileStream stream = new FileStream(path, FileMode.Open))
				{
					foreach(XLDSubfile palette in XLDFile.EnumerateSubfiles(stream))
					{
						Palettes[id++] = ReadPalette(palette.Data);
					}
				}
			}
			using(FileStream stream = new FileStream(Paths.GlobalPalette, FileMode.Open))
			{
				GlobalPalette = ReadGlobalPalette((int)stream.Length, stream);
			}
		}
		
		private static void LoadGlobalPalette()
		{
			using(FileStream stream = new FileStream(Paths.GlobalPalette, FileMode.Open))
			{
				GlobalPalette = ReadGlobalPalette((int)stream.Length, stream);
			}
		}
		
		private static void LoadPalette(int index)
		{
			int subindex = index%100;
			int fileindex = index/100;
			using(FileStream stream = new FileStream(String.Format(Paths.PaletteN, fileindex), FileMode.Open))
			{
				int length = XLDFile.ReadToIndex(stream, subindex);
				Palettes[index] = ReadPalette(stream, length);
			}
		}
		
		/// <summary>
		/// Parses palette from stream.
		/// </summary>
		/// <param name="input">
		/// Input stream containing palette data.
		/// </param>
		/// <param name="length">
		/// Length of palette bytes. Usually triple of colors count.
		/// </param>
		/// <returns>
		/// Palette as color array.
		/// </returns>
		public static ImagePalette ReadPalette(Stream input, int length)
		{
			if(length%3!=0)
			{
				throw new Exception("Palette has not appropriate length.");
			}
			return ImagePalette.Load(input, length/3, PaletteFormat.Binary);
		}
		
		/// <summary>
		/// Parses palette from byte array.
		/// </summary>
		/// <param name="palette">
		/// Palette data as bytes. Usually multiple of three.
		/// </param>
		/// <returns>
		/// Palette as color array.
		/// </returns>
		public static ImagePalette ReadPalette(byte[] palette)
		{
			if(palette.Length%3!=0)
			{
				throw new Exception("Palette has not appropriate length.");
			}
			return ImagePalette.Load(new MemoryStream(palette), palette.Length/3, PaletteFormat.Binary);
		}
		
		private static ImagePalette ReadGlobalPalette(int length, Stream stream)
		{
			if(length != 192)
			{
				throw new Exception("Global palette has not appropriate length.");
			}
			return ImagePalette.Load(stream, length/3, PaletteFormat.Binary);
		}
		
		/// <summary>
		/// Returns the global palette which is used in combination with local palette.
		/// </summary>
		/// <returns>
		/// The global palette.
		/// </returns>
		public static ImagePalette GetGlobalPalette()
		{
			if(GlobalPalette == null)
			{
				LoadGlobalPalette();
			}
			return GlobalPalette;
		}
		
		/// <summary>
		/// Gets local palette using specified <paramref name="index"/>.
		/// </summary>
		/// <param name="index">
		/// Zero-based index.
		/// </param>
		/// <returns>
		/// The local palette.
		/// </returns>
		public static ImagePalette GetPalette(int index)
		{
			index -= 1;
			if(Palettes[index] == null)
			{
				LoadPalette(index);
			}
			return Palettes[index];
		}
		
		/// <summary>
		/// Joins local and global palettes.
		/// </summary>
		/// <param name="index">
		/// Zero-based local palette index.
		/// </param>
		/// <returns>
		/// The joined palette.
		/// </returns>
		public static ImagePalette GetFullPalette(int index)
		{
			return GetPalette(index)+GetGlobalPalette();
		}
		
		/// <summary>
		/// Loads palette from a <paramref name="file"/>.
		/// </summary>
		/// <param name="file">Path to a file.</param>
		/// <param name="numcolors">Number of colors in a palette.</param>
		/// <param name="format">Palette format.</param>
		/// <returns>Loaded palette.</returns>
		public static ImagePalette Load(string file, int numcolors, PaletteFormat format)
		{
			using(FileStream stream = new FileStream(file, FileMode.Open))
			{
				return Load(stream, numcolors, format);
			}
		}
		
		/// <summary>
		/// Loads palette from stream.
		/// </summary>
		/// <param name="sourceStream">Stream containg color data.</param>
		/// <param name="numcolors">Number of colors in a palette.</param>
		/// <param name="format">Palette format.</param>
		/// <returns>Loaded palette.</returns>
		public static ImagePalette Load(Stream sourceStream, int numcolors, PaletteFormat format)
		{
			Color[] colors = new Color[numcolors];
			switch(format)
			{
				case PaletteFormat.Binary:
					BinaryReader binReader = new BinaryReader(sourceStream);
					for(int i = 0; i < numcolors; i++)
					{
						colors[i] = Color.FromArgb(binReader.ReadByte(),binReader.ReadByte(),binReader.ReadByte());
					}
					break;
				case PaletteFormat.Text: case PaletteFormat.TextDOS:
					StreamReader strReader = new StreamReader(sourceStream);
					for(int i = 0; i < numcolors; i++)
					{
						string line = strReader.ReadLine();
						string[] split = line.Split(' ');
						if(format == PaletteFormat.Text)
							colors[i] = Color.FromArgb(Byte.Parse(split[0]), Byte.Parse(split[1]), Byte.Parse(split[2]));
						else
							colors[i] = Color.FromArgb(Convert.ToByte(Byte.Parse(split[0])*Constants.ColorConversion), Convert.ToByte(Byte.Parse(split[1])*Constants.ColorConversion), Convert.ToByte(Byte.Parse(split[2])*Constants.ColorConversion));
					}
					break;
				default:
					throw new NotImplementedException();
			}
			return new ListPalette(colors);
		}
		
		/// <summary>
		/// Loads palette in JASC format from stream.
		/// </summary>
		/// <param name="sourceStream">Stream containg color data.</param>
		/// <returns>Loaded palette.</returns>
		public static ImagePalette LoadJASC(Stream sourceStream)
		{
			StreamReader reader = new StreamReader(sourceStream);
			if(reader.ReadLine() != "JASC-PAL")throw new Exception("Not a JASC palette.");
			if(reader.ReadLine() != "0100")throw new Exception("Unknown version.");
			int colors = Int32.Parse(reader.ReadLine());
			return Load(sourceStream, colors, PaletteFormat.Text);
		}
		
		/// <summary>
		/// Creates new palette using color list.
		/// </summary>
		/// <param name="args">Colors.</param>
		/// <returns>Newly created palette.</returns>
		public static ImagePalette Create(params Color[] args)
		{
			return new ListPalette(args);
		}
		
		/// <summary>
		/// Creates new palette using color list.
		/// </summary>
		/// <param name="list">List of colors.</param>
		/// <returns>Newly created palette.</returns>
		public static ImagePalette Create(IList<Color> list)
		{
			return new ListPalette(list);
		}
		
		/// <summary>
		/// Joins two palettes together.
		/// </summary>
		/// <param name="a">Left palette.</param>
		/// <param name="b">Right palette.</param>
		/// <returns>Joined palette.</returns>
		public static ImagePalette Join(ImagePalette a, ImagePalette b)
		{
			return new JoinPalette(a,b);
		}
		
		/// <summary>
		/// Joins two palettes together.
		/// </summary>
		/// <param name="a">Left palette.</param>
		/// <param name="b">Right palette.</param>
		/// <returns>Joined palette.</returns>
		public static ImagePalette operator +(ImagePalette a, ImagePalette b)
		{
			return new JoinPalette(a,b);
		}
		
		/// <summary>
		/// Grayscale palette from black to white.
		/// </summary>
		public static ImagePalette Grayscale{
			get{
				return new GrayscalePalette();
			}
		}
		
		private sealed class GrayscalePalette : ImagePalette
		{
			public override int Length{
				get{
					return 256;
				}
			}
			
			public override Color this[int index]{
				get{
					return Color.FromArgb(index,index,index);
				}
			}
		}
		
		private sealed class JoinPalette : ImagePalette
		{
			private readonly ImagePalette left;
			private readonly ImagePalette right;
			
			public JoinPalette(ImagePalette a, ImagePalette b)
			{
				left = a;
				right = b;
			}
			
			public override int Length{
				get{
					return left.Length+right.Length;
				}
			}
			
			public override Color this[int index]{
				get{
					if(index<left.Length)
					{
						return left[index];
					}else{
						return right[index-left.Length];
					}
				}
			}
			
			public override IEnumerator<Color> GetEnumerator()
			{
				foreach(Color c in left)
					yield return c;
				foreach(Color c in right)
					yield return c;
			}
		}
		
		private sealed class ListPalette : ImagePalette
		{
			private readonly IList<Color> list;
			public ListPalette(IList<Color> list)
			{
				this.list = list;
			}
			
			public override int Length{
				get{
					return list.Count;
				}
			}
			
			public override Color this[int index]{
				get{
					return list[index];
				}
			}
			
			public override void CopyTo(Color[] array, int index)
			{
				list.CopyTo(array, index);
			}
			
			public override IEnumerator<Color> GetEnumerator()
			{
				return list.GetEnumerator();
			}
		}
	}
}