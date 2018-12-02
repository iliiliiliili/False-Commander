using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FalseCommander {
    public partial class Form1 : Form {

        public Form1() {

            InitializeComponent();

            richTextBox1.Text = FalseCommanderWindows.ReadFromFile ("Code.fls");

            new KeyboardController (OnKeyDown, OnKeyUp);
        }
        
        private bool isRecording = false;
        private bool isRecordingKeys = false;

        private string recordedCode;
        private string recordedText;
    

        public static void AppendText(RichTextBox box, string text, Color color) {

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }

        private void SetCodeText (RichTextBox box, string text) {

            Color color;
            bool isComment = false;
            bool isString = false;
            bool isSlash = false;

            for (int i = 0; i < text.Length; i++) {

                color = Color.Black;

                if (!isSlash && text [i] == '\\') {

                    isSlash = true;
                } else {

                    switch (text [i]) {
                    
                        case 'W':
                        case 'R':
                        case 'C':
                        case 'S':
                        case 'M':
                        case 'V':
                        case 'X':
                        case 'P':
                        case 'D':
                        case 'E':
                            color = Color.Blue;
                            break;

                        case '{':
                            isComment = true;
                            break;
                        case '}':
                            isComment = false;
                            break;

                        case '[':
                        case ']':
                            color = Color.Green;
                            break;


                    }

                    if (text [i] >= 'a' && text [i] <= 'z') {

                        color = Color.Red;
                    }
                
                    if (text [i] >= '0' && text [i] <= '9') {

                        color = Color.Purple;
                    }


                    if (text [i] == '"'  && !isSlash) {

                        isString = !isString;
                    }

                    if (isString || text [i] == '"') {

                        color = Color.Green;
                    }
                
                    if (i > 0 && text [i - 1] == '\\') {

                        color = Color.HotPink;
                    }

                    if (isComment || text [i] == '}') {

                        color = Color.Gray;
                    }

                    isSlash = false;

                }

                AppendText (box, text [i] + "", color);
            }
            
        }

        private void button1_Click(object sender, EventArgs e) {
            
            richTextBox1.Text = "";
            SetCodeText (richTextBox1, FalseCommanderWindows.ReadFromFile ("Code.fls"));

            var falseCommanderWindows = (new FalseCommanderWindows ("Input.txt", "Parameters.txt", "Output.txt"));

            ThreadStart childref = new ThreadStart(falseCommanderWindows.Process);
            Thread childThread = new Thread (childref) {
                IsBackground = true
            };
            childThread.Start();


        }

        private void AddRecordedCode (string cd) {

            recordedCode += cd;
            SetCodeText (richTextBox1, cd);
        }
        
        private bool isControl = false;

        private void OnKeyDown (Keys k) {

            if (isRecording) {

                if (isRecordingKeys) {

                    switch (k) {
                    
                        case Keys.LControlKey:
                            recordedText += "^(";
                            break;
                        case Keys.LMenu:
                            recordedText += "%(";
                            break;
                        case Keys.LShiftKey:
                            recordedText += "+(";
                            break;
                    }

                } else {

                    if (k == Keys.LControlKey) {

                        isControl = true;
                    }

                    if (k == Keys.S && isControl) {

                        button4_Click (null, null);
                    }

                }
                
            } 
        }

        private void OnKeyUp (Keys k) {

            if (isRecording) {

                if (k == Keys.F1 && isRecordingKeys) {

                    AddRecordedCode ("\"" + recordedText + "\"W\n");
                    recordedText = "";
                    isRecordingKeys = false;
                    return;
                }

                if (isRecordingKeys) {

                    
                    switch (k) {
                        
                        case Keys.LControlKey:
                        case Keys.LShiftKey:
                            recordedText += ")";
                            break;
                            
                        case Keys.LMenu:
                            recordedText += "%()";
                            break;

                        case Keys.Back:
                            recordedText += "{BACKSPACE}";
                            break;
                        case Keys.CapsLock:
                            recordedText += "{CAPSLOCK}";
                            break;
                        case Keys.Down:
                            recordedText += "{DOWN}";
                            break;
                        case Keys.End:
                            recordedText += "{END}";
                            break;
                        case Keys.Enter:
                            recordedText += "{ENTER}";
                            break;
                        case Keys.Escape:
                            recordedText += "{ESC}";
                            break;
                        case Keys.Help:
                            recordedText += "{HELP}";
                            break;
                        case Keys.Home:
                            recordedText += "{HOME}";
                            break;
                        case Keys.Insert:
                            recordedText += "{INSERT}";
                            break;
                        case Keys.Left:
                            recordedText += "{LEFT}";
                            break;
                        case Keys.NumLock:
                            recordedText += "{NUMLOCK}";
                            break;
                        case Keys.PageDown:
                            recordedText += "{PGDN}";
                            break;
                        case Keys.PageUp:
                            recordedText += "{PGUP}";
                            break;
                        case Keys.PrintScreen:
                            recordedText += "{PRTSC}";
                            break;
                        case Keys.Right:
                            recordedText += "{RIGHT}";
                            break;
                        case Keys.Scroll:
                            recordedText += "{SCROLLLOCK}";
                            break;
                        case Keys.Tab:
                            recordedText += "{TAB}";
                            break;
                        case Keys.Up:
                            recordedText += "{UP}";
                            break;
                        case Keys.F1:
                        case Keys.F2:
                        case Keys.F3:
                        case Keys.F4:
                        case Keys.F5:
                        case Keys.F6:
                        case Keys.F7:
                        case Keys.F8:
                        case Keys.F9:
                        case Keys.F10:
                        case Keys.F11:
                        case Keys.F12:
                        case Keys.F13:
                        case Keys.F14:
                        case Keys.F15:
                        case Keys.F16:
                            recordedText += "{"+k.ToString ()+"}";
                            break;
                        case Keys.Add:
                            recordedText += "{ADD}";
                            break;
                        case Keys.Subtract:
                            recordedText += "{SUBTRACT}";
                            break;
                        case Keys.Multiply:
                            recordedText += "{MULTIPLY}";
                            break;
                        case Keys.Divide:
                            recordedText += "{DIVIDE}";
                            break;


                        default:
                            recordedText += k.ToString ().ToLower ();
                            break;

                    }
                }

                
                if (k == Keys.F1) {
                    
                    isRecordingKeys = true;
                }
                
                if (k == Keys.F2) {

                    AddRecordedCode (Cursor.Position.X + " " + Cursor.Position.Y + "V100S\n");
                }
                
                if (k == Keys.F3) {

                    AddRecordedCode (Cursor.Position.X + " " + Cursor.Position.Y + " 0C100S\n");
                }

                if (k == Keys.F4) {

                    AddRecordedCode (Cursor.Position.X + " " + Cursor.Position.Y + " 1C100S\n");
                }

            }

                
            if (k == Keys.LControlKey) {

                isControl = false;
            }
               
        }


        private void button2_Click(object sender, EventArgs e) {

            recordedCode = "";
            recordedText = "";

            isRecording = true;
            isRecordingKeys = false;
        }

        private void button3_Click(object sender, EventArgs e) {

            isRecording = false;
            isRecordingKeys = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) {

            if (KeyboardController.instance != null)
                KeyboardController.instance.Destroy ();
        }

        private void button4_Click(object sender, EventArgs e) {

            
            FalseCommanderWindows.WriteToFile ("Code.fls", richTextBox1.Text);
            richTextBox1.Text = "";
            SetCodeText (richTextBox1, FalseCommanderWindows.ReadFromFile ("Code.fls"));
        }

        private void button5_Click(object sender, EventArgs e) {

            var sfd = new SaveFileDialog ();
            sfd.DefaultExt = ".fls";
            sfd.InitialDirectory = ".";
            sfd.ShowDialog ();

            FalseCommanderWindows.WriteToFile (sfd.FileName == "" ? "Code.fls" : sfd.FileName, richTextBox1.Text);
        }

        private void richTextBox1_TextChanged (object sender, EventArgs e) {

        }
    }
}
