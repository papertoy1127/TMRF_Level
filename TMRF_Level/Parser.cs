using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TMRF_Level {
    public class Parser {
        private List<object> _expression;
        
        public Parser(string expression) {
            var tokens = new List<object>();
            var idx = 0;

            while (true) {
                if (idx >= expression.Length) break;
                var chr = expression[idx];
                if (chr is '(' or ')' or '+' or '-' or '*' or '/' or '^' or '%') {
                    tokens.Add(chr.ToString());
                    idx++;
                    continue;
                }

                if (char.IsDigit(chr)) {
                    var builder = new StringBuilder();
                
                    while (char.IsDigit(chr) || chr == '.') {
                        builder.Append(chr);
                        idx++;
                        if (idx >= expression.Length) break;
                        chr = expression[idx];
                    }
                    
                    tokens.Add(Convert.ToDecimal(builder.ToString()));
                    continue;
                }
                
                if (char.IsLetter(chr) || chr == '$') {
                    var builder = new StringBuilder();
                
                    while (char.IsLetter(chr) || chr == '$') {
                        builder.Append(chr);
                        idx++;
                        if (idx >= expression.Length) break;
                        chr = expression[idx];
                    }

                    var token = builder.ToString();
                    if (token == "sqrt") {
                        if (idx >= expression.Length) throw new Exception("Unexpected end of expression.");
                        if (expression[idx] != '(') throw new Exception("Expected '(' after 'sqrt'.");
                        idx++;
                        tokens.Add("sqrt(");
                    }
                    else {
                        tokens.Add(token);
                    }
                    continue;
                }
                
                throw new Exception($"Unexpected character: {chr}");
            }

            var stack = new Stack<string>();
            var output = new List<object>();
            
            // to prefix
            
            foreach (var token in tokens) {
                if (token is decimal) {
                    output.Add(token);
                    continue;
                }

                if (token is string str) {
                    if (str == "(") {
                        stack.Push(str);
                        continue;
                    }

                    if (str == ")") {
                        while (stack.Count > 0 && !stack.Peek().EndsWith("(")) {
                            output.Add(stack.Pop());
                        }

                        if (stack.Count == 0) throw new Exception("Mismatched parentheses.");
                        var s = stack.Pop();
                        if (s != "(") output.Add(s.Substring(0, s.Length - 1));
                        continue;
                    }

                    if (str is "+" or "-") {
                        while (stack.Count > 0 && GetPriority(stack.Peek()) <= GetPriority(str)) {
                            output.Add(stack.Pop());
                        }
                        
                        stack.Push(str);
                        continue;
                    }

                    if (str == "*" || str == "/" || str == "%") {
                        while (stack.Count > 0 && GetPriority(stack.Peek()) <= GetPriority(str)) {
                            output.Add(stack.Pop());
                        }

                        stack.Push(str);
                        continue;
                    }
                    
                    if (str == "^") {
                        while (stack.Count > 0 && GetPriority(stack.Peek()) < GetPriority(str)) {
                            output.Add(stack.Pop());
                        }

                        stack.Push(str);
                        continue;
                    }

                    if (str == "sqrt(") {
                        stack.Push(str);
                        continue;
                    }

                    output.Add(str);
                }
            }
            
            while (stack.Count > 0 && stack.Peek() != "(") {
                output.Add(stack.Pop());
            }

            _expression = output;
        }

        public decimal Parse(object obj) {
            var stack = new Stack<decimal>();

            foreach (var xpr in _expression) {
                if (xpr is decimal d) {
                    stack.Push(d);
                    continue;
                }

                if (xpr is string s) {
                    switch (s) {
                        case "+":
                            stack.Push(stack.Pop() + stack.Pop());
                            break;
                        case "-":
                            stack.Push(-stack.Pop() + stack.Pop());
                            break;
                        case "*":
                            stack.Push(stack.Pop() * stack.Pop());
                            break;
                        case "/":
                            var a = stack.Pop();
                            var b = stack.Pop();
                            stack.Push(b / a);
                            break;
                        case "%":
                            a = stack.Pop();
                            b = stack.Pop();
                            stack.Push(b % a);
                            break;
                        case "^":
                            a = stack.Pop();
                            b = stack.Pop();
                            stack.Push((decimal) Math.Pow((double) b, (double) a));
                            break;
                        case "sqrt":
                            stack.Push((decimal) Math.Pow((double) stack.Pop(), 0.5));
                            break;
                        
                        default:
                            if (!s.StartsWith("$")) throw new Exception($"Unexpected token: {s}");
                            var key = s.Substring(1);
                            var value = GetValue(obj, key);
                            stack.Push(value);
                            break;

                    }
                }
            }
            
            if (stack.Count != 1) throw new Exception("Invalid expression.");
            return stack.Pop();

            static decimal GetValue(object obj, string key) {
                var type = obj.GetType();
                var property = type.GetProperty(key);
                if (property != null) return Convert.ToDecimal(property.GetValue(obj));
                var field = type.GetField(key);
                if (field != null) return Convert.ToDecimal(field.GetValue(obj));
                throw new Exception($"Invalid key: {key}");
            }
        }

        private int GetPriority(string op) {
            return op switch {
                "(" or ")" => 5,
                _ when op.EndsWith("(") => 5,
                "^" => 0,
                "*" or "/" or "%" => 1,
                "+" or "-" => 2,
                _ => 3,
            };
        }
    }
}
