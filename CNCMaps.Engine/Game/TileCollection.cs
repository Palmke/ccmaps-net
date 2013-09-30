﻿using System;
using System.Collections.Generic;
using System.Drawing;
using CNCMaps.Engine.Map;
using CNCMaps.FileFormats;
using CNCMaps.Shared;
using CNCMaps.VirtualFileSystem;

namespace CNCMaps.Engine.Game {

	public class TileCollection : GameCollection {

		static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
		readonly IniFile _theaterIni;
		readonly List<short> _tileNumToSet = new List<short>();
		readonly List<short> _setNumToFirstTile = new List<short>();
		readonly List<TileSet> _tileSets = new List<TileSet>();

		private readonly TheaterSettings _theaterSettings;

		public class TileSet {
			public string FileName { get; private set; }
			public string SetName { get; private set; }
			public int TilesInSet { get; private set; }
			public List<TileSetEntry> Entries { get; private set; }

			public TileSet(string fileName, string setName, int tilesInSet) {
				FileName = fileName;
				SetName = setName;
				TilesInSet = tilesInSet;
				Entries = new List<TileSetEntry>();
			}

			public override string ToString() {
				return SetName;
			}
		}
		public class TileSetEntry {
			static readonly Random RandomTileChooser = new Random();
			public readonly List<TmpFile> TmpFiles = new List<TmpFile>();
			public Drawable AnimationDrawable;
			public int AnimationSubtile = -1;
			public TileSet MemberOfSet { get; private set; }
			public int Index { get; private set; }

			public TileSetEntry(TileSet owner, int index) {
				MemberOfSet = owner;
				Index = index;
			}

			public void AddTile(TmpFile tmpFile) {
				TmpFiles.Add(tmpFile);
			}

			public void AddAnimation(int subtile, Drawable drawable) {
				AnimationSubtile = subtile;
				AnimationDrawable = drawable;
			}

			public TmpFile GetTmpFile(int subTile, bool damaged = false) {
				if (TmpFiles.Count == 0) return null;
				var randomChosen = TmpFiles[RandomTileChooser.Next(TmpFiles.Count)];
				// if this is not a randomizing tileset, but instead one with damaged data,
				// then return the "undamaged" version
				randomChosen.Initialize();
				if (randomChosen.Images[Math.Min(subTile, randomChosen.Images.Count - 1)].HasDamagedData)
					return TmpFiles[Math.Min(damaged ? 1 : 0, TmpFiles.Count - 1)];
				else
					return randomChosen;
			}

			public override string ToString() {
				return string.Format("{0} ({1})", MemberOfSet.SetName, Index);
			}
		}

		// ReSharper disable InconsistentNaming
		int ACliffMMPieces; short ACliffPieces; short BlackTile;
		short BridgeBottomLeft1; short BridgeBottomLeft2; short BridgeBottomRight1;
		short BridgeBottomRight2; short BridgeMiddle1; short BridgeMiddle2;
		short BridgeSet; short BridgeTopLeft1; short BridgeTopLeft2;
		short BridgeTopRight1; short BridgeTopRight2; short ClearTile;
		short ClearToGreenLat; short ClearToPaveLat; short ClearToRoughLat;
		short ClearToSandLat; short CliffRamps; short CliffSet;
		short DestroyableCliffs; short DirtRoadCurve; short DirtRoadJunction;
		short DirtRoadSlopes; short DirtRoadStraight; short DirtTrackTunnels;
		short DirtTunnels; short GreenTile; short HeightBase;
		short Ice1Set; short Ice2Set; short Ice3Set;
		short IceShoreSet; short MMRampBase; short MMWaterCliffAPieces;
		short Medians; short MiscPaveTile; short MonorailSlopes;
		short PaveTile; short PavedRoadEnds; short PavedRoadSlopes;
		short PavedRoads; short RampBase; short RampSmooth;
		short Rocks; short RoughGround; short RoughTile;
		short SandTile; short ShorePieces; short SlopeSetPieces;
		short SlopeSetPieces2; short TrackTunnels; short TrainBridgeSet;
		short Tunnels; short WaterBridge; short WaterCaves;
		short WaterCliffAPieces; short WaterCliffs; short WaterSet;
		short WaterfallEast; short WaterfallNorth; short WaterfallSouth;
		short WaterfallWest; short WoodBridgeSet;
		// ReSharper restore InconsistentNaming

		private int _animsSectionsStartIdx = -1;

