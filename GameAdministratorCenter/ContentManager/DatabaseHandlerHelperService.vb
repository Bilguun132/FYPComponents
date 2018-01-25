Imports System.Windows
Imports GameAdministratorCenter.Contracts

Public Module DatabaseHandlerHelperService
    Public Function getContentManager(ID As Integer) As CONTENT_MANAGER
        Try
            Dim UEIOSEntity As FYP_DATABASEEntities = getDatabaseEntity()
            Dim returnedEntity As CONTENT_MANAGER = (From query In UEIOSEntity.CONTENT_MANAGER Where query.ID = ID AndAlso (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False)).FirstOrDefault()
            Return returnedEntity
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function getAllRelatedManagers(currentManager As CONTENT_MANAGER) As List(Of CONTENT_MANAGER)
        Dim processedList As New List(Of CONTENT_MANAGER)
        Dim unProcessedList As New List(Of CONTENT_MANAGER)

        unProcessedList.Add(currentManager)

        While (unProcessedList.Count > 0)
            Dim item As CONTENT_MANAGER = unProcessedList.First()
            If item.CONTENT_MANAGER2 IsNot Nothing AndAlso Not processedList.Contains(item.CONTENT_MANAGER2) Then
                If item.CONTENT_MANAGER2.IS_DELETED = False Then
                    unProcessedList.Add(item.CONTENT_MANAGER2)
                End If
            End If
            If item.CONTENT_MANAGER1 IsNot Nothing Then
                For Each subbranch As CONTENT_MANAGER In item.CONTENT_MANAGER1
                    If subbranch.IS_DELETED = False AndAlso Not processedList.Contains(subbranch) Then
                        unProcessedList.Add(subbranch)
                    End If
                Next
            End If
            unProcessedList.Remove(item)
            processedList.Add(item)
        End While

        Return processedList
    End Function

    Public Function getAllRootManagers(managerType As ContentManagerType) As List(Of CONTENT_MANAGER)
        Try
            Dim UEIOSEntity As FYP_DATABASEEntities = getDatabaseEntity()

            Select Case managerType
                Case ContentManagerType.Image
                    Dim returnedEntities As List(Of CONTENT_MANAGER) = (From query In UEIOSEntity.CONTENT_MANAGER Where query.IS_DELETED = False AndAlso
                                                                    query.CONTENT_MANAGER_TYPE = ContentManagerType.Image AndAlso
                                                                    query.CONTENT_MANAGER2 Is Nothing).ToList()
                    Return returnedEntities
                Case ContentManagerType.Audio
                    Dim returnedEntities As List(Of CONTENT_MANAGER) = (From query In UEIOSEntity.CONTENT_MANAGER Where query.IS_DELETED = False AndAlso
                                                                    query.CONTENT_MANAGER_TYPE = ContentManagerType.Audio AndAlso
                                                                    query.CONTENT_MANAGER2 Is Nothing).ToList()
                    Return returnedEntities
                Case ContentManagerType.Video
                    Dim returnedEntities As List(Of CONTENT_MANAGER) = (From query In UEIOSEntity.CONTENT_MANAGER Where query.IS_DELETED = False AndAlso
                                                                    query.CONTENT_MANAGER_TYPE = ContentManagerType.Video AndAlso
                                                                    query.CONTENT_MANAGER2 Is Nothing).ToList()
                    Return returnedEntities
                Case ContentManagerType.GeneralFile
                    Dim returnedEntities As List(Of CONTENT_MANAGER) = (From query In UEIOSEntity.CONTENT_MANAGER Where query.IS_DELETED = False AndAlso
                                                                    query.CONTENT_MANAGER_TYPE = ContentManagerType.GeneralFile AndAlso
                                                                    query.CONTENT_MANAGER2 Is Nothing).ToList()
                    Return returnedEntities
                Case Else
                    Return Nothing
            End Select

        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Function getDefaultVersion(cluster As List(Of CONTENT_MANAGER)) As CONTENT_MANAGER
        If cluster IsNot Nothing Then
            For Each item As CONTENT_MANAGER In cluster
                If item.IS_DEFAULT = True AndAlso item.IS_DELETED = False Then
                    Return item
                End If
            Next
        End If

        Return Nothing
    End Function

    Public Function getAllDefaultManagers(managerType As ContentManagerType) As List(Of CONTENT_MANAGER)
        Try
            Dim allRootManagers As List(Of CONTENT_MANAGER) = getAllRootManagers(managerType)
            If allRootManagers IsNot Nothing Then
                Dim returnedList As New List(Of CONTENT_MANAGER)
                For Each item As CONTENT_MANAGER In allRootManagers
                    Dim cluster As List(Of CONTENT_MANAGER) = getAllSubbranchManagers(item)
                    Dim defaultVersion As CONTENT_MANAGER = getDefaultVersion(cluster)
                    If defaultVersion IsNot Nothing Then
                        returnedList.Add(defaultVersion)
                    End If
                Next
                Return returnedList
            Else
                Return Nothing
            End If
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function getAllSubbranchManagers(topLevelManager As CONTENT_MANAGER) As List(Of CONTENT_MANAGER)
        Dim processedList As New List(Of CONTENT_MANAGER)
        Dim unProcessedList As New List(Of CONTENT_MANAGER)

        unProcessedList.Add(topLevelManager)

        While (unProcessedList.Count > 0)
            Dim item As CONTENT_MANAGER = unProcessedList.First()
            If item.CONTENT_MANAGER1 IsNot Nothing Then
                For Each subbranch As CONTENT_MANAGER In item.CONTENT_MANAGER1
                    If subbranch.IS_DELETED = False AndAlso Not processedList.Contains(subbranch) Then
                        unProcessedList.Add(subbranch)
                    End If
                Next
            End If
            unProcessedList.Remove(item)
            processedList.Add(item)
        End While

        Return processedList
    End Function

    Public Function getRootManager(currentManager As CONTENT_MANAGER) As CONTENT_MANAGER
        If currentManager IsNot Nothing Then
            While currentManager.CONTENT_MANAGER2 IsNot Nothing
                currentManager = currentManager.CONTENT_MANAGER2
            End While

            Return currentManager
        Else
            Return Nothing
        End If
    End Function

    Public Function deleteManagerAndResetRoot(managerToDelete As CONTENT_MANAGER) As String
        Try
            Dim allSubVersionList As List(Of CONTENT_MANAGER) = getAllSubbranchManagers(managerToDelete)
            For Each item As CONTENT_MANAGER In allSubVersionList
                If item.IS_DEFAULT = True Then
                    Dim rootManager As CONTENT_MANAGER = getRootManager(managerToDelete)
                    rootManager.IS_DEFAULT = True
                End If
            Next

            For Each item As CONTENT_MANAGER In allSubVersionList
                item.IS_DELETED = True
            Next
            getDatabaseEntity.SaveChanges()
            Return ""
        Catch ex As Exception
            Return ex.ToString
        End Try
    End Function

    Public Sub setClusterDefaultManager(managerToSet As CONTENT_MANAGER)
        Try
            Dim cluster As List(Of CONTENT_MANAGER) = getAllRelatedManagers(managerToSet)
            For Each item As CONTENT_MANAGER In cluster
                item.IS_DEFAULT = False
            Next
            managerToSet.IS_DEFAULT = True
            getDatabaseEntity.SaveChanges()
        Catch ex As Exception

        End Try
    End Sub

    Public Sub downloadContentByManagerId(managerId As Integer, contentType As ContentManagerType)
        Dim targetManager As CONTENT_MANAGER = getContentManager(managerId)
        If targetManager IsNot Nothing Then
            Select Case contentType
                Case ContentManagerType.Image
                    Dim matchingImageString As IMAGE_STRING = targetManager.IMAGE_STRING
                    If matchingImageString IsNot Nothing AndAlso matchingImageString.IMAGE_HEX_STRING IsNot Nothing Then
                        downloadImage(matchingImageString.IMAGE_HEX_STRING, matchingImageString.NAME)
                    End If
                Case ContentManagerType.Audio
                    Dim matchingAudioStorage As AUDIO_STORAGE = targetManager.AUDIO_STORAGE
                    If matchingAudioStorage IsNot Nothing AndAlso matchingAudioStorage.AUDIO_STRING IsNot Nothing Then
                        downloadAudio(matchingAudioStorage.AUDIO_STRING, matchingAudioStorage.NAME)
                    End If
                Case ContentManagerType.Video
                    Dim matchingVideoStorage As VIDEO_STORAGE = targetManager.VIDEO_STORAGE
                    If matchingVideoStorage IsNot Nothing AndAlso matchingVideoStorage.VIDEO_STRING IsNot Nothing Then
                        downloadVideo(matchingVideoStorage.VIDEO_STRING, matchingVideoStorage.NAME)
                    End If
                Case ContentManagerType.GeneralFile
                    Dim matchingFileStorage As FILE_STORAGE = targetManager.FILE_STORAGE
                    If matchingFileStorage IsNot Nothing AndAlso matchingFileStorage.FILE_STRING IsNot Nothing Then
                        downloadFile(matchingFileStorage.FILE_STRING, matchingFileStorage.FILE_NAME)
                    End If
            End Select
        End If
    End Sub

    Private Sub downloadFile(fileString As String, originalFileName As String)
        If fileString IsNot Nothing Then
            Dim saveFileDialog As New Forms.SaveFileDialog()
            If originalFileName IsNot Nothing Then
                Dim extension As String = IO.Path.GetExtension(originalFileName)
                saveFileDialog.Filter = extension.Replace(".", "") + " file|*" + extension
                saveFileDialog.FileName = IO.Path.GetFileNameWithoutExtension(originalFileName)
            Else
                saveFileDialog.Filter = "All files|*.*"
            End If

            saveFileDialog.Title = "Save File"
            Dim saveFileResult As Forms.DialogResult = saveFileDialog.ShowDialog()
            If (saveFileResult = System.Windows.Forms.DialogResult.OK) Then
                Dim fileName As String = saveFileDialog.FileName
                Dim byteArray As Byte() = Convert.FromBase64String(fileString)
                IO.File.WriteAllBytes(fileName, byteArray)
            End If
        End If
    End Sub

    Private Sub downloadVideo(videoString As String, originalFileName As String)
        If videoString IsNot Nothing Then
            Dim saveFileDialog As New Forms.SaveFileDialog()
            If originalFileName IsNot Nothing Then
                Dim extension As String = IO.Path.GetExtension(originalFileName)
                saveFileDialog.Filter = extension.Replace(".", "") + " file|*" + extension
                saveFileDialog.FileName = IO.Path.GetFileNameWithoutExtension(originalFileName)
            Else
                saveFileDialog.Filter = "mp4 file|*.mp4"
            End If

            saveFileDialog.Title = "Save Video"
            Dim saveFileResult As Forms.DialogResult = saveFileDialog.ShowDialog()
            If (saveFileResult = System.Windows.Forms.DialogResult.OK) Then
                Dim fileName As String = saveFileDialog.FileName
                Dim videoByteArray As Byte() = Convert.FromBase64String(videoString)
                IO.File.WriteAllBytes(fileName, videoByteArray)
            End If
        End If
    End Sub

    Private Sub downloadAudio(audioString As String, originalFileName As String)
        If audioString IsNot Nothing Then
            Dim saveFileDialog As New Forms.SaveFileDialog()
            If originalFileName IsNot Nothing Then
                Dim extension As String = IO.Path.GetExtension(originalFileName)
                saveFileDialog.Filter = extension.Replace(".", "") + " file|*" + extension
                saveFileDialog.FileName = IO.Path.GetFileNameWithoutExtension(originalFileName)
            Else
                saveFileDialog.Filter = "mp3 file|*.mp3"
            End If

            saveFileDialog.Title = "Save Audio"
            Dim saveFileResult As Forms.DialogResult = saveFileDialog.ShowDialog()
            If (saveFileResult = System.Windows.Forms.DialogResult.OK) Then
                Dim fileName As String = saveFileDialog.FileName
                Dim audioByteArray As Byte() = Convert.FromBase64String(audioString)
                IO.File.WriteAllBytes(fileName, audioByteArray)
            End If
        End If
    End Sub

    Private Sub downloadImage(imageString As String, originalFileName As String)
        If imageString IsNot Nothing Then
            Dim saveFileDialog As New Forms.SaveFileDialog()
            If originalFileName IsNot Nothing Then
                Dim extension As String = IO.Path.GetExtension(originalFileName)
                saveFileDialog.Filter = extension.Replace(".", "") + " file|*" + extension
                saveFileDialog.FileName = IO.Path.GetFileNameWithoutExtension(originalFileName)
            Else
                saveFileDialog.Filter = "bmp file|*.bmp"
            End If

            saveFileDialog.Title = "Save Image"
            Dim saveFileResult As Forms.DialogResult = saveFileDialog.ShowDialog()
            If (saveFileResult = System.Windows.Forms.DialogResult.OK) Then
                Dim fileName As String = saveFileDialog.FileName
                Dim audioByteArray As Byte() = Convert.FromBase64String(imageString)
                IO.File.WriteAllBytes(fileName, audioByteArray)
            End If
        End If
    End Sub

    Public Function getAllManagers(managerType As ContentManagerType) As List(Of CONTENT_MANAGER)
        Try
            Dim UEIOSEntity As FYP_DATABASEEntities = getDatabaseEntity()

            Select Case managerType
                Case ContentManagerType.Image
                    Dim returnedEntities As List(Of CONTENT_MANAGER) = (From query In UEIOSEntity.CONTENT_MANAGER Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                    query.CONTENT_MANAGER_TYPE = ContentManagerType.Image).ToList()
                    Return returnedEntities
                Case ContentManagerType.Audio
                    Dim returnedEntities As List(Of CONTENT_MANAGER) = (From query In UEIOSEntity.CONTENT_MANAGER Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                    query.CONTENT_MANAGER_TYPE = ContentManagerType.Audio).ToList()
                    Return returnedEntities
                Case ContentManagerType.Video
                    Dim returnedEntities As List(Of CONTENT_MANAGER) = (From query In UEIOSEntity.CONTENT_MANAGER Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                    query.CONTENT_MANAGER_TYPE = ContentManagerType.Video).ToList()
                    Return returnedEntities
                Case ContentManagerType.GeneralFile
                    Dim returnedEntities As List(Of CONTENT_MANAGER) = (From query In UEIOSEntity.CONTENT_MANAGER Where (query.IS_DELETED Is Nothing OrElse query.IS_DELETED = False) AndAlso
                                                                    query.CONTENT_MANAGER_TYPE = ContentManagerType.GeneralFile).ToList()
                    Return returnedEntities
                Case Else
                    Return Nothing
            End Select

        Catch ex As Exception
            Return Nothing
        End Try
    End Function
End Module
