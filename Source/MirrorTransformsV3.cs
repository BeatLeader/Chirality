using BeatmapSaveDataVersion3;
using System.Collections.Generic;

namespace Chirality.V3
{
    class MirrorTransforms
    {
        internal static System.Random rand;
        internal static List<BeatmapSaveDataCommon.NoteCutDirection> directions = new List<BeatmapSaveDataCommon.NoteCutDirection> {
            BeatmapSaveDataCommon.NoteCutDirection.Up, 
            BeatmapSaveDataCommon.NoteCutDirection.Down, 
            BeatmapSaveDataCommon.NoteCutDirection.Left, 
            BeatmapSaveDataCommon.NoteCutDirection.Right,
            BeatmapSaveDataCommon.NoteCutDirection.UpLeft, 
            BeatmapSaveDataCommon.NoteCutDirection.UpRight, 
            BeatmapSaveDataCommon.NoteCutDirection.DownLeft, 
            BeatmapSaveDataCommon.NoteCutDirection.DownRight,
            BeatmapSaveDataCommon.NoteCutDirection.Any,
            BeatmapSaveDataCommon.NoteCutDirection.None
        };
        internal static Dictionary<BeatmapSaveDataCommon.NoteCutDirection, BeatmapSaveDataCommon.NoteCutDirection> horizontal_cut_transform;
        internal static Dictionary<BeatmapSaveDataCommon.NoteCutDirection, BeatmapSaveDataCommon.NoteCutDirection> vertical_cut_transform;

        private static bool SliderTailPositionOverlapsWithNote(SliderData slider, NoteData note) => slider.tailLineIndex == note.lineIndex && slider.tailLineLayer == note.noteLineLayer;


        #region "Main Transform Functions"
        internal static BeatmapSaveData Mirror_Horizontal(BeatmapSaveData beatmapSaveData, int numberOfLines, bool flip_lines, bool remove_walls, bool is_ME)
        {
            // Bombs:
            List<BombNoteData> h_bombNotes = new List<BombNoteData>();
            foreach (BombNoteData bombNoteData in beatmapSaveData.bombNotes)
            {
                if (flip_lines == false)
                {
                    h_bombNotes.Add(new BombNoteData(bombNoteData.beat, bombNoteData.line, bombNoteData.layer));
                }
                else
                {
                    h_bombNotes.Add(new BombNoteData(bombNoteData.beat, numberOfLines - 1 - bombNoteData.line, bombNoteData.layer));
                }
            }

            // ColorNotes:
            List<ColorNoteData> h_colorNotes = new List<ColorNoteData>();
            foreach (ColorNoteData colorNote in beatmapSaveData.colorNotes)
            {
                h_colorNotes.Add(Mirror_Horizontal_Note(colorNote, numberOfLines, flip_lines, is_ME));
            }

            // Obstacles:
            List<BeatmapSaveDataVersion3.ObstacleData> h_obstacleDatas = new List<BeatmapSaveDataVersion3.ObstacleData>();
            if (remove_walls == false)
            {
                foreach (BeatmapSaveDataVersion3.ObstacleData obstacleData in beatmapSaveData.obstacles)
                {
                    h_obstacleDatas.Add(Mirror_Horizontal_Obstacle(obstacleData, numberOfLines, flip_lines));
                }
            }

            // Sliders:
            List<BeatmapSaveDataVersion3.SliderData> h_sliderDatas = new List<BeatmapSaveDataVersion3.SliderData>();
            foreach (BeatmapSaveDataVersion3.SliderData sliderData in beatmapSaveData.sliders)
            {
                h_sliderDatas.Add(Mirror_Horizontal_Slider(sliderData, numberOfLines, flip_lines, is_ME));
            }

            // BurstSliders:
            List<BurstSliderData> h_burstSliderDatas = new List<BurstSliderData>();
            foreach (BurstSliderData burstSliderData in beatmapSaveData.burstSliders)
            {
                BeatmapSaveDataCommon.NoteCutDirection headcutDirection = burstSliderData.headCutDirection;
                var mirroredNote = Mirror_Horizontal_BurstSlider(burstSliderData, numberOfLines, flip_lines, is_ME);
                if (mirroredNote.headCutDirection != headcutDirection) {
                    foreach (var note in h_colorNotes)
                    {
                        if (mirroredNote.headLine == note.line && mirroredNote.headLayer == note.layer && mirroredNote.beat == note.beat) {
                            note.x = mirroredNote.tx;
                        }
                    }

                    var tailLineIdex = mirroredNote.tailLine;
			        mirroredNote.tx = mirroredNote.headLine;
			        mirroredNote.x = tailLineIdex;
                }

                h_burstSliderDatas.Add(mirroredNote);
            }


            return new BeatmapSaveData(beatmapSaveData.bpmEvents, beatmapSaveData.rotationEvents, h_colorNotes, h_bombNotes, h_obstacleDatas, h_sliderDatas, 
                                       h_burstSliderDatas, beatmapSaveData.waypoints, beatmapSaveData.basicBeatmapEvents, beatmapSaveData.colorBoostBeatmapEvents, 
                                       beatmapSaveData.lightColorEventBoxGroups, beatmapSaveData.lightRotationEventBoxGroups, beatmapSaveData.lightTranslationEventBoxGroups, // 1.26.0 added LightTranslationEventBoxGroup 
                                       beatmapSaveData.vfxEventBoxGroups, beatmapSaveData._fxEventsCollection, beatmapSaveData.basicEventTypesWithKeywords, beatmapSaveData.useNormalEventsAsCompatibleEvents);
        }


