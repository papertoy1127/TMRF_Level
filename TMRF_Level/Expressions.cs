using System;
using System.IO;

namespace TMRF_Level {
    public static class Expressions {
        public static Parser Difficulty_Expr;

        public static void LoadExprs() {
            var expr_path = Path.Combine(Environment.CurrentDirectory, "expression.txt");
            Difficulty_Expr = new Parser(File.ReadAllText(expr_path));
        }
    }
}
