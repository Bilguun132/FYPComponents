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

Partial Public Class USER_ROLE
    Public Property ID As Integer
    Public Property USER_ROLE_NAME As String
    Public Property AUTHORIZATION_LEVEL As Nullable(Of Integer)
    Public Property IS_DELETED As Nullable(Of Boolean)

    Public Overridable Property USER_USER_ROLE_GAME_RELATIONSHIP As ICollection(Of USER_USER_ROLE_GAME_RELATIONSHIP) = New HashSet(Of USER_USER_ROLE_GAME_RELATIONSHIP)

End Class