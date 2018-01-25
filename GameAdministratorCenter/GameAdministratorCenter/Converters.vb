Imports System.Globalization
Imports System.Windows.Data
Imports ContentManager
Imports GameAdministratorCenter.Contracts

Public Class EnumConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value.value) = True Then
            Return convertNumberOfDayToTimeString(value.value)
        Else
            Return ""
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function

    Private Function convertNumberOfDayToTimeString(numberOfDay As Decimal) As String
        Dim numberOfMonths = Math.Floor(numberOfDay / 30)
        Dim numberOfDayExcludeMonth = Math.Floor(numberOfDay - numberOfMonths * 30)
        Dim numberOfHour = Math.Floor(numberOfDay - numberOfMonths * 30 - numberOfDayExcludeMonth) * 24

        Dim returnedString = String.Format("{0} months {1} days {2} hours", numberOfMonths.ToString, numberOfDayExcludeMonth.ToString, numberOfHour.ToString)
        Return returnedString
    End Function
End Class

Public Class StatusConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value.value) = True Then
            Select Case value.value
                Case ProgressStatusEnum.notStarted
                    Return "Not Started"
                Case ProgressStatusEnum.inProgress
                    Return "In Progress"
                Case ProgressStatusEnum.completed
                    Return "Completed"
                Case Else
                    Return "Not Started"
            End Select
        Else
            Return "Not Started"
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class

Public Class ImageConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value.value) = True Then
            Select Case value.value
                Case ProgressStatusEnum.notStarted
                    Return convertBitmapToBitmapSource(My.Resources.Orange_circle)
                Case ProgressStatusEnum.inProgress
                    Return convertBitmapToBitmapSource(My.Resources.Green_circle)
                Case ProgressStatusEnum.completed
                    Return convertBitmapToBitmapSource(My.Resources.Blue_circle)
                Case Else
                    Return convertBitmapToBitmapSource(My.Resources.Orange_circle)
            End Select
        Else
            Return convertBitmapToBitmapSource(My.Resources.Orange_circle)
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class

Public Class ImageViewerConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value.value) = True Then
            Dim viewer As New ImageViewer(Integer.Parse(value.value))
            viewer.hideButtonPanel()
            viewer.disableImageManipulation()
            Return viewer
        Else
            Return Nothing
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class

Public Class CashflowTypeConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value.value) = True Then
            Return convertFlowTypeToText(value.value)
        Else
            Return ""
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        If value IsNot Nothing Then
            Select Case value.ToString
                Case "Revenue"
                    Return CashflowType.revenue
                Case "Cost"
                    Return CashflowType.cost
                Case Else
                    Return Nothing
            End Select
        Else
            Return Nothing
        End If
    End Function

    Private Function convertFlowTypeToText(flowType As Integer) As String
        Select Case flowType
            Case CashflowType.revenue
                Return "Revenue"
            Case CashflowType.cost
                Return "Cost"
            Case Else
                Return ""
        End Select
    End Function
End Class

Public Class PaymentTypeConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value.value) = True Then
            Return convertPaymentTypeToText(value.value)
        Else
            Return ""
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function

    Private Function convertPaymentTypeToText(type As Integer) As String
        Select Case type
            Case PaymentType.oneTime
                Return "One Time"
            Case PaymentType.weeklyRecursive
                Return "Weekly"
            Case PaymentType.monthlyRecursive
                Return "Monthly"
            Case PaymentType.yearlyRecursive
                Return "Yearly"
            Case Else
                Return ""
        End Select
    End Function
End Class

Public Class TargetTypeConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If IsNumeric(value.value) = True Then
            Return convertTargetTypeToText(value.value)
        Else
            Return ""
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function

    Private Function convertTargetTypeToText(type As Integer) As String
        Select Case type
            Case PaymentTargetType.asset
                Return "Asset"
            Case PaymentTargetType.liability
                Return "Liability"
            Case PaymentTargetType.equity
                Return "Equity"
            Case Else
                Return ""
        End Select
    End Function
End Class

Public Class PlayerCountConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value Is Nothing Then
            Return 0
        Else
            'Dim relationshipList As List(Of USER_USER_ROLE_GAME_FIRM_RELATIONSHIP) = DirectCast(value, ICollection(Of USER_USER_ROLE_GAME_FIRM_RELATIONSHIP))
            'Dim userIdList As New List(Of Integer)
            'For Each relationship In relationshipList
            '    If relationship.IS_DELETED Is Nothing OrElse relationship.IS_DELETED = False Then
            '        If relationship.USER_LINK_ID IsNot Nothing AndAlso Not userIdList.Contains(relationship.USER_LINK_ID) Then
            '            userIdList.Add(relationship.USER_LINK_ID)
            '        End If
            '    End If
            'Next
            'Return userIdList.Count
            Return ""
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class

Public Class IsEnabledConverterForGridControl
    Implements IValueConverter

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If value Is Nothing Then
            Return False
        Else
            Return True
        End If
    End Function

    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        Return Nothing
    End Function
End Class


