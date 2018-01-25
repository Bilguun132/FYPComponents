Imports System.Windows.Media.Imaging
Imports System.Windows
Imports System.Runtime.InteropServices
Imports System.Windows.Controls
Imports System.Drawing
Imports System.IO
Imports GameAdministratorCenter.Contracts

Public Module ImageProcessing
    Public defaultThumbnailSize As Integer = 500

    Public Function getBitmapSourceFromImageHexString(imageHexString As String) As BitmapSource
        Try
            Dim image As New Controls.Image
            image = convertHexStringToControlImage(imageHexString)
            Dim drawingImage As Drawing.Image = convertControlImageToDrawingImage(image)
            Return getImageStream(drawingImage)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function getImageStream(myImage As Drawing.Image) As BitmapSource
        Try
            Dim bitmap = New Bitmap(myImage)
            Return convertBitmapToBitmapSource(bitmap)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function convertBitmapToBitmapSource(bitmap As Bitmap) As BitmapSource
        Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
    End Function

    Public Function ImageSourceToBitmapImage(ByRef img As Drawing.Image) As BitmapImage
        Dim memoryStream As New IO.MemoryStream
        Dim bitmapImage As New BitmapImage
        img.Save(memoryStream, Imaging.ImageFormat.Png)
        bitmapImage.BeginInit()
        bitmapImage.DecodePixelHeight = 50
        bitmapImage.StreamSource = memoryStream
        bitmapImage.EndInit()
        Return bitmapImage
    End Function

    Public Function returnImageHexStringByContentManagerId(ByVal contentManagerId As Integer) As String
        Dim hexStringToReturn As String = Nothing
        Dim imageManager As CONTENT_MANAGER = (From query In getDatabaseEntity().CONTENT_MANAGER Where query.ID = contentManagerId).FirstOrDefault
        If imageManager IsNot Nothing AndAlso imageManager.IMAGE_STRING IsNot Nothing Then
            hexStringToReturn = imageManager.IMAGE_STRING.IMAGE_HEX_STRING
        End If

        Return hexStringToReturn
    End Function

    Public Function createThumbnailFromFullsizeImage(ByVal fullSizeImage As Drawing.Image) As Drawing.Image
        If fullSizeImage IsNot Nothing Then
            Dim theThumbnail As Drawing.Image = resizeDrawingImageBySize(fullSizeImage, defaultThumbnailSize)
            Return theThumbnail
        End If

        Return Nothing
    End Function

    Public Function createThumbnailFromFullsizeImage(ByVal fullSizeImage As Controls.Image) As Controls.Image
        Dim returnedImage As Controls.Image = Nothing
        If fullSizeImage IsNot Nothing Then
            Dim drawingImage As Drawing.Image = convertControlImageToDrawingImage(fullSizeImage)
            If drawingImage IsNot Nothing Then
                Dim thumbNail As Drawing.Image = resizeDrawingImageBySize(drawingImage, defaultThumbnailSize)
                If thumbNail IsNot Nothing Then
                    returnedImage = convertDrawingImageToControlImage(thumbNail)
                End If
            End If
        End If
        Return returnedImage
    End Function

    Public Function resizeDrawingImageBySize(ByRef originalImage As Drawing.Image, ByVal size As Integer) As Drawing.Image
        Dim newHeight As Integer
        Dim newWidth As Integer
        Dim aspectRatio As Integer

        If originalImage.Width >= originalImage.Height Then ' image is wider than tall
            newWidth = size
            aspectRatio = (originalImage.Height / originalImage.Width) * size
            newHeight = aspectRatio
        End If

        If originalImage.Height >= originalImage.Width Then
            newHeight = size
            aspectRatio = (originalImage.Width / originalImage.Height) * size
            newWidth = aspectRatio
        End If

        If originalImage.Height = originalImage.Width Then
            newHeight = size
            newWidth = size
        End If

        Dim resizedImage As Drawing.Image = resizeDrawingImageByDimensionAndType(originalImage, newWidth, newHeight, 3)
        Return resizedImage
    End Function

    Public Function resizeDrawingImageByDimensionAndType(drawingImage As Drawing.Image, width As Integer, height As Integer, Optional type As Integer = 0) As Drawing.Image
        Dim bm As Bitmap = New Bitmap(width, height)
        Using g As Graphics = Graphics.FromImage(bm)
            g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

            Select Case type
                Case 0
                    g.InterpolationMode = Drawing2D.InterpolationMode.Default
                Case 1
                    g.InterpolationMode = Drawing2D.InterpolationMode.High
                Case 2
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBilinear
                Case 3
                    g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            End Select

            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            g.DrawImage(drawingImage, 0, 0, width, height)
        End Using
        Return DirectCast(bm, Drawing.Image)
    End Function

    Public Function ReplaceTransparency(bitmap As BitmapSource, color As Media.Color) As BitmapSource
        Dim rect = New Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight)
        Dim visual = New Media.DrawingVisual()
        Dim context = visual.RenderOpen()
        context.DrawRectangle(New Media.SolidColorBrush(color), Nothing, rect)
        context.DrawImage(bitmap, rect)
        context.Close()

        Dim render = New RenderTargetBitmap(bitmap.PixelWidth, bitmap.PixelHeight, 96, 96, Media.PixelFormats.Pbgra32)
        render.Render(visual)
        Return render
    End Function

    Public Sub createThumbnailForContentManager(ByRef contentManager As CONTENT_MANAGER)
        If contentManager IsNot Nothing AndAlso (contentManager.CONTENT_THUMBNAIL Is Nothing OrElse contentManager.CONTENT_THUMBNAIL.Trim = "") Then
            If contentManager.IMAGE_STRING IsNot Nothing AndAlso contentManager.IMAGE_STRING.IMAGE_HEX_STRING IsNot Nothing AndAlso contentManager.IMAGE_STRING.IMAGE_HEX_STRING.Trim <> "" Then
                Dim controlImage As Controls.Image = convertHexStringToControlImage(contentManager.IMAGE_STRING.IMAGE_HEX_STRING)
                If controlImage IsNot Nothing AndAlso controlImage.Source IsNot Nothing Then
                    Dim thumbnail As Controls.Image = createThumbnailFromFullsizeImage(controlImage)
                    contentManager.CONTENT_THUMBNAIL = convertControlImageToHexString(thumbnail)
                    getDatabaseEntity.SaveChanges()
                End If
            End If
        End If
    End Sub

    Public Function convertHexStringToControlImage(ByVal hexString As String) As Controls.Image
        Dim returnedImage As Controls.Image = Nothing

        If hexString <> "" AndAlso hexString.Count > 1 Then
            Try
                returnedImage = New Controls.Image
                Dim bi As New BitmapImage()

                bi.BeginInit()
                bi.StreamSource = New IO.MemoryStream(System.Convert.FromBase64String(hexString))
                bi.EndInit()
                returnedImage.Source = bi
            Catch ex As Exception

            End Try
        End If

        Return returnedImage
    End Function

    Public Function convertControlImageToHexString(ByVal controlImage As Controls.Image) As String
        Dim imageString As String = ""
        If controlImage IsNot Nothing Then
            Dim drawingImage As Drawing.Image = convertControlImageToDrawingImage(controlImage)
            imageString = convertDrawingImageToHexString(drawingImage)
        End If
        Return imageString
    End Function

    Public Function convertDrawingImageToHexString(ByVal drawingImage As Drawing.Image) As String
        Dim imageHexString As String = ""
        imageHexString = ImageToBMPBase64(drawingImage)
        Return imageHexString
    End Function

    Public Function ImageToBMPBase64(drawingImage As Drawing.Image) As String
        Using ms As New IO.MemoryStream()
            ' Convert Image to byte[]
            drawingImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp)
            Dim imageBytes As Byte() = ms.ToArray()

            ' Convert byte[] to Base64 String
            Dim base64String As String = Convert.ToBase64String(imageBytes)
            Return base64String
        End Using
    End Function

    Public Function convertControlImageToDrawingImage(image As Controls.Image) As Drawing.Image
        Dim drawingImage As System.Drawing.Image = Nothing
        If image IsNot Nothing Then
            Dim imageSource As System.Windows.Media.Imaging.BitmapImage = image.Source
            drawingImage = System.Drawing.Image.FromStream(imageSource.StreamSource)
        End If
        Return drawingImage
    End Function

    Public Function convertDrawingImageToControlImage(drawingImage As Drawing.Image) As Controls.Image
        Dim controlImage As Controls.Image = Nothing
        If drawingImage IsNot Nothing Then
            Dim the_image_string As String = ImageToBMPBase64(drawingImage)
            controlImage = convertHexStringToControlImage(the_image_string)
        End If
        Return controlImage
    End Function
End Module
