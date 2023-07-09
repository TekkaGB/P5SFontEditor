# P5S Font Editor
Edit font .g1n files from Persona 5 Strikers and maybe other Koei Tecmo games. Check out this [guide](https://gamebanana.com/tuts/16336) if you want to know the full process of modding in your own font.

## Prerequisites
Download and install [.NET 8.0 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.0-preview.5-windows-x64-installer)

## Open G1N
Select a G1N font file to open and parse. Once a file was successfully parsed, you can utilize all the other features.

## Glyphs
![image](https://media.discordapp.net/attachments/792245872259235850/1127368316697198683/image.png?width=694&height=510)
### Glyph Index
Select the glyph index you want to view/edit

### Import Texture
Import a .raw file to replace the current selected glyph index's texture data. The file has to be less than or equal to the current texture's size.

### Export Texture
Export the currently selected glyph index's texture as a .raw to a location of your choice.

### Save Glyph Index
Save any changes you made to the parameters on the right hand side for the current glyph index. Note that this only saves the current session and doesn't overwrite anything in the file.

## Colors
![image](https://media.discordapp.net/attachments/792245872259235850/1127368579218686052/image.png?width=691&height=508)
### Color Index
Select a color index you want to view/edit

### Color Picker
You can click the box on the top to bring up a color picker to easily pick out a color

### Red/Green/Blue/Alpha Textboxes
You can also type the ARGB values manually.

### Save Color Index
Save any changes you made to the color values for the current color index. Note that this only saves the current session and doesn't overwrite anything in the file.

## UTF-16
![image](https://media.discordapp.net/attachments/792245872259235850/1127368578954428476/image.png?width=689&height=514)
### UTF-16 Index
Type in an index from 0-65536 that you wish to view/edit

### Load Glyph Index
Loads the UTF-16 index's value into the glyph index textbox

### Glyph Index
Type in whatever glyph index you want the UTF-16 index to use. The range is the same range of glyph indices under the glyphs tab.

### Save UTF-16 Index
Saves the currently selected UTF-16 index's glyph index. Note that this only saves the current session and doesn't overwrite anything in the file.

## Export G1N
Writes the entirety of the file with all of your edits to the location of your choice.