        internal static BeatmapSaveData Mirror_Vertical(BeatmapSaveData beatmapSaveData, bool flip_rows, bool remove_walls, bool is_ME)
        {
            // Bombs:
            List<BombNoteData> v_bombNotes = new List<BombNoteData>();
            foreach (BombNoteData bombNoteData in beatmapSaveData.bombNotes)
            {
                if (flip_rows)
                {
                    v_bombNotes.Add(new BombNoteData(bombNoteData.beat, bombNoteData.line, 3 - 1 - bombNoteData.layer));
                }
                else
                {
                    v_bombNotes.Add(bombNoteData);
                }
            }

            // ColorNotes:
            List<ColorNoteData> v_colorNotes = new List<ColorNoteData>();
            foreach (ColorNoteData colorNote in beatmapSaveData.colorNotes)
            {
                v_colorNotes.Add(Mirror_Vertical_Note(colorNote, flip_rows, is_ME));
            }

            // Obstacles:
            List<BeatmapSaveDataVersion3.ObstacleData> v_obstacleDatas = new List<BeatmapSaveDataVersion3.ObstacleData>();
            if (remove_walls == false)
            {
                foreach (BeatmapSaveDataVersion3.ObstacleData obstacleData in beatmapSaveData.obstacles)
                {
                    v_obstacleDatas.Add(Mirror_Vertical_Obstacle(obstacleData, flip_rows));
                }
            }

            // Sliders:
            List<BeatmapSaveDataVersion3.SliderData> v_sliderDatas = new List<BeatmapSaveDataVersion3.SliderData>();
            foreach (BeatmapSaveDataVersion3.SliderData sliderData in beatmapSaveData.sliders)
            {
                v_sliderDatas.Add(Mirror_Vertical_Slider(sliderData, flip_rows, is_ME));
                //v_sliderDatas.Add((BeatmapSaveDataVersion3.SliderData)Mirror_Vertical_Slider_Generic(sliderData, flip_rows, is_ME));
            }

            // BurstSliders:
            List<BurstSliderData> v_burstSliderDatas = new List<BurstSliderData>();
            foreach (BurstSliderData burstSliderData in beatmapSaveData.burstSliders)
            {
                BeatmapSaveDataCommon.NoteCutDirection headcutDirection = burstSliderData.headCutDirection;
                var mirroredNote = Mirror_Vertical_BurstSlider(burstSliderData, flip_rows, is_ME);
                if (mirroredNote.headCutDirection != headcutDirection) {
                    foreach (var note in v_colorNotes)
                    {
                        if (mirroredNote.headLine == note.line && mirroredNote.headLayer == note.layer && mirroredNote.beat == note.beat) {
                            note.y = mirroredNote.ty;
                        }
                    }

                    var tailLineLayer = mirroredNote.tailLayer;
			        mirroredNote.ty = mirroredNote.headLayer;
			        mirroredNote.y = tailLineLayer;
                }
                v_burstSliderDatas.Add(mirroredNote);
                //v_burstSliderDatas.Add((BeatmapSaveDataVersion3.BurstSliderData)Mirror_Vertical_Slider_Generic(burstSliderData, flip_rows, is_ME));
            }


            return new BeatmapSaveData(beatmapSaveData.bpmEvents, beatmapSaveData.rotationEvents, v_colorNotes, v_bombNotes, v_obstacleDatas, v_sliderDatas,
                                       v_burstSliderDatas, beatmapSaveData.waypoints, beatmapSaveData.basicBeatmapEvents, beatmapSaveData.colorBoostBeatmapEvents,
                                       beatmapSaveData.lightColorEventBoxGroups, beatmapSaveData.lightRotationEventBoxGroups, beatmapSaveData.lightTranslationEventBoxGroups,
                                       beatmapSaveData.vfxEventBoxGroups, beatmapSaveData._fxEventsCollection, beatmapSaveData.basicEventTypesWithKeywords, beatmapSaveData.useNormalEventsAsCompatibleEvents);
        }


