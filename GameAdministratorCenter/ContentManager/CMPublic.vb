Imports System.IO
Imports System.Windows.Controls
Imports System.Windows.Media.Imaging
Imports Accord.Video.FFMPEG
Imports GameAdministratorCenter.Contracts

Public Module CMPublic
    Public Sub open_content_viewer_by_content_manager_id(ByVal content_manager_id As Integer)
        Dim matching_content = (From i In DatabaseEntitySingleton.getDatabaseEntity.CONTENT_MANAGER Where i.ID = content_manager_id).SingleOrDefault
        If matching_content IsNot Nothing AndAlso matching_content.CONTENT_MANAGER_TYPE IsNot Nothing Then

            If matching_content.CONTENT_MANAGER_TYPE = ContentManagerType.Audio Then
                openAudioView(content_manager_id)
            ElseIf matching_content.CONTENT_MANAGER_TYPE = ContentManagerType.Image Then
                openImageView(content_manager_id)
            ElseIf matching_content.CONTENT_MANAGER_TYPE = ContentManagerType.GeneralFile Then
                Dim file_storage As FILE_STORAGE = matching_content.FILE_STORAGE
                If file_storage IsNot Nothing Then
                    Dim file_string As String = file_storage.FILE_STRING
                    Dim file_name As String = file_storage.FILE_NAME
                    If GetExtension(file_name) = ".pdf" Then
                        Dim pdf_viewer As New PDFViewer(file_string)
                        pdf_viewer.afterCloseFunctionDelegate = AddressOf close_popup
                        open_popup_with_max_size(pdf_viewer)
                    ElseIf GetExtension(file_name) = ".xlsx" OrElse GetExtension(file_name) = ".xls" Then
                        Dim excel_viewer As New ExcelViewer(file_string)
                        excel_viewer.DelegateCloseFunction = AddressOf close_popup
                        open_popup_with_max_size(excel_viewer)
                    Else

                    End If
                End If
            ElseIf matching_content.CONTENT_MANAGER_TYPE = ContentManagerType.Video Then
                openVideoView(content_manager_id)
            End If
        End If

    End Sub

    Public Sub close_popup()
        PopupDialogFunctions.close_dialog()
    End Sub

    Public Function GetExtension(ByVal inp_file_name As String)
        Dim extension_position As Integer = inp_file_name.LastIndexOf(".")
        Return inp_file_name.Substring(extension_position)
    End Function


    Public Sub open_popup_with_max_size(ByVal uc As UserControl)
        PopupDialogFunctions.open_dialog(uc)
    End Sub

    Public Sub openImageView(selectedManagerID As Integer)
        Dim viewPage As New ImageViewer(selectedManagerID)
        viewPage.showButtonPanel()
        viewPage.afterCloseFunctionDelegate = AddressOf PopupDialogFunctions.close_dialog
        open_popup_with_max_size(viewPage)
    End Sub

    Public Sub openVideoView(selectedManagerID As Integer)
        Dim videoPlayer As New MediaPlayer(selectedManagerID)
        videoPlayer.afterCloseFunctionDelegate = AddressOf PopupDialogFunctions.close_dialog
        open_popup_with_max_size(videoPlayer)
    End Sub

    Public Sub openAudioView(selectedManagerID As Integer)
        Dim audioPlayer As New MediaPlayer(selectedManagerID)
        audioPlayer.afterCloseFunctionDelegate = AddressOf PopupDialogFunctions.close_dialog
        open_popup_with_max_size(audioPlayer)
    End Sub

    Public Function getDrawingImageFromFilePath(fileName As String) As Drawing.Image
        Dim uri As Uri = New Uri(fileName)
        Dim bitmap As New BitmapImage(uri)
        Dim white = ReplaceTransparency(bitmap, System.Windows.Media.Colors.White)
        Dim encoder As New JpegBitmapEncoder()
        encoder.Frames.Add(BitmapFrame.Create(white))

        Dim ms As New MemoryStream()
        encoder.Save(ms)
        Dim drawingImage As Drawing.Image = Drawing.Image.FromStream(ms)
        Return drawingImage
    End Function

    ''' <summary>
    ''' Function to upload files into the database. The database in which the files are uploaded to
    ''' is determined by the preset connection string in GlobalArcopsFunctionality.DatabaseFunctions
    ''' </summary>
    ''' <param name="fileContent">The content of the file to be uploaded</param>
    ''' <param name="uploadSetting">Additional settings for the CONTENT_MANAGER entity to be created</param>
    ''' <returns></returns>
    Public Function uploadFile(fileContent As FileContent, uploadSetting As UploadSetting) As Integer
        Try
            Dim newManager As New CONTENT_MANAGER
            newManager.IS_DELETED = False
            getDatabaseEntity.CONTENT_MANAGER.Add(newManager)

            If fileContent IsNot Nothing Then
                updateManagerWithFileContent(fileContent, newManager)
                populateManagerWithUploadSetting(newManager, uploadSetting)
                getDatabaseEntity.SaveChanges()
                Return newManager.ID
            End If
        Catch ex As Exception

        End Try
        Return 0
    End Function

    Private Sub updateManagerWithFileContent(fileContent As FileContent, newManager As CONTENT_MANAGER)
        If fileContent.filePath IsNot Nothing AndAlso fileContent.filePath.Trim.Length > 0 Then
            If fileContent.fileType <> ContentManagerType.NotSet Then
                newManager.CONTENT_MANAGER_TYPE = fileContent.fileType
            Else
                Dim extension As String = Path.GetExtension(fileContent.filePath).ToLower
                If extension IsNot Nothing AndAlso extension.Trim.Length > 0 Then
                    Select Case extension
                        Case ".jpeg", ".jpg", ".png", ".bmp"
                            newManager.CONTENT_MANAGER_TYPE = ContentManagerType.Image
                        Case ".wmv", ".mp4", ".avi"
                            newManager.CONTENT_MANAGER_TYPE = ContentManagerType.Video
                        Case ".mp3", ".wav", ".wma"
                            newManager.CONTENT_MANAGER_TYPE = ContentManagerType.Audio
                        Case Else
                            newManager.CONTENT_MANAGER_TYPE = ContentManagerType.GeneralFile
                    End Select
                End If
            End If

            Select Case newManager.CONTENT_MANAGER_TYPE
                Case ContentManagerType.Image
                    updateManagerWithImage(fileContent, newManager)
                Case ContentManagerType.Video
                    updateManagerWithVideo(fileContent, newManager)
                Case ContentManagerType.Audio
                    updateManagerWithAudio(fileContent, newManager)
                Case ContentManagerType.GeneralFile
                    updateManagerWithFile(fileContent, newManager)
            End Select
        Else
            If fileContent.fileHexString IsNot Nothing AndAlso fileContent.fileHexString.Trim.Length > 0 AndAlso fileContent.fileType <> ContentManagerType.NotSet Then
                newManager.CONTENT_MANAGER_TYPE = fileContent.fileType

                Select Case newManager.CONTENT_MANAGER_TYPE
                    Case ContentManagerType.Image
                        updateManagerWithImage(fileContent, newManager, True)
                    Case ContentManagerType.Video
                        updateManagerWithVideo(fileContent, newManager, True)
                    Case ContentManagerType.Audio
                        updateManagerWithAudio(fileContent, newManager, True)
                    Case ContentManagerType.GeneralFile
                        updateManagerWithFile(fileContent, newManager, True)
                End Select
            End If
        End If
    End Sub

    Private Sub updateManagerWithFile(fileContent As FileContent, newManager As CONTENT_MANAGER, Optional useHexString As Boolean = False)
        Dim currentFileString As String
        Dim fileStorageEntity As New FILE_STORAGE
        If Not useHexString Then
            Dim byteArray As Byte() = File.ReadAllBytes(fileContent.filePath)
            currentFileString = Convert.ToBase64String(byteArray)
            fileStorageEntity.FILE_NAME = fileContent.filePath

            Dim extension As String = Path.GetExtension(fileContent.filePath).ToLower
            If extension.ToLower = ".pdf" Then
                Dim pdf_processor As New DevExpress.Pdf.PdfDocumentProcessor
                pdf_processor.LoadDocument(fileContent.filePath)
                Dim bitmap As BitmapSource = convertBitmapToBitmapSource(pdf_processor.CreateBitmap(1, 120))
                Dim drawingImage As Drawing.Image
                Using outStream As New MemoryStream()
                    Dim enc As BitmapEncoder = New BmpBitmapEncoder()
                    enc.Frames.Add(BitmapFrame.Create(bitmap))
                    enc.Save(outStream)
                    drawingImage = New Drawing.Bitmap(outStream).GetThumbnailImage(100, bitmap.Height * 100 / bitmap.Width, Nothing, Nothing)
                End Using
                Dim thumbnailImageString As String = ImageToBMPBase64(drawingImage)
                newManager.CONTENT_THUMBNAIL = thumbnailImageString
            End If
        Else
            currentFileString = fileContent.fileHexString
        End If

        fileStorageEntity.FILE_STRING = currentFileString
        fileStorageEntity.IS_DELETED = False

        Dim currentStorage As FILE_STORAGE = newManager.FILE_STORAGE
        If currentStorage IsNot Nothing Then
            currentStorage.IS_DELETED = True
        End If

        fileStorageEntity.CONTENT_MANAGER.Add(newManager)
        getDatabaseEntity.FILE_STORAGE.Add(fileStorageEntity)
    End Sub

    Private Sub updateManagerWithAudio(fileContent As FileContent, newManager As CONTENT_MANAGER, Optional useHexString As Boolean = False)
        Dim currentAudioString As String
        Dim audioStorageEntity As New AUDIO_STORAGE
        If Not useHexString Then
            Dim byteArray As Byte() = File.ReadAllBytes(fileContent.filePath)
            currentAudioString = Convert.ToBase64String(byteArray)
            audioStorageEntity.NAME = fileContent.filePath
        Else
            currentAudioString = fileContent.fileHexString
        End If

        audioStorageEntity.AUDIO_STRING = currentAudioString
        audioStorageEntity.IS_DELETED = False

        Dim currentStorage As AUDIO_STORAGE = newManager.AUDIO_STORAGE
        If currentStorage IsNot Nothing Then
            currentStorage.IS_DELETED = True
        End If

        audioStorageEntity.CONTENT_MANAGER.Add(newManager)
        getDatabaseEntity.AUDIO_STORAGE.Add(audioStorageEntity)
    End Sub

    Private Sub updateManagerWithVideo(fileContent As FileContent, newManager As CONTENT_MANAGER, Optional useHexString As Boolean = False)
        Dim currentVideoString As String
        Dim videoStorageEntity As New VIDEO_STORAGE
        Dim ffmpeg = New VideoFileReader
        Dim tempFileName As String

        If Not useHexString Then
            videoStorageEntity.NAME = fileContent.filePath
            Dim byteArray As Byte() = File.ReadAllBytes(fileContent.filePath)
            ffmpeg.Open(fileContent.filePath)
            currentVideoString = Convert.ToBase64String(byteArray)
        Else
            currentVideoString = fileContent.fileHexString
            tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
            Dim byteArray As Byte() = Convert.FromBase64String(currentVideoString)
            IO.File.WriteAllBytes(tempFileName, byteArray)
            ffmpeg.Open(fileContent.filePath)
        End If

        Dim drawingBitmap As Drawing.Bitmap = ffmpeg.ReadVideoFrame()
        Dim thumbnailDrawingImage As Drawing.Image = drawingBitmap.GetThumbnailImage(120, 90, Nothing, Nothing)
        Dim thumbnailImageString As String = ImageToBMPBase64(thumbnailDrawingImage)
        newManager.CONTENT_THUMBNAIL = thumbnailImageString

        If tempFileName IsNot Nothing Then
            File.Delete(tempFileName)
        End If

        videoStorageEntity.VIDEO_STRING = currentVideoString
        videoStorageEntity.IS_DELETED = False

        Dim currentStorage As VIDEO_STORAGE = newManager.VIDEO_STORAGE
        If currentStorage IsNot Nothing Then
            currentStorage.IS_DELETED = True
        End If

        videoStorageEntity.CONTENT_MANAGER.Add(newManager)
        getDatabaseEntity.VIDEO_STORAGE.Add(videoStorageEntity)
    End Sub

    Private Sub updateManagerWithImage(fileContent As FileContent, newManager As CONTENT_MANAGER, Optional useHexString As Boolean = False)
        Dim drawingImage As Drawing.Image
        Dim imageStringEntity As New IMAGE_STRING
        Dim imageHexString As String

        If Not useHexString Then
            drawingImage = getDrawingImageFromFilePath(fileContent.filePath)
            imageStringEntity.NAME = fileContent.filePath
            imageHexString = ImageToBMPBase64(drawingImage)
        Else
            Dim controlImage As Image = convertHexStringToControlImage(fileContent.fileHexString)
            drawingImage = convertControlImageToDrawingImage(controlImage)
            imageHexString = fileContent.fileHexString
        End If

        Dim thumbNailImage As Drawing.Image = createThumbnailFromFullsizeImage(drawingImage)
        Dim thumbnailImageString As String = ImageToBMPBase64(thumbNailImage)
        newManager.CONTENT_THUMBNAIL = thumbnailImageString

        imageStringEntity.IMAGE_HEX_STRING = imageHexString
        imageStringEntity.IS_DELETED = False

        Dim currentStorage As IMAGE_STRING = newManager.IMAGE_STRING
        If currentStorage IsNot Nothing Then
            currentStorage.IS_DELETED = True
        End If

        imageStringEntity.CONTENT_MANAGER.Add(newManager)
        getDatabaseEntity.IMAGE_STRING.Add(imageStringEntity)
    End Sub

    ''' <summary>
    ''' Delete the content manager with the specified ID. Will reset the default version
    ''' of the versioning tree if the deleted manager is the default
    ''' </summary>
    ''' <param name="managerId">The ID of the manager to be deleted</param>
    ''' <returns></returns>
    Public Function deleteManagerById(managerId As Integer) As Boolean
        Try
            Dim contentManager As CONTENT_MANAGER = getContentManager(managerId)
            deleteManagerAndResetRoot(contentManager)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Modify an existing content manager. Behavior-wise this is the same as 
    ''' uploading a new file to a new content manager, except that the already existing manager will
    ''' be used with all old data wiped out and replacedS
    ''' </summary>
    ''' <param name="managerId"></param>
    ''' <param name="fileContent"></param>
    ''' <param name="uploadSetting"></param>
    ''' <returns></returns>
    Public Function modifyExistingManager(managerId As Integer, fileContent As FileContent, uploadSetting As UploadSetting) As Boolean
        Try
            Dim newManager As CONTENT_MANAGER = getContentManager(managerId)
            If newManager IsNot Nothing Then
                newManager.IS_DELETED = False

                If fileContent IsNot Nothing Then
                    updateManagerWithFileContent(fileContent, newManager)
                    populateManagerWithUploadSetting(newManager, uploadSetting)
                    getDatabaseEntity.SaveChanges()
                    Return True
                Else
                    Return False
                End If
            End If
        Catch ex As Exception

        End Try
        Return False
    End Function

    Private Sub populateManagerWithUploadSetting(ByRef newManager As CONTENT_MANAGER, uploadSetting As UploadSetting)
        If uploadSetting IsNot Nothing Then
            newManager.CONTENT_NAME = uploadSetting.contentName
            newManager.CONTENT_GROUP = uploadSetting.contentGroup
            newManager.CONTENT_TYPE = uploadSetting.contentType
            newManager.CONTENT_CATEGORY = uploadSetting.contentCategory
            newManager.NOTES = uploadSetting.contentNotes
            newManager.VERSION_NUMBER = uploadSetting.versionNumber

            If uploadSetting.parentManagerId IsNot Nothing Then
                Dim parentManager As CONTENT_MANAGER = getContentManager(uploadSetting.parentManagerId)
                If parentManager IsNot Nothing Then
                    If parentManager.CONTENT_MANAGER_TYPE = newManager.CONTENT_MANAGER_TYPE Then
                        newManager.CONTENT_MANAGER2 = parentManager
                    End If
                End If

                If uploadSetting.isDefault = True Then
                    setClusterDefaultManager(newManager)
                End If
            Else
                If newManager.PARENT_VERSION_LINK_ID Is Nothing Then
                    newManager.IS_DEFAULT = True
                Else
                    If uploadSetting.isDefault = True Then
                        setClusterDefaultManager(newManager)
                    End If
                End If
            End If
        End If
    End Sub
End Module
