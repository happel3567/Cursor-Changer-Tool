# Cursor Customizer Tool for Windows

Simply download the exe file named "Cursor Customizer" to use the tool. 

This tool lets you upload a PNG file and it will convert it to a cursor file, this cursor file will be automatically named "cursor.cur" and placed in your C drive. Simply press "Use This Image" which will open the Windows Mouse Properties application where you can set the image as your new cursor. To find the image, press browse and it should be at the bottom.

![Cursor Changer Preview](https://github.com/happel3567/Cursor-Changer-Tool/blob/main/Images/cursor%20customizer%20working%20image.png)

### Warning
Because the tool makes and names a file on your computer it will be flagged by windows defender as malicious but you can look through the code to see that it does nothing but make the file named "cursor.cur"

## Technical Details
The tool works by accepting a png file. It then scales down the image by lowering the amount of bytes taken to store it so that it could match a .cur file. Then it changes meta information about the file so that it matches a .cur file, all while preserving the initial colors and transparency of the png file. Once, upload image is pressed, a cur file that looks like your png will be placed in your C drive.