		public TileCollection(TheaterSettings ths, IniFile theaterIni)
			: base() {
			_theaterSettings = ths;
			_theaterIni = theaterIni;
		}

		public TileCollection(CollectionType type, TheaterType theater, EngineType engine, IniFile rules, IniFile art,
			TheaterSettings theaterSettings, IniFile theaterIni = null)
			: base(type, theater, engine, rules, art) {

			_theaterSettings = theaterSettings;
			if (theaterIni == null) {
				_theaterIni = VFS.Open<IniFile>(theaterSettings.TheaterIni);
				if (_theaterIni == null) {
					Logger.Warn("Unavailable theater loaded, theater.ini not found");
					return;
				}
			}
			else _theaterIni = theaterIni;

			#region Set numbers

			IniFile.IniSection General = _theaterIni.GetSection("General");
			ACliffMMPieces = General.ReadShort("ACliffMMPieces", -1);
			ACliffPieces = General.ReadShort("ACliffPieces", -1);
			BlackTile = General.ReadShort("BlackTile", -1);
			BridgeBottomLeft1 = General.ReadShort("BridgeBottomLeft1", -1);
			BridgeBottomLeft2 = General.ReadShort("BridgeBottomLeft2", -1);
			BridgeBottomRight1 = General.ReadShort("BridgeBottomRight1", -1);
			BridgeBottomRight2 = General.ReadShort("BridgeBottomRight2", -1);
			BridgeMiddle1 = General.ReadShort("BridgeMiddle1", -1);
			BridgeMiddle2 = General.ReadShort("BridgeMiddle2", -1);
			BridgeSet = General.ReadShort("BridgeSet", -1);
			BridgeTopLeft1 = General.ReadShort("BridgeTopLeft1", -1);
			BridgeTopLeft2 = General.ReadShort("BridgeTopLeft2", -1);
			BridgeTopRight1 = General.ReadShort("BridgeTopRight1", -1);
			BridgeTopRight2 = General.ReadShort("BridgeTopRight2", -1);
			ClearTile = General.ReadShort("ClearTile", -1);
			ClearToGreenLat = General.ReadShort("ClearToGreenLat", -1);
			ClearToPaveLat = General.ReadShort("ClearToPaveLat", -1);
			ClearToRoughLat = General.ReadShort("ClearToRoughLat", -1);
			ClearToSandLat = General.ReadShort("ClearToSandLat", -1);
			CliffRamps = General.ReadShort("CliffRamps", -1);
			CliffSet = General.ReadShort("CliffSet", -1);
			DestroyableCliffs = General.ReadShort("DestroyableCliffs", -1);
			DirtRoadCurve = General.ReadShort("DirtRoadCurve", -1);
			DirtRoadJunction = General.ReadShort("DirtRoadJunction", -1);
			DirtRoadSlopes = General.ReadShort("DirtRoadSlopes", -1);
			DirtRoadStraight = General.ReadShort("DirtRoadStraight", -1);
			DirtTrackTunnels = General.ReadShort("DirtTrackTunnels", -1);
			DirtTunnels = General.ReadShort("DirtTunnels", -1);
			GreenTile = General.ReadShort("GreenTile", -1);
			HeightBase = General.ReadShort("HeightBase", -1);
			Ice1Set = General.ReadShort("Ice1Set", -1);
			Ice2Set = General.ReadShort("Ice2Set", -1);
			Ice3Set = General.ReadShort("Ice3Set", -1);
			IceShoreSet = General.ReadShort("IceShoreSet", -1);
			MMRampBase = General.ReadShort("MMRampBase", -1);
			MMWaterCliffAPieces = General.ReadShort("MMWaterCliffAPieces", -1);
			Medians = General.ReadShort("Medians", -1);
			MiscPaveTile = General.ReadShort("MiscPaveTile", -1);
			MonorailSlopes = General.ReadShort("MonorailSlopes", -1);
			PaveTile = General.ReadShort("PaveTile", -1);
			PavedRoadEnds = General.ReadShort("PavedRoadEnds", -1);
			PavedRoadSlopes = General.ReadShort("PavedRoadSlopes", -1);
			PavedRoads = General.ReadShort("PavedRoads", -1);
			RampBase = General.ReadShort("RampBase", -1);
			RampSmooth = General.ReadShort("RampSmooth", -1);
			Rocks = General.ReadShort("Rocks", -1);
			RoughGround = General.ReadShort("RoughGround", -1);
			RoughTile = General.ReadShort("RoughTile", -1);
			SandTile = General.ReadShort("SandTile", -1);
			ShorePieces = General.ReadShort("ShorePieces", -1);
			SlopeSetPieces = General.ReadShort("SlopeSetPieces", -1);
			SlopeSetPieces2 = General.ReadShort("SlopeSetPieces2", -1);
			TrackTunnels = General.ReadShort("TrackTunnels", -1);
			TrainBridgeSet = General.ReadShort("TrainBridgeSet", -1);
			Tunnels = General.ReadShort("Tunnels", -1);
			WaterBridge = General.ReadShort("WaterBridge", -1);
			WaterCaves = General.ReadShort("WaterCaves", -1);
			WaterCliffAPieces = General.ReadShort("WaterCliffAPieces", -1);
			WaterCliffs = General.ReadShort("WaterCliffs", -1);
			WaterSet = General.ReadShort("WaterSet", -1);
			WaterfallEast = General.ReadShort("WaterfallEast", -1);
			WaterfallNorth = General.ReadShort("WaterfallNorth", -1);
			WaterfallSouth = General.ReadShort("WaterfallSouth", -1);
			WaterfallWest = General.ReadShort("WaterfallWest", -1);
			WoodBridgeSet = General.ReadShort("WoodBridgeSet", -1);

			#endregion
		}

