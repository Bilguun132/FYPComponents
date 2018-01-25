Imports System.IO
Imports System.Windows
Imports GameAdministratorCenter.Contracts

Public Class PDFViewer
    Private tempFileName As String = ""
    Private originalFileName As String = ""
    Public Delegate Sub afterCloseFunction()
    Public afterCloseFunctionDelegate As afterCloseFunction = Nothing

    Public Sub New(contentManagerID As Integer)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim fileManager As CONTENT_MANAGER = (From query In getDatabaseEntity().CONTENT_MANAGER Where query.ID = contentManagerID).FirstOrDefault
        If fileManager IsNot Nothing Then
            Dim associatedFileStorage As FILE_STORAGE = fileManager.FILE_STORAGE
            If associatedFileStorage IsNot Nothing AndAlso associatedFileStorage.FILE_STRING IsNot Nothing Then
                Try
                    loadPdfString(associatedFileStorage.FILE_STRING)
                Catch ex As Exception

                End Try
            End If
        End If
    End Sub

    Public Sub New(inputString As String, Optional inputStringType As inputStringType = inputStringType.pdfFileString)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Try
            If inputStringType = inputStringType.fileName Then
                originalFileName = inputString
            Else
                loadPdfString(inputString)
            End If
        Catch ex As Exception

        End Try
    End Sub

    Private Sub loadPdfString(inputString As String)
        tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf")
        Dim fileByteArray As Byte() = Convert.FromBase64String(inputString)
        File.WriteAllBytes(tempFileName, fileByteArray)
    End Sub

    Public Sub hideCloseButton()
        closeButton.Visibility = Visibility.Collapsed
    End Sub

    Public Sub showCloseButton()
        closeButton.Visibility = Visibility.Visible
    End Sub

    Private Sub PDFViewer_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
        Try
            File.Delete(tempFileName)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub closeButton_Click(sender As Object, e As RoutedEventArgs) Handles closeButton.Click
        If afterCloseFunctionDelegate IsNot Nothing Then
            afterCloseFunctionDelegate()
        End If
    End Sub

    Private Sub pdfViewer_Loaded(sender As Object, e As RoutedEventArgs) Handles pdfViewer.Loaded
        If tempFileName <> "" Then
            pdfViewer.DocumentSource = tempFileName
        Else
            pdfViewer.DocumentSource = originalFileName
        End If
    End Sub

    Private Sub PDFViewer_RequestBringIntoView(sender As Object, e As RequestBringIntoViewEventArgs) Handles Me.RequestBringIntoView
        e.Handled = True
    End Sub

    Public Enum inputStringType
        fileName = 1
        pdfFileString = 2
    End Enum
End Class
