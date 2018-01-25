Public Module PublicFunctions

#Region "Delegates"
    Public Delegate Sub ShowThisUserControl(ByVal userControl As Object)
    Public ShowThisUserControlCallback As ShowThisUserControl = Nothing
#End Region

End Module
