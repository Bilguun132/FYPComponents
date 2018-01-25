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

Partial Public Class REVENUE_AND_COST
    Public Property ID As Integer
    Public Property FLOW_TYPE As Nullable(Of Integer)
    Public Property PAYMENT_TYPE As Nullable(Of Integer)
    Public Property RECURSIVE_DURATION As Nullable(Of Integer)
    Public Property PAYMENT_AMOUNT As Nullable(Of Decimal)
    Public Property NAME As String
    Public Property PARENT_REVENUE_AND_COST_LINK_ID As Nullable(Of Integer)
    Public Property DESCRIPTION As String
    Public Property FIRM_LINK_ID As Nullable(Of Integer)
    Public Property PERIOD_LINK_ID As Nullable(Of Integer)
    Public Property IS_DELETED As Nullable(Of Boolean)
    Public Property PERIOD_NUMBER As Nullable(Of Integer)
    Public Property TARGET_TYPE As Nullable(Of Integer)
    Public Property FIRST_PAYMENT_DATE As Nullable(Of Decimal)
    Public Property BUSINESS_ASPECT As Nullable(Of Integer)

    Public Overridable Property BALANCE_SHEET_TRANSACTION As ICollection(Of BALANCE_SHEET_TRANSACTION) = New HashSet(Of BALANCE_SHEET_TRANSACTION)
    Public Overridable Property GAME_FIRM As GAME_FIRM
    Public Overridable Property PERIOD As PERIOD
    Public Overridable Property PRODUCTION_DECISIONS As ICollection(Of PRODUCTION_DECISIONS) = New HashSet(Of PRODUCTION_DECISIONS)
    Public Overridable Property REVENUE_AND_COST1 As ICollection(Of REVENUE_AND_COST) = New HashSet(Of REVENUE_AND_COST)
    Public Overridable Property REVENUE_AND_COST2 As REVENUE_AND_COST
    Public Overridable Property RND_DECISIONS As ICollection(Of RND_DECISIONS) = New HashSet(Of RND_DECISIONS)

End Class