        internal static BeatmapSaveData Mirror_Inverse(BeatmapSaveData beatmapSaveData, int numberOfLines, bool flip_lines, bool flip_rows, bool remove_walls, bool is_ME)
        {
            //Plugin.Log.Debug("Mirror Inverse");
            
            return Mirror_Vertical(Mirror_Horizontal(beatmapSaveData, numberOfLines, flip_lines, remove_walls, is_ME), flip_rows, remove_walls, is_ME);
        }
        #endregion


        #region "Horizontal Transform Functions"

        internal static void Create_Horizontal_Transforms()
        {
            Plugin.Log.Debug("Create Horizontal Transforms");

            horizontal_cut_transform = new Dictionary<BeatmapSaveDataCommon.NoteCutDirection, BeatmapSaveDataCommon.NoteCutDirection>();

            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Up, BeatmapSaveDataCommon.NoteCutDirection.Up);
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Down, BeatmapSaveDataCommon.NoteCutDirection.Down);
                                        
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.UpLeft, BeatmapSaveDataCommon.NoteCutDirection.UpRight);
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.DownLeft, BeatmapSaveDataCommon.NoteCutDirection.DownRight);
                                         
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.UpRight, BeatmapSaveDataCommon.NoteCutDirection.UpLeft);
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.DownRight, BeatmapSaveDataCommon.NoteCutDirection.DownLeft);
                                         
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Left, BeatmapSaveDataCommon.NoteCutDirection.Right);
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Right, BeatmapSaveDataCommon.NoteCutDirection.Left);
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Any, BeatmapSaveDataCommon.NoteCutDirection.Any);
            horizontal_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.None, BeatmapSaveDataCommon.NoteCutDirection.None);
        }


        private static ColorNoteData Mirror_Horizontal_Note(ColorNoteData colorNoteData, int numberOfLines, bool flip_lines, bool is_ME)
        {
            int h_line;

            BeatmapSaveDataCommon.NoteColorType color;
            if (colorNoteData.color == BeatmapSaveDataCommon.NoteColorType.ColorA)
            {
                color = BeatmapSaveDataCommon.NoteColorType.ColorB;
            }
            else
            {
                color = BeatmapSaveDataCommon.NoteColorType.ColorA;
            }

            // Precision maps will not have indexes flipped (complicated math) but their colors will
            // Yes, it will be weird like streams will zigzag in the wrong direction...hence introducing chaos mode. Might as well make use of the weirdness!
            // Other option is to just not support ME and NE maps
            // Also Note: Not worth reusing check function because non-extended map block will become unnecessarily complicated

            if (colorNoteData.line >= 1000 || colorNoteData.line <= -1000)
            {
                h_line = colorNoteData.line / 1000 - 1; // Definition from ME
                color = colorNoteData.color; // Actually fixed the color swap here for BS 1.20.0
            }

            // Keep This Note: This isn't a robust way to check for extended maps
            /*if (noteData.lineIndex > 10 || noteData.lineIndex < 0) 
            {
                h_lineIndex = rand.Next(4); // ME chaos mode kekeke turns out this is too chaotic, not that fun
            }*/

            // Only non-precision-placement maps can have the option to be index flipped
            // Maps with extended non-precision-placement indexes are handled properly by numberOfLines
            else if (flip_lines)
            {
                h_line = numberOfLines - 1 - colorNoteData.line;
            }
            else
            {
                h_line = colorNoteData.line;
                color = colorNoteData.color;
            }

            BeatmapSaveDataCommon.NoteCutDirection h_cutDirection; // Yes, this is support for precision placement and ME LOL
            if (horizontal_cut_transform.TryGetValue(colorNoteData.cutDirection, out h_cutDirection) == false || is_ME)
            {
                h_cutDirection = Get_Random_Direction();
            }

            return new ColorNoteData(colorNoteData.beat, h_line, Check_Layer(colorNoteData.layer), color, h_cutDirection, colorNoteData.angleOffset);
        }


        private static BeatmapSaveDataVersion3.ObstacleData Mirror_Horizontal_Obstacle(BeatmapSaveDataVersion3.ObstacleData obstacleData, int numberOfLines, bool flip_lines)
        {
            if (flip_lines)
            {
                return new BeatmapSaveDataVersion3.ObstacleData(obstacleData.beat, numberOfLines - obstacleData.width - obstacleData.line, obstacleData.layer, obstacleData.duration, obstacleData.width, obstacleData.height);
            }

            return obstacleData;
        }

        private static BeatmapSaveDataVersion3.SliderData Mirror_Horizontal_Slider(BeatmapSaveDataVersion3.SliderData sliderData, int numberOfLines, bool flip_lines, bool is_ME)
        {
            int h_headline;
            int h_tailline;

            BeatmapSaveDataCommon.NoteColorType color;
            if (sliderData.colorType == BeatmapSaveDataCommon.NoteColorType.ColorA)
            {
                color = BeatmapSaveDataCommon.NoteColorType.ColorB;
            }
            else
            {
                color = BeatmapSaveDataCommon.NoteColorType.ColorA;
            }


            if (sliderData.headLine >= 1000 || sliderData.headLine <= -1000)
            {
                h_headline = sliderData.headLine / 1000 - 1; // Definition from ME
                color = sliderData.colorType; // Actually fixed the color swap here for BS 1.20.0
            }
            else if (flip_lines)
            {
                h_headline = numberOfLines - 1 - sliderData.headLine;
            }
            else
            {
                h_headline = sliderData.headLine;
                color = sliderData.colorType;
            }


            if (sliderData.tailLine >= 1000 || sliderData.tailLine <= -1000)
            {
                h_tailline = sliderData.tailLine / 1000 - 1; // Definition from ME
                color = sliderData.colorType; // Actually fixed the color swap here for BS 1.20.0
            }
            else if (flip_lines)
            {
                h_tailline = numberOfLines - 1 - sliderData.tailLine;
            }
            else
            {
                h_tailline = sliderData.tailLine;
                color = sliderData.colorType;
            }

            BeatmapSaveDataCommon.NoteCutDirection h_headcutDirection; // Yes, this is support for precision placement and ME LOL
            if (horizontal_cut_transform.TryGetValue(sliderData.headCutDirection, out h_headcutDirection) == false || is_ME)
            {
                h_headcutDirection = Get_Random_Direction();
            }

            BeatmapSaveDataCommon.NoteCutDirection h_tailcutDirection; // Yes, this is support for precision placement and ME LOL
            if (horizontal_cut_transform.TryGetValue(sliderData.tailCutDirection, out h_tailcutDirection) == false || is_ME)
            {
                h_headcutDirection = Get_Random_Direction();
            }

            return new BeatmapSaveDataVersion3.SliderData(color, sliderData.beat, h_headline, Check_Layer(sliderData.headLayer), sliderData.headControlPointLengthMultiplier, (BeatmapSaveDataCommon.NoteCutDirection)h_headcutDirection,
                                                  sliderData.tailBeat, h_tailline, Check_Layer(sliderData.tailLayer), sliderData.tailControlPointLengthMultiplier, (BeatmapSaveDataCommon.NoteCutDirection)h_tailcutDirection, sliderData.sliderMidAnchorMode);
        }


        private static BurstSliderData Mirror_Horizontal_BurstSlider(BurstSliderData burstSliderData, int numberOfLines, bool flip_lines, bool is_ME)
        {
            int h_headline;
            int h_tailline;

            BeatmapSaveDataCommon.NoteColorType color;
            if (burstSliderData.colorType == BeatmapSaveDataCommon.NoteColorType.ColorA)
            {
                color = BeatmapSaveDataCommon.NoteColorType.ColorB;
            }
            else
            {
                color = BeatmapSaveDataCommon.NoteColorType.ColorA;
            }


            if (burstSliderData.headLine >= 1000 || burstSliderData.headLine <= -1000)
            {
                h_headline = burstSliderData.headLine / 1000 - 1; // Definition from ME
                color = burstSliderData.colorType; // Actually fixed the color swap here for BS 1.20.0
            }
            else if (flip_lines)
            {
                h_headline = numberOfLines - 1 - burstSliderData.headLine;
            }
            else
            {
                h_headline = burstSliderData.headLine;
                color = burstSliderData.colorType;
            }


            if (burstSliderData.tailLine >= 1000 || burstSliderData.tailLine <= -1000)
            {
                h_tailline = burstSliderData.tailLine / 1000 - 1; // Definition from ME
                color = burstSliderData.colorType; // Actually fixed the color swap here for BS 1.20.0
            }
            else if (flip_lines)
            {
                h_tailline = numberOfLines - 1 - burstSliderData.tailLine;
            }
            else
            {
                h_tailline = burstSliderData.tailLine;
                color = burstSliderData.colorType;
            }

            BeatmapSaveDataCommon.NoteCutDirection h_headcutDirection; // Yes, this is support for precision placement and ME LOL
            if (horizontal_cut_transform.TryGetValue(burstSliderData.headCutDirection, out h_headcutDirection) == false || is_ME)
            {
                h_headcutDirection = Get_Random_Direction();
            }

            return new BurstSliderData(color, burstSliderData.beat, h_headline, Check_Layer(burstSliderData.headLayer), (BeatmapSaveDataCommon.NoteCutDirection)h_headcutDirection,
                                                  burstSliderData.tailBeat, h_tailline, Check_Layer(burstSliderData.tailLayer), burstSliderData.sliceCount, burstSliderData.squishAmount);
        }
        #endregion


        #region "Vertical Transform Functions"

        internal static void Create_Vertical_Transforms()
        {
            Plugin.Log.Debug("Create Vertical Transforms");

            vertical_cut_transform = new Dictionary<BeatmapSaveDataCommon.NoteCutDirection, BeatmapSaveDataCommon.NoteCutDirection>();

            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Up, BeatmapSaveDataCommon.NoteCutDirection.Down);
            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Down, BeatmapSaveDataCommon.NoteCutDirection.Up);

            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.UpLeft, BeatmapSaveDataCommon.NoteCutDirection.DownLeft);
            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.DownLeft, BeatmapSaveDataCommon.NoteCutDirection.UpLeft);

            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.UpRight, BeatmapSaveDataCommon.NoteCutDirection.DownRight);
            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.DownRight, BeatmapSaveDataCommon.NoteCutDirection.UpRight);

            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Left, BeatmapSaveDataCommon.NoteCutDirection.Left);
            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Right, BeatmapSaveDataCommon.NoteCutDirection.Right);
            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.Any, BeatmapSaveDataCommon.NoteCutDirection.Any);
            vertical_cut_transform.Add(BeatmapSaveDataCommon.NoteCutDirection.None, BeatmapSaveDataCommon.NoteCutDirection.None);
        }


        private static ColorNoteData Mirror_Vertical_Note(ColorNoteData colorNoteData, bool flip_rows, bool has_ME)
        {
            int v_layer;

            // All precision placements will not be layer-flipped (complicated math)
            // This could be weird, consider it part of chaos mode KEK
            if (colorNoteData.layer >= 1000 || colorNoteData.layer <= -1000)
            {
                v_layer = (colorNoteData.layer / 1000) - 1; // Definition from ME
            }

            // Keep This Note: This is not a robust way to check for extended maps (see above)
            /*if ((int)noteData.noteLineLayer > 2)
            {
                v_noteLineLayer = (NoteLineLayer)rand.Next(3); // ME chaos mode
            }*/

            // Only non-precision-placement maps can have the option to be layer flipped
            // Maps with extended layers but non-precision-placement (eg: noteLineLayer is 5) may have odd results. Consider that part of chaos mode lol
            else if (flip_rows)
            {
                v_layer = 3 - 1 - colorNoteData.layer;
            }
            else
            {
                v_layer = colorNoteData.layer;
            }

            BeatmapSaveDataCommon.NoteCutDirection v_cutDirection;
            if (vertical_cut_transform.TryGetValue(colorNoteData.cutDirection, out v_cutDirection) == false || has_ME)
            {
                v_cutDirection = Get_Random_Direction();
            }

            return new ColorNoteData(colorNoteData.beat, Check_Index(colorNoteData.line), v_layer, colorNoteData.color, v_cutDirection, colorNoteData.angleOffset);
        }


        private static BeatmapSaveDataVersion3.ObstacleData Mirror_Vertical_Obstacle(BeatmapSaveDataVersion3.ObstacleData obstacleData, bool flip_rows)
        {
            if (flip_rows)
            {
                return new BeatmapSaveDataVersion3.ObstacleData(obstacleData.beat, 0, 0, 0, 0, 0);
            }

            return obstacleData;
        }

        private static BeatmapSaveDataVersion3.SliderData Mirror_Vertical_Slider(BeatmapSaveDataVersion3.SliderData sliderData, bool flip_rows, bool has_ME)
        {
            int v_headlayer;
            int v_taillayer;

            if (sliderData.headLayer >= 1000 || sliderData.headLayer<= -1000)
            {
                v_headlayer = (sliderData.headLayer / 1000) - 1; // Definition from ME
            }
            else if (flip_rows)
            {
                v_headlayer = 3 - 1 - sliderData.headLayer;
            }
            else
            {
                v_headlayer = sliderData.headLayer;
            }


            if (sliderData.tailLayer >= 1000 || sliderData.tailLayer <= -1000)
            {
                v_taillayer = (sliderData.tailLayer / 1000) - 1; // Definition from ME
            }
            else if (flip_rows)
            {
                v_taillayer = 3 - 1 - sliderData.tailLayer;
            }
            else
            {
                v_taillayer = sliderData.tailLayer;
            }


            BeatmapSaveDataCommon.NoteCutDirection v_headcutDirection;
            if (vertical_cut_transform.TryGetValue(sliderData.headCutDirection, out v_headcutDirection) == false || has_ME)
            {
                v_headcutDirection = Get_Random_Direction();
            }

            BeatmapSaveDataCommon.NoteCutDirection v_tailcutDirection;
            if (vertical_cut_transform.TryGetValue(sliderData.tailCutDirection, out v_tailcutDirection) == false || has_ME)
            {
                v_tailcutDirection = Get_Random_Direction();
            }


            return new BeatmapSaveDataVersion3.SliderData(sliderData.colorType, sliderData.beat, Check_Index(sliderData.headLine), v_headlayer, sliderData.headControlPointLengthMultiplier, v_headcutDirection,
                                                              sliderData.tailBeat, Check_Index(sliderData.tailLine), v_taillayer, sliderData.tailControlPointLengthMultiplier, v_tailcutDirection, sliderData.sliderMidAnchorMode);
        }


        private static BurstSliderData Mirror_Vertical_BurstSlider(BurstSliderData burstSliderData, bool flip_rows, bool has_ME)
        {
            int v_headlayer;
            int v_taillayer;

            if (burstSliderData.headLayer >= 1000 || burstSliderData.headLayer <= -1000)
            {
                v_headlayer = (burstSliderData.headLayer / 1000) - 1; // Definition from ME
            }
            else if (flip_rows)
            {
                v_headlayer = 3 - 1 - burstSliderData.headLayer;
            }
            else
            {
                v_headlayer = burstSliderData.headLayer;
            }


            if (burstSliderData.tailLayer >= 1000 || burstSliderData.tailLayer <= -1000)
            {
                v_taillayer = (burstSliderData.tailLayer / 1000) - 1; // Definition from ME
            }
            else if (flip_rows)
            {
                v_taillayer = 3 - 1 - burstSliderData.tailLayer;
            }
            else
            {
                v_taillayer = burstSliderData.tailLayer;
            }


            BeatmapSaveDataCommon.NoteCutDirection v_headcutDirection;
            if (vertical_cut_transform.TryGetValue(burstSliderData.headCutDirection, out v_headcutDirection) == false || has_ME)
            {
                v_headcutDirection = Get_Random_Direction();
            }

            return new BurstSliderData(burstSliderData.colorType, burstSliderData.beat, Check_Index(burstSliderData.headLine), v_headlayer, v_headcutDirection,
                                                              burstSliderData.tailBeat, Check_Index(burstSliderData.tailLine), v_taillayer, burstSliderData.sliceCount, burstSliderData.squishAmount);
        }


        // Experiment with reusing this function. Not sure its actually better with casting in the main function
        /*private static BeatmapSaveDataVersion3.BaseSliderData Mirror_Vertical_Slider_Generic(BeatmapSaveDataVersion3.BaseSliderData baseSliderData, bool flip_rows, bool has_ME)
        {
            int v_head_noteLineLayer;
            int v_tail_noteLineLayer;

            if (baseSliderData.headLayer >= 1000 || baseSliderData.headLayer <= -1000)
            {
                v_head_noteLineLayer = (baseSliderData.headLayer / 1000) - 1; // Definition from ME
            }
            else if (flip_rows)
            {
                v_head_noteLineLayer = 3 - 1 - baseSliderData.headLayer;
            }
            else
            {
                v_head_noteLineLayer = baseSliderData.headLayer;
            }


            if (baseSliderData.tailLayer >= 1000 || baseSliderData.tailLayer <= -1000)
            {
                v_tail_noteLineLayer = (baseSliderData.tailLayer / 1000) - 1; // Definition from ME
            }
            else if (flip_rows)
            {
                v_tail_noteLineLayer = 3 - 1 - baseSliderData.tailLayer;
            }
            else
            {
                v_tail_noteLineLayer = baseSliderData.tailLayer;
            }


            NoteCutDirection v_headcutDirection;
            if (vertical_cut_transform.TryGetValue(baseSliderData.headCutDirection, out v_headcutDirection) == false || has_ME)
            {
                v_headcutDirection = Get_Random_Direction();
            }

            BeatmapSaveDataVersion3.SliderData sliderData;
            NoteCutDirection v_tailcutDirection;
            if ((sliderData = baseSliderData as BeatmapSaveDataVersion3.SliderData) != null)
            {
                if (vertical_cut_transform.TryGetValue(sliderData.tailCutDirection, out v_tailcutDirection) == false || has_ME)
                {
                    v_tailcutDirection = Get_Random_Direction();
                }

                return new BeatmapSaveDataVersion3.SliderData(baseSliderData.colorType, baseSliderData.beat, Check_Index(baseSliderData.headLine), v_head_noteLineLayer, sliderData.headControlPointLengthMultiplier, v_headcutDirection,
                                                      baseSliderData.tailBeat, Check_Index(baseSliderData.tailLine), v_tail_noteLineLayer, sliderData.tailControlPointLengthMultiplier, v_tailcutDirection, sliderData.sliderMidAnchorMode);
            }

            else
            {
                BeatmapSaveDataVersion3.BurstSliderData burstSliderData = (BeatmapSaveDataVersion3.BurstSliderData)baseSliderData;

                return new BeatmapSaveDataVersion3.BurstSliderData(baseSliderData.colorType, baseSliderData.beat, Check_Index(baseSliderData.headLine), v_head_noteLineLayer, v_headcutDirection,
                                                           baseSliderData.tailBeat, Check_Index(baseSliderData.tailLine), v_tail_noteLineLayer, burstSliderData.sliceCount, burstSliderData.squishAmount);
            }
        }*/

        #endregion


        #region "Utility Functions"
        internal static BeatmapSaveDataCommon.NoteCutDirection Get_Random_Direction()
        {
            int index = rand.Next(directions.Count);

            return directions[index];
        }

        internal static int Check_Index(int lineIndex)
        {
            if (lineIndex >= 500 || lineIndex <= -500)
            {
                return lineIndex / 1000;
                //return rand.Next(4); // ME chaos mode
            }

            return lineIndex;
        }

        internal static int Check_Layer(int lineLayer)
        {
            if (lineLayer >= 500 || lineLayer <= -500)
            {
                return lineLayer / 1000;
                //return (NoteLineLayer)rand.Next(3); // ME chaos mode
            }

            return lineLayer;
        }
        #endregion
    }
}