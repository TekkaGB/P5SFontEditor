using Microsoft.Win32;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static P5SFontEditor.MainWindow;

namespace P5SFontEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Glyph> Glyphs;
        public List<Color> Colors;
        public List<short> GlyphIndices;
        public byte[] fileBytes;
        public int textureLength;
        public string fileName;
        public bool ExternalColorChange = false;
        public int currentGlyphIndex = -1;
        public MainWindow()
        {
            InitializeComponent();
        }
        public class Glyph
        {
            public int Index { get; set;  }
            public byte TextureWidth;
            public byte TextureHeight;
            public byte LeftSpace;
            public byte BaseLine;
            public byte GlyphWidth;
            public byte[] TextureData;
        }
        public class Color
        {
            public int Index { get; set; }
            public byte Alpha;
            public byte Red;
            public byte Green;
            public byte Blue;
        }

        private void FileSelectButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".g1n";
            dlg.Filter = "G1N Files (*.g1n)|*.g1n";
            dlg.Title = "Open G1N";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                fileName = dlg.FileName;
                ParseFontG1N(fileName);
            }
        }
        private void ParseFontG1N(string fileName)
        {
            fileBytes = File.ReadAllBytes(fileName);
            var magic = fileBytes[0..4];
            if (BitConverter.ToString(magic) != "5F-4E-31-47")
            {
                MessageBox.Show($"{BitConverter.ToString(magic)} Invalid magic");
                return;
            }
            var characterDataOffsetStart = BitConverter.ToInt32(fileBytes[32..36]);
            var characterDataOffsetEnd = characterDataOffsetStart + (2 * 65536);
            var textureDataOffset = BitConverter.ToInt32(fileBytes[20..24]);
            var colorDataBytes = fileBytes[36..characterDataOffsetStart];
            var utf16DataBytes = fileBytes[characterDataOffsetStart..characterDataOffsetEnd];
            var glyphDataBytes = fileBytes[characterDataOffsetEnd..textureDataOffset];
            var textureDataBytes = fileBytes[textureDataOffset..];
            var counter = 0;
            // Get color data
            Colors = new();
            for (int i = 0; i < colorDataBytes.Length; i += 4)
            {
                var buffer = new byte[4];
                Buffer.BlockCopy(colorDataBytes, i, buffer, 0, 4);
                Color color = new();
                color.Blue = buffer[0];
                color.Green = buffer[1];
                color.Red = buffer[2];
                color.Alpha = buffer[3];
                color.Index = counter;
                Colors.Add(color);
                counter++;
            }
            // Get UTF16 data
            GlyphIndices = new();
            for (int i = 0; i < utf16DataBytes.Length; i += 2)
            {
                var buffer = new byte[2];
                Buffer.BlockCopy(utf16DataBytes, i, buffer, 0, 2);
                GlyphIndices.Add(BitConverter.ToInt16(buffer));
            }
            Glyphs = new();
            // Get glyph data
            for (int i = 0; i < glyphDataBytes.Length; i += 12)
            {
                var buffer = new byte[12];
                Buffer.BlockCopy(glyphDataBytes, i, buffer, 0, 12);
                Glyph glyph = new();
                glyph.TextureWidth = buffer[0];
                glyph.TextureHeight = buffer[1];
                glyph.LeftSpace = buffer[2];
                glyph.BaseLine = buffer[3];
                glyph.GlyphWidth = buffer[4];
                Glyphs.Add(glyph);
            }
            var numGlyphs = glyphDataBytes.Length / 12;
            textureLength = textureDataBytes.Length / numGlyphs;
            counter = 0;
            // Get texture data
            for (int i = 0; i < textureDataBytes.Length; i += textureLength)
            {
                var buffer = new byte[textureLength];
                Buffer.BlockCopy(textureDataBytes, i, buffer, 0, textureLength);
                Glyphs[counter].TextureData = buffer;
                Glyphs[counter].Index = counter;
                counter++;
            }
            // Reset
            ImageViewer.Source = null;
            MainGlyphTextbox.Clear();
            TextureWidthBox.Clear();
            TextureHeightBox.Clear();
            LeftSpaceBox.Clear();
            BaseLineBox.Clear();
            GlyphWidthBox.Clear();
            AlphaTextbox.Clear();
            AlphaTextbox.Clear();
            RedTextbox.Clear();
            GreenTextbox.Clear();
            BlueTextbox.Clear();
            UTFTextbox.Clear();
            GlyphTextbox.Clear();
            currentGlyphIndex = -1;
            ExternalColorChange = true;
            ColorPreview.SelectedColor = System.Windows.Media.Colors.Black;
            ColorNoAlphaPreview.Background = new SolidColorBrush(System.Windows.Media.Colors.Black);
            ExternalColorChange = false;
            // Populate dropdowns
            ColorSelector.ItemsSource = Colors;
            ColorSelector.DisplayMemberPath = "Index";
            MessageBox.Show("Finished parsing G1N!");
        }
        private void DisplayImage16(Glyph glyph)
        {
            try
            {
                double dpi = 96;
                int width = (int)Math.Ceiling((decimal)glyph.TextureWidth / (decimal)2);
                int height = glyph.TextureHeight;

                BitmapSource bmpSource = BitmapSource.Create(width, height, dpi, dpi,
                    PixelFormats.Gray8, null, glyph.TextureData, width);
                ImageViewer.Source = bmpSource;
                TextureWidthBox.Text = width.ToString();
                TextureHeightBox.Text = glyph.TextureHeight.ToString();
                LeftSpaceBox.Text = glyph.LeftSpace.ToString();
                BaseLineBox.Text = glyph.BaseLine.ToString();
                GlyphWidthBox.Text = glyph.GlyphWidth.ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error",
                       MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGlyphButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentGlyphIndex < 0)
                return;
            try
            {
                var textureWidth = Convert.ToByte(TextureWidthBox.Text.Trim());
                var textureHeight = Convert.ToByte(TextureHeightBox.Text.Trim());
                var leftSpace = Convert.ToByte(LeftSpaceBox.Text.Trim());
                var baseLine = Convert.ToByte(BaseLineBox.Text.Trim());
                var glyphWidth = Convert.ToByte(GlyphWidthBox.Text.Trim());
                Glyphs[currentGlyphIndex].TextureWidth = Convert.ToByte(Convert.ToInt32(textureWidth) * 2);
                Glyphs[currentGlyphIndex].TextureHeight = textureHeight;
                Glyphs[currentGlyphIndex].LeftSpace = leftSpace;
                Glyphs[currentGlyphIndex].BaseLine = baseLine;
                Glyphs[currentGlyphIndex].GlyphWidth = glyphWidth;
                DisplayImage16(Glyphs[currentGlyphIndex]);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExportAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentGlyphIndex < 0)
                return;

            SaveFileDialog dlg = new();
            dlg.Filter = "G1N file (*.g1n)|*.g1n";
            dlg.DefaultExt = ".g1n";
            dlg.FileName = fileName;
            dlg.Title = "Export G1N";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                var textureDataOffset = BitConverter.ToInt32(fileBytes[20..24]);
                var characterDataOffsetStart = BitConverter.ToInt32(fileBytes[32..36]);
                var characterDataOffsetEnd = characterDataOffsetStart + (2 * 65536);
                var colorDataBytes = fileBytes[36..characterDataOffsetStart];
                var utf16DataBytes = fileBytes[characterDataOffsetStart..characterDataOffsetEnd];
                var glyphDataBytes = fileBytes[characterDataOffsetEnd..textureDataOffset];
                var textureDataBytes = fileBytes[textureDataOffset..];
                for (int i = 0; i < Colors.Count; i++)
                {
                    // Overwrite color data
                    colorDataBytes[i * 4] = Colors[i].Blue;
                    colorDataBytes[i * 4 + 1] = Colors[i].Green;
                    colorDataBytes[i * 4 + 2] = Colors[i].Red;
                    colorDataBytes[i * 4 + 3] = Colors[i].Alpha;
                }
                for (int i = 0; i < GlyphIndices.Count; i++)
                {
                    // Overwrite UTF16 data
                    Array.Copy(BitConverter.GetBytes(GlyphIndices[i]), 0, utf16DataBytes, i * 2, 2);
                }
                for (int i = 0; i < Glyphs.Count; i++)
                {
                    // Overwrite glyph data
                    glyphDataBytes[i * 12] = Glyphs[i].TextureWidth;
                    glyphDataBytes[i * 12 + 1] = Glyphs[i].TextureHeight;
                    glyphDataBytes[i * 12 + 2] = Glyphs[i].LeftSpace;
                    glyphDataBytes[i * 12 + 3] = Glyphs[i].BaseLine;
                    glyphDataBytes[i * 12 + 4] = Glyphs[i].GlyphWidth;
                    // Overwrite texture data
                    var textureStartOffset = i * textureLength;
                    Array.Copy(Glyphs[i].TextureData, 0, textureDataBytes, textureStartOffset, textureLength);
                }
                Array.Copy(glyphDataBytes, 0, fileBytes, characterDataOffsetEnd, glyphDataBytes.Length);
                Array.Copy(textureDataBytes, 0, fileBytes, textureDataOffset, textureDataBytes.Length);
                Array.Copy(colorDataBytes, 0, fileBytes, 36, colorDataBytes.Length);
                Array.Copy(utf16DataBytes, 0, fileBytes, characterDataOffsetStart, utf16DataBytes.Length);
                File.WriteAllBytes(dlg.FileName, fileBytes);
            }
        }

        private void ImportTextureButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentGlyphIndex < 0)
                return;
            OpenFileDialog dlg = new();
            dlg.Filter = "RAW file (*.raw)|*.raw";
            dlg.DefaultExt = ".raw";
            dlg.Title = "Import Texture";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                var importedFileBytes = File.ReadAllBytes(dlg.FileName);
                if (importedFileBytes.Length == Glyphs[currentGlyphIndex].TextureData.Length)
                {
                    Glyphs[currentGlyphIndex].TextureData = importedFileBytes;
                    DisplayImage16(Glyphs[currentGlyphIndex]);
                }
                else if (importedFileBytes.Length < Glyphs[currentGlyphIndex].TextureData.Length)
                {
                    var diff = Glyphs[currentGlyphIndex].TextureData.Length - importedFileBytes.Length;
                    var paddedImportedFileBytes = importedFileBytes.Concat(Enumerable.Repeat((byte)0x00, diff)).ToArray();
                    Glyphs[currentGlyphIndex].TextureData = paddedImportedFileBytes;
                    DisplayImage16(Glyphs[currentGlyphIndex]);
                }
                else
                    MessageBox.Show("Imported file is too large.");
            }
        }

        private void ExportTextureButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentGlyphIndex < 0)
                return;
            SaveFileDialog dlg = new();
            dlg.Filter = "RAW file (*.raw)|*.raw";
            dlg.DefaultExt = ".raw";
            dlg.FileName = $"{currentGlyphIndex}.raw";
            dlg.Title = "Export Texture";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                File.WriteAllBytes(dlg.FileName, Glyphs[currentGlyphIndex].TextureData);
            }
        }

        private void ColorSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded && ColorSelector.ItemsSource != null)
            {
                ExternalColorChange = true;
                var color = (sender as ComboBox).SelectedItem as Color;
                if (color == null)
                    return;
                AlphaTextbox.Text = color.Alpha.ToString();
                RedTextbox.Text = color.Red.ToString();
                GreenTextbox.Text = color.Green.ToString();
                BlueTextbox.Text = color.Blue.ToString();
                ColorPreview.SelectedColor = System.Windows.Media.Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
                ExternalColorChange = false;
            }
        }

        private void SaveColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (ColorSelector.SelectedIndex < 0)
                return;
            try
            {
                var alpha = Convert.ToByte(AlphaTextbox.Text.Trim());
                var red = Convert.ToByte(RedTextbox.Text.Trim());
                var green = Convert.ToByte(GreenTextbox.Text.Trim());
                var blue = Convert.ToByte(BlueTextbox.Text.Trim());
                Colors[ColorSelector.SelectedIndex].Alpha = alpha;
                Colors[ColorSelector.SelectedIndex].Red = red;
                Colors[ColorSelector.SelectedIndex].Green = green;
                Colors[ColorSelector.SelectedIndex].Blue = blue;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MainLoadGlyphIndexButton_Click(object sender, RoutedEventArgs e)
        {
            if (Glyphs == null)
                return;
            try
            {
                var glyphIndex = Convert.ToInt32(MainGlyphTextbox.Text.Trim());
                if (glyphIndex >= 0 && glyphIndex < Glyphs.Count)
                {
                    currentGlyphIndex = glyphIndex;
                    DisplayImage16(Glyphs[currentGlyphIndex]);
                }
                else
                {
                    MessageBox.Show($"{glyphIndex} is out of bounds. Max index is {Glyphs.Count - 1}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SaveUTF16Button_Click(object sender, RoutedEventArgs e)
        {
            if (GlyphIndices == null)
                return;
            try
            {
                var utfIndex = Convert.ToInt32(UTFTextbox.Text.Trim());
                if (utfIndex >= 0 && utfIndex < 65536)
                {
                        var glyphIndex = Convert.ToByte(GlyphTextbox.Text.Trim());
                        GlyphIndices[utfIndex] = glyphIndex;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ColorPreview_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if (!ExternalColorChange && IsLoaded)
            {
                AlphaTextbox.Text = ColorPreview.SelectedColor.Value.A.ToString();
                RedTextbox.Text = ColorPreview.SelectedColor.Value.R.ToString();
                GreenTextbox.Text = ColorPreview.SelectedColor.Value.G.ToString();
                BlueTextbox.Text = ColorPreview.SelectedColor.Value.B.ToString();
                ColorNoAlphaPreview.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(ColorPreview.SelectedColor.Value.R,
                        ColorPreview.SelectedColor.Value.G, ColorPreview.SelectedColor.Value.B));
            }
        }

        private void ColorTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ExternalColorChange = true;
            byte R, G, B, A;
            if (Byte.TryParse(RedTextbox.Text, out R) && Byte.TryParse(GreenTextbox.Text, out G)
                && Byte.TryParse(BlueTextbox.Text, out B) && Byte.TryParse(AlphaTextbox.Text, out A))
            {
                ColorPreview.SelectedColor = System.Windows.Media.Color.FromArgb(A, R, G, B);
                ColorNoAlphaPreview.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(R, G, B));
            }
            ExternalColorChange = false;
        }

        private void LoadGlyphIndexButton_Click(object sender, RoutedEventArgs e)
        {
            if (GlyphIndices == null)
                return;
            try
            {
                var utfIndex = Convert.ToInt32(UTFTextbox.Text.Trim());
                if (utfIndex >= 0 && utfIndex < 65536)
                {
                    GlyphTextbox.Text = GlyphIndices[utfIndex].ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}