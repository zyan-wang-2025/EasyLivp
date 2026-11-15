using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using LibHeifSharp;

namespace EasyLivp;

/// <summary>
/// HEIC 解码辅助类。
/// </summary>
public static class HeicHelper
{
    public static Bitmap DecodeToBitmap(string heicPath)
    {
        using var context = new HeifContext(heicPath);
        using var imageHandle = context.GetPrimaryImageHandle();
        using var image = imageHandle.Decode(HeifColorspace.Rgb, HeifChroma.InterleavedRgb24);

        int width = image.Width;
        int height = image.Height;

        var plane = image.GetPlane(HeifChannel.Interleaved);
        IntPtr srcData = plane.Scan0;
        int srcStride = plane.Stride;

        var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var bmpData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly,
            PixelFormat.Format24bppRgb);

        try
        {
            // 修复越界：严格使用 width * 3 作为真实像素数据的长度
            int widthInBytes = width * 3; 
            int destStride = Math.Abs(bmpData.Stride);
            
            // 缓冲区大小取源和目标的最大值
            byte[] rowBuffer = new byte[Math.Max(srcStride, destStride)];

            for (int y = 0; y < height; y++)
            {
                IntPtr srcRow = IntPtr.Add(srcData, y * srcStride);
                IntPtr dstRow = IntPtr.Add(bmpData.Scan0, y * bmpData.Stride);

                // 1. 从非托管内存复制一整行（可能包含尾部 padding）
                Marshal.Copy(srcRow, rowBuffer, 0, srcStride);

                // 2. 严格在 widthInBytes 范围内交换 R 和 B 通道，避免碰到 padding 区域越界
                for (int i = 0; i < widthInBytes; i += 3)
                {
                    byte temp = rowBuffer[i];
                    rowBuffer[i] = rowBuffer[i + 2];
                    rowBuffer[i + 2] = temp;
                }

                // 3. 复制到目标位图
                Marshal.Copy(rowBuffer, 0, dstRow, destStride);
            }
        }
        finally
        {
            bitmap.UnlockBits(bmpData);
        }

        return bitmap;
    }
}