using System;
using System.Collections.Generic;
using System.IO;
using JSON;
using Ookii.Dialogs.Wpf;

namespace TMRF_Level {
    internal static class Program {
        //public const string EXPRESSION = "(2^($BPM/100)/100)*($tiles/($length/60)^0.5)";
        
        public static void Main(string[] args) {
            while (true) {// VistaFileDialog
                Expressions.LoadExprs();
                
                var dialog = new VistaOpenFileDialog {
                    Filter = "ADOFAI Level File|*.adofai",
                    Title = "Select ADOFAI Level File",
                };
            
                if (dialog.ShowDialog() != true) return;
                var path = dialog.FileName;
                var data = (JsonObject) JsonNode.Parse(File.ReadAllText(path));
                var analyzer = new LevelAnalyzer(data);
                analyzer.CalcSection();
            
                Console.WriteLine($"Average BPM: {analyzer.BPM}");
                Console.WriteLine($"Length: {analyzer.length}");
                Console.WriteLine($"Tiles: {analyzer.angleData.Count}");
                for (var i = 0; i < analyzer.D.Count; i++) {
                    Console.Write($"Section {i + 1}: {{");
                    Console.Write($"N: {analyzer.N[i]}, ");
                    Console.WriteLine($"D: {analyzer.D[i]}}}");
                }
                Console.WriteLine($"Result (section sum): {analyzer.Sum()}");
                analyzer.CalcSection(true);
                Console.WriteLine($"Result (tile sum): {analyzer.Sum()}");
                
                Console.WriteLine();
                Console.WriteLine("Press any key to quit...");
                Console.WriteLine("Press Enter key to continue...");
                if (Console.ReadKey().Key != ConsoleKey.Enter) return;
            }
        }
    }
}
