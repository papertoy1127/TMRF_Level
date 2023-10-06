using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JSON;

// ReSharper disable once CompareOfFloatsByEqualityOperator
namespace TMRF_Level {
    public class LevelAnalyzer {
        public ReadOnlyCollection<SimpleFloor> angleData { get; }
        public decimal BPM { get; }
        public decimal length { get; }
        public decimal tiles => angleData.Count;
        
        public List<decimal> N { get; } = new();
        public List<decimal> D { get; } = new();
        
        public LevelAnalyzer(JsonObject data) {
            var settings = data["settings"];
            var curr_bpm = settings["bpm"].AsDecimal;
            decimal le = 0;

            List<float> angle_data;
            if (data.HasKey("pathData")) {
                angle_data = FloorMeshConverter.ToMesh(data["pathData"].AsString);
            }
            else {
                angle_data = data["angleData"].Cast<float>().ToList();
            }
            var actions = data["actions"].AsArray;
            
            var list = new List<SimpleFloor>();
            var ccw = false;

            var last = 0m;
            
            for (var i = 0; i < angle_data.Count; i++) {
                var change_section = false;
                var abs_angle = Convert.ToDecimal(angle_data[i]);
                var evnts = actions.Where(evnt => evnt.Value["floor"].AsInt == i).ToList();
                var pause_beat = 0m;
                foreach (var e in evnts) {
                    var evnt = e.Value.AsObject;
                    var event_type = (string) evnt["eventType"];
                    switch (event_type) {
                        case "SetSpeed":
                            decimal bpm;
                            if (!evnt.HasKey("speedType") || evnt["speedType"].AsString.Equals("Bpm")) bpm = evnt["beatsPerMinute"].AsDecimal;
                            else bpm = evnt["bpmMultiplier"].AsDecimal * curr_bpm;
                            curr_bpm = bpm;
                            change_section = true;
                            break;

                        case "Twirl": 
                            ccw = !ccw;
                            break;
                        case "Pause":
                            pause_beat += evnt["duration"].AsDecimal;
                            break;
                    }
                }

                decimal angle;
                if (abs_angle == 999) {
                    last = (last + 180) % 360;
                } else {
                    angle = (540 + last - abs_angle) % 360;
                    last = abs_angle;
                    if (ccw) angle = 360 - angle;
                    if (angle < 0.05m) angle = 360 - angle;
                    le += 60 / curr_bpm * (angle / 180);

                    angle += pause_beat * 180;
                
                    list.Add(new SimpleFloor {
                        BPM = curr_bpm,
                        Angle = angle,
                        ChangeSection = change_section,
                    });
                }
            }

            angleData = list.AsReadOnly();
            length = le;
        }

        public void CalcSection(bool? single_tile = null) {
            N.Clear();
            D.Clear();
            
            (int x, decimal z, decimal N) section = (0, 0, angleData[0].BPM);

            foreach (var floor in angleData) {
                if (single_tile ?? floor.ChangeSection) FinishSection(floor.BPM);
                
                section.x++;
                section.z += floor.Angle; // 180으로 나누기는 마지막에 한 번에
            }
            FinishSection(0);

            void FinishSection(decimal bpm) {
                if (section.x == 0) return;
                section.z /= 180;
                N.Add(section.N);
                D.Add(Expressions.Difficulty_Expr.Parse(new {
                    L = length,
                    section.z,
                    BPM = section.N,
                }));
                
                section = (0, 0, bpm);
            }
        }

        public decimal Sum() {
            return D.Sum();
        }
    }
    
    public struct SimpleFloor {
        public decimal BPM;
        public decimal Angle;
        public bool ChangeSection;
    }
}