		public void InitTilesets() {
			int sectionIdx = 0;
			int setNum = 0;
			while (true) {
				string sectionName = "TileSet" + sectionIdx.ToString("0000");
				var sect = _theaterIni.GetSection(sectionName);
				if (sect == null)
					break;
				sectionIdx++;

				Logger.Trace("Loading tileset {0}", sectionName);
				var ts = new TileSet(sect.ReadString("FileName"), sect.ReadString("SetName"), sect.ReadInt("TilesInSet"));
				_setNumToFirstTile.Add((short)_drawables.Count);
				_tileSets.Add(ts);

				for (int j = 1; j <= ts.TilesInSet; j++) {
					_tileNumToSet.Add((short)setNum);
					var rs = new TileSetEntry(ts, j-1);

					// add as many tiles (with randomized name) as can be found
					for (var r = (char)('a' - 1); r <= 'z'; r++) {
						if ((r >= 'a') && ts.SetName == "Bridges") continue;

						// filename = set filename + dd + .tmp/.urb/.des etc
						string filename = ts.FileName + j.ToString("00");
						if (r >= 'a') filename += r;
						filename += _theaterSettings.Extension;
						var tmpFile = VFS.Open<TmpFile>(filename);
						if (tmpFile != null) rs.AddTile(tmpFile);
						else break;
					}
					ts.Entries.Add(rs);
					_drawables.Add(new TmpDrawable(null, null, rs));
				}
				setNum++;
			}

			_animsSectionsStartIdx = sectionIdx + 1;
		}

		public void InitAnimations(ObjectCollection animations) {
			// the remaining sections contain animations to be attached to certain tiles
			for (int j = _animsSectionsStartIdx; j < _theaterIni.Sections.Count; j++) {
				var extraSection = _theaterIni.Sections[j];
				var tileSet = _tileSets.Find(ts => ts.SetName == extraSection.Name);
				if (tileSet == null) continue;

				for (int a = 1; a <= tileSet.TilesInSet; a++) {
					string n = string.Format("Tile{0:00}", a);
					string anim = extraSection.ReadString(n + "Anim");
					var drawable = animations.GetDrawable(anim);

					if (string.IsNullOrEmpty(anim) || drawable == null) {
						Logger.Debug("Missing anim {0} for tileset {1}", anim, tileSet.SetName);
						continue;
					}

					// clone, so that the tile-specific offset doesn't require setting on the original 
					// drawable, meaning it can be reused
					drawable = drawable.Clone();

					// in pixels
					int attachTo = extraSection.ReadInt(n + "AttachesTo");
					var offset = new Point(extraSection.ReadInt(n + "XOffset"), extraSection.ReadInt(n + "YOffset"));
					drawable.Props.Offset.Offset(offset.X, offset.Y);
					tileSet.Entries[a - 1].AddAnimation(attachTo, drawable);
				}
			}
		}

