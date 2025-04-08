using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicProcess
{
    public partial class Form1 : Form
    {
        const int MaxFileSize = 200 * 1024; // 200KB
        const int MaxWidth = 1920;
        const int MaxHeight = 1080;

        public Form1()
        {
            InitializeComponent();
            // 设置窗体居中
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            string id = textBox2.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id))
            {
                MessageBox.Show("请先输入姓名和工号！");
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string subFolder = $"{name}_{id}";
                string outputDir = Path.Combine(Application.StartupPath, "Compressed", subFolder);
                Directory.CreateDirectory(outputDir);

                int counter = 1;
                foreach (string file in dialog.FileNames)
                {
                    using (Image originalImage = Image.FromFile(file))
                    {
                        // 先缩放尺寸
                        Image resizedImage = ResizeImageIfNeeded(originalImage, MaxWidth, MaxHeight);

                        // 再进行压缩
                        byte[] compressedBytes = CompressJpegToTargetSize(resizedImage, MaxFileSize);
                        resizedImage.Dispose();

                        // 修改文件名格式：功耗_名字_工号_日期
                        string newName = $"{id}_{name}.jpg";
                        string outputPath = Path.Combine(outputDir, newName);
                        File.WriteAllBytes(outputPath, compressedBytes);
                        counter++;
                    }
                }

                MessageBox.Show($"图片压缩完成，已保存至：{subFolder} 文件夹！");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            string id = textBox2.Text.Trim();

            // 确保姓名和工号都已输入
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id))
            {
                MessageBox.Show("请先输入姓名和工号！");
                return;
            }

            // 构造保存图片的文件夹路径
            string subFolder = $"{name}_{id}";
            string folderPath = Path.Combine(Application.StartupPath, "Compressed", subFolder);

            // 输出路径，用于调试
            MessageBox.Show($"文件夹路径: {folderPath}");

            // 检查文件夹是否存在
            if (Directory.Exists(folderPath))
            {
                // 使用资源管理器打开文件夹
                System.Diagnostics.Process.Start("explorer.exe", folderPath);
            }
            else
            {
                MessageBox.Show("文件夹不存在，请先执行图片压缩！");
            }
        }


        private Image ResizeImageIfNeeded(Image image, int maxWidth, int maxHeight)
        {
            if (image.Width <= maxWidth && image.Height <= maxHeight)
                return (Image)image.Clone();

            float ratioX = (float)maxWidth / image.Width;
            float ratioY = (float)maxHeight / image.Height;
            float ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(image.Width * ratio);
            int newHeight = (int)(image.Height * ratio);

            Bitmap newImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }

        private byte[] CompressJpegToTargetSize(Image image, int maxSizeBytes)
        {
            long quality = 90;
            int step = 5;
            byte[] result = null;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters encoderParams = new EncoderParameters(1);

            while (quality >= 10)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                    image.Save(ms, jpgEncoder, encoderParams);

                    if (ms.Length <= maxSizeBytes)
                    {
                        result = ms.ToArray();
                        break;
                    }
                }

                quality -= step;
            }

            if (result == null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 10L);
                    image.Save(ms, jpgEncoder, encoderParams);
                    result = ms.ToArray();
                }
            }

            return result;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return null;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string name = textBox1.Text.Trim();
            string id = textBox2.Text.Trim();

            // 确保姓名和工号都已输入
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id))
            {
                MessageBox.Show("请先输入姓名和工号！");
                return;
            }

            // 构造保存图片的文件夹路径
            string subFolder = $"{name}_{id}";
            string folderPath = Path.Combine(Application.StartupPath, "Compressed", subFolder);

            // 输出路径，用于调试
            //MessageBox.Show($"文件夹路径: {folderPath}");

            // 检查文件夹是否存在
            if (Directory.Exists(folderPath))
            {
                // 使用资源管理器打开文件夹
                System.Diagnostics.Process.Start("explorer.exe", folderPath);
            }
            else
            {
                MessageBox.Show("文件夹不存在，请先执行图片压缩！");
            }
        }
    }
}
