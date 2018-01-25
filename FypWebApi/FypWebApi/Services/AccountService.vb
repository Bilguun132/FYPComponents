Imports System.Net.Http
Imports GameAdministratorCenter.Contracts
Imports Newtonsoft
Imports Newtonsoft.Json

Public Module AccountService
    Public Function registerNewAccount(username As String, email As String, password As String) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage
        If (username IsNot Nothing OrElse email IsNot Nothing) AndAlso password IsNot Nothing Then
            Dim existingUsernamePersonnel As USER = getPersonnelWitthUsername(username)

            Dim existingEmailPersonnel As USER = getPersonnelWithEmail(email)

            If existingUsernamePersonnel Is Nothing AndAlso existingEmailPersonnel Is Nothing Then
                If (notEmptyOrNullString(username) OrElse notEmptyOrNullString(email)) AndAlso notEmptyOrNullString(password) Then
                    createNewAccount(username, email, password)
                    returnedMessage.StatusCode = Net.HttpStatusCode.OK
                    Dim wrapperObject As New BaseModel(Enums.success, "Successfully registerd")
                    returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Else
                    returnedMessage.StatusCode = Net.HttpStatusCode.OK
                    Dim wrapperObject As New BaseModel(Enums.missingParameters, "Either username or email and password is required")
                    returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                End If
            Else
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                Dim wrapperObject As New BaseModel(Enums.conflict, "An account with that username or email already exists")
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            End If
        Else
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.missingParameters, "Missing username/email or password")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End If

        Return returnedMessage
    End Function

    Private Sub createNewAccount(username As String, email As String, password As String)
        Dim entities = DatabaseService.getDatabaseEntity
        Dim personnel As New USER
        personnel.IS_DELETED = False
        If notEmptyOrNullString(username) Then
            personnel.USER_NAME = username.ToLower.Trim
        End If

        If notEmptyOrNullString(email) Then
            personnel.EMAIL_PRIMARY = email.ToLower.Trim
        End If

        personnel.PASSWORD_HASH = password
        entities.USERs.Add(personnel)
        entities.SaveChanges()
        entities = Nothing
    End Sub

    Private Function notEmptyOrNullString(passedInString As String) As Boolean
        Return passedInString IsNot Nothing AndAlso passedInString.Trim.Length > 0
    End Function

    Private Function getPersonnelWithEmail(email As String) As USER
        Try
            Dim personnel As USER = (From query In DatabaseService.getDatabaseEntity.USERs Where query.EMAIL_PRIMARY = email.ToLower.Trim AndAlso query.IS_DELETED = False).First
            Return personnel
        Catch
            Return Nothing
        End Try
    End Function

    Private Function getPersonnelWitthUsername(username As String) As USER
        Try
            Dim personnel As USER = (From query In DatabaseService.getDatabaseEntity.USERs Where query.USER_NAME = username.ToLower.Trim AndAlso query.IS_DELETED = False).First
            Return personnel
        Catch
            Return Nothing
        End Try
    End Function

    Public Function loginAccount(username As String, email As String, password As String) As HttpResponseMessage
        Dim returnedMessage As New HttpResponseMessage
        If (username IsNot Nothing OrElse email IsNot Nothing) AndAlso password IsNot Nothing Then
            Dim existingPersonnel = getPersonnelWitthUsername(username)

            If existingPersonnel Is Nothing AndAlso email IsNot Nothing Then
                existingPersonnel = getPersonnelWithEmail(email)
            ElseIf notEmptyOrNullString(email) AndAlso existingPersonnel.EMAIL_PRIMARY <> email.ToLower.Trim Then
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                Dim wrapperObject As New BaseModel(Enums.conflict, "The provided username and email do not match")
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Return returnedMessage
            End If

            If existingPersonnel Is Nothing Then
                returnedMessage.StatusCode = Net.HttpStatusCode.OK
                Dim wrapperObject As New BaseModel(Enums.notFound, "The username or email provided is not registered")
                returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
            Else
                If existingPersonnel.PASSWORD_HASH = password Then
                    returnedMessage.StatusCode = Net.HttpStatusCode.OK
                    Dim wrapperObject As New BaseModel(Enums.success, "Personnel Id: " + existingPersonnel.ID.ToString)
                    returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                Else
                    returnedMessage.StatusCode = Net.HttpStatusCode.OK
                    Dim wrapperObject As New BaseModel(Enums.parameterInvalid, "The provided password is incorrect")
                    returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
                End If
            End If
        Else
            returnedMessage.StatusCode = Net.HttpStatusCode.OK
            Dim wrapperObject As New BaseModel(Enums.missingParameters, "Missing username/email or password")
            returnedMessage.Content = New StringContent(JsonConvert.SerializeObject(wrapperObject), Text.Encoding.UTF8, "application/json")
        End If

        Return returnedMessage
    End Function
End Module
