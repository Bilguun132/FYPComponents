Public Module DatabaseEntitySingleton
    Private Property databaseEntity As FYP_DATABASEEntities

    Public Function getDatabaseEntity() As FYP_DATABASEEntities
        If databaseEntity Is Nothing Then
            databaseEntity = New FYP_DATABASEEntities()
        End If

        Return databaseEntity
    End Function
End Module
