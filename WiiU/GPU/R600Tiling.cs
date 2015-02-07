using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiiU.GPU
{
	public class R600Tiling
	{
		private static readonly uint[] bankSwapOrder = { 0, 1, 3, 2, 6, 7, 5, 4 };

		uint m_class = 6;
		uint m_chipFamily = 0;
		uint m_chipRevision = 0;
		uint m_version = 502;
		uint m_pElemLib = 0;
		uint m_pipes = 2;//0;
		uint m_banks = 8;//0;
		uint m_pipeInterleaveBytes = 256;
		uint m_rowSize = 2048;//0;
		uint m_backendDisables = 0;
		uint m_configFlags = 0;

		uint m_swapSize = 256;//0;
		uint m_splitSize = 2048;//0;

		public struct _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_INPUT
		{
			public uint size;        //dd ?
			public uint x;         // dd ?
			public uint y;          //dd ?
			public uint slice;          //dd ?
			public uint sample;         //dd ?
			public uint bpp;         //dd ?
			public uint pitch;         //dd ?
			public uint height;       //dd ?
			public uint numSlices;      //dd ?
			public uint numSamples;      //dd ?
			public int tileMode;     // dd ?                    ; enum _AddrTileMode
			public bool isDepth;       //dd ?
			public uint tileBase;       //dd ?
			public uint compBits;     // dd ?
			public uint pipeSwizzle;     //dd ?
			public uint bankSwizzle;     //dd ?
			public uint numFrags;       //dd ?
			public int tileType;        //dd ?                    ; enum _AddrTileType
			public uint _bf72;        //dd ?
			public uint pTileInfo;       //dd ?                    ; offset
			public uint tileIndex;       //dd ?
		}

		public struct _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT
		{
			public uint size;    //       dd ?
			//byte                 db ? ; undefined
			//byte                 db ? ; undefined
			//byte                 db ? ; undefined
			//byte                 db ? ; undefined
			public ulong addr;      //    dq ?
			public uint bitPosition; //   dd ?
			//byte _padding        db 4 dup(?)
		}

		public ulong ComputeSurfaceAddrFromCoord(/*R600AddrLib *this, */ref _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_INPUT pIn, ref _ADDR_COMPUTE_SURFACE_ADDRFROMCOORD_OUTPUT pOut)
		{
			/* _AddrTileMode*/
			int v3; // ecx@4
			ulong v4 = 0; // qax@5
			ulong result; // qax@9
			/*_AddrTileMode*/
			int v6; // [sp+4h] [bp-4Ch]@1
			uint v7; // [sp+8h] [bp-48h]@2
			//uint /***/pBitPosition; // [sp+Ch] [bp-44h]@4
			uint bankSwizzle; // [sp+10h] [bp-40h]@4
			uint pipeSwizzle; // [sp+14h] [bp-3Ch]@4
			uint compBits; // [sp+18h] [bp-38h]@4
			uint tileBase; // [sp+1Ch] [bp-34h]@4
			bool isDepth; // [sp+20h] [bp-30h]@4
			/*_AddrTileMode*/
			int tileMode; // [sp+24h] [bp-2Ch]@4
			uint numSamples; // [sp+28h] [bp-28h]@4
			uint numSlices; // [sp+2Ch] [bp-24h]@1
			uint height; // [sp+30h] [bp-20h]@1
			uint pitch; // [sp+34h] [bp-1Ch]@1
			uint bpp; // [sp+38h] [bp-18h]@1
			uint sample; // [sp+3Ch] [bp-14h]@1
			uint slice; // [sp+40h] [bp-10h]@1
			uint y; // [sp+44h] [bp-Ch]@1
			uint x; // [sp+48h] [bp-8h]@1
			// AddrLib *thisa; // [sp+4Ch] [bp-4h]@1

			//memset(&v6, -858993460, 0x4Cu);
			// thisa = (AddrLib *)this;
			x = pIn.x;
			y = pIn.y;
			slice = pIn.slice;
			sample = pIn.sample;
			bpp = pIn.bpp;
			pitch = pIn.pitch;
			height = pIn.height;
			numSlices = pIn.numSlices;
			if (pIn.numSamples != 0)
				v7 = pIn.numSamples;
			else
				v7 = 1;
			numSamples = v7;
			tileMode = pIn.tileMode;
			isDepth = pIn.isDepth;
			tileBase = pIn.tileBase;
			compBits = pIn.compBits;
			pipeSwizzle = pIn.pipeSwizzle;
			bankSwizzle = pIn.bankSwizzle;
			//pBitPosition = &pOut->bitPosition;
			v6 = tileMode;
			v3 = tileMode;
			switch (tileMode)
			{
				case 0:
				case 1:
					/*v4 = ComputeSurfaceAddrFromCoordLinear(
						   thisa,
						   x,
						   y,
						   slice,
						   sample,
						   bpp,
						   pitch,
						   height,
						   numSlices,
						   pBitPosition);*/
					break;
				case 2:
				case 3:
					/* v4 = ComputeSurfaceAddrFromCoordMicroTiled(
							(R600AddrLib *)thisa,
							x,
							y,
							slice,
							bpp,
							pitch,
							height,
							tileMode,
							isDepth,
							tileBase,
							compBits,
							pBitPosition);*/
					break;
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
				case 12:
				case 13:
				case 14:
				case 15:
					v4 = ComputeSurfaceAddrFromCoordMacroTiled(
						//(R600AddrLib *)thisa,
						   x,
						   y,
						   slice,
						   sample,
						   bpp,
						   pitch,
						   height,
						   numSamples,
						   tileMode,
						   isDepth,
						   tileBase,
						   compBits,
						   pipeSwizzle,
						   bankSwizzle,
						   out pOut.bitPosition);// pBitPosition);
					break;
				default:
					v4 = 0;
					//HIDWORD(v4) = 0;
					break;
			}
			//LODWORD(result) = _RTC_CheckEsp(v3, HIDWORD(v4));
			//return result;
			return v4;
		}

		private static uint Log2_0(uint x)
		{
			uint y; // [sp+0h] [bp-4h]@1

			y = 0;
			while (x > 1)
			{
				x >>= 1;
				++y;
			}
			return y;
		}

		private ulong ComputeSurfaceAddrFromCoordMacroTiled(/*R600AddrLib *this, */uint x, uint y, uint slice, uint sample, uint bpp, uint pitch, uint height, uint numSamples, /*_AddrTileMode*/int tileMode, bool isDepth, uint tileBase, uint compBits, uint pipeSwizzle, uint bankSwizzle, /*unsigned int **/out uint pBitPosition)
		{
			/*_AddrTileType*/
			int v16; // eax@1
			ulong v17; // qax@24
			ulong result; // qax@24
			int v19; // [sp+8h] [bp-C8h]@1
			uint swapIndex; // [sp+Ch] [bp-C4h]@23
			uint bankSwapWidth; // [sp+10h] [bp-C0h]@23
			uint sliceIn; // [sp+14h] [bp-BCh]@12
			uint bankPipe; // [sp+18h] [bp-B8h]@12
			uint swizzle; // [sp+1Ch] [bp-B4h]@12
			uint rotation; // [sp+20h] [bp-B0h]@12
			uint tileSliceBits; // [sp+24h] [bp-ACh]@10
			uint bytesPerSample; // [sp+28h] [bp-A8h]@8
			uint numBanks; // [sp+2Ch] [bp-A4h]@1
			uint numPipes; // [sp+30h] [bp-A0h]@1
			ulong groupMask; // [sp+34h] [bp-9Ch]@24
			uint elemOffset; // [sp+54h] [bp-7Ch]@8
			uint pixelOffset; // [sp+58h] [bp-78h]@4
			uint sampleOffset; // [sp+5Ch] [bp-74h]@4
			uint pixelIndex; // [sp+60h] [bp-70h]@1
			ulong macroTileOffset; // [sp+64h] [bp-6Ch]@17
			uint macroTileIndexY; // [sp+6Ch] [bp-64h]@17
			uint macroTileIndexX; // [sp+70h] [bp-60h]@17
			long macroTileBytes; // [sp+74h] [bp-5Ch]@17
			uint macroTilesPerRow; // [sp+7Ch] [bp-54h]@17
			uint macroTileHeight; // [sp+80h] [bp-50h]@14
			uint macroTilePitch; // [sp+84h] [bp-4Ch]@14
			ulong sliceOffset; // [sp+88h] [bp-48h]@14
			ulong sliceBytes; // [sp+90h] [bp-40h]@14
			uint microTileThickness; // [sp+98h] [bp-38h]@1
			uint bank; // [sp+9Ch] [bp-34h]@12
			uint pipe; // [sp+A0h] [bp-30h]@12
			uint sampleSlice; // [sp+A4h] [bp-2Ch]@10
			uint numSampleSplits; // [sp+A8h] [bp-28h]@10
			uint samplesPerSlice; // [sp+ACh] [bp-24h]@10
			uint microTileBits; // [sp+B0h] [bp-20h]@1
			uint microTileBytes; // [sp+B4h] [bp-1Ch]@1
			uint numBankBits; // [sp+B8h] [bp-18h]@1
			uint numPipeBits; // [sp+BCh] [bp-14h]@1
			uint numGroupBits; // [sp+C0h] [bp-10h]@1
			//R600AddrLib *thisa; // [sp+CCh] [bp-4h]@1

			numPipes = this/*->baseclass_0*/.m_pipes;
			numBanks = this/*->baseclass_0*/.m_banks;
			numGroupBits = Log2_0(this/*->baseclass_0*/.m_pipeInterleaveBytes);
			numPipeBits = Log2_0(this/*a->baseclass_0*/.m_pipes);
			numBankBits = Log2_0(this/*a->baseclass_0*/.m_banks);
			microTileThickness = ComputeSurfaceThickness(tileMode);
			microTileBits = numSamples * bpp * (microTileThickness << 6);
			microTileBytes = numSamples * bpp * (microTileThickness << 6) >> 3;
			v16 = GetTileType(isDepth);
			pixelIndex = ComputePixelIndexWithinMicroTile(/*&thisa->baseclass_0,*/ x, y, slice, bpp, tileMode, v16);
			if (isDepth)
			{
				if (compBits != 0 && compBits != bpp)
				{
					sampleOffset = tileBase + compBits * sample;
					pixelOffset = numSamples * compBits * pixelIndex;
				}
				else
				{
					sampleOffset = bpp * sample;
					pixelOffset = numSamples * bpp * pixelIndex;
				}
			}
			else
			{
				sampleOffset = sample * microTileBits / numSamples;
				pixelOffset = bpp * pixelIndex;
			}
			elemOffset = pixelOffset + sampleOffset;
			pBitPosition = (pixelOffset + sampleOffset) % 8;//*pBitPosition = (pixelOffset + sampleOffset) % 8;
			bytesPerSample = microTileBytes / numSamples;
			if (numSamples <= 1 || microTileBytes <= this/*a->*/.m_splitSize)
			{
				samplesPerSlice = numSamples;
				numSampleSplits = 1;
				sampleSlice = 0;
			}
			else
			{
				samplesPerSlice = this/*a->*/.m_splitSize / bytesPerSample;
				numSampleSplits = numSamples / samplesPerSlice;
				numSamples = samplesPerSlice;
				tileSliceBits = microTileBits / numSampleSplits;
				sampleSlice = elemOffset / (microTileBits / numSampleSplits);
				elemOffset %= microTileBits / numSampleSplits;
			}
			elemOffset >>= 3;
			pipe = ComputePipeFromCoordWoRotation(/*thisa, */x, y);
			bank = ComputeBankFromCoordWoRotation(/*thisa, */x, y);
			bankPipe = pipe + numPipes * bank;
			rotation = ComputeSurfaceRotationFromTileMode(/*thisa, */tileMode);
			swizzle = pipeSwizzle + numPipes * bankSwizzle;
			sliceIn = slice;
			if (IsThickMacroTiled(tileMode))
				sliceIn >>= 2;
			bankPipe ^= numPipes * sampleSlice * ((numBanks >> 1) + 1) ^ (swizzle + sliceIn * rotation);
			bankPipe %= numPipes * numBanks;
			pipe = bankPipe % numPipes;
			bank = bankPipe / numPipes;
			sliceBytes = (height * (ulong)pitch * microTileThickness * bpp * numSamples + 7) / 8;
			sliceOffset = sliceBytes * (sampleSlice + numSampleSplits * slice) / microTileThickness;
			macroTilePitch = 8 * this/*a->baseclass_0*/.m_banks;
			macroTileHeight = 8 * this/*a->baseclass_0*/.m_pipes;
			v19 = tileMode - 5;
			switch (tileMode)
			{
				case 5:
				case 9:
					macroTilePitch >>= 1;
					macroTileHeight *= 2;
					break;
				case 6:
				case 10:
					macroTilePitch >>= 2;
					macroTileHeight *= 4;
					break;
				default:
					break;
			}
			macroTilesPerRow = pitch / macroTilePitch;
			macroTileBytes = (numSamples * microTileThickness * bpp * macroTileHeight * macroTilePitch + 7) >> 3;
			macroTileIndexX = x / macroTilePitch;
			macroTileIndexY = y / macroTileHeight;
			macroTileOffset = (x / macroTilePitch + pitch / macroTilePitch * y / macroTileHeight)
							* (ulong)((numSamples * microTileThickness * bpp * macroTileHeight * macroTilePitch + 7) >> 3);
			if (tileMode == 8 || tileMode == 9 || tileMode == 10 || tileMode == 11 || tileMode == 14 || tileMode == 15)
			{
				uint nop;
				bankSwapWidth = ComputeSurfaceBankSwappedWidth(/*thisa,*/ tileMode, bpp, numSamples, pitch, out nop);//0);
				swapIndex = macroTilePitch * macroTileIndexX / bankSwapWidth;
				bank ^= bankSwapOrder[swapIndex & (this/*a->baseclass_0*/.m_banks - 1)];
			}
			v17 = elemOffset + ((macroTileOffset + sliceOffset) >> ((byte)numBankBits + (byte)numPipeBits));
			groupMask = (1u << (int)numGroupBits) - 1;
			ulong addr = ((v17 & (ulong)~(long)((1 << (int)numGroupBits) - 1)) << ((byte)numBankBits
																	   + (byte)numPipeBits)) | groupMask & v17 | (pipe << (int)numGroupBits);
			addr = addr | (bank << (int)(numPipeBits + numGroupBits));
			return addr;
		}

		private static bool IsBankSwappedTileMode(/*_AddrTileMode*/ int tileMode)
		{
			bool bankSwapped; // [sp+4h] [bp-4h]@1

			bankSwapped = false;
			switch (tileMode)
			{
				case 8:
				case 9:
				case 10:
				case 11:
				case 14:
				case 15:
					bankSwapped = true;
					break;
				default:
					return bankSwapped;
			}
			return bankSwapped;
		}

		private static uint ComputeMacroTileAspectRatio(/*_AddrTileMode*/int tileMode)
		{
			uint ratio; // [sp+4h] [bp-4h]@1

			ratio = 1;
			switch (tileMode)
			{
				case 8:
				case 12:
				case 14:
					ratio = 1;
					break;
				case 5:
				case 9:
					ratio = 2;
					break;
				case 6:
				case 10:
					ratio = 4;
					break;
				default:
					return ratio;
			}
			return ratio;
		}

		private uint ComputeSurfaceBankSwappedWidth(/*R600AddrLib *this, *//*_AddrTileMode*/int tileMode, uint bpp, uint numSamples, uint pitch, out uint /***/pSlicesPerTile)
		{
			uint v6; // edx@6
			uint v7; // ecx@6
			uint v9; // [sp+4h] [bp-54h]@1
			uint v10; // [sp+8h] [bp-50h]@11
			uint v11; // [sp+Ch] [bp-4Ch]@8
			uint swapMin; // [sp+10h] [bp-48h]@10
			uint swapMax; // [sp+14h] [bp-44h]@10
			uint heightBytes; // [sp+18h] [bp-40h]@10
			uint swapWidth; // [sp+1Ch] [bp-3Ch]@10
			uint swapTiles; // [sp+20h] [bp-38h]@7
			uint factor; // [sp+24h] [bp-34h]@7
			uint bytesPerTileSlice; // [sp+28h] [bp-30h]@6
			uint samplesPerTile; // [sp+2Ch] [bp-2Ch]@1
			uint bytesPerSample; // [sp+30h] [bp-28h]@1
			int slicesPerTile; // [sp+34h] [bp-24h]@1
			uint groupSize; // [sp+38h] [bp-20h]@1
			uint splitSize; // [sp+3Ch] [bp-1Ch]@1
			uint rowSize; // [sp+40h] [bp-18h]@1
			uint swapSize; // [sp+44h] [bp-14h]@1
			uint numPipes; // [sp+48h] [bp-10h]@1
			uint numBanks; // [sp+4Ch] [bp-Ch]@1
			uint bankSwapWidth; // [sp+50h] [bp-8h]@1
			/*R600AddrLib *thisa;*/
			// [sp+54h] [bp-4h]@1

			//memset(&v9, -858993460, 0x54u);
			//thisa = this;
			bankSwapWidth = 0;
			numBanks = this/*->baseclass_0*/.m_banks;
			numPipes = this/*->baseclass_0*/.m_pipes;
			swapSize = this/*->*/.m_swapSize;
			rowSize = this/*->baseclass_0*/.m_rowSize;
			splitSize = this/*->*/.m_splitSize;
			groupSize = this/*->baseclass_0*/.m_pipeInterleaveBytes;
			slicesPerTile = 1;
			bytesPerSample = 8 * bpp & 0x1FFFFFFF;
			samplesPerTile = splitSize / bytesPerSample;
			if ((splitSize / bytesPerSample) != 0)
			{
				slicesPerTile = (int)(numSamples / samplesPerTile);
				if ((numSamples / samplesPerTile) == 0)
					slicesPerTile = 1;
			}
			pSlicesPerTile = (uint)slicesPerTile;
			//SafeAssign_2(pSlicesPerTile, slicesPerTile);
			if (IsThickMacroTiled(tileMode))
				numSamples = 4;
			bytesPerTileSlice = (uint)(numSamples * bytesPerSample / slicesPerTile);
			if (IsBankSwappedTileMode(tileMode))
			{
				factor = ComputeMacroTileAspectRatio(tileMode);
				swapTiles = (swapSize >> 1) / bpp;
				if (swapTiles != 0)
					v11 = swapTiles;
				else
					v11 = 1;
				v7 = v11 * 8 * numBanks;
				swapWidth = v11 * 8 * numBanks;
				heightBytes = (uint)(numSamples * factor * numPipes * bpp / slicesPerTile);
				swapMax = numPipes * numBanks * rowSize / heightBytes;
				swapMin = groupSize * 8 * numBanks / bytesPerTileSlice;
				if (numPipes * numBanks * rowSize / heightBytes >= swapWidth)
				{
					if (swapMin <= swapWidth)
						v9 = swapWidth;
					else
						v9 = swapMin;
					v7 = v9;
					v10 = v9;
				}
				else
				{
					v10 = swapMax;
				}
				v6 = v10;
				for (bankSwapWidth = v10; bankSwapWidth >= 2 * pitch; bankSwapWidth >>= 1)
					v7 = bankSwapWidth >> 1;
			}
			return bankSwapWidth;//_RTC_CheckEsp(v7, v6);
		}

		private static bool IsThickMacroTiled(/*_AddrTileMode*/int tileMode)
		{
			bool thickMacroTiled; // [sp+4h] [bp-4h]@1

			thickMacroTiled = false;
			switch (tileMode)
			{
				case 7:
				case 11:
				case 13:
				case 15:
					thickMacroTiled = true;
					break;
				default:
					return thickMacroTiled;
			}
			return thickMacroTiled;
		}

		private uint ComputeSurfaceRotationFromTileMode(/*R600AddrLib *this, *//*_AddrTileMode*/int tileMode)
		{
			uint result; // eax@2
			uint v3; // [sp+0h] [bp-14h]@4
			uint pipes; // [sp+Ch] [bp-8h]@1

			pipes = this/*->baseclass_0*/.m_pipes;
			switch (tileMode)
			{
				case 4:
				case 5:
				case 6:
				case 7:
				case 8:
				case 9:
				case 10:
				case 11:
					result = pipes * ((this/*->baseclass_0*/.m_banks >> 1) - 1);
					break;
				case 12:
				case 13:
				case 14:
				case 15:
					if (pipes >= 4)
						v3 = (pipes >> 1) - 1;
					else
						v3 = 1;
					result = v3;
					break;
				default:
					result = 0;
					break;
			}
			return result;
		}

		private uint ComputePipeFromCoordWoRotation(/*R600AddrLib *this, */uint x, uint y)
		{
			int pipe; // [sp+14h] [bp-8h]@2

			switch (this/*->baseclass_0*/.m_pipes)
			{
				case 1u:
					pipe = 0;
					break;
				case 2u:
					pipe = ((byte)(y >> 3) ^ (byte)(x >> 3)) & 1;
					break;
				case 4u:
					pipe = ((byte)(y >> 3) ^ (byte)(x >> 4)) & 1 | 2
						* (((byte)(y >> 4) ^ (byte)(x >> 3)) & 1);
					break;
				case 8u:
					pipe = ((byte)(y >> 3) ^ (byte)(x >> 5)) & 1 | 2
						* (((byte)(y >> 4) ^ (byte)((x >> 5) ^ (x >> 4))) & 1) | 4 * (((byte)(y >> 5) ^ (byte)(x >> 3)) & 1);
					break;
				default:
					pipe = 0;
					break;
			}
			return (uint)pipe;
		}

		private uint ComputeBankFromCoordWoRotation(/*R600AddrLib *this, */uint x, uint y)
		{
			uint bankOpt; // [sp+8h] [bp-20h]@1
			uint numBanks; // [sp+Ch] [bp-1Ch]@1
			uint numPipes; // [sp+10h] [bp-18h]@1
			uint bankBit0; // [sp+1Ch] [bp-Ch]@4
			uint bankBit0a; // [sp+1Ch] [bp-Ch]@8
			uint bank; // [sp+20h] [bp-8h]@3

			numPipes = this/*->baseclass_0*/.m_pipes;
			numBanks = this/*->baseclass_0*/.m_banks;
			bankOpt = (this/*->baseclass_0*/.m_configFlags/*.value*/ >> 1) & 1;
			if (numBanks == 4)
			{
				bankBit0 = (y / (16 * numPipes) ^ (byte)(x >> 3)) & 1;
				if (bankOpt == 1 && numPipes == 8)
					bankBit0 ^= x / 0x20 & 1;
				bank = bankBit0 | 2 * ((y / (8 * numPipes) ^ (byte)(x >> 4)) & 1);
			}
			else
			{
				if (this/*->baseclass_0*/.m_banks == 8)
				{
					bankBit0a = (y / (32 * numPipes) ^ (byte)(x >> 3)) & 1;
					if (bankOpt == 1 && numPipes == 8)
						bankBit0a ^= x / (8 * numBanks) & 1;
					bank = bankBit0a | 2 * ((y / (32 * numPipes) ^ (byte)(y / (16 * numPipes) ^ (x >> 4))) & 1) | 4 * ((y / (8 * numPipes) ^ (byte)(x >> 5)) & 1);
				}
				else
				{
					bank = 0;
				}
			}
			return bank;
		}

		private static int GetTileType(bool isDepth)
		{
			return (isDepth) ? 1 : 0;
		}

		private uint ComputePixelIndexWithinMicroTile(/*AddrLib *this,*/  uint x, uint y, uint z, uint bpp, /*_AddrTileMode*/int tileMode, /*_AddrTileType*/int microTileType)
		{
			uint v8; // [sp+4h] [bp-34h]@1
			uint thickness; // [sp+8h] [bp-30h]@1
			uint pixelBit8; // [sp+10h] [bp-28h]@1
			uint pixelBit7; // [sp+14h] [bp-24h]@1
			uint pixelBit6; // [sp+18h] [bp-20h]@1
			uint pixelBit5; // [sp+1Ch] [bp-1Ch]@4
			uint pixelBit4; // [sp+20h] [bp-18h]@4
			uint pixelBit3; // [sp+24h] [bp-14h]@4
			uint pixelBit2; // [sp+28h] [bp-10h]@4
			uint pixelBit1; // [sp+2Ch] [bp-Ch]@4
			uint pixelBit0; // [sp+30h] [bp-8h]@4
			// AddrLib *thisa; // [sp+34h] [bp-4h]@1

			// memset(&v8, -858993460, 0x34u);
			//thisa = this;
			pixelBit6 = 0;
			pixelBit7 = 0;
			pixelBit8 = 0;
			thickness = ComputeSurfaceThickness(tileMode);
			if (microTileType == 3)
			{
				pixelBit0 = x & 1;
				pixelBit1 = y & 1;
				pixelBit2 = z & 1;
				pixelBit3 = (x & 2) >> 1;
				pixelBit4 = (y & 2) >> 1;
				pixelBit5 = (z & 2) >> 1;
				pixelBit6 = (x & 4) >> 2;
				pixelBit7 = (y & 4) >> 2;
			}
			else
			{
				if (microTileType != 0)
				{
					pixelBit0 = x & 1;
					pixelBit1 = y & 1;
					pixelBit2 = (x & 2) >> 1;
					pixelBit3 = (y & 2) >> 1;
					pixelBit4 = (x & 4) >> 2;
					pixelBit5 = (y & 4) >> 2;
				}
				else
				{
					v8 = bpp - 8;
					switch (bpp)
					{
						case 8u:
							pixelBit0 = x & 1;
							pixelBit1 = (x & 2) >> 1;
							pixelBit2 = (x & 4) >> 2;
							pixelBit3 = (y & 2) >> 1;
							pixelBit4 = y & 1;
							pixelBit5 = (y & 4) >> 2;
							break;
						case 0x10u:
							pixelBit0 = x & 1;
							pixelBit1 = (x & 2) >> 1;
							pixelBit2 = (x & 4) >> 2;
							pixelBit3 = y & 1;
							pixelBit4 = (y & 2) >> 1;
							pixelBit5 = (y & 4) >> 2;
							break;
						case 0x20u:
						case 0x60u:
							pixelBit0 = x & 1;
							pixelBit1 = (x & 2) >> 1;
							pixelBit2 = y & 1;
							pixelBit3 = (x & 4) >> 2;
							pixelBit4 = (y & 2) >> 1;
							pixelBit5 = (y & 4) >> 2;
							break;
						case 0x40u:
							pixelBit0 = x & 1;
							pixelBit1 = y & 1;
							pixelBit2 = (x & 2) >> 1;
							pixelBit3 = (x & 4) >> 2;
							pixelBit4 = (y & 2) >> 1;
							pixelBit5 = (y & 4) >> 2;
							break;
						case 0x80u:
							pixelBit0 = y & 1;
							pixelBit1 = x & 1;
							pixelBit2 = (x & 2) >> 1;
							pixelBit3 = (x & 4) >> 2;
							pixelBit4 = (y & 2) >> 1;
							pixelBit5 = (y & 4) >> 2;
							break;
						default:
							pixelBit0 = x & 1;
							pixelBit1 = (x & 2) >> 1;
							pixelBit2 = y & 1;
							pixelBit3 = (x & 4) >> 2;
							pixelBit4 = (y & 2) >> 1;
							pixelBit5 = (y & 4) >> 2;
							break;
					}
				}
				if (thickness > 1)
				{
					pixelBit6 = z & 1;
					pixelBit7 = (z & 2) >> 1;
				}
			}
			if (thickness == 8) pixelBit8 = (z & 4) >> 2;
			return (pixelBit8 << 8) | (pixelBit7 << 7) | (pixelBit6 << 6) | 32 * pixelBit5 | 16 * pixelBit4 | 8 * pixelBit3 | 4 * pixelBit2 | pixelBit0 | 2 * pixelBit1;
		}

		private static uint ComputeSurfaceThickness(/*_AddrTileMode*/int tileMode)
		{
			uint thickness; // [sp+4h] [bp-4h]@2

			switch (tileMode)
			{
				case 3:
				case 7:
				case 11:
				case 13:
				case 15:
					thickness = 4;
					break;
				case 16:
				case 17:
					thickness = 8;
					break;
				default:
					thickness = 1;
					break;
			}
			return thickness;
		}
	}
}
