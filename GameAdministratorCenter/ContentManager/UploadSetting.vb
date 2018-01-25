''' <summary>
''' Settings for file upload using the ContentManager public functions
''' </summary>
Public Class UploadSetting
    ''' <summary>
    ''' Matches database
    ''' </summary>
    ''' <returns></returns>
    Public Property contentName As String
    ''' <summary>
    ''' Matches database
    ''' </summary>
    ''' <returns></returns>
    Public Property contentType As String
    ''' <summary>
    ''' Matches database
    ''' </summary>
    ''' <returns></returns>
    Public Property contentGroup As String
    ''' <summary>
    ''' Matches database
    ''' </summary>
    ''' <returns></returns>
    Public Property contentCategory As String
    ''' <summary>
    ''' Matches database
    ''' </summary>
    ''' <returns></returns>
    Public Property contentNotes As String
    ''' <summary>
    ''' Matches database, default to be 1 if not set
    ''' </summary>
    ''' <returns></returns>
    Public Property versionNumber As String = "1"
    ''' <summary>
    ''' Indicates whether the manager to be added/editted is set to be
    ''' default in the versioning tree or not
    ''' If the manager is the only one in its tree this is default to be true,
    ''' if not it is default to be false
    ''' </summary>
    ''' <returns></returns>
    Public Property isDefault As Boolean?
    ''' <summary>
    ''' The parent version manager id
    ''' Set this to perform versioning of the added/editted manager
    ''' ****NOTE**** This id must be of a valid manager in the database, which has the same
    ''' CONTENT_MANAGER_TYPE as the one to be added/editted
    ''' </summary>
    ''' <returns></returns>
    Public Property parentManagerId As Integer?
End Class
