# False-Commander
Windows tool to program keyboard and mouse actions using FALSE programming language.
FALSE is an esoteric programming language. You can read about it [here](https://github.com/iliiliiliili/Android-False-Interpreter).

## New commands
There are some new commands:
- W
Gets string from stack (now they are in stack) and simulates keyboard hits to print this string.
- R
Processes first command from Parameters.txt and removes it from list.
- C
Gets x, y, btn and clicks on (x,y) using Left (btn = 0) or Right (btn = 1) Mouse Button.
- S
Gets number and sleeps this amount of ms.
- M
Gets a string and shows it as a message.
- V
Gets x, y and moves mouse to (x, y)
- X
Pushes to stack R, G, B of a pixel under the mouse.
- P
Pushes to stack X and Y of mouse position.
- E
Gets a string and starts a process with it's name in string.
- D
Pushes random number to stack.

## Usage
You can record your actions using StartRecord and StopRecord buttons.
In record mode you can use:
- F1 to start recording keyboard.
- F2 to create move mouse command with current cursor position.
- F3 to create mouse left-click command with current cursor position.
- F4 to create mouse right-click command with current cursor position. 
