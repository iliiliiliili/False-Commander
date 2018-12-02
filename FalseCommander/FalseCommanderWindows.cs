using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FalseCommander {
    
    class StackElement {

        public bool isInteger;

        public int i = -100;
        public string f = null;

        public StackElement (int _i) {

            isInteger = true;
            i = _i;
        }

        public StackElement (string _f) {

            isInteger = false;
            f = _f;
        }

        public StackElement (bool b) {

            isInteger = true;
            i = (b ? -1 : 0);
        }

        public bool IsTrue () {

            return isInteger ? i != 0 : f != "";
        }

    }

    class FalseCommanderWindows {
        
        private readonly string inputFileName;
        private readonly string parametersFileName;
        private readonly string outputFileName;
        
        public string code;
        public string input;
        public string parameters;

        private List <string> paramtersList;

        public string output;
        private string stackString;

        private Stack <StackElement> stack;
        private List <StackElement> vars;
        
        private int inputDelta = 0;

        public static string ReadFromFile (string filename) {

            StreamReader sr = new StreamReader (filename);
            string res = sr.ReadToEnd ();
            sr.Close ();

            return res;
        }

        public static void WriteToFile (string filename, string data) {

            StreamWriter sw = new StreamWriter (filename);
            sw.Write (data);
            sw.Close ();
        }

        private StackElement Get(bool destroy) {

            if (stack.Count () == 0)
                return null;


            StackElement r = stack.Peek ();
            if (destroy)
                stack.Pop();

            return r;
        }

        private void Push (StackElement val) {

            stack.Push (val);
        }

        private bool IsNumber (char c) {

            return  c >= '0' && c <= '9';
        }

        private StackElement ReadChar () {

            if (inputDelta >= input.Count ())
                return null;

            char c;
            c = input [inputDelta];
            inputDelta ++;
            return new StackElement((int) c);
        }

        private bool IsVar (char c) {

            return  c >= 'a' && c <= 'z';
        }

        private void ProcessComment (string s) {

			string[] ss = s.Split(' ');

			if (ss.Length == 2 && ss[0] == ("USE") && ss[1] != ("") ) {

                if (File.Exists (ss [1] + ".fls")) {
                    
                    ProcessCode(ReadFromFile (ss [1] + ".fls"));
                } else {

                    output += "FileLoadError '" + ss [1] + ".fls'";
                }
			}

		}

        private string TranslateChar (char c) {

            switch (c) {
                
                case 'n':
                    return "\n";
                case 't':
                    return "    ";

                default:
                    return c + "";
            }

        }

        private string ProcessCode (string codeToProcess) {

			int index = 0;

			bool isReadingNumber = false;
			int number = 0;

			int bracketsSign = 0;
			bool isReadingFunction = false;
			string function = "";

			bool isReadingString = false;
			string readString = "";

			bool isReadVar = false;
			int varIndex = -1;

			bool isReadingChar = false;
            bool isLastSlash = false;

			bool isReadingComment = false;
			int commentsSign = 0;
			string comment = "";

			while (index < codeToProcess.Length) {

				char c = codeToProcess [index];
                
                if (!isLastSlash && c == '\\' && (isReadingComment || isReadingFunction || isReadingString)) {

                    isLastSlash = true;
                    index ++;
                    continue;
                }


				if (isReadingComment) {

                    if (isLastSlash) {

                        comment += TranslateChar (c);
                        isLastSlash = false;
                        index ++;
                        continue;
                    }

					if (c == '}') {
						commentsSign --;
						if (commentsSign == 0) {
							ProcessComment(comment);
							comment = "";
							isReadingComment = false;
							index++;
							continue;
						}
						if (commentsSign < 0)
							return "Error: Wrong {}comment expression. At: " + index  + ".";
					}

					if (c == '{')
						commentsSign ++;

					comment += c;
					index++;
					continue;
				}

				if (isReadingFunction) {

                    if (isLastSlash) {

                        function += TranslateChar (c);
                        isLastSlash = false;
                        index ++;
                        continue;
                    }

					if (c == ']') {

						bracketsSign--;
						if (bracketsSign == 0) {

							isReadingFunction = false;
							Push(new StackElement(function));
							function = "";
							bracketsSign = 0;
							index++;
							continue;
						}

						if (bracketsSign < 0)
							return "Error: Wrong []brackets expression. At: " + index  + ".";

					}

					if (c == '[') {

						bracketsSign++;
					}

					function += c;
					index++;
					continue;
				}

				if (c == '\"' && !isLastSlash) {

					if (isReadingString) {

						isReadingString = false;
						Push (new StackElement (readString));
						readString = "";
					} else {

						isReadingString = true;
						readString = "";
					}
					index ++;
					continue;
				}

				if (isReadingString) {

                    if (isLastSlash) {

                        readString += TranslateChar (c);
                        isLastSlash = false;
                        index ++;
                        continue;
                    }

					readString += c;
					index ++;
					continue;
				}

				if (isReadingChar) {

					Push(new StackElement((int) c));
					isReadingChar = false;
					index ++;
					continue;
				}

				if (IsVar(c)) {

					if (isReadVar)
						return "Variable can only have a one-symbol name from 'a' to 'z'. At: " + index + ".";

					varIndex = (int) c - (int) 'a';
					isReadVar = true;

					index++;
					continue;
				}

				if (IsNumber(c)) {
					isReadingNumber = true;
					number = number*10 + ((int)c - (int) '0');
				} else {

					if (isReadingNumber) {

						isReadingNumber = false;
						stack.Push(new StackElement (number));
						number = 0;
					}
				}


				StackElement a,b,p;
				switch (c) {

					case '+':

						a = Get(true); b = Get(true);

                        if (b == null)
							return "Error: Not enough parameters. Function: \"+\". At: " + index + ".";

                        if (a.isInteger) {

                            if (b.isInteger) {

						        Push(new StackElement(b.i + a.i));
                            } else {
                                
						        Push(new StackElement(b.f + a.i.ToString ()));
                            }

                        } else {

                            if (b.isInteger) {

						        Push(new StackElement(b.i.ToString () + a.f));
                            } else {
                                
						        Push(new StackElement(b.f + a.f));
                            }
                        }

						break;
					case '-':

						a = Get(true); b = Get(true);

						if (b == null || !(a.isInteger && b.isInteger))
							return "Error: One of parameters is not a number. Function: \"-\". At: " + index + ".";
						Push(new StackElement(b.i - a.i));
						break;
					case '*':

						a = Get(true); b = Get(true);
						if (b == null || !(a.isInteger && b.isInteger))
							return "Error: One of parameters is not a number. Function: \"*\". At: " + index + ".";
						Push(new StackElement(b.i * a.i));
						break;
					case '/':

						a = Get(true); b = Get(true);
						if (b == null || !(a.isInteger && b.isInteger))
							return "Error: One of parameters is not a number. Function: \"/\". At: " + index + ".";

						if ( a.i == 0)
							return "Error: Division by zero. Function: \"/\". At: " + index + ".";
						Push(new StackElement(b.i / a.i));
						break;
					case '_':

						a = Get(true);

						if (a == null || !(a.isInteger))
							return "Error: Parameter is not a number. Function: \"_\". At: " + index + ".";
						Push(new StackElement(-a.i));
						break;

					case '=':

						a = Get(true); b = Get(true);

						if (b == null || a.isInteger != b.isInteger)
							return "Error: Parameters have different types. At: " + index + ".";
						Push(new StackElement((a.isInteger ? a.i == b.i : a.f == b.f)));
						break;

					case '>':

						a = Get(true); b = Get(true);

						if (b == null || !(a.isInteger && b.isInteger))
							return "Error: One of parameters is not a number. Function: \">\". At: " + index + ".";
						Push(new StackElement(a.i < b.i));
						break;
					case '~':

						a = Get(true);

                        if (a == null) 
							return "Error: One of parameters is not a number. Function: \"~\". At: " + index + ".";

						Push(new StackElement(!a.IsTrue ()));
						break;

					case '&':

						a = Get(true); b = Get(true);
                        
                        if (b == null) 
							return "Error: One of parameters is not a number. Function: \"&\". At: " + index + ".";

						Push(new StackElement(a.IsTrue () && b.IsTrue ()));
						break;
					case '|':

						a = Get(true); b = Get(true);
                        
                        if (b == null) 
							return "Error: One of parameters is not a number. Function: \"&\". At: " + index + ".";
						Push(new StackElement(a.IsTrue () || b.IsTrue ()));
						break;
					case '[':

						isReadingFunction = true;
						bracketsSign = 1;
						break;
					case ']':
						return "Error: Wrong []brackets expression. At: " + index + ".";

					case '$':

						a = Get(false);
                        if (a == null) 
							return "Error: Not enough parameters. Function: \"$\". At: " + index + ".";
						Push(a);
						break;
					case '%':

						Get(true);
						break;
					case '\\':

						a = Get(true);
						b = Get(true);

						if (b == null)
							return "Error: Not enough parameters. Function: \"\\\". At: " + index + ".";

						Push(a);
						Push(b);
						break;
					case '@':

						a = Get(true);
						b = Get(true);
						p = Get(true);

						if (p == null)
							return "Error: Not enough parameters. Function: \"@\". At: " + index + ".";

						Push(p);
						Push(a);
						Push(b);
						break;
					case 'ø':case 'O':

						a = Get(true);
						b = a;

						if (a == null || !a.isInteger)
							return "Error: Parameter is not a number. Function: \"ø\\O\". At: "+index + ".";

						a.i ++;

                        var d = new Stack <StackElement> (stack);
						StackElement res = null;

						while (d.Count > 0 && a.i > 0) {

							res = d.Pop();
							a.i--;
						}

						if (a.i == 0)
							Push(res);
						else
							return "Error: No such index \""+b.i+"\" in stack. Function: \"ø\\O\". At: "+index + ".";

						break;

					case '!':

						a = Get(true);

						if (a == null || a.isInteger)
							return "Error: Parameter is not a function. Function: \"!\". At: " + index + ".";

						string resProcess = ProcessCode(a.f);

						if (resProcess != "")
							return resProcess + " Caused by \"["+a.f + "]\" At: "+ index + ".\n";

						break;

					case '?':

						a = Get(true);
						b = Get(true);

						if (a == null || b == null || a.isInteger)
							return "Error: Parameter is not a function. Function: \"?\". At: " + index + ".";

						if (b.IsTrue ()) {

							string resProcess1 = ProcessCode(a.f);

							if (resProcess1 != "")
								return resProcess1 + " Caused by \"[" + a.f + "]\" At: " + index + ".\n";
						}

						break;


					case '#':

						StackElement a1 = Get(true);
						StackElement a2 = Get(true);

						if (a2 == null || a1 == null || a1.isInteger || a2.isInteger)
							return "Error: Parameters are not a functions. Function: \"#\". At: " + index + ".";


						do {

							string resProcess2 = ProcessCode(a2.f);

							if (resProcess2 != "")
								return resProcess2 + " Caused by \"[" + a2.f + "]\" At: " + index + ".\n";

							b = Get(true);

							if (b.IsTrue ()) {

								string resProcess1 = ProcessCode(a1.f);

								if (resProcess1 != "")
									return resProcess1 + " Caused by \"[" + a1.f + "]\" At: " + index + ".\n";
							}
						} while (b.IsTrue ());

						break;

					case '.':

						a = Get(true);

                        if (a == null) 
                            return "Error: Not enough parameters. Function: \".\". At: " + index + ".";

						output += (a.isInteger?a.i.ToString () : a.f);

						break;

					case ',':

						a = Get(true);

						if (!a.isInteger)
							return "Error: Parameter is not a symbol. Function: \",\". At: "+index + ".";

						output += (char) (int) a.i;

						break;
					case '^':

						Push(ReadChar());

						break;

					case ':':

						a = Get(true);


						if (!isReadVar)
							return "Error: No variables have found (use it like '1f:' - Push 1; f := 1;). At: "+index + ".";

						if (a == null)
							return "Error: Parameter is NULL. At: " + index + ".";

						vars[varIndex] = a;

						break;

					case ';':

						if (!isReadVar)
							return "Error: No variables have found (use it like 'f;' - Push f;). At: "+index + ".";
						Push(vars[varIndex]);

						break;


					case '\'':

						isReadingChar = true;
						break;
					case 'ß':case 'B':

						input = "";
						output = "";
						break;

					case '{':

						isReadingComment = true;
						comment = "";
						commentsSign = 1;
						break;
                        
                    case 'W':

                        a = Get(true);

                        if (a == null) 
                            return "Error: Not enough parameters. Function: \"W\". At: " + index + ".";

                        Write (a.isInteger ? a.i.ToString () : a.f);
                        break;
                        
                    case 'R':

                        if (paramtersList.Count > 0) {

                            string resProcess1 = ProcessCode (paramtersList [0]);

                            if (resProcess1 != "")
								return resProcess1 + " Caused by \"[" + paramtersList [0] + "]\" At: " + index + ".\n";

                            paramtersList.RemoveAt (0);
                        }

                        break;
                        
                    case 'C':
                        
                        a = Get (true);
                        b = Get (true);
                        p = Get (true);

                        if (p == null)
                            return "Error: Not enough parameters. Function: \"C\". At: " + index + ".";
                        
                        if (!a.isInteger || !b.isInteger || !p.isInteger) {

                            return "Error: parameters are not integers. Function: \"C\". At: " + index + ".";
                        }

                        Click (p.i, b.i, a.i);


                        break;
                        
                    case 'S':

                        a = Get (true);

                        if (a == null)
                            return "Error: Not enough parameters. Function: \"S\". At: " + index + ".";
                        
                        if (!a.isInteger) {

                            return "Error: parameter is not integer. Function: \"S\". At: " + index + ".";
                        }

                        Sleep (a.i);
                        break;
                        
                    case 'M':

                        a = Get (true);

                        if (a == null)
                            return "Error: Not enough parameters. Function: \"M\". At: " + index + ".";

                        Message (a.isInteger ? a.i.ToString () : a.f);
                        break;
                        
                    case 'V':

                        a = Get (true);
                        b = Get (true);

                        if (b == null)
                            return "Error: Not enough parameters. Function: \"V\". At: " + index + ".";
                        
                        if (!a.isInteger || !b.isInteger) {

                            return "Error: parameters are not integers. Function: \"V\". At: " + index + ".";
                        }

                        MoveMouse (b.i, a.i);
                        break;

                    case 'X':

                        a = Get (true);
                        b = Get (true);

                        if (b == null)
                            return "Error: Not enough parameters. Function: \"X\". At: " + index + ".";
                        
                        if (!a.isInteger || !b.isInteger) {

                            return "Error: parameters are not integers. Function: \"X\". At: " + index + ".";
                        }

                        var color = GetPixel (b.i, a.i);
                        
                        Push (new StackElement (color.R));
                        Push (new StackElement (color.G));
                        Push (new StackElement (color.B));

                        break;

                    case 'P':

                        var pos = GetMousePosition ();
                        
                        Push (new StackElement (pos.X));
                        Push (new StackElement (pos.Y));

                        break;

                    case 'E':
                        
                        a = Get (true);

                        if (a == null)
                            return "Error: Not enough parameters. Function: \"S\". At: " + index + ".";
                        
                        if (a.isInteger) {

                            return "Error: parameter is not string. Function: \"S\". At: " + index + ".";
                        }

                        System.Diagnostics.Process.Start (a.f);

                        break;
                        
                    case 'D':
                        
                        Push (new StackElement (r.Next ()));

                        break;


				}


				index ++;
				isReadVar = false;
                isLastSlash = false;
			}
			if (isReadingFunction)
				return "Error: Wrong []brackets expression. At: the end.";

			if (isReadingNumber) {

				stack.Push(new StackElement (number));
			}
			if (isReadingString) {

				return "Error: Missing \" in string expression. At: the end.";
			}

			return "";
		}

        Random r;

        public void Process () {

            r = new Random ();

            code = ReadFromFile ("Code.fls");
            input = ReadFromFile (inputFileName);
            parameters = ReadFromFile (parametersFileName);

            paramtersList = new List<string> (parameters.Split ('\n'));

			output = "";

			vars = new List <StackElement> ();
			for (int i = 0; i < 26; i++) {

				vars.Add (null);
			}

			stack = new Stack<StackElement> ();

			string res = ProcessCode (code);

			if (res != "") {

				stackString = "";
				StackElement q;

				while (stack.Count () > 0) {

					q = Get(true);
					stackString += (q.isInteger? q.i.ToString () : "\""+q.f+"\"") + (stack.Count() <= 0 ? ". " : ", ");
				}
				output = res + "\nStack: " + stackString;
			}

			input = "";
            
            WriteToFile (outputFileName, output);
		}

        public FalseCommanderWindows (string _inputFileName, string _parametersFileName, string _outputFileName) {

            inputFileName = _inputFileName;
            parametersFileName = _parametersFileName;
            outputFileName = _outputFileName;
        }

        private void Write (string text) {
            
            SendKeys.SendWait (text);
        }

        private void Sleep (int ms) {

            System.Threading.Thread.Sleep (ms);
        }

        private void MoveMouse (int x, int y) {

            Cursor.Position = new Point(x, y);
        }

        private void Click (int x, int y, int btn) {

            MoveMouse (x, y);

            if (btn == 0) {

                MouseSimulator.ClickLeftMouseButton ();
            } else {

                MouseSimulator.ClickRightMouseButton ();
            }

        }

        private void Message (string msg) {

            MessageBox.Show (msg);
        }
        
        private Color GetPixel (int x, int y) {

            return PixelController.GetPixelColor (x, y);
        }

        public Point GetMousePosition () {

            return Cursor.Position;
        }

    }
}
