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

Partial Public Class IMAGE_STRING
    Public Property ID As Integer
    Public Property NAME As String
    Public Property IMAGE_HEX_STRING As String
    Public Property IS_DELETED As Nullable(Of Boolean)
    Public Property IMAGE_STRING_ID_FOR_INTERNAL_REFERENCE_TO_UNDO_AND_REDO As Nullable(Of Integer)

    Public Overridable Property CONTENT_MANAGER As ICollection(Of CONTENT_MANAGER) = New HashSet(Of CONTENT_MANAGER)

End Class