		public bool ConnectTiles(int setNum1, int setNum2) {
			if (setNum1 == setNum2) return false;

			// grass doesn't connect with shores
			else if (setNum1 == GreenTile && setNum2 == ShorePieces ||
				(setNum2 == GreenTile && setNum1 == ShorePieces)) return false;

			// grass doesn't connect with waterbridges
			else if (setNum1 == GreenTile && setNum2 == WaterBridge ||
				(setNum2 == GreenTile && setNum1 == WaterBridge)) return false;

			// pave's don't connect with paved roads
			else if (setNum1 == PaveTile && setNum2 == PavedRoads ||
				(setNum2 == PaveTile && setNum1 == PavedRoads)) return false;

			// pave's don't connect with medians
			else if (setNum1 == PaveTile && setNum2 == Medians ||
				(setNum2 == PaveTile && setNum1 == Medians)) return false;

			// all other transitions are connected with a CLAT set
			return true;
		}

		public bool IsLAT(short setNum) {
			return
				setNum == RoughTile ||
				setNum == SandTile ||
				setNum == GreenTile ||
				setNum == PaveTile;
		}

		public bool IsCLAT(short setNum) {
			return
				setNum == ClearToRoughLat ||
				setNum == ClearToSandLat ||
				setNum == ClearToGreenLat ||
				setNum == ClearToPaveLat;
		}

		public short GetLAT(short clatSetNum) {
			if (clatSetNum == ClearToRoughLat)
				return RoughTile;
			else if (clatSetNum == ClearToSandLat)
				return SandTile;
			else if (clatSetNum == ClearToGreenLat)
				return GreenTile;
			else if (clatSetNum == ClearToPaveLat)
				return PaveTile;
			else
				return -1;
		}

		public short GetCLATSet(short setNum) {
			if (setNum == RoughTile)
				return ClearToRoughLat;
			else if (setNum == SandTile)
				return ClearToSandLat;
			else if (setNum == GreenTile)
				return ClearToGreenLat;
			else if (setNum == PaveTile)
				return ClearToPaveLat;
			else
				return -1;
		}

		public short GetSetNum(short tileNum) {
			if (tileNum < 0 || tileNum >= _tileNumToSet.Count) return 0;
			return _tileNumToSet[tileNum];
		}

		public short GetTileNumFromSet(short setNum, byte tileNumWithinSet = 0) {
			return (short)(_setNumToFirstTile[setNum] + tileNumWithinSet);
		}

		/// <summary>Recalculates tile system. </summary>
		public void RecalculateTileSystem(TileLayer tiles) {
			Logger.Info("Recalculating tile LAT system");

			// change all CLAT tiles to their corresponding LAT tiles
			foreach (MapTile t in tiles) {
				if (t == null) continue;

				// If this tile comes from a CLAT (connecting lat) set,
				// then replace it's set and tilenr by corresponding LAT sets'
				t.SetNum = GetSetNum(t.TileNum);

				if (IsCLAT(t.SetNum)) {
					t.SetNum = GetLAT(t.SetNum);
					t.TileNum = GetTileNumFromSet(t.SetNum);
				}
			}

			foreach (MapTile t in tiles) {
				if (t == null) continue;

				// If this tile is a LAT tile, we might have to connect it
				if (IsLAT(t.SetNum)) {
					// Which tile to use from CLAT tileset
					byte transitionTile = 0;

					// Find out setnums of adjacent cells

					MapTile tileTopRight = tiles.GetNeighbourTile(t, TileLayer.TileDirection.TopRight);
					if (tileTopRight != null && ConnectTiles(t.SetNum, tileTopRight.SetNum))
						transitionTile += 1;

					MapTile tileBottomRight = tiles.GetNeighbourTile(t, TileLayer.TileDirection.BottomRight);
					if (tileBottomRight != null && ConnectTiles(t.SetNum, tileBottomRight.SetNum))
						transitionTile += 2;

					MapTile tileBottomLeft = tiles.GetNeighbourTile(t, TileLayer.TileDirection.BottomLeft);
					if (tileBottomLeft != null && ConnectTiles(t.SetNum, tileBottomLeft.SetNum))
						transitionTile += 4;

					MapTile tileTopLeft = tiles.GetNeighbourTile(t, TileLayer.TileDirection.TopLeft);
					if (tileTopLeft != null && ConnectTiles(t.SetNum, tileTopLeft.SetNum))
						transitionTile += 8;

					if (transitionTile > 0) {
						// Find Tileset that contains the connecting pieces
						short clatSet = GetCLATSet(t.SetNum);
						// Do not change this setnum, as then we could recognize it as
						// a different tileset for later tiles around this one.
						// (T->SetNum = clatSet;)
						t.TileNum = GetTileNumFromSet(clatSet, transitionTile);
					}
				}

			}
		}

		public int NumTiles {
			get { return _drawables.Count; }
		}

	}
}