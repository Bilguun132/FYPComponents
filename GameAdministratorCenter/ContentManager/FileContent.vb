Imports GameAdministratorCenter.Contracts

''' <summary>
''' This class holds the structure for representing files to be uploaded
''' into the database by the ContentManager public upload functions
''' </summary>
Public Class FileContent
    ''' <summary>
    ''' The full physical path to the file on the .Net device
    ''' Example: C:\Users\User\Desktop\test.doc
    ''' </summary>
    ''' <returns></returns>
    Public Property filePath As String
    ''' <summary>
    ''' The type of the file to be uploaded
    ''' ****NOTE**** This must be set if the upload is done using the file hex string instead of the file path
    ''' ****NOTE**** If filePath is used to upload the file and this is not set, the type of file is determined by the file extension
    ''' </summary>
    ''' <returns></returns>
    Public Property fileType As ContentManagerType
    ''' <summary>
    ''' The hex string of the file to be uploaded
    ''' This can be used in place of the filePath if the file is gotten from a stream 
    ''' instead of a physical file
    ''' </summary>
    ''' <returns></returns>
    Public Property fileHexString As String
End Class
