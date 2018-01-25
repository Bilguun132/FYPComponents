'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated from a template.
'
'     Manual changes to this file may cause unexpected behavior in your application.
'     Manual changes to this file will be overwritten if the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Imports System
Imports System.Collections.Generic

Partial Public Class USER
    Public Property ID As Integer
    Public Property USER_NAME As String
    Public Property FIRST_NAME As String
    Public Property LAST_NAME As String
    Public Property POSITION As String
    Public Property DEPARTMENT As String
    Public Property GROUP As String
    Public Property MOBILE_PERSONAL As String
    Public Property MOBILE_OFFICE As String
    Public Property LAND_LINE_OFFICE As String
    Public Property LAND_LINE_HOME As String
    Public Property EMAIL_PRIMARY As String
    Public Property EMAIL_SECONDARY As String
    Public Property SECURITY_LEVEL As Nullable(Of Short)
    Public Property LOGIN_ALLOWED As Nullable(Of Boolean)
    Public Property PASSWORD_HASH As String
    Public Property PASSWORD_SALT As String
    Public Property IS_AVAILABLE As Nullable(Of Boolean)
    Public Property IS_DELETED As Nullable(Of Boolean)
    Public Property PIN_NUMBER_HASH As String
    Public Property PIN_NUMBER_SALT As String
    Public Property IMAGE_MANAGER_LINK_ID As Nullable(Of Integer)

    Public Overridable Property ACTION_TRANSACTIONS As ICollection(Of ACTION_TRANSACTIONS) = New HashSet(Of ACTION_TRANSACTIONS)
    Public Overridable Property APP_DEVICES As ICollection(Of APP_DEVICES) = New HashSet(Of APP_DEVICES)
    Public Overridable Property CONTENT_MANAGER As ICollection(Of CONTENT_MANAGER) = New HashSet(Of CONTENT_MANAGER)
    Public Overridable Property CONTENT_MANAGER1 As CONTENT_MANAGER
    Public Overridable Property MARKETING_DECISIONS As ICollection(Of MARKETING_DECISIONS) = New HashSet(Of MARKETING_DECISIONS)
    Public Overridable Property PRODUCTION_DECISIONS As ICollection(Of PRODUCTION_DECISIONS) = New HashSet(Of PRODUCTION_DECISIONS)
    Public Overridable Property RND_DECISIONS As ICollection(Of RND_DECISIONS) = New HashSet(Of RND_DECISIONS)
    Public Overridable Property USER_USER_ROLE_GAME_FIRM_RELATIONSHIP As ICollection(Of USER_USER_ROLE_GAME_FIRM_RELATIONSHIP) = New HashSet(Of USER_USER_ROLE_GAME_FIRM_RELATIONSHIP)
    Public Overridable Property WEB_API_AUTHORIZATION As ICollection(Of WEB_API_AUTHORIZATION) = New HashSet(Of WEB_API_AUTHORIZATION)

End Class