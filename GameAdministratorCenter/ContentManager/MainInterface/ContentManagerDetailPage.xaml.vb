
Imports System.IO
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Media.Imaging
Imports Accord.Video.FFMPEG
Imports DevExpress.Xpf.Editors
Imports GameAdministratorCenter.Contracts

Public Class ContentManagerDetailPage
    Public Enum ViewMode
        view = 1
        edit = 2
    End Enum

    Public currentManager As CONTENT_MANAGER
    Private managerType As ContentManagerType

    Private currentImage As Controls.Image
    Private currentAudioString As String
    Private currentVideoString As String
    Private currentFileString As String

    Private itemChanged As Boolean = False

    Public Delegate Sub afterClose()
    Public afterCloseDelegate As afterClose = Nothing

    Public Delegate Sub afterSave(currentManager As CONTENT_MANAGER)
    Public afterSaveDelegate As afterSave = Nothing

    Public Sub New(selectedImageManager As CONTENT_MANAGER, viewMode As ViewMode, type As ContentManagerType)

        ' This call is required by the designer.
        InitializeComponent()
        If selectedImageManager IsNot Nothing Then
            currentManager = selectedImageManager
        Else
            currentManager = New CONTENT_MANAGER
        End If

        managerType = type
        ' Add any initialization after the InitializeComponent() call.
        setLayout(type)
        setView(viewMode)
    End Sub

    Private Sub setLayout(type As ContentManagerType)
        Dim keyWord As String = ""

        Select Case type
            Case ContentManagerType.Image
                keyWord = "Image"
            Case ContentManagerType.Audio
                keyWord = "Audio"
            Case ContentManagerType.Video
                keyWord = "Video"
            Case ContentManagerType.GeneralFile
                keyWord = "File"
            Case Else
                keyWord = "Unknown file"
        End Select

        mainContainter.Header = keyWord + " Details"
        nameContainer.Label = keyWord + " Name:"
        typeContainer.Label = keyWord + " Type:"
        groupContainer.Label = keyWord + " Group:"
        categoryContainer.Label = keyWord + " Category:"
        previewContainer.Label = keyWord + " Preview:"
    End Sub

    Private Sub ContentManagerDetailPage_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        populateContent()
    End Sub

    Private Sub populateContent()
        'Dim allRelatedVersion As List(Of CONTENT_MANAGER) = helperService.getAllRelatedManagers(imageManager)

        versionNumberTextbox.Text = If(String.IsNullOrEmpty(currentManager.VERSION_NUMBER), "1", currentManager.VERSION_NUMBER)
        If currentManager.IS_DEFAULT = True AndAlso versionNumberTextbox.IsEnabled = False Then
            versionNumberTextbox.Text += " (Default Version)"
        End If
        nameTextbox.Text = currentManager.CONTENT_NAME
        typeTextbox.Text = currentManager.CONTENT_TYPE
        groupTextBox.Text = currentManager.CONTENT_GROUP
        categoryTextBox.Text = currentManager.CONTENT_CATEGORY

        Select Case managerType
            Case ContentManagerType.Image
                Dim imageEntity As IMAGE_STRING = currentManager.IMAGE_STRING
                If imageEntity IsNot Nothing AndAlso imageEntity.NAME IsNot Nothing Then
                    originalFileNameTextBox.Text = imageEntity.NAME
                End If
            Case ContentManagerType.Audio
                Dim audioEntity As AUDIO_STORAGE = currentManager.AUDIO_STORAGE
                If audioEntity IsNot Nothing AndAlso audioEntity.NAME IsNot Nothing Then
                    originalFileNameTextBox.Text = audioEntity.NAME
                End If
            Case ContentManagerType.Video
                Dim videoEntity As VIDEO_STORAGE = currentManager.VIDEO_STORAGE
                If videoEntity IsNot Nothing AndAlso videoEntity.NAME IsNot Nothing Then
                    originalFileNameTextBox.Text = videoEntity.NAME
                End If
            Case ContentManagerType.GeneralFile
                Dim fileEntity As FILE_STORAGE = currentManager.FILE_STORAGE
                If fileEntity IsNot Nothing AndAlso fileEntity.FILE_NAME IsNot Nothing Then
                    originalFileNameTextBox.Text = fileEntity.FILE_NAME
                End If
            Case Else

        End Select

        previewContainer.Content = Nothing

        Dim rootParent As CONTENT_MANAGER = getRootManager(currentManager)
        If rootParent IsNot Nothing Then
            versionComboBox.Items.Clear()
            versionComboBox.Clear()
            populateComboboxWithVersion(rootParent, 0)
        End If

        populatePreviewItem()
    End Sub

    Private Sub populatePreviewItem()
        Select Case managerType
            Case ContentManagerType.Image
                populatePreviewItemWWithImage()
            Case ContentManagerType.Audio
                populatePreviewItemWithAudio()
            Case ContentManagerType.Video
                populatePreviewItemWithVideo()
            Case ContentManagerType.GeneralFile
        End Select

    End Sub

    Private Sub populatePreviewItemWithVideo()
        Dim videoStorageEntity As VIDEO_STORAGE = currentManager.VIDEO_STORAGE
        If videoStorageEntity IsNot Nothing AndAlso videoStorageEntity.VIDEO_STRING IsNot Nothing Then
            currentVideoString = videoStorageEntity.VIDEO_STRING
            Dim videoPlayer As New MediaPlayer(currentVideoString)
            previewContainer.Content = videoPlayer
        End If
    End Sub

    Private Sub populatePreviewItemWithAudio()
        Dim audioStringEntity As AUDIO_STORAGE = currentManager.AUDIO_STORAGE
        If audioStringEntity IsNot Nothing AndAlso audioStringEntity.AUDIO_STRING IsNot Nothing Then
            currentAudioString = audioStringEntity.AUDIO_STRING
            Dim audioPlayer As New MediaPlayer(currentAudioString)
            previewContainer.Content = audioPlayer
        End If
    End Sub

    Private Sub populatePreviewItemWWithImage()
        Dim imageStringEntity As IMAGE_STRING = currentManager.IMAGE_STRING
        If imageStringEntity IsNot Nothing Then
            Dim fullImage As Image = convertHexStringToControlImage(imageStringEntity.IMAGE_HEX_STRING)
            If fullImage IsNot Nothing Then
                previewContainer.Content = fullImage
                currentImage = fullImage
            End If
        End If
    End Sub

    Private Sub populateComboboxWithVersion(currentVersion As CONTENT_MANAGER, currentLevel As Integer)
        Dim comboBoxEditItem As New ComboBoxEditItem
        Dim prefixString As String = ""
        If currentLevel > 0 Then
            For i As Integer = 1 To currentLevel Step 1
                prefixString += "      "
            Next
        End If

        comboBoxEditItem.Content = prefixString + currentVersion.VERSION_NUMBER
        comboBoxEditItem.Tag = currentVersion
        If currentVersion.IS_DEFAULT = True Then
            comboBoxEditItem.FontWeight = FontWeights.Bold
            comboBoxEditItem.Content += " (Default Version)"
        End If
        versionComboBox.Items.Add(comboBoxEditItem)

        If currentVersion.CONTENT_MANAGER1 IsNot Nothing AndAlso currentVersion.CONTENT_MANAGER1.Count > 0 Then
            For Each item As CONTENT_MANAGER In currentVersion.CONTENT_MANAGER1
                If item.IS_DELETED = False Then
                    populateComboboxWithVersion(item, currentLevel + 1)
                End If
            Next
        End If
    End Sub

    Public Sub setView(viewMode As ViewMode)
        If viewMode = ViewMode.edit Then
            buttonGroup.Visibility = Visibility.Visible
            versionComboBox.IsEnabled = True
            versionNumberTextbox.IsEnabled = True
            nameTextbox.IsEnabled = True
            typeTextbox.IsEnabled = True
            groupTextBox.IsEnabled = True
            categoryTextBox.IsEnabled = True
            versionNumberContainer.FontWeight = FontWeights.Bold
        Else
            buttonGroup.Visibility = Visibility.Collapsed
            versionNumberTextbox.IsEnabled = False
            versionComboBox.IsEnabled = True
            nameTextbox.IsEnabled = False
            typeTextbox.IsEnabled = False
            groupTextBox.IsEnabled = False
            categoryTextBox.IsEnabled = False
            versionNumberContainer.FontWeight = FontWeights.Normal
        End If
    End Sub

    Private Sub versionComboBox_SelectedIndexChanged(sender As Object, e As RoutedEventArgs) Handles versionComboBox.SelectedIndexChanged
        If versionComboBox.SelectedItem IsNot Nothing Then
            Try
                Dim comboxEditItem As ComboBoxEditItem = DirectCast(versionComboBox.SelectedItem, ComboBoxEditItem)
                Dim selectedVersion As CONTENT_MANAGER = DirectCast(comboxEditItem.Tag, CONTENT_MANAGER)
                currentManager = selectedVersion
                populateContent()
                setView(ViewMode.view)
            Catch ex As Exception
                Return
            End Try
        End If
    End Sub

    Private Sub uploadButton_Click(sender As Object, e As RoutedEventArgs) Handles uploadButton.Click
        Try
            Select Case managerType
                Case ContentManagerType.Image
                    uploadImage()
                Case ContentManagerType.Audio
                    uploadAudio()
                Case ContentManagerType.Video
                    uploadVideo()
                Case ContentManagerType.GeneralFile
                    uploadFile()
            End Select
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub

    Private Sub uploadFile()
        Dim fileUpLoadDialog As New Forms.OpenFileDialog()
        fileUpLoadDialog.Filter = "All Files (*.*)|*.*"
        fileUpLoadDialog.Multiselect = False

        Dim fileUploadResult As String = fileUpLoadDialog.ShowDialog.ToString

        If fileUploadResult.ToUpper.Equals("OK") Then
            Dim fileName As String = fileUpLoadDialog.FileName
            Dim extension As String = Path.GetExtension(fileName).ToLower

            Try
                Dim byteArray As Byte() = File.ReadAllBytes(fileName)
                currentFileString = Convert.ToBase64String(byteArray)
                itemChanged = True
                originalFileNameTextBox.Text = fileName

                If extension.ToLower = ".pdf" Then
                    'Dim pdfViewer As New DevExpress.Xpf.PdfViewer.PdfViewerControl
                    'pdfViewer.DocumentSource = fileName
                    Dim pdf_processor As New DevExpress.Pdf.PdfDocumentProcessor
                    pdf_processor.LoadDocument(fileName)
                    ' PDFViewer.Document.CreateBitmap(0, 120)
                    Dim bitmap As BitmapSource = convertBitmapToBitmapSource(pdf_processor.CreateBitmap(1, 120))
                    Dim drawingImage As Drawing.Image
                    Using outStream As New MemoryStream()
                        Dim enc As BitmapEncoder = New BmpBitmapEncoder()
                        enc.Frames.Add(BitmapFrame.Create(bitmap))
                        enc.Save(outStream)
                        drawingImage = New Drawing.Bitmap(outStream).GetThumbnailImage(100, bitmap.Height * 100 / bitmap.Width, Nothing, Nothing)
                    End Using
                    Dim thumbnailImageString As String = ImageToBMPBase64(drawingImage)
                    currentManager.CONTENT_THUMBNAIL = thumbnailImageString

                End If
                refresh_content_name(Path.GetFileNameWithoutExtension(fileName))
                Dim textBlock As New TextBlock
                textBlock.HorizontalAlignment = HorizontalAlignment.Center
                textBlock.VerticalAlignment = VerticalAlignment.Center
                textBlock.FontSize = 20
                textBlock.FontWeight = FontWeights.DemiBold
                textBlock.Text = "Upload Complete"
                previewContainer.Content = textBlock
            Catch ex As Exception
                MessageBox.Show("Unable to read the file.")
            End Try
        End If
    End Sub

    Private Sub uploadVideo()
        Dim fileUpLoadDialog As New Forms.OpenFileDialog()
        fileUpLoadDialog.Filter = "Video Files (*.WMV;*.AVI;*.mp4)|*.WMV;*.AVI;*.mp4"
        fileUpLoadDialog.Multiselect = False

        Dim fileUploadResult As String = fileUpLoadDialog.ShowDialog.ToString

        If fileUploadResult.ToUpper.Equals("OK") Then
            Dim fileName As String = fileUpLoadDialog.FileName
            Dim extension As String = Path.GetExtension(fileName).ToLower

            If extension.ToLower() = ".mp4" OrElse extension = ".avi" OrElse extension = ".wmv" Then
                Try
                    Dim byteArray As Byte() = File.ReadAllBytes(fileName)
                    currentVideoString = Convert.ToBase64String(byteArray)
                    itemChanged = True
                    originalFileNameTextBox.Text = fileName
                    previewContainer.Content = New MediaPlayer(currentVideoString)

                    Dim ffmpeg = New VideoFileReader
                    ffmpeg.Open(fileName)
                    Dim drawingBitmap As Drawing.Bitmap = ffmpeg.ReadVideoFrame()
                    Dim thumbnailDrawingImage As Drawing.Image = drawingBitmap.GetThumbnailImage(120, 90, Nothing, Nothing)
                    Dim thumbnailImageString As String = ImageToBMPBase64(thumbnailDrawingImage)
                    currentManager.CONTENT_THUMBNAIL = thumbnailImageString
                    refresh_content_name(Path.GetFileNameWithoutExtension(fileName))
                Catch ex As Exception
                    MessageBox.Show("Unable to read the video file.")
                End Try
            Else
                MessageBox.Show("Image should be in MP4, AVI or WMV format.")
            End If
        End If
    End Sub

    Private Sub uploadAudio()
        Dim fileUpLoadDialog As New Forms.OpenFileDialog()
        fileUpLoadDialog.Filter = "Audio Files|*.mp3;*.wav;*.wma"
        fileUpLoadDialog.Multiselect = False

        Dim fileUploadResult As String = fileUpLoadDialog.ShowDialog.ToString

        If fileUploadResult.ToUpper.Equals("OK") Then
            Dim fileName As String = fileUpLoadDialog.FileName
            Dim extension As String = Path.GetExtension(fileName).ToLower

            If extension = ".mp3" OrElse extension = ".wav" OrElse extension = ".mp4" OrElse extension = ".wma" Then
                Try
                    Dim audioPlayer As New MediaPlayer(fileName, MediaPlayer.InputStringType.fileName)
                    previewContainer.Content = audioPlayer
                    Dim byteArray As Byte() = File.ReadAllBytes(fileName)
                    currentAudioString = Convert.ToBase64String(byteArray)
                    itemChanged = True
                    originalFileNameTextBox.Text = fileName
                    refresh_content_name(Path.GetFileNameWithoutExtension(fileName))
                Catch ex As Exception
                    MessageBox.Show("Unable to read the audio file.")
                End Try
            Else
                MessageBox.Show("Image should be in WAV, MP4 or Mp3 format.")
            End If
        End If
    End Sub

    Private Sub uploadImage()
        Dim fileUpLoadDialog As New Forms.OpenFileDialog()
        fileUpLoadDialog.Filter = "Image Files|*.jpeg;*.png;*.jpg;*.bmp"
        fileUpLoadDialog.Multiselect = False

        Dim fileUploadResult As String = fileUpLoadDialog.ShowDialog.ToString

        If fileUploadResult.ToUpper.Equals("OK") Then
            Dim fileName As String = fileUpLoadDialog.FileName
            Dim extension As String = Path.GetExtension(fileName).ToLower

            If extension = ".jpeg" OrElse extension = ".jpg" OrElse extension = ".png" OrElse extension = ".bmp" Then
                Try
                    Dim drawingImage As Drawing.Image = getDrawingImageFromFilePath(fileName)

                    If drawingImage IsNot Nothing Then
                        Dim controlImage As Controls.Image = convertDrawingImageToControlImage(drawingImage)
                        If controlImage IsNot Nothing Then
                            currentImage = controlImage
                            previewContainer.Content = controlImage
                            itemChanged = True
                            originalFileNameTextBox.Text = fileName

                            Dim thumbnailControlImage As Controls.Image = createThumbnailFromFullsizeImage(currentImage)
                            Dim thumbnailDrawingImage As Drawing.Image = convertControlImageToDrawingImage(thumbnailControlImage)
                            Dim thumbnailImageString As String = ImageToBMPBase64(thumbnailDrawingImage)
                            currentManager.CONTENT_THUMBNAIL = thumbnailImageString
                            refresh_content_name(Path.GetFileNameWithoutExtension(fileName))
                        Else
                            MessageBox.Show("Unable to read the image file.")
                        End If
                    End If

                Catch ex As Exception
                    MessageBox.Show("Unable to read the image file.")
                End Try
            Else
                MessageBox.Show("Image should be in PNG, JPG or BMP format.")
            End If
        End If
    End Sub

    Private Sub refresh_content_name(fileName As String)
        If Not String.IsNullOrEmpty(fileName) Then
            If String.IsNullOrEmpty(nameTextbox.Text) Then
                nameTextbox.Text = fileName
            End If
        End If
    End Sub

    Private Sub cancelButton_Click(sender As Object, e As RoutedEventArgs) Handles cancelButton.Click
        If afterCloseDelegate IsNot Nothing Then
            afterCloseDelegate()
        Else
            populateContent()
            setView(ViewMode.view)
        End If
    End Sub

    Private Sub saveButton_Click(sender As Object, e As RoutedEventArgs) Handles saveButton.Click

        Dim noError As Boolean = checkNoErrorBeforeSave()
        If noError Then
            Dim proceed As Boolean = pullInfoIntoProperty()
            If proceed Then
                Try
                    If currentManager.ID = 0 Then
                        getDatabaseEntity.CONTENT_MANAGER.Add(currentManager)
                    End If
                    getDatabaseEntity.SaveChanges()
                    MessageBox.Show("Save Successful")
                    If afterSaveDelegate IsNot Nothing Then
                        afterSaveDelegate(currentManager)
                    Else
                        setView(ViewMode.view)
                    End If
                Catch ex As Exception
                    MessageBox.Show("Save Failed")
                End Try
            End If
        End If

    End Sub

    Private Function pullInfoIntoProperty() As Boolean
        Dim proceed As Boolean = True
        If itemChanged = True Then
            Select Case managerType
                Case ContentManagerType.Image
                    proceed = saveNewImage()
                Case ContentManagerType.Audio
                    proceed = saveNewAudio()
                Case ContentManagerType.Video
                    proceed = saveNewVideo()
                Case ContentManagerType.GeneralFile
                    proceed = saveNewFile()
            End Select

        End If

        If proceed Then
            currentManager.CONTENT_NAME = nameTextbox.Text
            currentManager.CONTENT_TYPE = typeTextbox.Text
            currentManager.CONTENT_GROUP = groupTextBox.Text
            currentManager.CONTENT_CATEGORY = categoryTextBox.Text
            currentManager.VERSION_NUMBER = versionNumberTextbox.Text.Replace(" (Default Version)", "")
        End If

        Return proceed
    End Function

    Private Function saveNewFile() As Boolean
        Try
            Dim fileStorageEntity As New FILE_STORAGE
            fileStorageEntity.FILE_STRING = currentFileString
            fileStorageEntity.FILE_NAME = originalFileNameTextBox.Text
            fileStorageEntity.IS_DELETED = False

            Dim currentStorage As FILE_STORAGE = currentManager.FILE_STORAGE
            If currentStorage IsNot Nothing Then
                currentStorage.IS_DELETED = True
            End If

            fileStorageEntity.CONTENT_MANAGER.Add(currentManager)
            getDatabaseEntity.FILE_STORAGE.Add(fileStorageEntity)
            getDatabaseEntity.SaveChanges()
            Return True
        Catch ex As Exception
            MessageBox.Show("Unable to save file to the database")
            Return False
        End Try
    End Function

    Private Function saveNewVideo() As Boolean
        Try
            Dim videoStorageEntity As New VIDEO_STORAGE
            videoStorageEntity.VIDEO_STRING = currentVideoString
            videoStorageEntity.NAME = originalFileNameTextBox.Text
            videoStorageEntity.IS_DELETED = False

            Dim currentStorage As VIDEO_STORAGE = currentManager.VIDEO_STORAGE
            If currentStorage IsNot Nothing Then
                currentStorage.IS_DELETED = True
            End If

            videoStorageEntity.CONTENT_MANAGER.Add(currentManager)
            getDatabaseEntity.VIDEO_STORAGE.Add(videoStorageEntity)
            getDatabaseEntity.SaveChanges()

            Return True
        Catch ex As Exception
            MessageBox.Show("Unable to save video to the database")
            Return False
        End Try
    End Function

    Private Function saveNewAudio() As Boolean
        Try
            Dim audioStorageEntity As New AUDIO_STORAGE
            audioStorageEntity.AUDIO_STRING = currentAudioString
            audioStorageEntity.NAME = originalFileNameTextBox.Text
            audioStorageEntity.IS_DELETED = False

            Dim currentStorage As AUDIO_STORAGE = currentManager.AUDIO_STORAGE
            If currentStorage IsNot Nothing Then
                currentStorage.IS_DELETED = True
            End If

            audioStorageEntity.CONTENT_MANAGER.Add(currentManager)
            getDatabaseEntity.AUDIO_STORAGE.Add(audioStorageEntity)
            getDatabaseEntity.SaveChanges()

            'currentManager.IMAGE_STRING_ID_LINK = DatabaseFunctions.get_global_ueios_entity().IMAGE_STRING.ToList().FindLast(Function(c) c.IS_DELETED = False).ID
            Return True
        Catch ex As Exception
            MessageBox.Show("Unable to save audio to the database")
            Return False
        End Try
    End Function

    Private Function saveNewImage() As Boolean
        Try
            Dim imageStringEntity As New IMAGE_STRING
            Dim drawingImage As Drawing.Image = convertControlImageToDrawingImage(currentImage)
            Dim imageHexString As String = ImageToBMPBase64(drawingImage)
            imageStringEntity.IMAGE_HEX_STRING = imageHexString
            imageStringEntity.NAME = originalFileNameTextBox.Text
            imageStringEntity.IS_DELETED = False

            Dim currentStorage As IMAGE_STRING = currentManager.IMAGE_STRING
            If currentStorage IsNot Nothing Then
                currentStorage.IS_DELETED = True
            End If

            imageStringEntity.CONTENT_MANAGER.Add(currentManager)
            getDatabaseEntity.IMAGE_STRING.Add(imageStringEntity)
            getDatabaseEntity.SaveChanges()
            Return True
        Catch ex As Exception
            MessageBox.Show("Unable to save image to the database")
            Return False
        End Try
    End Function

    Private Function checkNoErrorBeforeSave() As Boolean
        Dim noError As Boolean = True
        If versionNumberTextbox.Text Is Nothing OrElse versionNumberTextbox.Text.Trim.Length = 0 Then
            MessageBox.Show("Must insert a version number")
            versionNumberTextbox.Focus()
            noError = False
        End If

        If noError Then
            Select Case managerType
                Case ContentManagerType.Image
                    If currentImage Is Nothing Then
                        MessageBox.Show("No picture was selected")
                        noError = False
                    End If
                Case ContentManagerType.Audio
                    If currentAudioString Is Nothing OrElse currentAudioString.Trim.Length = 0 Then
                        MessageBox.Show("No audio file was selected")
                        noError = False
                    End If
                Case ContentManagerType.Video
                    If currentVideoString Is Nothing OrElse currentVideoString.Trim.Length = 0 Then
                        MessageBox.Show("No video file was selected")
                        noError = False
                    End If
                Case ContentManagerType.GeneralFile
                    If currentFileString Is Nothing OrElse currentFileString.Trim.Length = 0 Then
                        MessageBox.Show("No file was selected")
                        noError = False
                    End If
            End Select
        End If

        Return noError
    End Function

    Public Sub hideVersionBrowserGroup()
        versionBrowserGroup.Visibility = Visibility.Collapsed
    End Sub

    Public Sub hideVersionBrowserButton()
        createSubversionButton.Visibility = Visibility.Collapsed
    End Sub

    Public Sub hideSetDefaultVersionButton()
        setDefaultVersionButton.Visibility = Visibility.Collapsed
    End Sub

    Private Sub createSubversionButton_Click(sender As Object, e As RoutedEventArgs) Handles createSubversionButton.Click
        If currentManager IsNot Nothing Then
            Dim newManager As New CONTENT_MANAGER
            newManager.IS_DEFAULT = False
            newManager.IS_DELETED = False
            newManager.CONTENT_MANAGER_TYPE = managerType
            newManager.PARENT_VERSION_LINK_ID = currentManager.ID

            Dim newManagerDetailPage As New ContentManagerDetailPage(newManager, ContentManagerDetailPage.ViewMode.edit, managerType)
            newManagerDetailPage.hideVersionBrowserGroup()
            newManagerDetailPage.hideSetDefaultVersionButton()

            Dim viewParentPage As New ContentManagerDetailPage(currentManager, ViewMode.view, managerType)
            viewParentPage.hideVersionBrowserButton()
            viewParentPage.hideSetDefaultVersionButton()

            Dim keyWord As String = ""
            Select Case managerType
                Case ContentManagerType.Image
                    keyWord = "Image"
                Case ContentManagerType.Audio
                    keyWord = "Audio"
                Case ContentManagerType.Video
                    keyWord = "Video"
                Case ContentManagerType.GeneralFile
                    keyWord = "File"
                Case Else
                    keyWord = "Unknown file"
            End Select

            newManagerDetailPage.addViewPage(viewParentPage, "Parent " + keyWord + ": " + currentManager.VERSION_NUMBER)
            newManagerDetailPage.afterCloseDelegate = AddressOf PopupDialogFunctions.close_dialog
            newManagerDetailPage.afterSaveDelegate = AddressOf closeDialogAndPopulateContent

            open_popup_with_max_size(newManagerDetailPage)
        End If
    End Sub

    Private Sub closeDialogAndPopulateContent(passedInManager As CONTENT_MANAGER)
        currentManager = passedInManager
        PopupDialogFunctions.close_dialog()
        populateContent()
    End Sub

    Public Sub addViewPage(userControl As Controls.UserControl, Optional header As String = "")
        extraViewPageContainer.Visibility = Visibility.Visible
        extraViewPageContainer.Header = header
        extraViewPageContainer.Children.Clear()
        extraViewPageContainer.Children.Add(userControl)
    End Sub

    Private Sub setDefaultVersionButton_Click(sender As Object, e As RoutedEventArgs) Handles setDefaultVersionButton.Click
        If currentManager IsNot Nothing Then
            setClusterDefaultManager(currentManager)
            populateContent()
        End If
    End Sub

    'Private Sub recordButton_Click(sender As Object, e As RoutedEventArgs) Handles recordButton.Click
    '    If managerType = ContentManagerType.Audio Then
    '        Dim audioRecorder = New AudioRecorder()
    '        PopupDialogFunctions.open_dialog(1000, 1000, audioRecorder)
    '    End If
    'End Sub
End Class
