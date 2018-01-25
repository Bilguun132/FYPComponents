Imports GameAdministratorCenter.Contracts

Public Module DatabaseService
    Private Property databaseEntity As FYP_DATABASEEntities

    Public Function getDatabaseEntity() As FYP_DATABASEEntities
        'If databaseEntity Is Nothing Then
        '    databaseEntity = New FYP_DATABASEEntities
        'End If

        'Return databaseEntity

        Return New FYP_DATABASEEntities
    End Function
End Module
