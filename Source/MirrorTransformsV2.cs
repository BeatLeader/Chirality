// BS <=1.19.1
using BeatmapSaveDataVersion2_6_0AndEarlier;
using System.Collections.Generic;

namespace Chirality.V2
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
            BeatmapSaveDataCommon.NoteCutDirection.None};
        internal static Dictionary<BeatmapSaveDataCommon.NoteCutDirection, BeatmapSaveDataCommon.NoteCutDirection> horizontal_cut_transform;
        internal static Dictionary<BeatmapSaveDataCommon.NoteCutDirection, BeatmapSaveDataCommon.NoteCutDirection> vertical_cut_transform;


        #region "Main Transform Functions"
        internal static BeatmapSaveData Mirror_Horizontal(BeatmapSaveData beatmapSaveData, int numberOfLines, bool flip_lines, bool remove_walls, bool is_ME)
        {
            List<BeatmapSaveDataVersion2_6_0AndEarlier.NoteData> h_colorNotes = new List<BeatmapSaveDataVersion2_6_0AndEarlier.NoteData>();
            foreach (var colorNote in beatmapSaveData.notes)
            {
                // Bombs:
                if (colorNote.type != NoteType.NoteA && colorNote.type != NoteType.NoteB) {
                    if (flip_lines == false)
                    {
                        h_colorNotes.Add(new BeatmapSaveDataVersion2_6_0AndEarlier.NoteData(colorNote.time, colorNote.lineIndex, colorNote.lineLayer, colorNote.type, colorNote.cutDirection));
                    }
                    else
                    {
                        h_colorNotes.Add(new BeatmapSaveDataVersion2_6_0AndEarlier.NoteData(colorNote.time, numberOfLines - 1 - colorNote.lineIndex, colorNote.lineLayer, colorNote.type, colorNote.cutDirection));
                    }
                } else {
                    h_colorNotes.Add(Mirror_Horizontal_Note(colorNote, numberOfLines, flip_lines, is_ME));
                }
            }

            // Obstacles:
            var h_obstacleDatas = new List<BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData>();
            if (remove_walls == false)
            {
                foreach (var obstacleData in beatmapSaveData.obstacles)
                {
                    h_obstacleDatas.Add(Mirror_Horizontal_Obstacle(obstacleData, numberOfLines, flip_lines));
                }
            }

            // Sliders:
            var h_sliderDatas = new List<BeatmapSaveDataVersion2_6_0AndEarlier.SliderData>();
            foreach (var sliderData in beatmapSaveData.sliders)
            {
                h_sliderDatas.Add(Mirror_Horizontal_Slider(sliderData, numberOfLines, flip_lines, is_ME));
            }

            return new BeatmapSaveData(beatmapSaveData.events, h_colorNotes, h_sliderDatas, beatmapSaveData.waypoints, h_obstacleDatas, beatmapSaveData.specialEventsKeywordFilters);
        }


        internal static BeatmapSaveData Mirror_Vertical(BeatmapSaveData beatmapSaveData, bool flip_rows, bool remove_walls, bool is_ME)
        {
            
            // ColorNotes:
            List<BeatmapSaveDataVersion2_6_0AndEarlier.NoteData> v_colorNotes = new List<BeatmapSaveDataVersion2_6_0AndEarlier.NoteData>();
            foreach (var colorNote in beatmapSaveData.notes)
            {
                // Bombs:
                if (colorNote.type != NoteType.NoteA && colorNote.type != NoteType.NoteB) {
                    if (flip_rows)
                    {
                        v_colorNotes.Add(new BeatmapSaveDataVersion2_6_0AndEarlier.NoteData(colorNote.time, colorNote.lineIndex, 3 - 1 - colorNote.lineLayer, colorNote.type, colorNote.cutDirection));
                    }
                    else
                    {
                        v_colorNotes.Add(colorNote);
                    }
                } else {
                    v_colorNotes.Add(Mirror_Vertical_Note(colorNote, flip_rows, is_ME));
                }
            }

            // Obstacles:
            var v_obstacleDatas = new List<BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData>();
            if (remove_walls == false)
            {
                foreach (var obstacleData in beatmapSaveData.obstacles)
                {
                    v_obstacleDatas.Add(Mirror_Vertical_Obstacle(obstacleData, flip_rows));
                }
            }

            // Sliders:
            var v_sliderDatas = new List<BeatmapSaveDataVersion2_6_0AndEarlier.SliderData>();
            foreach (var sliderData in beatmapSaveData.sliders)
            {
                v_sliderDatas.Add(Mirror_Vertical_Slider(sliderData, flip_rows, is_ME));
                //v_sliderDatas.Add((SliderData)Mirror_Vertical_Slider_Generic(sliderData, flip_rows, is_ME));
            }


            return new BeatmapSaveData(beatmapSaveData.events, v_colorNotes, v_sliderDatas, beatmapSaveData.waypoints, v_obstacleDatas, beatmapSaveData.specialEventsKeywordFilters);
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


        private static BeatmapSaveDataVersion2_6_0AndEarlier.NoteData Mirror_Horizontal_Note(BeatmapSaveDataVersion2_6_0AndEarlier.NoteData colorNoteData, int numberOfLines, bool flip_lines, bool is_ME)
        {
            int h_line;

            NoteType color;
            if (colorNoteData.type == NoteType.NoteA)
            {
                color = NoteType.NoteB;
            }
            else
            {
                color = NoteType.NoteA;
            }

            // Precision maps will not have indexes flipped (complicated math) but their colors will
            // Yes, it will be weird like streams will zigzag in the wrong direction...hence introducing chaos mode. Might as well make use of the weirdness!
            // Other option is to just not support ME and NE maps
            // Also Note: Not worth reusing check function because non-extended map block will become unnecessarily complicated

            if (colorNoteData.lineIndex >= 1000 || colorNoteData.lineIndex <= -1000)
            {
                h_line = colorNoteData.lineIndex / 1000 - 1; // Definition from ME
                color = colorNoteData.type; // Actually fixed the color swap here for BS 1.20.0
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
                h_line = numberOfLines - 1 - colorNoteData.lineIndex;
            }
            else
            {
                h_line = colorNoteData.lineIndex;
                color = colorNoteData.type;
            }

            BeatmapSaveDataCommon.NoteCutDirection h_cutDirection; // Yes, this is support for precision placement and ME LOL
            if (horizontal_cut_transform.TryGetValue(colorNoteData.cutDirection, out h_cutDirection) == false || is_ME)
            {
                h_cutDirection = Get_Random_Direction();
            }

            return new BeatmapSaveDataVersion2_6_0AndEarlier.NoteData(colorNoteData.time, h_line, (BeatmapSaveDataCommon.NoteLineLayer)Check_Layer((int)colorNoteData.lineLayer), color, h_cutDirection);
        }


        private static BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData Mirror_Horizontal_Obstacle(BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData obstacleData, int numberOfLines, bool flip_lines)
        {
            if (flip_lines && obstacleData.type == ObstacleType.FullHeight)
            {
                return new BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData(obstacleData.time, numberOfLines - obstacleData.width - obstacleData.lineIndex, ObstacleType.FullHeight, obstacleData.duration, obstacleData.width);
            }

            return obstacleData;
        }

        private static BeatmapSaveDataVersion2_6_0AndEarlier.SliderData Mirror_Horizontal_Slider(BeatmapSaveDataVersion2_6_0AndEarlier.SliderData sliderData, int numberOfLines, bool flip_lines, bool is_ME)
        {
            int h_headline;
            int h_tailline;

            BeatmapSaveDataVersion2_6_0AndEarlier.ColorType color;
            if (sliderData.colorType == BeatmapSaveDataVersion2_6_0AndEarlier.ColorType.ColorA)
            {
                color = BeatmapSaveDataVersion2_6_0AndEarlier.ColorType.ColorB;
            }
            else
            {
                color = BeatmapSaveDataVersion2_6_0AndEarlier.ColorType.ColorA;
            }


            if (sliderData.headLineIndex >= 1000 || sliderData.headLineIndex <= -1000)
            {
                h_headline = sliderData.headLineIndex / 1000 - 1; // Definition from ME
                color = sliderData.colorType; // Actually fixed the color swap here for BS 1.20.0
            }
            else if (flip_lines)
            {
                h_headline = numberOfLines - 1 - sliderData.headLineIndex;
            }
            else
            {
                h_headline = sliderData.headLineIndex;
                color = sliderData.colorType;
            }


            if (sliderData.tailLineIndex >= 1000 || sliderData.tailLineIndex <= -1000)
            {
                h_tailline = sliderData.tailLineIndex / 1000 - 1; // Definition from ME
                color = sliderData.colorType; // Actually fixed the color swap here for BS 1.20.0
            }
            else if (flip_lines)
            {
                h_tailline = numberOfLines - 1 - sliderData.tailLineIndex;
            }
            else
            {
                h_tailline = sliderData.tailLineIndex;
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

            return new BeatmapSaveDataVersion2_6_0AndEarlier.SliderData(color, sliderData.beat, h_headline, (BeatmapSaveDataCommon.NoteLineLayer)Check_Layer((int)sliderData.headLineLayer), sliderData.headControlPointLengthMultiplier, h_headcutDirection,
                                                  sliderData.tailTime, h_tailline, (BeatmapSaveDataCommon.NoteLineLayer)Check_Layer((int)sliderData.tailLineLayer), sliderData.tailControlPointLengthMultiplier, h_tailcutDirection, sliderData.sliderMidAnchorMode);
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


        private static BeatmapSaveDataVersion2_6_0AndEarlier.NoteData Mirror_Vertical_Note(BeatmapSaveDataVersion2_6_0AndEarlier.NoteData colorNoteData, bool flip_rows, bool has_ME)
        {
            int v_layer;

            // All precision placements will not be layer-flipped (complicated math)
            // This could be weird, consider it part of chaos mode KEK
            if ((int)colorNoteData.lineLayer >= 1000 || (int)colorNoteData.lineLayer <= -1000)
            {
                v_layer = ((int)colorNoteData.lineLayer / 1000) - 1; // Definition from ME
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
                v_layer = 3 - 1 - (int)colorNoteData.lineLayer;
            }
            else
            {
                v_layer = (int)colorNoteData.lineLayer;
            }

            BeatmapSaveDataCommon.NoteCutDirection v_cutDirection;
            if (vertical_cut_transform.TryGetValue(colorNoteData.cutDirection, out v_cutDirection) == false || has_ME)
            {
                v_cutDirection = Get_Random_Direction();
            }

            return new BeatmapSaveDataVersion2_6_0AndEarlier.NoteData(colorNoteData.beat, Check_Index(colorNoteData.lineIndex), (BeatmapSaveDataCommon.NoteLineLayer)v_layer, colorNoteData.type, v_cutDirection);
        }


        private static BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData Mirror_Vertical_Obstacle(BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData obstacleData, bool flip_rows)
        {
            if (flip_rows)
            {
                return new BeatmapSaveDataVersion2_6_0AndEarlier.ObstacleData(obstacleData.time, 0, 0, 0, 0);
            }

            return obstacleData;
        }

        private static BeatmapSaveDataVersion2_6_0AndEarlier.SliderData Mirror_Vertical_Slider(BeatmapSaveDataVersion2_6_0AndEarlier.SliderData sliderData, bool flip_rows, bool has_ME)
        {
            int v_headlayer;
            int v_taillayer;

            if ((int)sliderData.headLineLayer >= 1000 || (int)sliderData.headLineLayer <= -1000)
            {
                v_headlayer = ((int)sliderData.headLineLayer / 1000) - 1; // Definition from ME
            }
            else if (flip_rows)
            {
                v_headlayer = 3 - 1 - (int)sliderData.headLineLayer;
            }
            else
            {
                v_headlayer = (int)sliderData.headLineLayer;
            }


            if ((int)sliderData.tailLineLayer >= 1000 || (int)sliderData.tailLineLayer <= -1000)
            {
                v_taillayer = ((int)sliderData.tailLineLayer / 1000) - 1; // Definition from ME
            }
            else if (flip_rows)
            {
                v_taillayer = 3 - 1 - (int)sliderData.tailLineLayer;
            }
            else
            {
                v_taillayer = (int)sliderData.tailLineLayer;
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


            return new BeatmapSaveDataVersion2_6_0AndEarlier.SliderData(sliderData.colorType, sliderData.time, Check_Index(sliderData.headLineIndex), (BeatmapSaveDataCommon.NoteLineLayer)v_headlayer, sliderData.headControlPointLengthMultiplier, v_headcutDirection,
                                                              sliderData.tailTime, Check_Index(sliderData.tailLineIndex), (BeatmapSaveDataCommon.NoteLineLayer)v_taillayer, sliderData.tailControlPointLengthMultiplier, v_tailcutDirection, sliderData.sliderMidAnchorMode);
        }

        // Experiment with reusing this function. Not sure its actually better with casting in the main function
        /*private static BaseSliderData Mirror_Vertical_Slider_Generic(BaseSliderData baseSliderData, bool flip_rows, bool has_ME)
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

            SliderData sliderData;
            NoteCutDirection v_tailcutDirection;
            if ((sliderData = baseSliderData as SliderData) != null)
            {
                if (vertical_cut_transform.TryGetValue(sliderData.tailCutDirection, out v_tailcutDirection) == false || has_ME)
                {
                    v_tailcutDirection = Get_Random_Direction();
                }

                return new SliderData(baseSliderData.colorType, baseSliderData.beat, Check_Index(baseSliderData.headLine), v_head_noteLineLayer, sliderData.headControlPointLengthMultiplier, v_headcutDirection,
                                                      baseSliderData.tailBeat, Check_Index(baseSliderData.tailLine), v_tail_noteLineLayer, sliderData.tailControlPointLengthMultiplier, v_tailcutDirection, sliderData.sliderMidAnchorMode);
            }

            else
            {
                BurstSliderData burstSliderData = (BurstSliderData)baseSliderData;

                return new BurstSliderData(baseSliderData.colorType, baseSliderData.beat, Check_Index(baseSliderData.headLine), v_head_noteLineLayer, v_headcutDirection,
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
