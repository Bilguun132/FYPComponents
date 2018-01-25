Imports System.IO
Imports System.Windows
Imports ContentManager
Imports System.Windows.Xps.Packaging
Imports GameAdministratorCenter.Contracts

Public Class ExcelViewer
    Private tempFileName As String = Nothing
    Private originalFileName As String = Nothing
    Public Delegate Sub CloseFuction()
    Public DelegateCloseFunction As CloseFuction = Nothing

    Public Sub New(ByVal contentManagerId As Integer)
        InitializeComponent()

        Dim contentManager As CONTENT_MANAGER = (From query In getDatabaseEntity().CONTENT_MANAGER Where query.ID = contentManagerId).FirstOrDefault
        If contentManager IsNot Nothing Then
            Dim fileStorageEntity As FILE_STORAGE = contentManager.FILE_STORAGE
            If fileStorageEntity IsNot Nothing AndAlso fileStorageEntity.FILE_STRING IsNot Nothing Then
                Try
                    LoadExcelString(fileStorageEntity.FILE_STRING)
                Catch ex As Exception

                End Try
            End If
        End If
        If DelegateCloseFunction Is Nothing Then
            closeButton.Visibility = Visibility.Collapsed
        Else
            closeButton.Visibility = Visibility.Visible
        End If
    End Sub

    Public Sub New(ByVal inputString As String, Optional ByVal inputStringType As InputStringType = InputStringType.ExcelFileString)
        InitializeComponent()

        Try
            If inputStringType = InputStringType.FileName Then
                originalFileName = inputString
            Else
                LoadExcelString(inputString)
            End If
        Catch ex As Exception

        End Try
        If DelegateCloseFunction Is Nothing Then
            closeButton.Visibility = Visibility.Collapsed
        Else
            closeButton.Visibility = Visibility.Visible
        End If
    End Sub

    Private Sub LoadExcelString(ByVal intputString As String)
        tempFileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".xls")
        Dim byteArray As Byte() = Convert.FromBase64String(intputString)
        File.WriteAllBytes(tempFileName, byteArray)
    End Sub

    Public Enum InputStringType
        FileName = 1
        ExcelFileString = 2
    End Enum

    Private Sub CloseButtonClick(sender As Object, e As RoutedEventArgs)
        If DelegateCloseFunction IsNot Nothing Then
            DelegateCloseFunction()
        End If
    End Sub

    Private Sub ExcelViewer_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Dim excelApplication As Microsoft.Office.Interop.Excel.Application
        Dim excelWorkbook As Microsoft.Office.Interop.Excel.Workbook = Nothing
        Dim paramSource As String = Nothing

        If tempFileName IsNot Nothing Then
            paramSource = tempFileName
        Else
            paramSource = originalFileName
        End If

        Dim extension As String = Path.GetExtension(paramSource)
        Dim paramExportPath As String = paramSource.Replace(extension, ".xps")
        Dim paramExportFormat As Microsoft.Office.Interop.Excel.XlFixedFormatType = Microsoft.Office.Interop.Excel.XlFixedFormatType.xlTypeXPS
        Dim paramExportQuality As Microsoft.Office.Interop.Excel.XlFixedFormatQuality = Microsoft.Office.Interop.Excel.XlFixedFormatQuality.xlQualityStandard
        Dim paramOpenAfterPublish As Boolean = False
        Dim paramIncludeDocPros As Boolean = True
        Dim paramIgnorePrintArea As Boolean = True
        Dim paramFromPage As Object = Type.Missing
        Dim paramToPage As Object = Type.Missing

        Try
            excelApplication = CreateObject("Excel.Application")
            excelWorkbook = excelApplication.Workbooks.Open(paramSource)
            If excelWorkbook IsNot Nothing Then
                excelWorkbook.ExportAsFixedFormat(paramExportFormat, paramExportPath, paramExportQuality, paramIncludeDocPros, paramIgnorePrintArea, paramFromPage, paramToPage, paramOpenAfterPublish)
            End If
        Catch ex As Exception
            MessageBox.Show("Unable to open file.")
        Finally
            If excelWorkbook IsNot Nothing Then
                excelWorkbook.Close(False)
                excelWorkbook = Nothing
            End If

            If excelApplication IsNot Nothing Then
                excelApplication.Quit()
                excelApplication = Nothing
            End If

            GC.Collect()
            GC.WaitForPendingFinalizers()
        End Try

        Dim xpsDoc As New XpsDocument(paramExportPath, FileAccess.Read)
        docViewer.Document = xpsDoc.GetFixedDocumentSequence()
    End Sub

    Private Sub ExcelViewer_Unloaded(sender As Object, e As RoutedEventArgs) Handles Me.Unloaded
        Try
            File.Delete(tempFileName)
        Catch ex As Exception

        End Try
    End Sub

    Private Sub ExcelViewer_RequestBringIntoView(sender As Object, e As RequestBringIntoViewEventArgs) Handles Me.RequestBringIntoView
        e.Handled = True
    End Sub
End Class
